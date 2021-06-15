using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.NoxShared.WebSockets.Internal;

namespace WalletLibrary.NoxShared.WebSockets
{
  public class PingPongManager : IPingPongManager
  {
    private readonly WebSocketImplementation _webSocket;
    private readonly Guid _guid;
    private readonly TimeSpan _keepAliveInterval;
    private readonly Task _pingTask;
    private readonly CancellationToken _cancellationToken;
    private Stopwatch _stopwatch;
    private long _pingSentTicks;

    public event EventHandler<PongEventArgs> Pong;
    public PingPongManager(Guid guid, WebSocket webSocket, TimeSpan keepAliveInterval, CancellationToken cancellationToken)
    {
      var webSocketImpl = webSocket as WebSocketImplementation;
      _webSocket = webSocketImpl ?? throw new InvalidCastException("Cannot cast WebSocket to an instance of WebSocketImplementation. Please use the web socket factories to create a web socket");
      _guid = guid;
      _keepAliveInterval = keepAliveInterval;
      _cancellationToken = cancellationToken;
      webSocketImpl.Pong += WebSocketImpl_Pong;
      _stopwatch = Stopwatch.StartNew();

      if (keepAliveInterval == TimeSpan.Zero)
      {
        _pingTask = Task.FromResult(0);
      }
      else
      {
        _pingTask = Task.Run(PingForever, cancellationToken);
      }
    }
    public async Task SendPing(ArraySegment<byte> payload, CancellationToken cancellation)
    {
      await _webSocket.SendPingAsync(payload, cancellation);
    }

    protected virtual void OnPong(PongEventArgs e)
    {
      Pong?.Invoke(this, e);
    }

    private async Task PingForever()
    {

      try
      {
        while (!_cancellationToken.IsCancellationRequested)
        {
          await Task.Delay(_keepAliveInterval, _cancellationToken);

          if (_webSocket.State != WebSocketState.Open)
          {
            break;
          }

          if (_pingSentTicks != 0)
          {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"No Pong message received in response to a Ping after KeepAliveInterval {_keepAliveInterval}", _cancellationToken);
            break;
          }

          if (!_cancellationToken.IsCancellationRequested)
          {
            _pingSentTicks = _stopwatch.Elapsed.Ticks;
            ArraySegment<byte> buffer = new ArraySegment<byte>(BitConverter.GetBytes(_pingSentTicks));
            await SendPing(buffer, _cancellationToken);
          }
        }
      }
      catch (OperationCanceledException)
      {
        // normal, do nothing
      }

    }

    private void WebSocketImpl_Pong(object sender, PongEventArgs e)
    {
      _pingSentTicks = 0;
      OnPong(e);
    }
  }
}
