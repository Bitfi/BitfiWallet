using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.Core.Abstract
{
  public interface IBip32Node : IDisposable
  {
    UInt64 Depth { get; }
    UInt64 Child { get; }

    IBip32Node Clone();
    bool DerivePublic(UInt32 child);
    bool DerivePrivate(UInt32 child);
    NoxManagedArray GetPrivateKey();
    NoxManagedArray GetPublicKey(bool isCompressed);

  }
}
