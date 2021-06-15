#if !NOSOCKET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitcoin.Protocol.Filters
{
    public class NodeFiltersCollection : ThreadSafeCollection<INodeFilter>
    {

    }
}
#endif