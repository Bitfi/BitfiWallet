using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Models
{
  public class AccountTxes
  {
    public string RequestProcessingTime { get; set; }
    public Transaction[] Transactions { get; set; }
  }
}
