using EthereumLibrary.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WalletLibrary.ActiveApe.ApeShift
{
	public class ApeAPI
	{


		public async static Task<string> Request(api_methods Method, object Params)
		{
			var data = new
			{
				Jsonrpc = "2.0",
				Method = Method.ToString(),
				Id = "2",
				Params = Params
			};

			var resp = await SendRequest(data);
			var jwr = JsonConvert.DeserializeObject<JResponseWrapper<object>>(resp);

			if (jwr.Error != null)
			{
				throw new Exception(jwr.Error.Message);
			}

			return resp;

		}

		public async static Task<GetEnvoyTokenResponse> GetEnvoyToken(EnvoyTask envoyTask)
		{

			if (envoyTask == null)
				return null;

			try
			{
				var o = new GetEnvoyTokenRequest()
				{
					reusable_minutes = 60,
					envoy_task = envoyTask
				};

				var data = new
				{
					Jsonrpc = "2.0",
					Method = "get_envoy_token",
					Id = "2",
					Params = o
				};

				var resp = await SendRequest(data);
				var jwr = JsonConvert.DeserializeObject<JResponseWrapper<GetEnvoyTokenResponse>>(resp);

				if (jwr.Error != null)
					return null;

				return jwr.Result;

			}
			catch { return null; }

		}

		public async static Task SendEnvoyMessage(DeviceEvent<object> deviceEvent, string taskId, bool endOfSession)
		{

			try
			{

				uint timeout = 60;

				if (endOfSession)
					timeout = 0;

				var o = new InvokeEnvoyRequest()
				{
					message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes
					(JsonConvert.SerializeObject(deviceEvent))),
					new_timeout = timeout,
					task_id = taskId,
				};

				var data = new
				{
					Jsonrpc = "2.0",
					Method = "invoke_envoy",
					Id = "2",
					Params = o
				};

				await SendRequest(data);
			}
			catch { }
		}

		public async static Task<bool> SendMessage(string Message, string QPublicKey, string Context, string ECPubKey, bool Encrypt)
		{

			return await SendMessage(System.Text.Encoding.UTF8.GetBytes(Message), QPublicKey, Context, ECPubKey, Encrypt);

		}

		public async static Task<bool> SendMessage(byte[] Message, string QPublicKey, string Context, string ECPubKey, bool Encrypt)
		{
			try
			{

				string protected_message = string.Empty;

				if (Encrypt && !string.IsNullOrEmpty(ECPubKey))
				{
					var enc = BitfiWallet.NoxChannel.Current._ActiveProcessor._metaSession.Encrypt(Message, ECPubKey.HexToByteArray());
					protected_message = Convert.ToBase64String(enc);
				}
				else
				{
					protected_message = Convert.ToBase64String(Message);
				}

				var o = new InvokeApplicatonRequest()
				{
					protected_message = protected_message,
					from_public_key = BitfiWallet.NoxChannel.Current.MY_CHANNEL_PUBLIC_KEY,

					to_public_key = QPublicKey,
					request_context = Context
				};

				var data = new
				{
					Jsonrpc = "2.0",
					Method = "invoke_application",
					Id = "2",
					Params = o
				};

				var resp = await SendRequest(data);


				return true;

			}
			catch { }

			return false;

		}

		static string GetHeaderSign(string obj)
		{
			HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(BitfiWallet.NoxChannel.Current.APEAPI_SECRET));
			byte[] hashmessage = hmac.ComputeHash(Encoding.UTF8.GetBytes(obj));
			return hashmessage.ToHex();
		}

		async static Task<string> SendRequest(object Request, string url = "https://api.async360.com/api")
		{

			try
			{
				JObject obj = (JObject)JToken.FromObject(Request);
				byte[] byteArray = System.Text.Encoding.Default.GetBytes(obj.ToString());

				using (var client = new HttpClient())
				{

					client.DefaultRequestHeaders.Add("public-id", BitfiWallet.NoxChannel.Current.APEAPI_PUBLIC_ID);
					client.DefaultRequestHeaders.Add("sign", GetHeaderSign(obj.ToString()));

					var content = new ByteArrayContent(byteArray);
					var response = await client.PostAsync(url, content);
					var responseString = await response.Content.ReadAsStringAsync();

					return responseString;
				}
			}
			catch (HttpRequestException ex)
			{
	//			Console.WriteLine(ex.Message);

			}

			return null;
		}

		public async static Task<ProviderInfoResponse> WSRequest()
		{

			try
			{

				string url = "https://ws.async360.com/" + "get_connection";

				var hashingKey = BitfiWallet.NoxDPM.NoxT.noxDevice.GetSharedSecret
					("02b50fb17216c3ea72f69fa0e8c4ca9ff578f888b77d689b3c883657f85bc7e961".HexToByteArray());

				string obj = Guid.NewGuid().ToString();

				using (var client = new HttpClient())
				{

					HMACSHA512 hmac = new HMACSHA512(hashingKey);
					byte[] hashmessage = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(obj));

					client.DefaultRequestHeaders.Add("pub_id", BitfiWallet.NoxDPM.NoxT.noxDevice.GetDeviceID());
					client.DefaultRequestHeaders.Add("pub_key", BitfiWallet.NoxDPM.NoxT.noxDevice.TEEPubCompressed.ToHex());
					client.DefaultRequestHeaders.Add("sign", hashmessage.ToHex());

					var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(obj));
					var response = await client.PostAsync(url, content);
					var responseString = await response.Content.ReadAsStringAsync();

					var info = JsonConvert.DeserializeObject<ProviderInfoResponse>(responseString);

					if (string.IsNullOrEmpty(info.error_message))
					{
						BitfiWallet.NoxChannel.Current.APEAPI_SECRET = hashingKey.ToHex();
						BitfiWallet.NoxChannel.Current.APEAPI_PUBLIC_ID = BitfiWallet.NoxDPM.NoxT.noxDevice.GetDeviceID();
						BitfiWallet.NoxChannel.Current.APE_CONNECTION_STRING = info.connection_sring;
						BitfiWallet.NoxChannel.Current.MY_CHANNEL_PUBLIC_KEY = info.public_key;

						if (info.infoItems != null)
						{
							BitfiWallet.NoxChannel.Current.des_items.Clear();
							foreach (var citem in info.infoItems)
							{
								BitfiWallet.NoxChannel.Current.des_items.Add(new WalletLibrary.ActiveApe.SatusListItem()
								{
									StatusTitle = citem.Title,
									StatusValue1 = citem.Info

								});
							}
						}

						return info;

					}
				}
			}
			catch (HttpRequestException ex)
			{

			}
			catch (Exception ex)
			{

			}

			return new ProviderInfoResponse()
			{
				error_message = "Connection error, check internet"
			};

		}

		class JError
		{
			[JsonProperty("message")]
			public string Message { get; set; }

			[JsonProperty("code")]
			public int Code { get; set; }
		}

		class JResponseWrapper<T>
		{
			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("jsonrpc")]
			public string Jsonrpc { get; set; }

			[JsonProperty("error")]
			public JError Error { get; set; }

			[JsonProperty("result")]
			public T Result { get; set; }
		}

	}
}