using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumLibrary.RLP
{
  public class RLPCollection : List<IRLPElement>, IRLPElement
  {
    public byte[] RLPData { get; set; }
  }
}
