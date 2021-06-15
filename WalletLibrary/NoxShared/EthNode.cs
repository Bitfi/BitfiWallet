using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.NoxShared
{
  public class EthNode
  {
    public const string ADDRESS = "0xfF97E4e335B2Aa9013b332C988835A0980843ECf";
    public const string CONTRACT = "0x3947824866f950fbD088bFAf107d5F4af2FecF13";
    public const string DEFAULT_HOST = "mainnet.infura.io";

    public const string DEFAULT_HOST_URI = "https://mainnet.infura.io/v3/b70bdf8a929e4ab7b730b6b4b1f6742c";
    public async static Task<uint> GetNextNonce(string baseUrl, string filter = "latest")
    {

      var data = new
      {
        Jsonrpc = "2.0",
        Method = "eth_getTransactionCount",
        Id = 1,
        Params = new object[]
        {
          ADDRESS,
          filter
        },
      };

      var jsonFrom = await PostAsync(baseUrl, data);
      var responseFrom = JsonConvert.DeserializeObject<EthRpcResponseWrapper<string>>(jsonFrom);

      if (responseFrom.Error != null)
        throw new Exception(responseFrom.Error.Message);

      return uint.Parse(RemoveHexPrefix(responseFrom.Result), System.Globalization.NumberStyles.HexNumber);
    }

    public async static Task<string> GetPeer(string name, string baseUrl)
    {
      try
      {
        var prefix = await GetByName(name, "0x29d8344a", baseUrl);


        if (prefix != null)
        {
          var x = await GetByName(name, "0x7a9bcc54", baseUrl);

          if (x != null)
          {
            return prefix.Substring(2, 2) + RemoveHexPrefix(x);
          }
        }
        return null;
      }
      catch
      {
        return "";
      }
    }

    async static Task<string> GetByName(string name, string methodCode, string baseUrl)
    {
      var data = new
      {
        Jsonrpc = "2.0",
        Method = "eth_call",
        Id = 1,
        Params = new object[]
        {
          new {
            To = CONTRACT,
            Data = methodCode + NameTo256Uint(name)
          },
          "pending"
        },
      };

      var jsonFrom = await PostAsync(baseUrl, data);
      var responseFrom = JsonConvert.DeserializeObject<EthRpcResponseWrapper<string>>(jsonFrom);

      if (responseFrom.Error != null)
        throw new Exception(responseFrom.Error.Message);

      var notFound = RemoveHexPrefix(responseFrom.Result).Any(v => v != '0') == false;
      return notFound ? null : responseFrom.Result.ToLower();
    }


    public async static Task<string> GetByPubKey(string comppubKey, string baseUrl)
    {
      try
      {
        var keccak256 = new EthereumLibrary.Util.Sha3Keccack();
        var compressedPubKeyHash = BytesToHex(keccak256.CalculateHash(comppubKey.HexToByteArray()));

        var methodCode = "0x20c38e2b";
        var data = new
        {
          Jsonrpc = "2.0",
          Method = "eth_call",
          Id = 1,
          Params = new object[]
          {
          new {
            To = CONTRACT,
            Data = methodCode + RemoveHexPrefix(compressedPubKeyHash)
          },
          "pending"
          },
        };


        var jsonFrom = await PostAsync(baseUrl, data);
        var responseFrom = JsonConvert.DeserializeObject<EthRpcResponseWrapper<string>>(jsonFrom);

        if (responseFrom.Error != null)
          throw new Exception(responseFrom.Error.Message);

        string result = RemoveHexPrefix(responseFrom.Result);

        if (!NameOK(result)) return "";

        return System.Text.Encoding.ASCII.GetString(result.HexToByteArray()).Replace("\0", "");
      }
      catch
      {
        return "";
      }

    }

    static bool NameOK(string Result)
    {
      var notFound = Result.Any(v => v != '0') == false;
      return !notFound;
    }

    public async static Task<bool> IsRegistered(string comppubKey, string baseUrl)
    {

      var keccak256 = new EthereumLibrary.Util.Sha3Keccack();
      var compressedPubKeyHash = BytesToHex(keccak256.CalculateHash(comppubKey.HexToByteArray()));

      var methodCode = "0x20c38e2b";
      var data = new
      {
        Jsonrpc = "2.0",
        Method = "eth_call",
        Id = 1,
        Params = new object[]
        {
          new {
            To = CONTRACT,
            Data = methodCode + RemoveHexPrefix(compressedPubKeyHash)
          },
          "pending"
        },
      };

      var jsonFrom = await PostAsync(baseUrl, data);
      var responseFrom = JsonConvert.DeserializeObject<EthRpcResponseWrapper<string>>(jsonFrom);

      if (responseFrom.Error != null)
        throw new Exception(responseFrom.Error.Message);

      var notFound = RemoveHexPrefix(responseFrom.Result).Any(v => v != '0') == false;
      return !notFound;
    }

    private async static Task<string> PostAsync(string url, object obj)
    {
      try
      {
        url = "https://" + url.ToLower().Trim();
        if (url.IndexOf(DEFAULT_HOST) > -1) url = DEFAULT_HOST_URI;

        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var json = JsonConvert.SerializeObject(obj, serializerSettings);

        byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
        using (var client = new HttpClient())
        {
          client.Timeout = TimeSpan.FromMilliseconds(10000);
          var content = new ByteArrayContent(byteArray);
          content.Headers.Add("Content-Type", "application/json");
          using (var response = await client.PostAsync(url, content))
          {

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
          }
        }
      }
      catch (HttpRequestException ex)
      {
        return "";
      }
      catch (TaskCanceledException e)
      {
        return "";
      }
      catch (Exception)
      {
        return "";
      }
    }


    public static string BytesToHex(IEnumerable<byte> value)
    {
      StringBuilder sb = new StringBuilder();
      foreach (byte b in value)
        sb.AppendFormat("{0:x2}", b);
      return sb.ToString();
    }

    public static string RemoveHexPrefix(string str)
    {
      return (str.Length >= 2 && str[0] == '0' && str[1] == 'x') ? str.Substring(2) : str;
    }

    public static string NameTo256Uint(string name)
    {
      var hex = BytesToHex(System.Text.Encoding.UTF8.GetBytes(name));
      return Fill256uint(hex);
    }

    public static string Fill256uint(string ba)
    {
      return (RemoveHexPrefix(ba) + new string('0', 64)).Substring(0, 64);
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
      protected override string ResolvePropertyName(string propertyName)
      {
        return propertyName.ToLower();
      }
    }
    public class EthRpcError
    {
      [JsonProperty("code")]
      public int Code { get; set; }

      [JsonProperty("message")]
      public string Message { get; set; }
    }

    public class EthRpcResponseWrapper<T>
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("jsonrpc")]
      public string Jsonrpc { get; set; }

      [JsonProperty("error")]
      public EthRpcError Error { get; set; }

      [JsonProperty("result")]
      public T Result { get; set; }
    }

  }
}
