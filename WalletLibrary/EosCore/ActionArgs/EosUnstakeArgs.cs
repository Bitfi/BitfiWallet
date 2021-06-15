using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ActionArgs
{
  public class EosUnstakeArgs
  {
    public string from { get; set; }
    public string receiver { get; set; }
    public string unstake_net_quantity { get; set; }
    public string unstake_cpu_quantity { get; set; }
    public bool transfer { get; set; }
  }
}
