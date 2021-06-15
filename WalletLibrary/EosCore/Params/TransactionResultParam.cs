using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Params
{
  public class TransactionResultParam
  {
    public string id { get; set; }
    public uint? block_num_hint { get; set; }
  }
}
