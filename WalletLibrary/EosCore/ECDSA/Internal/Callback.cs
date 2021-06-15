using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ECDSA.Internal
{
  public class Callback : EventArgs
  {
    public Callback()
    {
    }

    public Callback(string message)
    {
      Message = message;
    }

    public string Message;
  }
}
