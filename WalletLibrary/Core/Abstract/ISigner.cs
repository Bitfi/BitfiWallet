using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.Core.Abstract
{
  public interface ISigner : IDisposable
  {
    DerResponse GetDerResponse(byte[] mes);
    byte[] GetBFASecret();
    byte[] Sign(byte[] mes);
    bool Verify(byte[] message, byte[] signature);
  }

  public class DerResponse
  {
    public byte[] signature { get; set; }
    public byte[] public_key { get; set; }
  }
}
