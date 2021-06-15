using System;
using System.Collections.Generic;
using System.Text;

namespace WalletLibrary.NoxShared.WebSockets
{
  public class PongEventArgs : EventArgs
  {
    public ArraySegment<byte> Payload { get; private set; }
    public PongEventArgs(ArraySegment<byte> payload)
    {
      Payload = payload;
    }
  }
}
