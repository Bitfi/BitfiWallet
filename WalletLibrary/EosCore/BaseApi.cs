using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore
{
  public class BaseAPI
  {
    protected Uri HOST = new Uri("https://api.eosnewyork.io");

    public BaseAPI() { }

    public BaseAPI(string host)
    {
      HOST = new Uri(host);
    }
  }
}
