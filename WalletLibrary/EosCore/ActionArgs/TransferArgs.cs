using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ActionArgs
{
  public class TransferArgs
  {
    public string from { get; set; }
    public string to { get; set; }
    public string quantity { get; set; }
    public string memo { get; set; }
  }
}
