using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.RPL;
using EthereumLibrary.Signer;
using NBitcoin;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.ActiveApe;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.ActiveWallets
{
	public class EthWallet : CommonWallet, IActiveWallet
	{
		public string Symbol { get; private set; } = "eth";

		public EthWallet(WalletActiveFactory.Products product = WalletActiveFactory.Products.ETH)
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
				return ekey.GetPublicAddress();
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

		public TransferInfoRespose SignPaymentRequest(TransferInfo info, List<SatusListItem> PromptInfo, uint index)
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

				ActiveApe.KnownCurrencies knownCurrency = null;

				EthereumLibrary.Signer.Transaction tx = null;

				switch (info.TransferType)
				{
					case ActiveApe.TransferType.BLIND_EXECUTION:
						tx = new EthereumLibrary.Signer.Transaction(contractAddress, to, amount, nonce, gasPrice, gasLimit, blindContract);
						break;

					case ActiveApe.TransferType.TOKENTRANSFER:
						knownCurrency = ActiveApe.ERCHelper.GetKnownCurrency(contractAddress);

						if (knownCurrency != null && knownCurrency.currencyDecimals != info.Decimals)
							throw new Exception("Invalid decimals, expecting " + knownCurrency.currencyDecimals + " decimals.");

						tx = new EthereumLibrary.Signer.Transaction(contractAddress, to, amount, nonce, gasPrice, gasLimit);
						break;

					case ActiveApe.TransferType.OUT_SELF:
						tx = new EthereumLibrary.Signer.Transaction(to, amount, nonce, gasPrice, gasLimit);
						break;

					default:
						throw new Exception("Invalid transfer type");

				}

				tx.Sign(key);
				var signedHex = tx.ToHex();
				string TransferType = string.Empty;

				var ethFeeLimitSat = tx.GasLimit.ToBigIntegerFromRLPDecoded() * tx.GasPrice.ToBigIntegerFromRLPDecoded();
				var ethFeeLimitBtc = NumUtils.Utils.FromSatoshi(ethFeeLimitSat, 18);

				PromptInfo.Add(new ActiveApe.SatusListItem()
				{
					StatusTitle = "MaxFee",
					StatusValue1 = ethFeeLimitBtc,
					Type = status_type.fee_value,
					RatePair = "ETH"
				});

				switch (info.TransferType)
				{
					case ActiveApe.TransferType.BLIND_EXECUTION:
						TransferType = "ERC20 Blind";
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "Contract",
							StatusValue1 = info.TokenAddr
						});
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "WARNING",
							StatusValue1 = "Cannot assert the underlying contract. Be sure that you trust the protocol with which you are interacting."
						});
						break;

					case ActiveApe.TransferType.TOKENTRANSFER:
						TransferType = "ERC20 Transfer";
						var TknSendingValue = NumUtils.Utils.FromSatoshi(amount, info.Decimals);

						if (knownCurrency != null)
						{
							PromptInfo.Add(new ActiveApe.SatusListItem()
							{
								StatusTitle = "SendingValue",
								StatusValue1 = TknSendingValue,
								Type = status_type.sending_value,
								RatePair = knownCurrency.currencySymbol.ToUpper()
							});
						}
						else
						{
							PromptInfo.Add(new ActiveApe.SatusListItem()
							{
								StatusTitle = "SendingValue",
								StatusValue1 = TknSendingValue
							});
						}

						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "Contract",
							StatusValue1 = info.TokenAddr
						});

						if (knownCurrency == null)
						{
							PromptInfo.Add(new ActiveApe.SatusListItem()
							{
								StatusTitle = "IMPORTANT",
								StatusValue1 = "Token not in known assets, cannot validate amount."
							});
						}
						else
						{
							if (!string.IsNullOrEmpty(info.TokenName))
							{
								PromptInfo.Add(new ActiveApe.SatusListItem()
								{
									StatusTitle = "TokenName",
									StatusValue1 = knownCurrency.displayName
								});
							}
						}

						break;

					case ActiveApe.TransferType.OUT_SELF:
						TransferType = "ETH Transfer";
						var EthSendingValue = NumUtils.Utils.FromSatoshi(amount, 18);
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "SendingValue",
							StatusValue1 = EthSendingValue,
							Type = status_type.sending_value,
							RatePair = "ETH"
						});
						PromptInfo.Add(new ActiveApe.SatusListItem()
						{
							StatusTitle = "ToAddress",
							StatusValue1 = info.To
						});
						break;

				}

				PromptInfo.Add(new ActiveApe.SatusListItem()
				{
					StatusTitle = "Action",
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
			if (!HasIndex(index))
				throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

			var keySet = KeySets[index];

			MsgTaskTransferResponse response = new MsgTaskTransferResponse();
			byte[] Msg = Convert.FromBase64String(req.Msg);

			using (var publicKeyUncompressed = keySet.GetPublicKey(false))
			{
				PubKey pubKeyUncompressed = new PubKey(publicKeyUncompressed.Value, (int)index, req.BlkNet);
				EthECKey ekey = new EthECKey(pubKeyUncompressed, NoxKeyGen);
				EthereumLibrary.MsgSigning elib = new EthereumLibrary.MsgSigning();
				response.MsgSig = elib.ETHMsgSign(Msg, ekey);
			}

			return response;
		}
	}
}