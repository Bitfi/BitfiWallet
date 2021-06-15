using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthereumLibrary.Model
{
  public class DefaultValues
  {

    public static DefaultValues Current { get; } = new DefaultValues();

    public static byte[] EMPTY_BYTE_ARRAY = new byte[0];
    public static readonly byte[] ZERO_BYTE_ARRAY = { 0 };


  }
}
