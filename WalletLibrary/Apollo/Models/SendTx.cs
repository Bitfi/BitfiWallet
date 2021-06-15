using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Models
{
  public struct SendTx
  {
    public string SenderPublicKey { get; set; }
    public string Signature { get; set; }
    public string FeeATM { get; set; }
    public Int64 Type { get; set; }
    public string FullHash { get; set; }
    public Int64 Version { get; set; }
    public string EcBlockId { get; set; }
    public string SignatureHash { get; set; }
    public string SenderRS { get; set; }
    public Int64 Subtype { get; set; }
    public string AmountATM { get; set; }
    public string Sender { get; set; }
    public string RecipientRS { get; set; }
    public string Recipient { get; set; }
    public Int64 EcBlockHeight { get; set; }
    public Int64 Deadline { get; set; }
    public string Transaction { get; set; }
    public Int64 Timestamp { get; set; }
    public Int64 Height { get; set; } 
  }
}
