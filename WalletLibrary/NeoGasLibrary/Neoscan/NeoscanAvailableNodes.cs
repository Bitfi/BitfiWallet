using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanAvailableNodes
  {
    [JsonProperty("urls")]
    public string[] Urls { get; set; }
  }
}
