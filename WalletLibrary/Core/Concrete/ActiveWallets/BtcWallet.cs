using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;
using WalletLibrary.ActiveApe;

namespace WalletLibrary.Core.Concrete.ActiveWallets
{
	public class BtcWallet : CommonWallet, IActiveWallet
	{
		private Network Network;

		public BtcWallet(WalletActiveFactory.Products product): base(product)
		{
			Network = Sclear.GetBLKNetworkAlt(Symbol);
			NoxKeyGen = new NoxKeys.NoxKeyGen(KeySets);
		}

		public string GetLegacyAddress(uint index)
		{
			return GetAddress(index, false);
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

		public string GetSegwitAddress(uint index)
		{

		//	if (Network != NBitcoin.Altcoins.DigiByte.Instance.Mainnet
		//		&& Network != Network.Main)
			//{
			//	throw new OrderStateException("Segwit not supported.");
			//}

			return GetAddress(index, true);
		}

		public TransferInfoRespose SignPaymentRequest(TransferInfo info, List<SatusListItem> PromptInfo, uint index)
		{

			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			if (info.Outs == null || info.Outs.Length == 0)
				throw new Exception("Missing unspent outputs.");

			var keySet = KeySets[index];

			List<PubKey> KBList = new List<PubKey>();
			KBList.Add(new PubKey(keySet.GetPublicKey(true).Value, (int)index, Symbol));
			KBList.Add(new PubKey(keySet.GetPublicKey(false).Value, (int)index, Symbol));

			List<NoxKeys.BCUnspent> txnRaw = new List<NoxKeys.BCUnspent>();
			AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);

			foreach (var output in info.Outs)
			{
				NoxKeys.BCUnspent bCUnspent = new NoxKeys.BCUnspent();
				bCUnspent.Address = output.Address;
				bCUnspent.OutputN = output.Number;
				bCUnspent.TxHash = output.TxnHash;
				bCUnspent.Amount = NumUtils.Utils.FromSatoshi(System.Numerics.BigInteger.Parse(output.Amount), 8);
				txnRaw.Add(bCUnspent);
			}

			var BTCValue = NumUtils.Utils.FromSatoshi(System.Numerics.BigInteger.Parse(info.Amount), 8);
			var FeeValue = NumUtils.Utils.FromSatoshi(System.Numerics.BigInteger.Parse(info.FeeValue), 8);

			var txn = altCoinGen.AltCoinSign(KBList, info.To, txnRaw, BTCValue, info.From, FeeValue);

			if (txn.IsError) throw new Exception(txn.ErrorMessage);

			PromptInfo.Add(new ActiveApe.SatusListItem()
			{
				StatusTitle = "TotalFee",
				StatusValue1 = txn.Fee.ToString(),
				Type = status_type.fee_value,
				RatePair = Symbol.ToUpper()
			});
			PromptInfo.Add(new ActiveApe.SatusListItem()
			{
				StatusTitle = "SendingValue",
				StatusValue1 = BTCValue, Type = status_type.sending_value, RatePair = Symbol.ToUpper()
			});
			PromptInfo.Add(new ActiveApe.SatusListItem()
			{
				StatusTitle = "ToAddress",
				StatusValue1 = info.To
			});
			PromptInfo.Add(new ActiveApe.SatusListItem()
			{
				StatusTitle = "Action",
				StatusValue1 = Symbol.ToUpper() + " Transfer"
			});

			return new ActiveApe.TransferInfoRespose()
			{
				Transaction = txn.TxnHex
			};

		}

		public MsgTaskTransferResponse SignMessage(NoxMsgProcess req, uint index)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			MsgTaskTransferResponse response = new MsgTaskTransferResponse();
			byte[] msg = Convert.FromBase64String(req.Msg);


			var isCompressed = req.NoxAddress.DoSegwit;
			using (var publicKey = keySet.GetPublicKey(isCompressed))
			{
				PubKey pubKey = new PubKey(publicKey.Value, (int)index, req.BlkNet);
				AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);
				response.MsgSig = altCoinGen.AltMsgSign(pubKey, req.NoxAddress.BTCAddress, System.Text.Encoding.UTF8.GetString(msg));
			}

			return response;
		}

		private string GetAddress(uint index, bool isSegwit)
		{
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			using (var pubKeyUncompressed = keySet.GetPublicKey(isSegwit))
			{

				var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);
				AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);
				return altCoinGen.GetNewAddress(pubKey);
			}
		}

	}
}