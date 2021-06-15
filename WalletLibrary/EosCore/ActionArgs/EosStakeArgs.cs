using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.ActionArgs
{
  public class EosStakeArgs
  {
    public string from { get; set; }
    public string receiver { get; set; }

    //delegatingNet
    public string stake_net_quantity { get; set; }

    //delegatingCpu
    public string stake_cpu_quantity { get; set; }
    public bool transfer { get; set; }
  }
}
