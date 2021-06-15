using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanUnspent
  {
    [JsonProperty("txid")]
    public string Txid { get; set; }
    [JsonProperty("value")]
    public decimal Value { get; set; }
    [JsonProperty("n")]
    public UInt16 N { get; set; }
  }
}
