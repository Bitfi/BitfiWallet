using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Serialization
{
  public class AccountName : BaseName
  {
    public AccountName() { }

    public AccountName(string value)
        : base(value)
    {
    }
  }
}
