using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WalletLibrary.NoxShared.WebSockets
{
  public interface IBufferPool
  {
    MemoryStream GetBuffer();
  }
}
