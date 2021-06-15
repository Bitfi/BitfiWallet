using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Models
{
  public class UnsignedTx
  {
    public string UnsignedTransactionBytes { get; set; }
    public SendTx TransactionJSON { get; set; }
  }
}
