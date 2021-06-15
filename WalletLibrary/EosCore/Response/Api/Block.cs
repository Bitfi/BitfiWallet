using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Response.Api
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
    public uint block_num { get; set; }
    public uint ref_block_prefix { get; set; }
  }
}
