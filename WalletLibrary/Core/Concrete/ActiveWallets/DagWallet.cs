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
using WalletLibrary.ActiveApe;
using WalletLibrary.Core.Abstract;


namespace WalletLibrary.Core.Concrete.ActiveWallets
{
	public class DagWallet : CommonWallet, IActiveWallet
	{
		public string Symbol { get; private set; } = "dag";

		public DagWallet(WalletActiveFactory.Products product = WalletActiveFactory.Products.DAG)
				: base(product)
		{
		}

		public string GetLegacyAddress(uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			using (var privateKey = keySet.GetPrivateKey())
			{
				return DagLibrary.Account.GetDagAddressFromPrivateKey(privateKey.Value);
			}
		}

		public string GetPubKey(uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			using (var privateKey = keySet.GetPrivateKey())
			{
				return DagLibrary.Account.GetPublicKeyFromPrivatekey(privateKey.Value, false);
			}
		}

		public TransferInfoRespose SignPaymentRequest(TransferInfo info, List<SatusListItem> PromptInfo, uint index)
		{

			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			using (var privateKey = keySet.GetPrivateKey())
			using (var publicKey = keySet.GetPublicKey(false))
			{

				var uncompressedPk = publicKey.Value.ByteToHex();
				var addr = DagLibrary.Account.GetDagAddressFromPrivateKey(privateKey.Value);

				LastTxRef lastTxRef = JsonConvert.DeserializeObject<LastTxRef>(info.LastTxRef);

				if (!string.IsNullOrEmpty(lastTxRef.hash))
					lastTxRef.prevHash = lastTxRef.hash;

				DagLibrary.TransactionV2 transaction = new DagLibrary.TransactionV2(addr, info.To, info.Amount, info.FeeValue, lastTxRef);

				bool signed = transaction.sign(privateKey.Value, uncompressedPk);

				if (!signed)
					throw new Exception("Invalid transaction or not fully signed.");

				var json = JsonConvert.SerializeObject(transaction.tx);
				var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

				PromptInfo.Add(new SatusListItem()
				{
					StatusTitle = "FeeAmount",
					StatusValue1 = NumUtils.Utils.FromSatoshi(System.Numerics.BigInteger.Parse(info.FeeValue), 8),
					Type = status_type.fee_value,
					RatePair = "DAG"
				});

				PromptInfo.Add(new SatusListItem()
				{
					StatusTitle = "SendingValue",
					StatusValue1 = NumUtils.Utils.FromSatoshi(System.Numerics.BigInteger.Parse(info.Amount), 8),
					Type = status_type.sending_value,
					RatePair = "DAG"
				});

				PromptInfo.Add(new SatusListItem()
				{
					StatusTitle = "ToAddress",
					StatusValue1 = info.To
				});

				PromptInfo.Add(new SatusListItem()
				{
					StatusTitle = "Type",
					StatusValue1 = "DAG Transfer"
				});

				return new TransferInfoRespose()
				{
					Transaction = b64
				};
			}
		}


		public string GetSegwitAddress(uint index)
		{
			throw new NotImplementedException();
		}

		public MsgTaskTransferResponse SignMessage(NoxMsgProcess req, uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			MsgTaskTransferResponse response = new MsgTaskTransferResponse();
			byte[] Msg = Convert.FromBase64String(req.Msg);

			using (var privateKey = keySet.GetPrivateKey())
			using (var publicKey = keySet.GetPublicKey(false))
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


	}

}
