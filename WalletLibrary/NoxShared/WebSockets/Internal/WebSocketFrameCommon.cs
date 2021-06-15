using System;

namespace WalletLibrary.NoxShared.WebSockets.Internal
{
  internal static class WebSocketFrameCommon
  {
    public const int MaskKeyLength = 4;
    public static void ToggleMask(ArraySegment<byte> maskKey, ArraySegment<byte> payload)
    {
      if (maskKey.Count != MaskKeyLength)
      {
        throw new Exception($"MaskKey key must be {MaskKeyLength} bytes");
      }

      byte[] buffer = payload.Array;
      byte[] maskKeyArray = maskKey.Array;
      int payloadOffset = payload.Offset;
      int payloadCountPlusOffset = payload.Count + payloadOffset;
      int maskKeyOffset = maskKey.Offset;

      for (int i = payloadOffset; i < payloadCountPlusOffset; i++)
      {
        int payloadIndex = i - payloadOffset;
        int maskKeyIndex = maskKeyOffset + (payloadIndex % MaskKeyLength);
        buffer[i] = (Byte)(buffer[i] ^ maskKeyArray[maskKeyIndex]);
      }
    }
  }
}
