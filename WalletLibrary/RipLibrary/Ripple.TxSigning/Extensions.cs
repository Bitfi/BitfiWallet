using Ripple.Core;
using Ripple.Core.Binary;
using Ripple.Core.Hashing;
using Ripple.Core.Types;
using Ripple.Core.Util;

namespace Ripple.TxSigning
{
    internal static class Extensions
    {
        internal static byte[] Bytes(this HashPrefix hp)
        {
            return Bits.GetBytes((uint)hp);
        }
    }
}