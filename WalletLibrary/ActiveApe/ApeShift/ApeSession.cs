using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WalletLibrary.ActiveApe.ApeShift
{

	public class ApeSession
	{

		public event BroadcastRequestCompletedEventHandler _OnRequest;
		public event BroadcastResponseCompletedEventHandler _OnResponse;

		private EnvoyTask envoyTask;

		private bool UserIsBusy = false;

		CancellationTokenSource cancellationToken;
		CancellationTokenSource cancellationTokenMA;

		ApeMQ apeMQ = new ApeMQ();

		public bool IsUsuerBlocking { get; set; }


		private string _EnvoyTaskID;

		public ApeSession()
		{
			_OnResponse += ApeSession__OnResponse;

		}

		private void ApeSession__OnResponse(BroadcastResponseCompletedEventArgs e)
		{

		}

		public void CancelTasks()
		{
			try
			{

				apeMQ._OnMsg -= Processor_MSGEvent;

				BitfiWallet.DeviceManager.NoxDevice.Current.OnBatteryChanged
					-= Current_OnBatteryChanged;

				if (cancellationTokenMA.IsCancellationRequested)
					return;

				cancellationTokenMA.Cancel();
			}
			catch { }
		}

		public async Task StartMQ(CancellationTokenSource token)
		{

			_EnvoyTaskID = string.Empty;
			cancellationTokenMA = new CancellationTokenSource();
			cancellationToken = token;
			IsUsuerBlocking = false;

			for (int i = 0; i < 10; i++)
			{
				if (await SetEnvoyTask())
					break;

				await Task.Delay(2000);
			}

			if (string.IsNullOrEmpty(_EnvoyTaskID))
				return;

			apeMQ._OnMsg += Processor_MSGEvent;

			await apeMQ.StartWS(cancellationToken);

		}

		private async Task<bool> SetEnvoyTask()
		{

			try
			{

				BitfiWallet.NoxChannel.Current._APESession.RequestArrived(new WalletRequest<object>(), new MQDiffieResult()
				{
					ECStatus = "STARTING DEVICE ENVOY",
					MQContext = Guid.NewGuid().ToString()
				}, wallet_methods.status);

				envoyTask = new EnvoyTask()
				{
					persist_timeout = false,
					send_offline_message = true,
					single_instance = false,
					task_context = Guid.NewGuid().ToString(),
					timeout = 60
				};

				var resp = await ApeAPI.GetEnvoyToken(envoyTask);

				if (resp != null)
				{
					_EnvoyTaskID = resp.task_id;
				}
				else
				{
					BitfiWallet.NoxChannel.Current._APESession.RequestArrived(new WalletRequest<object>(), new MQDiffieResult()
					{
						ECStatus = "ENVOY NOT ACQUIRED",
						MQContext = Guid.NewGuid().ToString()
					}, wallet_methods.status);

					return false;
				}

				BitfiWallet.NoxChannel.Current._APESession.RequestArrived(new WalletRequest<object>(), new MQDiffieResult()
				{
					ECStatus = "DEVICE ENVOY ACQUIRED",
					MQContext = Guid.NewGuid().ToString()
				}, wallet_methods.status);

				LoadBatStatus(BitfiWallet.NoxDPM.NoxT.batStatus);

				BitfiWallet.DeviceManager.NoxDevice.Current.OnBatteryChanged += Current_OnBatteryChanged;

				return true;
			}
			catch { }

			_EnvoyTaskID = string.Empty;
			return false;
		}

		private void Current_OnBatteryChanged(object sender, EventArgs e)
		{
			try
			{

				if (string.IsNullOrEmpty(_EnvoyTaskID))
					return;

				LoadBatStatus(BitfiWallet.NoxDPM.NoxT.batStatus);
				
			}
			catch { }
		}


		public void LoadBatStatus(BitfiWallet.BatStatus batStatus)
		{

			try
			{
				if (batStatus.IsError) return;
				AddEnvoyEvent(new DeviceEvent<object>()
				{
					event_type = envoy_event.battery_changed,
					event_info = new BatteryInfo()
					{
						IsCharging = batStatus.IsCharging,
						Level = batStatus.Level
					}
				});

			}
			catch { }

		}

		public async Task GetResponse(string data, MQDiffieResult sender, string AuthorizationPubKey = null)
		{

			WalletRequest<object> walletRequest = null;

			try
			{

				if (!string.IsNullOrEmpty(AuthorizationPubKey))
				{
					if (UserIsBusy)
						throw new OrderStateException("USER IS BUSY");

					await RespondAsync(new WalletRequest<object>()
					{
						Params = data,
						Id = AuthorizationPubKey,
						Method = data
					}, sender, wallet_methods.connect_wallet);

				}
				else
				{

					JsonSerializerSettings settings = new JsonSerializerSettings();
					settings.NullValueHandling = NullValueHandling.Ignore;
					settings.MissingMemberHandling = MissingMemberHandling.Ignore;
					settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
					walletRequest = JsonConvert.DeserializeObject<WalletRequest<object>>(data, settings);

					if (walletRequest == null)
						throw new OrderStateException("Unable to deseserialize request.");
					

					Enum.TryParse(walletRequest.Method, out wallet_methods method);

					if (method == wallet_methods.transfer || method == wallet_methods.sign_message)
					{
						if (UserIsBusy)
							throw new OrderStateException("USER IS BUSY");
					}

					switch (method)
					{
						case wallet_methods.get_legacy_profile:
						case wallet_methods.vibe:
						case wallet_methods.get_device_envoy:
						case wallet_methods.get_addresses:
						case wallet_methods.get_device_info:
						case wallet_methods.get_pub_keys:
						case wallet_methods.sign_message:
						case wallet_methods.transfer:
							await RespondAsync(walletRequest, sender, method);
							break;

						default:
							throw new OrderStateException("Invalid method.");
					}
				}

			}
			catch (NullReferenceException)
			{
				throw new OrderStateException("Invalid request.");
			}

		}

		private async Task<object> RespondAsyncTKS(WalletRequest<object> walletRequest, MQDiffieResult sender, wallet_methods method)
		{
			try
			{
				AsyncDelegate asyncDelegate = new AsyncDelegate(this, walletRequest, sender, method);
				var resp = await asyncDelegate.RequestAsync(cancellationTokenMA.Token);

				if (resp.IsError)
				{
					throw new OrderStateException((string)resp.Response);
				}

				return resp.Response;

			}
			catch (TaskCanceledException) { }
			catch (OperationCanceledException) { }
			throw new OrderStateException("Cancelled");
		}

		private async Task RespondAsync(WalletRequest<object> walletRequest, MQDiffieResult sender, wallet_methods method)
		{

			bool Sent = false;

			try
			{

				GC.Collect();

				object walletResult = null;
				UpdateBusy(method, true);


				if (method == wallet_methods.vibe)
				{
					var vibe = BitfiWallet.DeviceManager.NoxDevice.Current.ApeVibe();

					if (!vibe)
						throw new OrderStateException("Vibe disabled.");

					walletResult = "Success";
		
				}

				if (method == wallet_methods.get_device_info)
				{

					walletResult = BitfiWallet.DeviceManager.NoxDevice.Current.GetSafeDeviceInfo();

					if (walletResult == null)
						throw new OrderStateException("Request failed.");
				}

				if (method == wallet_methods.get_device_envoy)
				{
					if (string.IsNullOrEmpty(_EnvoyTaskID))
						throw new OrderStateException("Task not started.");

					var resp = await ApeAPI.GetEnvoyToken(envoyTask);

					if (resp == null)
						throw new OrderStateException("Error getting token.");

					walletResult = resp.envoy_token;
				}

				if (walletResult == null)
				{
					walletResult = await RespondAsyncTKS(walletRequest, sender, method);
				}

				if (method == wallet_methods.connect_wallet)
				{
					try
					{
						if ((string)walletResult == (string)walletRequest.Params)
						{
							BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.AddAuthorization(walletRequest.Method, walletRequest.Id);
							await ApeAPI.SendMessage(BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.GetSessionPubKeyResponse(), sender.FromPublicKey,
								sender.MQContext, "", false);

							Sent = true;
						}
					}
					catch { }
				}

				if (!Sent)
				{
					await ApeAPI.SendMessage(BuildJsonSuccessResp(walletResult), sender.FromPublicKey,
						sender.MQContext, sender.ECPublicKey, true);
				}

			}
			catch (OrderStateException ex)
			{

				if (!Sent)
				{
					await ApeAPI.SendMessage(BuildJsonErrorResp(ex.Message), sender.FromPublicKey,
						sender.MQContext, "", false);
				}
			}
			finally
			{
				UpdateBusy(method, false);
			}
		}

		private void UpdateBusy(wallet_methods method, bool Busy)
		{
			switch (method)
			{
				case wallet_methods.connect_wallet:
				case wallet_methods.sign_message:
				case wallet_methods.transfer:
					UserIsBusy = Busy;
					break;
			}
		}

		private void Processor_MSGEvent(DataMSGCompletedEventArgs processor)
		{

			if (cancellationToken == null || cancellationToken.IsCancellationRequested)
				return;

			if (processor.Message == "0")
			{
				try
				{
					BitfiWallet.NoxChannel.Current._APESession.RequestArrived(new WalletRequest<object>(),
						new MQDiffieResult()
						{
							ECStatus = "PRIVATE CHANNEL ACQUIRED",
							MQContext = Guid.NewGuid().ToString()
						}, wallet_methods.status);
				}
				catch { }

				return;
			}


			if (processor.Message == "1")
			{
				try
				{
					BitfiWallet.NoxChannel.Current._APESession.RequestArrived(new WalletRequest<object>(),
						new MQDiffieResult()
						{
							ECStatus = "STARTING PRIVATE CHANNEL",
							MQContext = Guid.NewGuid().ToString()
						}, wallet_methods.status);
				}
				catch { }

				return;
			}

			Task.Run(async () =>
			{
				try
				{

					var sender = JsonConvert.DeserializeObject<MQResponse<MQDiffieResult>>
					(processor.Message).Result;

					if (sender == null)
						return;

					byte[] payload = null;

					try
					{
						payload = Convert.FromBase64String(sender.ProtectedMessage);
					}
					catch
					{
						await ApeAPI.SendMessage("Payload should be in base64 encoding.",
							sender.FromPublicKey, sender.MQContext, "", false);
						return;
					}

					try
					{

						var req_type = BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.GetRequestType(payload, out byte[] data);

						switch (req_type)
						{
							case request_type.process_request:
								var request = System.Text.Encoding.UTF8.GetString(BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.Decrypt
									(data, sender));

								await GetResponse(request, sender);
								break;

							case request_type.get_signing_data:

								if (IsUsuerBlocking)
									throw new OrderStateException("USER IS BLOCKING");

								if (UserIsBusy)
									throw new OrderStateException("USER IS BUSY");

								await ApeAPI.SendMessage(BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.GetSignDataResponse(data),
							sender.FromPublicKey, sender.MQContext, "", false);
								break;

							case request_type.get_authorization:

								if (IsUsuerBlocking)
									throw new OrderStateException("USER IS BLOCKING");

								if (UserIsBusy)
									throw new OrderStateException("USER IS BUSY");

								var codes = BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.GetAuthorizationResponse(data);
								await GetResponse(codes[0], sender, codes[1]);
								break;

							case request_type.ping_pong:
								await ApeAPI.SendMessage(data, sender.FromPublicKey, sender.MQContext, "", false);
								break;

							case request_type.not_defined:
							default:
								throw new OrderStateException("Invalid request.");
								break;

						}
					}
					catch (OrderStateException ex)
					{
						await ApeAPI.SendMessage(BuildJsonErrorResp(ex.Message), sender.FromPublicKey,
							sender.MQContext, "", false);
					}
					catch (Exception)
					{
						await ApeAPI.SendMessage(BuildJsonErrorResp("Error processing request."), sender.FromPublicKey,
							sender.MQContext, "", false);
					}

				}
				catch { }

			});

		}

		private string BuildJsonErrorResp(object response)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;

			JResponseWrapper<string> jResponseWrapper = new JResponseWrapper<string>();
			jResponseWrapper.Error = new JError() { Message = (string)response };
			return JsonConvert.SerializeObject(jResponseWrapper, settings);

		}

		private string BuildJsonSuccessResp(object response)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			JResponseWrapper<object> jResponseWrapper = new JResponseWrapper<object>();
			jResponseWrapper.Result = response;
			return JsonConvert.SerializeObject(jResponseWrapper, settings);

		}

		public void RequestArrived(WalletRequest<object> request, MQDiffieResult sender, wallet_methods method)
		{
			try
			{
				if (request == null)
					return;
				if (_OnRequest == null)
					return;

				Interlocked.CompareExchange(ref _OnRequest, null, null)?.Invoke(new BroadcastRequestCompletedEventArgs
					(request, sender, method));
			}
			catch { }

		}

		public void ErrorResonse(string ErrorMessage, MQDiffieResult sender)
		{
			try
			{
				sender.ECPublicKey = null;
				ResponseArrived(ErrorMessage, sender, true);
			}
			catch { }

		}

		private void ResponseArrived(object response, MQDiffieResult sender, bool IsError)
		{

			try
			{
				if (response == null)
					return;
				if (_OnResponse == null)
					return;

				Interlocked.CompareExchange(ref _OnResponse, null, null)?.Invoke(new BroadcastResponseCompletedEventArgs
					(response, sender, IsError));
			}
			catch { }
		}


		public void ResponseArrived(object response, MQDiffieResult sender)
		{

			try
			{
				if (response == null)
					return;
				if (_OnResponse == null)
					return;

				Interlocked.CompareExchange(ref _OnResponse, null, null)?.Invoke(new BroadcastResponseCompletedEventArgs
					(response, sender, false));
			}
			catch { }
		}

		public void AddEnvoyEvent(DeviceEvent<object> deviceEvent)
		{

			if (string.IsNullOrEmpty(_EnvoyTaskID))
				return;

			Task.Run(async () =>
			{
				await ApeAPI.SendEnvoyMessage(deviceEvent, _EnvoyTaskID, false);

			});
		}

		public async Task DisposeEnvoyEvent()
		{

			if (string.IsNullOrEmpty(_EnvoyTaskID))
				return;

			var e = new DeviceEvent<object>()
			{
				event_type = envoy_event.session_status,
				event_info = new SessionInfo()
				{
					IsDisposed = true
				},
			};

			await ApeAPI.SendEnvoyMessage(e, _EnvoyTaskID, true);
			_EnvoyTaskID = string.Empty;

		}

	}
	

}
