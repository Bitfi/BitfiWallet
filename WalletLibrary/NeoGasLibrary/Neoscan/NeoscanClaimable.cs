using Newtonsoft.Json;
using System;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanClaimable
  {
    [JsonProperty("txid")]
    public string Txid { get; set; }
    [JsonProperty("n")]
    public UInt16 N { get; set; }
    [JsonProperty("unclaimed")]
    public decimal Unclaimed { get; set; }
    [JsonProperty("start_height")]
    public UInt64 StartHeight { get; set; }
    [JsonProperty("end_height")]
    public UInt64 EndHeight { get; set; }
  }
}
