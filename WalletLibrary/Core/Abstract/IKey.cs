using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.Core.Abstract
{
  public interface IKey : IDisposable
  {
    
    IKey DerivePublic(UInt32 child);
    IKey DerivePrivate(UInt32 child);
    IKey Clone();
    NoxManagedArray GetPublicKey(bool isCompressed = true);
    NoxManagedArray GetPrivateKey();
    IKey DeriveKeyForCurrency(UInt32 child, string symbol);
    IKey DeriveCurrency(string symbol);
  }
}
