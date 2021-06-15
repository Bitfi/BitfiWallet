using System;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WalletLibrary.NoxShared.WebSockets.Internal
{
  internal class WebSocketImplementation : WebSocket
  {
    private readonly Guid _guid;
    private readonly Func<MemoryStream> _recycledStreamFactory;
    private readonly Stream _stream;
    private readonly bool _includeExceptionInCloseResponse;
    private readonly bool _isClient;
    private readonly string _subProtocol;
    private CancellationTokenSource _internalReadCts;
    private WebSocketState _state;
    private readonly IPingPongManager _pingPongManager;
    private bool _isContinuationFrame;
    private WebSocketMessageType _continuationFrameMessageType = WebSocketMessageType.Binary;
    private readonly bool _usePerMessageDeflate = false;
    private bool _tryGetBufferFailureLogged = false;
    const int MAX_PING_PONG_PAYLOAD_LEN = 125;
    private WebSocketCloseStatus? _closeStatus;
    private string _closeStatusDescription;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public event EventHandler<PongEventArgs> Pong;

    internal WebSocketImplementation(Guid guid, Func<MemoryStream> recycledStreamFactory, Stream stream, TimeSpan keepAliveInterval, string secWebSocketExtensions, bool includeExceptionInCloseResponse, bool isClient, string subProtocol)
    {
      _guid = guid;
      _recycledStreamFactory = recycledStreamFactory;
      _stream = stream;
      _isClient = isClient;
      _subProtocol = subProtocol;
      _internalReadCts = new CancellationTokenSource();
      _state = WebSocketState.Open;

      if (secWebSocketExtensions?.IndexOf("permessage-deflate") >= 0)
      {
        _usePerMessageDeflate = true;
      }
      else
      {
      }

      KeepAliveInterval = keepAliveInterval;
      _includeExceptionInCloseResponse = includeExceptionInCloseResponse;
      if (keepAliveInterval.Ticks < 0)
      {
        throw new InvalidOperationException("KeepAliveInterval must be Zero or positive");
      }

      if (keepAliveInterval == TimeSpan.Zero)
      {
        //log
      }
      else
      {
        _pingPongManager = new PingPongManager(guid, this, keepAliveInterval, _internalReadCts.Token);
      }
    }

    public override WebSocketCloseStatus? CloseStatus => _closeStatus;

    public override string CloseStatusDescription => _closeStatusDescription;

    public override WebSocketState State { get { return _state; } }

    public override string SubProtocol => _subProtocol;

    public TimeSpan KeepAliveInterval { get; private set; }

    public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
      try
      {
        while (true)
        {
          using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_internalReadCts.Token, cancellationToken))
          {
            WebSocketFrame frame = null;
            try
            {
              frame = await WebSocketFrameReader.ReadAsync(_stream, buffer, linkedCts.Token);
            }
            catch (InternalBufferOverflowException ex)
            {
              await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.MessageTooBig, "Frame too large to fit in buffer. Use message fragmentation", ex);
              throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
              await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.ProtocolError, "Payload length out of range", ex);
              throw;
            }
            catch (EndOfStreamException ex)
            {
              await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.InvalidPayloadData, "Unexpected end of stream encountered", ex);
              throw;
            }
            catch (OperationCanceledException ex)
            {
              await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.EndpointUnavailable, "Operation cancelled", ex);
              throw;
            }
            catch (Exception ex)
            {
              await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.InternalServerError, "Error reading WebSocket frame", ex);
              throw;
            }

            switch (frame.OpCode)
            {
              case WebSocketOpCode.ConnectionClose:
                return await RespondToCloseFrame(frame, buffer, linkedCts.Token);
              case WebSocketOpCode.Ping:
                ArraySegment<byte> pingPayload = new ArraySegment<byte>(buffer.Array, buffer.Offset, frame.Count);
                await SendPongAsync(pingPayload, linkedCts.Token);
                break;
              case WebSocketOpCode.Pong:
                ArraySegment<byte> pongBuffer = new ArraySegment<byte>(buffer.Array, frame.Count, buffer.Offset);
                Pong?.Invoke(this, new PongEventArgs(pongBuffer));
                break;
              case WebSocketOpCode.TextFrame:
                if (!frame.IsFinBitSet)
                {
                  _continuationFrameMessageType = WebSocketMessageType.Text;
                }
                return new WebSocketReceiveResult(frame.Count, WebSocketMessageType.Text, frame.IsFinBitSet);
              case WebSocketOpCode.BinaryFrame:
                if (!frame.IsFinBitSet)
                {
                  _continuationFrameMessageType = WebSocketMessageType.Binary;
                }
                return new WebSocketReceiveResult(frame.Count, WebSocketMessageType.Binary, frame.IsFinBitSet);
              case WebSocketOpCode.ContinuationFrame:
                return new WebSocketReceiveResult(frame.Count, _continuationFrameMessageType, frame.IsFinBitSet);
              default:
                Exception ex = new NotSupportedException($"Unknown WebSocket opcode {frame.OpCode}");
                await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.ProtocolError, ex.Message, ex);
                throw ex;
            }
          }
        }
      }
      catch (Exception catchAll)
      {
        if (_state == WebSocketState.Open)
        {
          await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.InternalServerError, "Unexpected error reading from WebSocket", catchAll);
        }

        throw;
      }
    }
    public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
      using (MemoryStream stream = _recycledStreamFactory())
      {
        WebSocketOpCode opCode = GetOppCode(messageType);

        WebSocketFrameWriter.Write(opCode, buffer, stream, endOfMessage, _isClient);

        await WriteStreamToNetwork(stream, cancellationToken);
        _isContinuationFrame = !endOfMessage;
      }
    }
    public async Task SendPingAsync(ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
      if (payload.Count > MAX_PING_PONG_PAYLOAD_LEN)
      {
        throw new InvalidOperationException($"Cannot send Ping: Max ping message size {MAX_PING_PONG_PAYLOAD_LEN} exceeded: {payload.Count}");
      }

      if (_state == WebSocketState.Open)
      {
        using (MemoryStream stream = _recycledStreamFactory())
        {
          WebSocketFrameWriter.Write(WebSocketOpCode.Ping, payload, stream, true, _isClient);
          await WriteStreamToNetwork(stream, cancellationToken);
        }
      }
    }
    public override void Abort()
    {
      _state = WebSocketState.Aborted;
      _internalReadCts.Cancel();
    }
    public override async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
      if (_state == WebSocketState.Open)
      {
        using (MemoryStream stream = _recycledStreamFactory())
        {
          ArraySegment<byte> buffer = BuildClosePayload(closeStatus, statusDescription);
          WebSocketFrameWriter.Write(WebSocketOpCode.ConnectionClose, buffer, stream, true, _isClient);
          await WriteStreamToNetwork(stream, cancellationToken);
          _state = WebSocketState.CloseSent;
        }
      }
      else
      {
        //log
      }
    }

    public override async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
      if (_state == WebSocketState.Open)
      {
        _state = WebSocketState.Closed;

        using (MemoryStream stream = _recycledStreamFactory())
        {
          ArraySegment<byte> buffer = BuildClosePayload(closeStatus, statusDescription);
          WebSocketFrameWriter.Write(WebSocketOpCode.ConnectionClose, buffer, stream, true, _isClient);
          await WriteStreamToNetwork(stream, cancellationToken);
        }
      }
      else
      {
        //log
      }

      _internalReadCts.Cancel();
    }
    public override void Dispose()
    {
      try
      {
        if (_state == WebSocketState.Open)
        {
          CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
          try
          {
            CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, "Service is Disposed", cts.Token).Wait();
          }
          catch (OperationCanceledException)
          {
            // log
          }
        }

        _internalReadCts.Cancel();
        _stream.Close();
      }
      catch (Exception ex)
      {
        // log
      }
    }
    protected virtual void OnPong(PongEventArgs e)
    {
      Pong?.Invoke(this, e);
    }
    private ArraySegment<byte> BuildClosePayload(WebSocketCloseStatus closeStatus, string statusDescription)
    {
      byte[] statusBuffer = BitConverter.GetBytes((ushort)closeStatus);
      Array.Reverse(statusBuffer); // network byte order (big endian)

      if (statusDescription == null)
      {
        return new ArraySegment<byte>(statusBuffer);
      }
      else
      {
        byte[] descBuffer = Encoding.UTF8.GetBytes(statusDescription);
        byte[] payload = new byte[statusBuffer.Length + descBuffer.Length];
        Buffer.BlockCopy(statusBuffer, 0, payload, 0, statusBuffer.Length);
        Buffer.BlockCopy(descBuffer, 0, payload, statusBuffer.Length, descBuffer.Length);
        return new ArraySegment<byte>(payload);
      }
    }
    private async Task SendPongAsync(ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
      // as per websocket spec
      if (payload.Count > MAX_PING_PONG_PAYLOAD_LEN)
      {
        Exception ex = new InvalidOperationException($"Max ping message size {MAX_PING_PONG_PAYLOAD_LEN} exceeded: {payload.Count}");
        await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.ProtocolError, ex.Message, ex);
        throw ex;
      }

      try
      {
        if (_state == WebSocketState.Open)
        {
          using (MemoryStream stream = _recycledStreamFactory())
          {
            WebSocketFrameWriter.Write(WebSocketOpCode.Pong, payload, stream, true, _isClient);
            await WriteStreamToNetwork(stream, cancellationToken);
          }
        }
      }
      catch (Exception ex)
      {
        await CloseOutputAutoTimeoutAsync(WebSocketCloseStatus.EndpointUnavailable, "Unable to send Pong response", ex);
        throw;
      }
    }
    private async Task<WebSocketReceiveResult> RespondToCloseFrame(WebSocketFrame frame, ArraySegment<byte> buffer, CancellationToken token)
    {
      _closeStatus = frame.CloseStatus;
      _closeStatusDescription = frame.CloseStatusDescription;

      if (_state == WebSocketState.CloseSent)
      {
        _state = WebSocketState.Closed;
      }
      else if (_state == WebSocketState.Open)
      {
        ArraySegment<byte> closePayload = new ArraySegment<byte>(new byte[0], 0, 0);
        _state = WebSocketState.CloseReceived;

        using (MemoryStream stream = _recycledStreamFactory())
        {
          WebSocketFrameWriter.Write(WebSocketOpCode.ConnectionClose, closePayload, stream, true, _isClient);
          await WriteStreamToNetwork(stream, token);
        }
      }
      else
      {
        //log
      }

      return new WebSocketReceiveResult(frame.Count, WebSocketMessageType.Close, frame.IsFinBitSet, frame.CloseStatus, frame.CloseStatusDescription);
    }

    private ArraySegment<byte> GetBuffer(MemoryStream stream)
    {

      ArraySegment<byte> buffer = null;

      bool _try = stream.TryGetBuffer(out buffer);

      if (!_try || buffer == null)
			{

        byte[] array = stream.ToArray();
        buffer = new ArraySegment<byte>(array, 0, array.Length);
      }

      return new ArraySegment<byte>(buffer.Array, buffer.Offset, (int)stream.Position);

    }

    private async Task WriteStreamToNetwork(MemoryStream stream, CancellationToken cancellationToken)
    {
      ArraySegment<byte> buffer = GetBuffer(stream);
      await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      try
      {
        await _stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
      }
      finally
      {
        _semaphore.Release();
      }
    }
    private WebSocketOpCode GetOppCode(WebSocketMessageType messageType)
    {
      if (_isContinuationFrame)
      {
        return WebSocketOpCode.ContinuationFrame;
      }
      else
      {
        switch (messageType)
        {
          case WebSocketMessageType.Binary:
            return WebSocketOpCode.BinaryFrame;
          case WebSocketMessageType.Text:
            return WebSocketOpCode.TextFrame;
          case WebSocketMessageType.Close:
            throw new NotSupportedException("Cannot use Send function to send a close frame. Use Close function.");
          default:
            throw new NotSupportedException($"MessageType {messageType} not supported");
        }
      }
    }
    private async Task CloseOutputAutoTimeoutAsync(WebSocketCloseStatus closeStatus, string statusDescription, Exception ex)
    {
      TimeSpan timeSpan = TimeSpan.FromSeconds(5);

      try
      {
        if (_includeExceptionInCloseResponse)
        {
          statusDescription = statusDescription + "\r\n\r\n" + ex.ToString();
        }

        var autoCancel = new CancellationTokenSource(timeSpan);
        await CloseOutputAsync(closeStatus, statusDescription, autoCancel.Token);
      }
      catch (OperationCanceledException)
      {
      }
      catch (Exception closeException)
      {

      }
    }
  }
}
