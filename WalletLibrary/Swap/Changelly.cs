using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChangellySwap
{

	class Changelly
	{

		public static string CreateSwap(string obj, string signExtra, string api_key, out CreateTxnResp swapResp)
		{

			var CreateTxnResponseObject = Post(obj, signExtra, api_key);

			var responseFrom = JsonConvert.DeserializeObject<JResponseWrapper<CreateTxnResp>>(CreateTxnResponseObject);

			if (responseFrom.Error != null && !string.IsNullOrEmpty(responseFrom.Error.Message)) throw new Exception(responseFrom.Error.Message);

			swapResp = responseFrom.Result;

			return CreateTxnResponseObject.ConvertToB64();

		}
		static string FormatJson(object obj)
		{
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			return JsonConvert.SerializeObject(obj, serializerSettings);
		}
		public static string Post(string obj, string sign, string api_key)
		{
			string error = null;
			string resp = null;
			try
			{

				byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(obj.ConvertFromB64());
				using (var client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler()))
				{
					var content = new ByteArrayContent(byteArray);
					content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
					content.Headers.Add("X-Api-Key", api_key);
					content.Headers.Add("X-Api-Signature", sign);
					using (var response = client.PostAsync("https://api.changelly.com/v2", content).Result)
					{

						var responseString = response.Content.ReadAsStringAsync().Result;
						resp = responseString;
					}
				}
			}
			catch (HttpRequestException ex)
			{
				error = ex.Message;
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}

			if (!string.IsNullOrEmpty(error)) throw new Exception("[chngly] " + error);

			return resp;
		}
		static string PostV1(string obj, string sign, string api_key)
		{

			string error = null;
			string resp = null;

			try
			{
				WebRequest request = WebRequest.Create("https://api.changelly.com");
				request.Method = "POST";
				request.ContentType = "application/json";
				request.Timeout = 10000;
				request.Headers.Add("api-key", api_key);
				request.Headers.Add("sign", sign);

				byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(obj.ConvertFromB64());
				request.ContentLength = byteArray.Length;

				using (System.IO.Stream dataStream = request.GetRequestStream())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
					dataStream.Close();
				}

				using (WebResponse response = request.GetResponse())
				using (System.IO.Stream stream = response.GetResponseStream())
				{
					System.IO.StreamReader sr = new System.IO.StreamReader(stream);
					resp = sr.ReadToEnd();
				}
			}
			catch (System.Net.WebException wex)
			{
				try
				{
					if (wex.Response != null)
					{
						using (var errorResponse = (System.Net.HttpWebResponse)wex.Response)
						{
							using (var reader = new System.IO.StreamReader(errorResponse.GetResponseStream())) error = reader.ReadToEnd();
						}
					}
					else
					{
						error = wex.Message;
					}
				}
				catch (Exception ex)
				{
					error = ex.Message;
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}

			if (!string.IsNullOrEmpty(error)) throw new Exception("[chngly] " + error);

			return resp;

		}

	}

	static class StringExtensions
	{
		public static string ConvertToB64(this string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			return Convert.ToBase64String(bytes);
		}
		public static string ConvertFromB64(this string value)
		{
			byte[] bytes = Convert.FromBase64String(value);
			return Encoding.UTF8.GetString(bytes);
		}
		public static string ConvertToHex(this byte[] value, bool prefix = false)
		{
			var strPrex = prefix ? "0x" : "";
			return strPrex + string.Concat(value.Select(b => b.ToString("x2")).ToArray());
		}

	}

	public class JError
	{
		[JsonProperty("message")]
		public string Message { get; set; }
	}

	public class JResponseWrapper<T>
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
	public class xxxCreateTxnResp
	{
		public string refundAddress { get; set; }
		public decimal amountExpectedFrom { get; set; }
		public decimal amountExpectedTo { get; set; }
		public string currencyFrom { get; set; }
		public string currencyTo { get; set; }
		public string payinAddress { get; set; }
		public string payoutAddress { get; set; }

	}


	public class CreateTxnResp
	{
		public string id { get; set; }
		public string apiExtraFee { get; set; }
		public string changellyFee { get; set; }
		public string payinExtraId { get; set; }
		public string payoutExtraId { get; set; }
		public string refundAddress { get; set; }
		public string amountExpectedFrom { get; set; }
		public string amountExpectedTo { get; set; }
		public string currencyFrom { get; set; }
		public string currencyTo { get; set; }
		public string payinAddress { get; set; }
		public string payoutAddress { get; set; }

	}
}

