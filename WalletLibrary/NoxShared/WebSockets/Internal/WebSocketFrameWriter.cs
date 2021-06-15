using System.IO;
using System;
using System.Net.WebSockets;
using System.Text;

namespace WalletLibrary.NoxShared.WebSockets.Internal
{
  internal static class WebSocketFrameWriter
  {
    private static readonly Random _random;

    static WebSocketFrameWriter()
    {
      _random = new Random((int)DateTime.Now.Ticks);
    }
    public static void Write(WebSocketOpCode opCode, ArraySegment<byte> fromPayload, MemoryStream toStream, bool isLastFrame, bool isClient)
    {
      MemoryStream memoryStream = toStream;
      byte finBitSetAsByte = isLastFrame ? (byte)0x80 : (byte)0x00;
      byte byte1 = (byte)(finBitSetAsByte | (byte)opCode);
      memoryStream.WriteByte(byte1);


      byte maskBitSetAsByte = isClient ? (byte)0x80 : (byte)0x00;


      if (fromPayload.Count < 126)
      {
        byte byte2 = (byte)(maskBitSetAsByte | (byte)fromPayload.Count);
        memoryStream.WriteByte(byte2);
      }
      else if (fromPayload.Count <= ushort.MaxValue)
      {
        byte byte2 = (byte)(maskBitSetAsByte | 126);
        memoryStream.WriteByte(byte2);
        BinaryReaderWriter.WriteUShort((ushort)fromPayload.Count, memoryStream, false);
      }
      else
      {
        byte byte2 = (byte)(maskBitSetAsByte | 127);
        memoryStream.WriteByte(byte2);
        BinaryReaderWriter.WriteULong((ulong)fromPayload.Count, memoryStream, false);
      }

 
      if (isClient)
      {
        byte[] maskKey = new byte[WebSocketFrameCommon.MaskKeyLength];
        _random.NextBytes(maskKey);
        memoryStream.Write(maskKey, 0, maskKey.Length);

    
        ArraySegment<byte> maskKeyArraySegment = new ArraySegment<byte>(maskKey, 0, maskKey.Length);
        WebSocketFrameCommon.ToggleMask(maskKeyArraySegment, fromPayload);
      }

      memoryStream.Write(fromPayload.Array, fromPayload.Offset, fromPayload.Count);
    }
  }
}
