using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanAssetBalance
  {
    [JsonProperty("asset")]
    public string Asset { get; set; }
    [JsonProperty("amount")]
    public float Amount { get; set; }
    [JsonProperty("unspent")]
    public NeoscanUnspent[] Unspent { get; set; }
  }
}
