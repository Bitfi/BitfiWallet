using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanEndpointInfo
  {
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("height")]
    public UInt64 Height { get; set; }
  }
}
