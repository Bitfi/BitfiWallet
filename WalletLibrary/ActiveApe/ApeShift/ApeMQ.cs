using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WalletLibrary.ActiveApe.ApeShift
{
	public class ApeMQ
	{
		public event DataMSGCompletedEventHandler _OnMsg;

		ClientWebSocket webSocket;

		CancellationTokenSource _token;

		public async Task StartWS(CancellationTokenSource token)
		{

			try
			{
				if (token == null || token.IsCancellationRequested)
					return;

		
				_token = token;
				webSocket = new ClientWebSocket();
				webSocket.Options.SetRequestHeader("Connection-Token", BitfiWallet.NoxChannel.Current.APE_CONNECTION_STRING);

				LocallyHandleMessageArrived("1");

				try
				{

					await Task.Delay(600);

					await webSocket.ConnectAsync(new Uri("wss://app.async360.com/"), _token.Token);

					LocallyHandleMessageArrived("0");


					await RunWS();

				}
				catch (Exception ex)
				{
					if (token == null || token.IsCancellationRequested)
						return;

					await StartWS(token);

				}
			}
			catch { }
		}

		private async Task RunWS()
		{
			while (webSocket.State == WebSocketState.Open)
			{

				var msg = await GetMessageResponse();

				LocallyHandleMessageArrived(msg);

				await PopMsg(msg);

			}

			throw new Exception("disconnected");
		}

		private async Task PopMsg(string Message)
		{
			if (string.IsNullOrEmpty(Message))
				return;

			MQResponse<object> resp = JsonConvert.DeserializeObject<MQResponse<object>>(Message);

			await Send(resp.DequeueCommand);

		}
		private async Task<string> GetMessageResponse()
		{
			List<byte[]> received = new List<byte[]>();

			while (true)
			{
				try
				{
					ArraySegment<byte> in_buffer = new ArraySegment<byte>(new byte[1024]);
					WebSocketReceiveResult result = await webSocket.ReceiveAsync(in_buffer, _token.Token);

					if (result == null || result.Count == 0)
						break;

					byte[] resp = new byte[result.Count];
					Buffer.BlockCopy(in_buffer.Array, 0, resp, 0, result.Count);
					received.Add(resp);

					if (result.EndOfMessage || result.Count == 0)
						break;
				}
				catch { break; }
			}

			byte[] eomresp = new byte[0];
			var bref = eomresp.Concat(received.ToArray());
			return Encoding.UTF8.GetString(bref);
		}

		public async Task Send(string Msg)
		{
			var array = Encoding.UTF8.GetBytes(Msg);
			var buffer = new ArraySegment<byte>(array);
			await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _token.Token);
		}

		public async Task WSClose()
		{
			if (webSocket.State != WebSocketState.Open)
				return;

			try
			{
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "user", CancellationToken.None);
			}
			catch { }
		}

		void LocallyHandleMessageArrived(string msg)
		{
			if (_token == null || _token.IsCancellationRequested)

				return;
			if (_OnMsg == null) return;

			try
			{
				Interlocked.CompareExchange(ref _OnMsg, null, null)?.Invoke(new DataMSGCompletedEventArgs(msg));
			}
			catch { }


		}
	}

	static class WSExtensions
	{
		public static int Find(this byte[] buff, byte[] search)
		{
			try
			{
				for (int start = 0; start < buff.Length - search.Length; start++)
				{
					if (buff[start] == search[0])
					{
						int next;
						for (next = 1; next < search.Length; next++)
						{
							if (buff[start + next] != search[next])
								break;
						}

						if (next == search.Length)
							return start;
					}
				}
			}
			catch { }
			return -1;
		}

		public static byte[] Concat(this byte[] arr, params byte[][] arrs)
		{
			var len = arr.Length + arrs.Sum(a => a.Length);
			var ret = new byte[len];
			Buffer.BlockCopy(arr, 0, ret, 0, arr.Length);
			var pos = arr.Length;
			foreach (var a in arrs)
			{
				Buffer.BlockCopy(a, 0, ret, pos, a.Length);
				pos += a.Length;
			}
			return ret;
		}
		public static ArraySegment<byte> Subarray(this byte[] array, int total_sent, out bool eof)
		{
			eof = false;
			int remaining = array.Length - total_sent;

			if (remaining <= 0)
				throw new Exception("no more...");

			int count = 1024;
			if (remaining <= count)
			{
				eof = true;
				count = remaining;
			}

			var buff = new byte[count];
			ArraySegment<byte> in_buffer = new ArraySegment<byte>(buff);
			Buffer.BlockCopy(array, total_sent, buff, 0, count);

			return in_buffer;
		}
	}
}
