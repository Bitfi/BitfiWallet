using Newtonsoft.Json;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanBalance
  {
    [JsonProperty("balance")]
    public NeoscanAssetBalance[] Balance { get; set; }
    [JsonProperty("address")]
    public string Address { get; set; }
  }
}
