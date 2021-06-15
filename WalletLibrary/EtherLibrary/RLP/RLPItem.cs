using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumLibrary.RLP
{
  public class RLPItem : IRLPElement
  {
    private readonly byte[] rlpData;

    public RLPItem(byte[] rlpData)
    {
      this.rlpData = rlpData;
    }

    public byte[] RLPData => GetRLPData();

    private byte[] GetRLPData()
    {
      return rlpData.Length == 0 ? null : rlpData;
    }
  }
}
