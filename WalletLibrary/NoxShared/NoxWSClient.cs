using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using WalletLibrary.NoxShared.WebSockets;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.NoxShared
{

  public delegate void MSGCompletedEventHandler(MSGCompletedEventArgs e);
  public class MessageProxy
  {
    public event MSGCompletedEventHandler MsgCompleted;
    public void LocallyHandleMessageArrived(string msg)
    {
      try
      {
        Interlocked.CompareExchange(ref MsgCompleted, null, null)?.Invoke(new MSGCompletedEventArgs(msg));
      }
      catch { }
    }
  }
  public partial class MSGCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
  {
    public MSGCompletedEventArgs(object result) : base(null, false, null) { Message = (string)result; }
    public string Message { get; }
  }
  public class NoxWSClient : IDisposable
  {
    WebSocket webSocket;
    WebSocketClientFactory factory = new WebSocketClientFactory();
    Uri uri = new Uri("wss://bitfi.com/MSGX/NoxWS.ashx");
    bool CloseRequested;

    MessageProxy _messageProxy;
    byte[] _PeerPubKey;
    public NoxWSClient(MessageProxy messageProxy, string Peer)
    {
      _messageProxy = messageProxy;
      _PeerPubKey = Peer.HexToByteArray();
    }
    private void Notification(string msg)
    {
      try
      {
        _messageProxy.LocallyHandleMessageArrived(msg);
      }
      catch { }
    }

    public byte[] Peer()
    {
      return _PeerPubKey;
    }
    public async Task StartWS(string authReq)
    {

      webSocket = await factory.ConnectAsync(uri);
      
      Run();
      await Send(authReq);

    }

    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 500]);
    private void Run()
    {
      Task.Run(async () =>
      {
        try
        {
          do
          {

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancellationToken.Token);
            if (result == null) break;

            string response = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Notification(response);

          } while (webSocket.State == WebSocketState.Open);

          if (!CloseRequested) Notification("close");
        }
        catch (Exception)
        {

        }
      });
    }
    public async Task WSClose()
    {
      if (webSocket.State != WebSocketState.Open)
      {
        return;
      }
      CloseRequested = true;
      await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "user", CancellationToken.None);
      Dispose();
    }
    public async Task Send(string Msg)
    {
      if (webSocket.State != WebSocketState.Open)
      {
        if (!CloseRequested) Notification("close");
        return;
      }

      var array = Encoding.UTF8.GetBytes(Msg);
      var buffer = new ArraySegment<byte>(array);
      await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public async void Dispose()
    {
      webSocket.Dispose();
    }
  }

}
