using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WalletLibrary.NoxShared.WebSockets
{
  public class BufferPool : IBufferPool
  {
    const int DEFAULT_BUFFER_SIZE = 16384;
    private readonly ConcurrentStack<byte[]> _bufferPoolStack;
    private readonly int _bufferSize;

    public BufferPool() : this(DEFAULT_BUFFER_SIZE)
    {
    }

    public BufferPool(int bufferSize)
    {
      _bufferSize = bufferSize;
      _bufferPoolStack = new ConcurrentStack<byte[]>();
    }

    protected class PublicBufferMemoryStream : MemoryStream
    {
      private readonly BufferPool _bufferPoolInternal;
      private byte[] _buffer;
      private MemoryStream _ms;

      public PublicBufferMemoryStream(byte[] buffer, BufferPool bufferPool) : base(new byte[0])
      {
        _bufferPoolInternal = bufferPool;
        _buffer = buffer;
        _ms = new MemoryStream(buffer, 0, buffer.Length, true, true);
      }

      public override long Length => base.Length;

      public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      {
        return _ms.BeginRead(buffer, offset, count, callback, state);
      }

      public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      {
        return _ms.BeginWrite(buffer, offset, count, callback, state);
      }

      public override bool CanRead => _ms.CanRead;
      public override bool CanSeek => _ms.CanSeek;
      public override bool CanTimeout => _ms.CanTimeout;
      public override bool CanWrite => _ms.CanWrite;
      public override int Capacity { get => _ms.Capacity; set => _ms.Capacity = value; }

      public override void Close()
      {
       
        Array.Clear(_buffer, 0, (int)_ms.Position);

        _ms.Close();

        _bufferPoolInternal.ReturnBuffer(_buffer);
      }

      public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
      {
        return _ms.CopyToAsync(destination, bufferSize, cancellationToken);
      }

      public override int EndRead(IAsyncResult asyncResult)
      {
        return _ms.EndRead(asyncResult);
      }

      public override void EndWrite(IAsyncResult asyncResult)
      {
        _ms.EndWrite(asyncResult);
      }

      public override void Flush()
      {
        _ms.Flush();
      }

      public override Task FlushAsync(CancellationToken cancellationToken)
      {
        return _ms.FlushAsync(cancellationToken);
      }

      public override byte[] GetBuffer()
      {
        return _buffer;
      }

      public override long Position { get => _ms.Position; set => _ms.Position = value; }

      public override int Read(byte[] buffer, int offset, int count)
      {
        return _ms.Read(buffer, offset, count);
      }

      private void EnlargeBufferIfRequired(int count)
      {

        if (count > (_buffer.Length - _ms.Position))
        {
          int position = (int)_ms.Position;

          long newSize = (long)_buffer.Length * 2;

          long requiredSize = (long)count + _buffer.Length - position;

          if (requiredSize > int.MaxValue)
          {
            throw new Exception($"Tried to create a buffer ({requiredSize:#,##0} bytes) that was larger than the max allowed size ({int.MaxValue:#,##0})");
          }

          if (requiredSize > newSize)
          {
            long candidateSize = (long)Math.Pow(2, Math.Ceiling(Math.Log(requiredSize) / Math.Log(2)));
            if (candidateSize > int.MaxValue)
            {
              newSize = requiredSize;
            }
            else
            {
              newSize = candidateSize;
            }
          }

          var newBuffer = new byte[newSize];
          Buffer.BlockCopy(_buffer, 0, newBuffer, 0, position);
          _ms = new MemoryStream(newBuffer, 0, newBuffer.Length, true, true)
          {
            Position = position
          };

          _buffer = newBuffer;
        }
      }

      public override void WriteByte(byte value)
      {
        EnlargeBufferIfRequired(1);
        _ms.WriteByte(value);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        EnlargeBufferIfRequired(count);
        _ms.Write(buffer, offset, count);
      }

      public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      {
        EnlargeBufferIfRequired(count);
        return _ms.WriteAsync(buffer, offset, count);
      }

      public override object InitializeLifetimeService()
      {
        return _ms.InitializeLifetimeService();
      }

      public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      {
        return _ms.ReadAsync(buffer, offset, count, cancellationToken);
      }

      public override int ReadByte()
      {
        return _ms.ReadByte();
      }

      public override int ReadTimeout { get => _ms.ReadTimeout; set => _ms.ReadTimeout = value; }

      public override long Seek(long offset, SeekOrigin loc)
      {
        return _ms.Seek(offset, loc);
      }

      public override void SetLength(long value)
      {
        EnlargeBufferIfRequired((int)value);
      }

      public override byte[] ToArray()
      {
        // you should never call this
        return _ms.ToArray();
      }

      public override int WriteTimeout { get => _ms.WriteTimeout; set => _ms.WriteTimeout = value; }

//#if !NET45
//      public override bool TryGetBuffer(out ArraySegment<byte> buffer)
//      {
//        buffer = new ArraySegment<byte>(_buffer, 0, (int)_ms.Position);
//        return true;
//      }
//#endif

      public override void WriteTo(Stream stream)
      {
        _ms.WriteTo(stream);
      }
    }

    public MemoryStream GetBuffer()
    {
      if (!_bufferPoolStack.TryPop(out byte[] buffer))
      {
        buffer = new byte[_bufferSize];
      }

      return new PublicBufferMemoryStream(buffer, this);
    }

    protected void ReturnBuffer(byte[] buffer)
    {
      _bufferPoolStack.Push(buffer);
    }
  }
}
