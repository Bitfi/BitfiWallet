﻿using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WalletLibrary.NoxShared.WebSockets.Internal
{
  internal static class WebSocketFrameReader
  {
    public static async Task<WebSocketFrame> ReadAsync(Stream fromStream, ArraySegment<byte> intoBuffer, CancellationToken cancellationToken)
    {
      var smallBuffer = new ArraySegment<byte>(new byte[8]);

      await BinaryReaderWriter.ReadExactly(2, fromStream, smallBuffer, cancellationToken);
      byte byte1 = smallBuffer.Array[0];
      byte byte2 = smallBuffer.Array[1];

      // process first byte
      byte finBitFlag = 0x80;
      byte opCodeFlag = 0x0F;
      bool isFinBitSet = (byte1 & finBitFlag) == finBitFlag;
      WebSocketOpCode opCode = (WebSocketOpCode)(byte1 & opCodeFlag);

      // read and process second byte
      byte maskFlag = 0x80;
      bool isMaskBitSet = (byte2 & maskFlag) == maskFlag;
      uint len = await ReadLength(byte2, smallBuffer, fromStream, cancellationToken);
      int count = (int)len;

      try
      {

        if (isMaskBitSet)
        {
          ArraySegment<byte> maskKey = new ArraySegment<byte>(smallBuffer.Array, 0, WebSocketFrameCommon.MaskKeyLength);
          await BinaryReaderWriter.ReadExactly(maskKey.Count, fromStream, maskKey, cancellationToken);
          await BinaryReaderWriter.ReadExactly(count, fromStream, intoBuffer, cancellationToken);
          ArraySegment<byte> payloadToMask = new ArraySegment<byte>(intoBuffer.Array, intoBuffer.Offset, count);
          WebSocketFrameCommon.ToggleMask(maskKey, payloadToMask);
        }
        else
        {
          await BinaryReaderWriter.ReadExactly(count, fromStream, intoBuffer, cancellationToken);
        }
      }
      catch (InternalBufferOverflowException e)
      {
        throw new InternalBufferOverflowException($"Supplied buffer too small to read {0} bytes from {Enum.GetName(typeof(WebSocketOpCode), opCode)} frame", e);
      }

      if (opCode == WebSocketOpCode.ConnectionClose)
      {
        return DecodeCloseFrame(isFinBitSet, opCode, count, intoBuffer);
      }
      else
      {
        return new WebSocketFrame(isFinBitSet, opCode, count);
      }
    }
    private static WebSocketFrame DecodeCloseFrame(bool isFinBitSet, WebSocketOpCode opCode, int count, ArraySegment<byte> buffer)
    {
      WebSocketCloseStatus closeStatus;
      string closeStatusDescription;

      if (count >= 2)
      {
        Array.Reverse(buffer.Array, buffer.Offset, 2); // network byte order
        int closeStatusCode = (int)BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        if (Enum.IsDefined(typeof(WebSocketCloseStatus), closeStatusCode))
        {
          closeStatus = (WebSocketCloseStatus)closeStatusCode;
        }
        else
        {
          closeStatus = WebSocketCloseStatus.Empty;
        }

        int offset = buffer.Offset + 2;
        int descCount = count - 2;

        if (descCount > 0)
        {
          closeStatusDescription = Encoding.UTF8.GetString(buffer.Array, offset, descCount);
        }
        else
        {
          closeStatusDescription = null;
        }
      }
      else
      {
        closeStatus = WebSocketCloseStatus.Empty;
        closeStatusDescription = null;
      }

      return new WebSocketFrame(isFinBitSet, opCode, count, closeStatus, closeStatusDescription);
    }
    private static async Task<uint> ReadLength(byte byte2, ArraySegment<byte> smallBuffer, Stream fromStream, CancellationToken cancellationToken)
    {
      byte payloadLenFlag = 0x7F;
      uint len = (uint)(byte2 & payloadLenFlag);


      if (len == 126)
      {
        len = await BinaryReaderWriter.ReadUShortExactly(fromStream, false, smallBuffer, cancellationToken);
      }
      else if (len == 127)
      {
        len = (uint)await BinaryReaderWriter.ReadULongExactly(fromStream, false, smallBuffer, cancellationToken);
        const uint maxLen = 2147483648; // 2GB - not part of the spec but just a precaution. Send large volumes of data in smaller frames.

        if (len > maxLen || len < 0)
        {
          throw new ArgumentOutOfRangeException($"Payload length out of range. Min 0 max 2GB. Actual {len:#,##0} bytes.");
        }
      }

      return len;
    }
  }
}
