using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace WalletLibrary.NoxShared.WebSockets
{
  public class HttpHelper
  {
    private const string HTTP_GET_HEADER_REGEX = @"^GET(.*)HTTP\/1\.1";
    public static string CalculateWebSocketKey()
    {
      Random rand = new Random((int)DateTime.Now.Ticks);
      byte[] keyAsBytes = new byte[16];
      rand.NextBytes(keyAsBytes);
      return Convert.ToBase64String(keyAsBytes);
    }
    public static string ComputeSocketAcceptString(string secWebSocketKey)
    {
      const string webSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
      string concatenated = secWebSocketKey + webSocketGuid;
      byte[] concatenatedAsBytes = Encoding.UTF8.GetBytes(concatenated);
      byte[] sha1Hash = SHA1.Create().ComputeHash(concatenatedAsBytes);
      string secWebSocketAccept = Convert.ToBase64String(sha1Hash);
      return secWebSocketAccept;
    }
    public static async Task<string> ReadHttpHeaderAsync(Stream stream, CancellationToken token)
    {
      int length = 1024 * 16; // 16KB buffer more than enough for http header
      byte[] buffer = new byte[length];
      int offset = 0;
      int bytesRead = 0;

      do
      {
        if (offset >= length)
        {
          throw new Exception("Http header message too large to fit in buffer (16KB)");
        }

        bytesRead = await stream.ReadAsync(buffer, offset, length - offset, token);
        offset += bytesRead;
        string header = Encoding.UTF8.GetString(buffer, 0, offset);

        // as per http specification, all headers should end this this
        if (header.Contains("\r\n\r\n"))
        {
          return header;
        }

      } while (bytesRead > 0);

      return string.Empty;
    }
    public static bool IsWebSocketUpgradeRequest(String header)
    {
      Regex getRegex = new Regex(HTTP_GET_HEADER_REGEX, RegexOptions.IgnoreCase);
      Match getRegexMatch = getRegex.Match(header);

      if (getRegexMatch.Success)
      {
        // check if this is a web socket upgrade request
        Regex webSocketUpgradeRegex = new Regex("Upgrade: websocket", RegexOptions.IgnoreCase);
        Match webSocketUpgradeRegexMatch = webSocketUpgradeRegex.Match(header);
        return webSocketUpgradeRegexMatch.Success;
      }

      return false;
    }

    public static string GetPathFromHeader(string httpHeader)
    {
      Regex getRegex = new Regex(HTTP_GET_HEADER_REGEX, RegexOptions.IgnoreCase);
      Match getRegexMatch = getRegex.Match(httpHeader);

      if (getRegexMatch.Success)
      {
        return getRegexMatch.Groups[1].Value.Trim();
      }

      return null;
    }

    public static IList<string> GetSubProtocols(string httpHeader)
    {
      Regex regex = new Regex(@"Sec-WebSocket-Protocol:(?<protocols>.+)", RegexOptions.IgnoreCase);
      Match match = regex.Match(httpHeader);

      if (match.Success)
      {
        const int MAX_LEN = 2048;
        if (match.Length > MAX_LEN)
        {
          throw new Exception($"Sec-WebSocket-Protocol exceeded the maximum of length of {MAX_LEN}");
        }

        string csv = match.Groups["protocols"].Value.Trim();
        return csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
      }

      return new List<string>();
    }
    public static string ReadHttpResponseCode(string response)
    {
      Regex getRegex = new Regex(@"HTTP\/1\.1 (.*)", RegexOptions.IgnoreCase);
      Match getRegexMatch = getRegex.Match(response);

      if (getRegexMatch.Success)
      {
        return getRegexMatch.Groups[1].Value.Trim();
      }

      return null;
    }
    public static async Task WriteHttpHeaderAsync(string response, Stream stream, CancellationToken token)
    {
      response = response.Trim() + "\r\n\r\n";
      Byte[] bytes = Encoding.UTF8.GetBytes(response);
      await stream.WriteAsync(bytes, 0, bytes.Length, token);
    }
  }
}
