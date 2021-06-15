using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.RPL;
using EthereumLibrary.Signer;
using NBitcoin;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;


namespace WalletLibrary.Core.Concrete.Wallets
{
	public class XDCWallet : CommonWallet, IWallet
	{
		public string Symbol { get; private set; } = "xdc";

		private NoxKeyGen NoxKeyGen;

		public XDCWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.XDC)
			: base(keySets, product)
		{
			NoxKeyGen = new NoxKeyGen(keySets);
		}

		public string GetLegacyAddress(uint index = 0)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets.FirstOrDefault(k => k.Index == index);
			using (var pubKeyUncompressed = keySet.Key.GetPublicKey(false))
			{

				var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);

				EthECKey ekey = new EthECKey(pubKey, NoxKeyGen);
				return ekey.GetPublicAddress().EnsureXdcPrefix();
			}
		}

		public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
		{
			PaymentRequestResponse response = new PaymentRequestResponse();

			string ethadr = GetLegacyAddress();
			if (ethadr.ToLower() != req.NoxAddress.BTCAddress.ToLower())
				throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

			using (var publicKey = KeySets[0].Key.GetPublicKey(false))
			{
				PubKey pubKey = new PubKey(publicKey.Value, 0, Symbol);
				EthECKey key = new EthECKey(pubKey, NoxKeyGen);


				var to = req.ToAddress.EnsureHexPrefix(true);
				var amount = NumUtils.Utils.ToSatoshi(req.Amount, 18);
				var nonce = System.Numerics.BigInteger.Parse(req.ETCNonce);
				var gasPrice = NumUtils.Utils.ToSatoshi(req.FeeValue, 18);
				var gasLimit = new System.Numerics.BigInteger(Int64.Parse(req.ETCGasUsed));

				var chainID = new BigInteger((int)Chain.XinFinMainnet);

				var tx = new TransactionChainId(to, amount, nonce, gasPrice, gasLimit, chainID);
				tx.Sign(key);
				var signedHex = tx.ToHex();

				response.TxnHex = signedHex;

				var ethFeeLimitSat = tx.GasLimit.ToBigIntegerFromRLPDecoded() * tx.GasPrice.ToBigIntegerFromRLPDecoded();
				var ethFeeLimitBtc = NumUtils.Utils.FromSatoshi(ethFeeLimitSat, 18);
				response.Addition = new Addition
				{
					MaxFeeAllowed = ethFeeLimitBtc
				};
			}

			return response;

		}

		public string GetSegwitAddress(uint index = 0)
		{
			throw new NotImplementedException();
		}

		public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
		{
			throw new NotImplementedException();
		}
	}
}
