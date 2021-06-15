using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Params
{
  public class PushTransactionParam
  {
    public string packed_trx { get; set; }
    public string packed_context_free_data { get; set; }
    public string compression { get; set; }
    public string[] signatures { get; set; }
  }
}
