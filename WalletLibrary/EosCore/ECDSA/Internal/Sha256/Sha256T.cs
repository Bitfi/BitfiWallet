using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ECDSA.Internal.Sha256
{
  internal class Sha256T
  {
    public UInt32[] S;
    public UInt32[] Buf; // In big endian
    public UInt32 Bytes;

    public Sha256T()
    {
      S = new uint[8];
      Buf = new uint[16];
      Bytes = 0;
    }
  }
}
