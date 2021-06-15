using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Serialization
{
  public class PermissionName : BaseName
  {
    public PermissionName() { }

    public PermissionName(string value)
        : base(value)
    {
    }
  }
}
