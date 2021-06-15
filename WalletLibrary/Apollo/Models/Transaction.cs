using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Models
{
  public struct Transaction
  {
    public string AmountATM { get; set; }
    public string SenderRS { get; set; }
    public string RecipientRS { get; set; }
    public string Timestamp { get; set; }
    public string FeeATM { get; set; }
    public Int64 Confirmations { get; set; }
    public string FullHash { get; set; }
    public string Height { get; set; }
  }
}
