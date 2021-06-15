using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoGasLibrary.NeoRpc
{
  public class NeoRpcRequest
  {
    public string Method { get; set; }
    public byte Id { get; set; }
    public string Jsonrpc { get; set; }
    public object[] Params { get; set; }
  }
}
