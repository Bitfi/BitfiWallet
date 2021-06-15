using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete;
using WalletLibrary.EosCore.Response.Api;
using WalletLibrary.EosCore.Serialization;

namespace WalletLibrary.EosCore
{
  public class ChainAPI : BaseAPI
  {
    private int delaySec = 30;
    public ChainAPI() { }

    public ChainAPI(string host) : base(host) { }

    public ChainAPI(string host, int delaySec) : base(host)
    {
      this.delaySec = delaySec;
    }
   
  }
}
