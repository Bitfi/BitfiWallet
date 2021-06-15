using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanUnclaimed
  {
    [JsonProperty("unclaimed")]
    public float Unclaimed { get; set; }
    [JsonProperty("claimable")]
    public NeoscanClaimable[] Claimable { get; set; }
    [JsonProperty("address")]
    public string Address { get; set; }
  }
}
