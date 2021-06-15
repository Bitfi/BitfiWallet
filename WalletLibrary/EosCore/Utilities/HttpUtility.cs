using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Utilities
{
  public class Block
  {
    public string timestamp { get; set; }
    public DateTime timestamp_datetime
    {
      get
      {
        return DateTime.SpecifyKind((DateTime.Parse(timestamp)), DateTimeKind.Utc);
      }
    }
    public string id { get; set; }
    public uint block_num { get; set; }
    public uint ref_block_prefix { get; set; }
  }
}
