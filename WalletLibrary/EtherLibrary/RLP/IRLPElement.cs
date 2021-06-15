using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumLibrary.RLP
{
  public interface IRLPElement
  {
    byte[] RLPData { get; }
  }
}
