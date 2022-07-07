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


namespace WalletLibrary.Core.Concrete.ActiveWallets
{
	public class XDCWallet : CommonWallet, IActiveWallet
	{
		public string Symbol { get; private set; } = "xdc";


		public XDCWallet(WalletActiveFactory.Products product = WalletActiveFactory.Products.XDC)
			: base(product)
		{
			NoxKeyGen = new NoxKeys.NoxKeyGen(KeySets);
		}

		public string GetLegacyAddress(uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];
			using (var pubKeyUncompressed = keySet.GetPublicKey(false))
			{

				var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);

				EthECKey ekey = new EthECKey(pubKey, NoxKeyGen);
				return ekey.GetPublicAddress().EnsureXdcPrefix();
			}
		}


		public string GetPubKey(uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];
			using (var pubKeyUncompressed = keySet.GetPublicKey(false))
			{
				var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);
				return pubKey.ToHex();
			}
		}

		public ActiveApe.TransferInfoRespose SignPaymentRequest(ActiveApe.TransferInfo info, List<ActiveApe.SatusListItem> PromptInfo, uint index)
		{

			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			using (var publicKey = keySet.GetPublicKey(false))
			{
				PubKey pubKey = new PubKey(publicKey.Value, (int)index, Symbol);
				EthECKey key = new EthECKey(pubKey, NoxKeyGen);


				var contractAddress = info.TokenAddr;
				var blindContract = info.ContractData;
				var to = info.To;
				var amount = System.Numerics.BigInteger.Parse(info.Amount);
				var nonce = System.Numerics.BigInteger.Parse(info.Nonce);
				var gasPrice = System.Numerics.BigInteger.Parse(info.FeeValue);
				var gasLimit = System.Numerics.BigInteger.Parse(info.GasUsed);
				var chainID = new BigInteger((int)Chain.XinFinMainnet);

				TransactionChainId tx = null;

				switch (info.TransferType)
				{
					case ActiveApe.TransferType.BLIND_EXECUTION:
						tx = new TransactionChainId(to, amount, nonce, gasPrice, gasLimit, chainID, blindContract);
						break;

					case ActiveApe.TransferType.OUT_SELF:
						tx = new TransactionChainId(to, amount, nonce, gasPrice, gasLimit, chainID, "");
						break;

					default:
						throw new Exception("Invalid transfer type");

				}

				tx.Sign(key);
				var signedHex = tx.ToHex();
				string TransferType = string.Empty;

				switch (info.TransferType)
				{
					case ActiveApe.TransferType.BLIND_EXECUTION:
						TransferType = "XDC Blind";
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "WARNING",
							StatusValue1 = "Your Bitfi hardware cannot assert the underlying contract. Be sure that you trust the protocol with which you are interacting."
						});
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "Contract",
							StatusValue1 = info.TokenAddr
						});
						break;

					case ActiveApe.TransferType.OUT_SELF:
						TransferType = "XDC Transfer";
						var EthSendingValue = NumUtils.Utils.FromSatoshi(amount, 18);
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "SendingValue",
							StatusValue1 = EthSendingValue
						});
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "ToAddress",
							StatusValue1 = info.To
						});
						break;

				}

				var ethFeeLimitSat = tx.GasLimit.ToBigIntegerFromRLPDecoded() * tx.GasPrice.ToBigIntegerFromRLPDecoded();
				var ethFeeLimitBtc = NumUtils.Utils.FromSatoshi(ethFeeLimitSat, 18);

				PromptInfo.Add(new ActiveApe.SatusListItem()
				{
					StatusTitle = "MaxFee",
					StatusValue1 = ethFeeLimitBtc
				});

				PromptInfo.Add(new ActiveApe.SatusListItem()
				{
					StatusTitle = "Type",
					StatusValue1 = TransferType
				});

				return new ActiveApe.TransferInfoRespose()
				{
					Transaction = signedHex
				};
			}
		}


		public string GetSegwitAddress(uint index)
		{
			throw new NotImplementedException();
		}

		public MsgTaskTransferResponse SignMessage(NoxMsgProcess req, uint index)
		{
			throw new NotImplementedException();
		}
	}
}