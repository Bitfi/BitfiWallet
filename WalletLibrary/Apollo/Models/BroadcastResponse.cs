using System;

namespace Apollo.Models
{
  public class BroadcastResponse
  {
    public Int64 RequestProcessingTime { get; set; }
    public string FullHash { get; set; }
    public string Transaction { get; set; }
  }
}
