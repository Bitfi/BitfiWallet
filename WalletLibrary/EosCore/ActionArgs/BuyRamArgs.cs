using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ActionArgs
{
  public class BuyRamArgs
  {
    public string payer { get; set; }
    public string receiver { get; set; }
    public UInt32 bytes { get; set; }
  }
}
