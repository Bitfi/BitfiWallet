using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.NoxShared.WebSockets.Internal;
using System.Security.Cryptography;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.NoxShared.WebSockets
{
	public class WebSocketClientFactory : IWebSocketClientFactory
	{
		private readonly Func<MemoryStream> _bufferFactory;
		private readonly IBufferPool _bufferPool;
		public WebSocketClientFactory()
		{
			_bufferPool = new BufferPool();
			_bufferFactory = _bufferPool.GetBuffer;
		}
		public WebSocketClientFactory(Func<MemoryStream> bufferFactory)
		{
			_bufferFactory = bufferFactory;
		}
		public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken token = default(CancellationToken))
		{
			return await ConnectAsync(uri, new WebSocketClientOptions(), token);
		}
		public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken token, string ServerPublicKey)
		{
			var options = new WebSocketClientOptions();
			Guid guid = Guid.NewGuid();
			string host = uri.Host;
			int port = uri.Port;
			string uriScheme = uri.Scheme.ToLower();
			Stream stream = await GetStream(guid, true, options.NoDelay, host, port, token, ServerPublicKey);
			return await PerformHandshake(guid, uri, stream, options, token);
		}
		public async Task<WebSocket> ConnectAsync(Uri uri, WebSocketClientOptions options, CancellationToken token = default(CancellationToken))
		{
			Guid guid = Guid.NewGuid();
			string host = uri.Host;
			int port = uri.Port;
			string uriScheme = uri.Scheme.ToLower();
			bool useSsl = uriScheme == "wss" || uriScheme == "https";
			Stream stream = await GetStream(guid, useSsl, options.NoDelay, host, port, token);
			return await PerformHandshake(guid, uri, stream, options, token);
		}
		public async Task<WebSocket> ConnectAsync(Stream responseStream, string secWebSocketKey, WebSocketClientOptions options, CancellationToken token = default(CancellationToken))
		{
			Guid guid = Guid.NewGuid();
			return await ConnectAsync(guid, responseStream, secWebSocketKey, options.KeepAliveInterval, options.SecWebSocketExtensions, options.IncludeExceptionInCloseResponse, token);
		}

		private async Task<WebSocket> ConnectAsync(Guid guid, Stream responseStream, string secWebSocketKey, TimeSpan keepAliveInterval, string secWebSocketExtensions, bool includeExceptionInCloseResponse, CancellationToken token)
		{
			string response = string.Empty;

			try
			{
				response = await HttpHelper.ReadHttpHeaderAsync(responseStream, token);
			}
			catch (Exception ex)
			{

				throw new Exception("Handshake unexpected failure", ex);
			}

			ThrowIfInvalidResponseCode(response);
			ThrowIfInvalidAcceptString(guid, response, secWebSocketKey);
			string subProtocol = GetSubProtocolFromHeader(response);
			return new WebSocketImplementation(guid, _bufferFactory, responseStream, keepAliveInterval, secWebSocketExtensions, includeExceptionInCloseResponse, true, subProtocol);
		}

		private string GetSubProtocolFromHeader(string response)
		{
			string regexPattern = "Sec-WebSocket-Protocol: (.*)";
			Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
			Match match = regex.Match(response);
			if (match.Success)
			{
				return match.Groups[1].Value.Trim();
			}

			return null;
		}

		private void ThrowIfInvalidAcceptString(Guid guid, string response, string secWebSocketKey)
		{

			string regexPattern = "Sec-WebSocket-Accept: (.*)";
			Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
			string actualAcceptString = regex.Match(response).Groups[1].Value.Trim();

			string expectedAcceptString = HttpHelper.ComputeSocketAcceptString(secWebSocketKey);
			if (expectedAcceptString != actualAcceptString)
			{
				string warning = string.Format($"Handshake failed because the accept string from the server '{expectedAcceptString}' was not the expected string '{actualAcceptString}'");

				throw new Exception(warning);
			}
			else
			{

			}
		}

		private void ThrowIfInvalidResponseCode(string responseHeader)
		{
			string responseCode = HttpHelper.ReadHttpResponseCode(responseHeader);
			if (!string.Equals(responseCode, "101 Switching Protocols", StringComparison.InvariantCultureIgnoreCase))
			{
				string[] lines = responseHeader.Split(new string[] { "\r\n" }, StringSplitOptions.None);

				for (int i = 0; i < lines.Length; i++)
				{
					// if there is more to the message than just the header
					if (string.IsNullOrWhiteSpace(lines[i]))
					{
						StringBuilder builder = new StringBuilder();
						for (int j = i + 1; j < lines.Length - 1; j++)
						{
							builder.AppendLine(lines[j]);
						}

						string responseDetails = builder.ToString();
						throw new InvalidHttpResponseCodeException(responseCode, responseDetails, responseHeader);
					}
				}
			}
		}

		protected virtual void TlsAuthenticateAsClient(SslStream sslStream, string host)
		{
			sslStream.AuthenticateAsClient(host, null, SslProtocols.Tls12, true);
		}

		static string _ServerPublicKey = null;

		protected virtual async Task<Stream> GetStream(Guid loggingGuid, bool isSecure, bool noDelay, string host, int port, CancellationToken cancellationToken, string ServerPublicKey = null)
		{
			var tcpClient = new TcpClient();
			tcpClient.NoDelay = noDelay;
			IPAddress ipAddress;
			if (IPAddress.TryParse(host, out ipAddress))
			{
				await tcpClient.ConnectAsync(ipAddress, port);
			}
			else
			{
				await tcpClient.ConnectAsync(host, port);
			}

			cancellationToken.ThrowIfCancellationRequested();
			Stream stream = tcpClient.GetStream();

			if (isSecure)
			{
				_ServerPublicKey = ServerPublicKey;
				SslStream sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

				TlsAuthenticateAsClient(sslStream, host);
				return sslStream;
			}
			else
			{
				return stream;
			}
		}

		static bool IsChainAllowed(X509ChainStatus[] chains)
		{
			foreach (var status in chains)
			{
				if (status.Status != X509ChainStatusFlags.NoIssuanceChainPolicy
					&& status.Status != X509ChainStatusFlags.UntrustedRoot)
					return false;

			}

			return true;
		}

		private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{

			if (!string.IsNullOrEmpty(_ServerPublicKey))
			{
				try
				{
					if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
						return false;

					if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
						return false;

					if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
					{

						if (!IsChainAllowed(chain.ChainStatus))
							return false;
					}

					SHA1Managed sHA = new SHA1Managed();
					var hash = sHA.ComputeHash(certificate.GetPublicKey());

					if (hash.ToHex().ToLower() == _ServerPublicKey.ToLower())
						return true;
				}
				catch
				{

				}

				return false;

			}

			if (sslPolicyErrors == SslPolicyErrors.None)
			{

				return true;
			}

			return false;
		}
		private static string GetAdditionalHeaders(Dictionary<string, string> additionalHeaders)
		{
			if (additionalHeaders == null || additionalHeaders.Count == 0)
			{
				return string.Empty;
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				foreach (KeyValuePair<string, string> pair in additionalHeaders)
				{
					builder.Append($"{pair.Key}: {pair.Value}\r\n");
				}

				return builder.ToString();
			}
		}

		private async Task<WebSocket> PerformHandshake(Guid guid, Uri uri, Stream stream, WebSocketClientOptions options, CancellationToken token)
		{
			Random rand = new Random();
			byte[] keyAsBytes = new byte[16];
			rand.NextBytes(keyAsBytes);
			string secWebSocketKey = Convert.ToBase64String(keyAsBytes);
			string additionalHeaders = GetAdditionalHeaders(options.AdditionalHttpHeaders);
			string handshakeHttpRequest = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
																		$"Host: {uri.Host}:{uri.Port}\r\n" +
																		 "Upgrade: websocket\r\n" +
																		 "Connection: Upgrade\r\n" +
																		$"Sec-WebSocket-Key: {secWebSocketKey}\r\n" +
																		$"Origin: http://{uri.Host}:{uri.Port}\r\n" +
																		$"Sec-WebSocket-Protocol: {options.SecWebSocketProtocol}\r\n" +
																		additionalHeaders +
																		 "Sec-WebSocket-Version: 13\r\n\r\n";

			byte[] httpRequest = Encoding.UTF8.GetBytes(handshakeHttpRequest);
			stream.Write(httpRequest, 0, httpRequest.Length);
			return await ConnectAsync(stream, secWebSocketKey, options, token);
		}
	}
}
