using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DagLibrary;
using EthereumLibrary.Signer;
using NBitcoin;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;


namespace WalletLibrary.Core.Concrete.Wallets
{
	public class DagWallet : CommonWallet, IWallet
	{
		public string Symbol { get; private set; } = "dag";

		private NoxKeyGen NoxKeyGen;

		public DagWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.DAG)
				: base(keySets, product)
		{
			NoxKeyGen = new NoxKeyGen(keySets);
		}

		public string GetLegacyAddress(uint index = 0)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			using (var privateKey = KeySets[0].Key.GetPrivateKey())
			{
				return DagLibrary.Account.GetDagAddressFromPrivateKey(privateKey.Value);
			}
		}

		public string GetPubKey(uint index = 0)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			using (var privateKey = KeySets[0].Key.GetPrivateKey())
			{
				return DagLibrary.Account.GetPublicKeyFromPrivatekey(privateKey.Value, false);
			}
		}

		public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
		{

			LastTxRef lastTxRef = JsonConvert.DeserializeObject<LastTxRef>
					(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(req.MXTxn)));

			using (var privateKey = KeySets[0].Key.GetPrivateKey())
			using (var publicKey = KeySets[0].Key.GetPublicKey(false))
			{

				var uncompressedPk = publicKey.Value.ByteToHex();
				var addr = DagLibrary.Account.GetDagAddressFromPrivateKey(privateKey.Value);

				if (addr != req.NoxAddress.BTCAddress)
					throw new Exception(Sclear.MSG_ERROR_SIGNMESSAGE);

				DagLibrary.TransactionV2 transaction = new DagLibrary.TransactionV2(addr, req.ToAddress, Convert.ToDecimal(req.Amount),
						Convert.ToDecimal(req.FeeTotal), lastTxRef);

				bool signed = transaction.sign(privateKey.Value, uncompressedPk);

				if (!signed)
					throw new Exception("Invalid transaction or not fully signed.");

				var json = JsonConvert.SerializeObject(transaction.tx);
				var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

				PaymentRequestResponse response = new PaymentRequestResponse();
				var addition = new Addition { };
				response.TxnHex = b64;
				addition.FeeAmount = req.FeeTotal;
				response.Addition = addition;
				return response;
			}

		}

		public string GetSegwitAddress(uint index = 0)
		{
			throw new NotImplementedException();
		}

		public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
		{

			MsgTaskTransferResponse response = new MsgTaskTransferResponse();
			byte[] Msg = Convert.FromBase64String(req.Msg);

			using (var privateKey = KeySets[0].Key.GetPrivateKey())
			using (var publicKey = KeySets[0].Key.GetPublicKey(false))
			{

				var addr = DagLibrary.Account.GetDagAddressFromPrivateKey(privateKey.Value);

				if (addr != req.NoxAddress.BTCAddress)
					throw new Exception(Sclear.MSG_ERROR_SIGNMESSAGE);

				var data = DagSigner.HashPrefixedMessage(Msg);
				var sig = DagSigner.DagSign(privateKey.Value, System.Text.Encoding.UTF8.GetString(data));
				response.MsgSig = sig.ByteToHex();

				var valid = DagSigner.DagVerify(publicKey.Value, System.Text.Encoding.UTF8.GetString(data),
					sig.ByteToHex());

				if (!valid)
					throw new Exception("Not fully signed.");

				return response;
			}

		}

		public MsgTaskTransferResponse SignBlindMessage(NoxMsgProcess req)
		{
			throw new Exception("Not implemented.");
		}

	}

}
