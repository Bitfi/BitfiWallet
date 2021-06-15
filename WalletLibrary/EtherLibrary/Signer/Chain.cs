using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumLibrary.Signer
{
  public enum Chain
  {
    MainNet = 1,
    Morden = 2,
    Ropsten = 3,
    Rinkeby = 4,
    RootstockMainNet = 30,
    RootstockTestNet = 31,
    Kovan = 42,
    ClassicMainNet = 61,
    ClassicTestNet = 62,
    XinFinTestnet = 51,
    XinFinMainnet = 50,
    Private = 1337
  }
}
