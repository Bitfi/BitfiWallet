using Newtonsoft.Json;
using NoxService.NWS;
using NoxService.SWS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.Core.Concrete;
using WalletLibrary.Core.Concrete.ActiveWallets;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using System.Collections.Concurrent;
using WalletLibrary.Core.Abstract;
using static WalletLibrary.Core.Concrete.WalletActiveFactory;
using WalletLibrary.ActiveApe.ApeShift;

namespace WalletLibrary.ActiveApe
{

	public class ActiveProcessor
	{

		private readonly SemaphoreSlim _semaphore;
		private WalletActiveFactory _walletFactory;
		public MetaSession _metaSession { get; private set; }

		private NoxAddressReviewV3 LegacyAddresses;
		public bool IsDisposed { get; private set; }
		private string AuthToken = string.Empty;

		public ActiveProcessor()
		{
			IsDisposed = true;
			_semaphore = new SemaphoreSlim(1);
		}

		public void Init(WalletActiveFactory walletFactory, string Token)
		{
			if (!IsDisposed)
				throw new Exception("PROCESSOR_NOT_DISPOSED");

			IsDisposed = false;
			AuthToken = Token;
			_metaSession = new MetaSession();
			_walletFactory = walletFactory;
			LegacyAddresses = null;

			new Thread(() =>
			{
				try
				{
					LegacyProfileRequest request = new LegacyProfileRequest();
					GetBitfiAddresses(request);
				}
				catch { }

			}).Start();

		}

		IActiveWallet GetWallet(Products product, uint index)
		{
			return _walletFactory.GetWallet(product, index);
		}

		IActiveWallet GetWallet(Products product, uint[] indexes)
		{
			return _walletFactory.GetWallet(product, indexes);
		}

		public async Task<MetaResponse<object>> GetWalletRequest(BroadcastRequestCompletedEventArgs e)
		{

			MetaResponse<object> metaResponse = new MetaResponse<object>();

			if (IsDisposed)
			{
				metaResponse.IsDisposed = true;
				return metaResponse;
			}

			if (e.Method == wallet_methods.get_legacy_profile)
				return GetLegacyProfiles(e);

			await _semaphore.WaitAsync();

			try
			{

				if (IsDisposed)
				{
					metaResponse.IsDisposed = true;
					return metaResponse;
				}

				metaResponse.Sender = e.Sender;
				metaResponse.Method = e.Method;
				metaResponse.RequestParam = e.Request.Params;

				var list = new List<SatusListItem>();
				metaResponse.PromptInfo = list;

				var request = e.Request.Params;

				switch (e.Method)
				{

					case wallet_methods.transfer:
						var info = JsonConvert.DeserializeObject<WalletRequest<TransferInfo>>
							(JsonConvert.SerializeObject(e.Request));
						metaResponse.Result = DoWalletTask(e.Method, info.Params, list);
						metaResponse.UserPrompt = true;
						metaResponse.PromptMessage = "AUTHORIZE TRANSACTION?";
						break;

					case wallet_methods.sign_message:
						var messageInfo = JsonConvert.DeserializeObject<WalletRequest<MessageInfo>>
							(JsonConvert.SerializeObject(e.Request));
						metaResponse.Result = DoWalletTask(e.Method, messageInfo.Params, list);
						metaResponse.UserPrompt = true;
						metaResponse.PromptMessage = "AUTHORIZE SIGNATURE?";
						break;

					case wallet_methods.get_addresses:
						var adrInfo = JsonConvert.DeserializeObject<WalletRequest<AddressInfo>>
							(JsonConvert.SerializeObject(e.Request));
						metaResponse.Result = DoWalletTask(e.Method, adrInfo.Params);
						metaResponse.UserPrompt = false;
						break;

					case wallet_methods.get_pub_keys:
						var pubInfo = JsonConvert.DeserializeObject<WalletRequest<PubKeyInfo>>
							(JsonConvert.SerializeObject(e.Request));
						metaResponse.Result = DoWalletTask(e.Method, pubInfo.Params);
						metaResponse.UserPrompt = false;
						break;

					case wallet_methods.connect_wallet:
						string data = (string)e.Request.Params;
						list.Add(new SatusListItem()
						{
							StatusTitle = "DisplayCode",
							StatusValue1 = data.ToUpper()
						});
						list.Add(new SatusListItem()
						{
							StatusTitle = "IMPORTANT",
							StatusValue1 = "Be sure this code matches what is displayed by your extension provider."
						});
						metaResponse.Result = data;
						metaResponse.UserPrompt = true;
						metaResponse.PromptMessage = "AUTHORIZE EXTENSION?";
						break;


					default:
						throw new Exception("Method not yet implemented here.");

				}
				return metaResponse;
			}
			catch (Exception ex)
			{
				metaResponse.IsError = true;
				metaResponse.ErrorMessage = ex.Message;
				return metaResponse;
			}
			finally
			{
				_semaphore.Release();
			}

		}

		object DoWalletTask(wallet_methods action, object request, List<SatusListItem> PromptInfo = null)
		{

			object workerTask = new object();


			if (IsDisposed)
				throw new Exception("Wallet closed.");

			switch (action)
			{

				case wallet_methods.transfer:
					workerTask = TransactionTask(request, PromptInfo);
					break;

				case wallet_methods.sign_message:
					workerTask = MessageTask(request, PromptInfo);
					break;

				case wallet_methods.get_addresses:
					workerTask = GetAddress(request);
					break;

				case wallet_methods.get_pub_keys:
					workerTask = GetPubKey(request);
					break;

				default:
					throw new Exception("Invalid method");
			}

			return workerTask;

		}

		private MetaResponse<object> GetLegacyProfiles(BroadcastRequestCompletedEventArgs e)
		{

			MetaResponse<object> metaResponse = new MetaResponse<object>();
			metaResponse.Sender = e.Sender;
			metaResponse.Method = e.Method;
			metaResponse.RequestParam = e.Request.Params;
			metaResponse.PromptInfo = new List<SatusListItem>();
			try
			{

				var info = JsonConvert.DeserializeObject<WalletRequest<LegacyProfileRequest>>(JsonConvert.SerializeObject(e.Request));
				metaResponse.Result = GetBitfiAddresses(info.Params);
				return metaResponse;
			}
			catch (OrderStateException ex)
			{
				metaResponse.IsError = true;
				metaResponse.ErrorMessage = ex.Message;
				return metaResponse;
			}
			catch (Exception ex)
			{
				metaResponse.IsError = true;
				metaResponse.ErrorMessage = "Error prcessing request.";
				return metaResponse;
			}

		}

		private List<LegacyProfile> GetBitfiAddresses(object request)
		{

			List<LegacyProfile> legacyProfiles;

			if (LegacyAddresses == null)
			{
				var WSS = new SWS();
				LegacyAddresses = WSS.GetAddressIndexes(AuthToken);

				if (LegacyAddresses == null)
					throw new OrderStateException("[BITFI_WS ERROR GETTING PROFILE");
			}


			try
			{
				_semaphore.Wait();

				legacyProfiles = GetReviewIndexes(LegacyAddresses, request);
				_semaphore.Release();

			}
			catch (Exception ex)
			{
				_semaphore.Release();
				throw new OrderStateException(ex.Message);
			}

			return legacyProfiles;

		}

		private List<LegacyProfile> GetReviewIndexes(NoxAddressReviewV3 noxAddressReviews, object request)
		{

			LegacyProfileRequest info = request as LegacyProfileRequest;

			var noxAddresses = new List<LegacyProfile>();

			foreach (var adrReview in noxAddressReviews.AdrReview)
			{
				string currencySymbol = adrReview.Blk;

				if (!string.IsNullOrEmpty(info.Symbol))
					if (info.Symbol.ToLower() != currencySymbol.ToLower())
						continue;

				uint[] indexes = new uint[adrReview.IndexCount];
				for (int i = 0; i < adrReview.IndexCount; ++i)
					indexes[i] = (uint)i;

				var product = WalletActiveFactory.SymbolToWalletProduct(currencySymbol);
				var wallet = GetWallet(product, indexes);

				var noxAddressesPortion = noxAddressReviews.Addresses.Where(addr => addr.BlkNet.ToLower() == currencySymbol.ToLower())
				.Select(addr =>
				{

					try
					{
						var btcAddress = addr.DoSegwit ? wallet.GetSegwitAddress((uint)addr.HDIndex)
						: wallet.GetLegacyAddress((uint)addr.HDIndex);

						if (btcAddress != addr.BTCAddress)
							return null;

						return new LegacyProfile
						{
							Symbol = currencySymbol,
							Address = btcAddress,
							DoSegwit = addr.DoSegwit,
							Index = addr.HDIndex
						};
					}
					catch (OrderStateException)
					{
						return null;
					}
				}).ToList();

				if (noxAddressesPortion != null)
					noxAddresses.AddRange(noxAddressesPortion);
			}

			return noxAddresses;

		}

		PubKeyInfoResponse GetPubKey(object request)
		{
			PubKeyInfo addressInfo = request as PubKeyInfo;
			var product = WalletActiveFactory.SymbolToWalletProduct(addressInfo.Symbol);
			var wallet = GetWallet(product, addressInfo.index);

			return new PubKeyInfoResponse()
			{
				PublicKeys = new string[] { wallet.GetPubKey(addressInfo.index) }

			};
		}

		AddressInfoResponse GetAddress(object request)
		{

			AddressInfo addressInfo = request as AddressInfo;
			var product = WalletActiveFactory.SymbolToWalletProduct(addressInfo.Symbol);
			var wallet = GetWallet(product, addressInfo.Indexes);

			List<WalletAddress> pub = new List<WalletAddress>();

			foreach (var index in addressInfo.Indexes)
			{
				if (product == Products.BTC && addressInfo.DoSegwit)
				{
					pub.Add(new WalletAddress() { Address = wallet.GetSegwitAddress(index), Index = index });
				}
				else
				{
					pub.Add(new WalletAddress() { Address = wallet.GetLegacyAddress(index), Index = index });
				}
			}

			return new AddressInfoResponse()
			{
				Addresses = pub.ToArray()
			};

		}

		TransferInfoRespose TransactionTask(object request, List<SatusListItem> PromptInfo)
		{
			TransferInfo info = request as TransferInfo;
			var product = WalletActiveFactory.SymbolToWalletProduct(info.Symbol);
			var wallet = GetWallet(product, info.Index);
			var transfer = wallet.SignPaymentRequest(info, PromptInfo, info.Index);

			var addition = PromptInfo.ToArray();

			List<SatusListItem> rateAddition = new List<SatusListItem>();

			for (int i = 0; i < addition.Length; i++)
			{

				SatusListItem extra = PromptInfo[i];
				bool added = false;

				if (extra.Type == status_type.fee_value || extra.Type == status_type.sending_value)
				{
					extra.Position = 1;
					string Title = "SendingUSD";

					if (extra.Type == status_type.fee_value)
					{
						extra.Position = 0;
						Title = "FeeUSD";
					}

					try
					{
						if (BitfiWallet.NoxChannel.Current._RateSpot.ContainsKey(extra.RatePair.ToUpper()))
						{
							decimal rate = BitfiWallet.NoxChannel.Current._RateSpot[extra.RatePair.ToUpper()];

							rateAddition.Add(new SatusListItem()
							{
								StatusTitle = Title,
								StatusValue1 = (Convert.ToDecimal(extra.StatusValue1) * rate).ToString("C2")
							});

							added = true;
						}
					}
					catch
					{
						continue;
					}
				}


				extra.Position = i + 2;
				rateAddition.Add(extra);
			}

			rateAddition = rateAddition.OrderBy(x => x.Position).ToList();

			PromptInfo.Clear();
			PromptInfo.AddRange(rateAddition);

			return transfer;

		}

		MessageInfoResponse MessageTask(object request, List<SatusListItem> list)
		{

			MessageInfo messageInfo = request as MessageInfo;
			NoxMsgProcess req = new NoxMsgProcess()
			{
				BlkNet = messageInfo.Symbol,
				Msg = messageInfo.Message,
				NoxAddress = new NoxService.NWS.NoxAddresses()
				{
					BTCAddress = messageInfo.Address,
					DoSegwit = messageInfo.DoSegwit,
					BlkNet = messageInfo.Symbol,
					HDIndex = (int)messageInfo.Index
				}
			};

			var product = WalletActiveFactory.SymbolToWalletProduct(messageInfo.Symbol);
			var wallet = GetWallet(product, messageInfo.Index);

			var msgTaskTransferResponse = wallet.SignMessage(req, messageInfo.Index);

			list.Add(new SatusListItem()
			{
				StatusTitle = "Method",
				StatusValue1 = "Sign Message Request"
			});
			list.Add(new SatusListItem()
			{
				StatusTitle = "MessageType",
				StatusValue1 = messageInfo.Symbol.ToUpper() + " Signed Message"
			});
			list.Add(new SatusListItem()
			{
				StatusTitle = "Address",
				StatusValue1 = messageInfo.Address
			});

			return new MessageInfoResponse()
			{
				Signature = msgTaskTransferResponse.MsgSig
			};

		}

		public void Dispose()
		{

			if (_walletFactory == null)
				return;

			if (IsDisposed)
				return;

			IsDisposed = true;

			_walletFactory.Dispose();

			if (_metaSession != null)
				_metaSession.Dispose();
		}

	}
}
