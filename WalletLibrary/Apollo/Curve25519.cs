using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo
{
  public struct Key
  {
    public byte[] P { get; set; }
    public byte[] K { get; set; }
    public byte[] S { get; set; }
  }

  public static class Curve25519
  {
    private const int UNPACKED_SIZE = 16;
    private const int KEY_SIZE = 32;
    private const int P25 = 33554431; /* (1 << 25) - 1 */
    private const int P26 = 67108863; /* (1 << 26) - 1 */

    private static readonly UInt16[] C1 = new UInt16[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly UInt16[] C9 = new UInt16[] { 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly UInt16[] C486671 = new UInt16[] { 0x6D0F, 0x0007, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly UInt16[] C39420360 = new UInt16[] { 0x81C8, 0x0259, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly UInt16[] BASE_R2Y = new UInt16[] {
        5744, 16384, 61977, 54121,
        8776, 18501, 26522, 34893,
        23833, 5823, 55924, 58749,
        24147, 14085, 13606, 6080
    };
    /* smallest multiple of the order that's >= 2^255 */
    private static readonly byte[] ORDER_TIMES_8 = new byte[] {
        104, 159, 174, 231,
        210, 24, 147, 192,
        178, 230, 188, 23,
        245, 206, 247, 166,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 128
    };

    /* group order (a prime near 2^252+2^124) */
    private static readonly byte[] ORDER = new byte[] {
        237, 211, 245, 92,
        26, 99, 18, 88,
        214, 156, 247, 162,
        222, 249, 222, 20,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 16
    };


    private static void Clamp(byte[] k)
    {
      k[31] &= 0x7F;
      k[31] |= 0x40;
      k[0] &= 0xF8;
    }

    private static UInt16[] CreateUnpackedArray()
    {
      return new UInt16[UNPACKED_SIZE];
    }

    /* Convert to internal format from little-endian byte format */
    private static void Unpack(UInt16[] x, byte[] m)
    {
      for (var i = 0; i < KEY_SIZE; i += 2)
      {
        x[i / 2] = (UInt16)((UInt16)(m[i] & 0xFF) | ((UInt16)(m[i + 1] & 0xFF) << 8));
      }
    }

    /* Copy a number */
    private static void Cpy(UInt16[] d, UInt16[] s)
    {
      for (var i = 0; i < UNPACKED_SIZE; ++i)
        d[i] = s[i];
    }

    /* Set a number to value, which must be in range -185861411 .. 185861411 */
    private static void Set(UInt16[] d, UInt16 s)
    {
      d[0] = s;
      for (var i = 1; i < UNPACKED_SIZE; ++i)
        d[i] = 0;
    }

    private static void Add(UInt16[] r, UInt16[] a, UInt16[] b)
    {
      Int64 v = 0;
      r[0] = (UInt16)((v = (Int64)((((a[15] / 0x8000) | 0) + ((b[15] / 0x8000) | 0)) * 19 + a[0] + b[0])) & 0xFFFF);
      for (var i = 1; i <= 14; ++i)
      {
        r[i] = (UInt16)((v = ((v / 0x10000) | 0) + a[i] + b[i]) & 0xFFFF);
      }
      r[15] = (UInt16)(((v / 0x10000) | 0) + (a[15] & 0x7FFF) + (b[15] & 0x7FFF));
    }

    private static void Sub(UInt16[] r, UInt16[] a, UInt16[] b)
    {
      Int64 v;
      r[0] = (UInt16)((v = 0x80000 + (((a[15] / 0x8000) | 0) - ((b[15] / 0x8000) | 0) - 1) * 19 + a[0] - b[0]) & 0xFFFF);
      for (var i = 1; i <= 14; ++i)
      {
        r[i] = (UInt16)((v = ((v / 0x10000) | 0) + 0x7fff8 + a[i] - b[i]) & 0xFFFF);
      }
      r[15] = (UInt16)(((v / 0x10000) | 0) + 0x7ff8 + (a[15] & 0x7FFF) - (b[15] & 0x7FFF));
    }

    private static void MontPrep(UInt16[] t1, UInt16[] t2, UInt16[] ax, UInt16[] az)
    {
      Add(t1, ax, az);
      Sub(t2, ax, az);
    }

    private static Int64[] C255lmul8h(Int64 a7, Int64 a6, Int64 a5, Int64 a4, Int64 a3,
      Int64 a2, Int64 a1, Int64 a0, Int64 b7, Int64 b6, Int64 b5, Int64 b4, Int64 b3, Int64 b2, Int64 b1, Int64 b0)
    {
      var r = new Int64[16];
      Int64 v = 0;
      r[0] = ((v = (Int64)((a0 * b0))) & 0xFFFF);
      r[1] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b1 + a1 * b0)) & 0xFFFF);
      r[2] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b2 + a1 * b1 + a2 * b0)) & 0xFFFF);
      r[3] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b3 + a1 * b2 + a2 * b1 + a3 * b0)) & 0xFFFF);
      r[4] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b4 + a1 * b3 + a2 * b2 + a3 * b1 + a4 * b0)) & 0xFFFF);
      r[5] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b5 + a1 * b4 + a2 * b3 + a3 * b2 + a4 * b1 + a5 * b0)) & 0xFFFF);
      r[6] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b6 + a1 * b5 + a2 * b4 + a3 * b3 + a4 * b2 + a5 * b1 + a6 * b0)) & 0xFFFF);
      r[7] = ((v = (Int64)(((v / 0x10000) | 0) + a0 * b7 + a1 * b6 + a2 * b5 + a3 * b4 + a4 * b3 + a5 * b2 + a6 * b1 + a7 * b0)) & 0xFFFF);
      r[8] = ((v = (Int64)(((v / 0x10000) | 0) + a1 * b7 + a2 * b6 + a3 * b5 + a4 * b4 + a5 * b3 + a6 * b2 + a7 * b1)) & 0xFFFF);
      r[9] = ((v = (Int64)(((v / 0x10000) | 0) + a2 * b7 + a3 * b6 + a4 * b5 + a5 * b4 + a6 * b3 + a7 * b2)) & 0xFFFF);
      r[10] = ((v = (Int64)(((v / 0x10000) | 0) + a3 * b7 + a4 * b6 + a5 * b5 + a6 * b4 + a7 * b3)) & 0xFFFF);
      r[11] = ((v = (Int64)(((v / 0x10000) | 0) + a4 * b7 + a5 * b6 + a6 * b5 + a7 * b4)) & 0xFFFF);
      r[12] = ((v = (Int64)(((v / 0x10000) | 0) + a5 * b7 + a6 * b6 + a7 * b5)) & 0xFFFF);
      r[13] = ((v = (Int64)(((v / 0x10000) | 0) + a6 * b7 + a7 * b6)) & 0xFFFF);
      r[14] = ((v = (Int64)(((v / 0x10000) | 0) + a7 * b7)) & 0xFFFF);
      r[15] = ((v / 0x10000) | 0);
      return r;
    }

    private static void C255lreduce(UInt16[] a, Int64 a15)
    {
      Int64 v = a15;
      a[15] = (UInt16)(v & 0x7FFF);
      v = ((v / 0x8000) | 0) * 19;
      for (var i = 0; i <= 14; ++i)
      {
        a[i] = (UInt16)((v += a[i]) & 0xFFFF);
        v = ((v / 0x10000) | 0);
      }

      a[15] += (UInt16)(v);
    }

    private static void Mul(UInt16[] r, UInt16[] a, UInt16[] b)
    {
      // Karatsuba multiplication scheme: x*y = (b^2+b)*x1*y1 - b*(x1-x0)*(y1-y0) + (b+1)*x0*y0
      var x = C255lmul8h(a[15], a[14], a[13], a[12], a[11], a[10], a[9], a[8], b[15], b[14], b[13], b[12], b[11], b[10], b[9], b[8]);
      var z = C255lmul8h(a[7], a[6], a[5], a[4], a[3], a[2], a[1], a[0], b[7], b[6], b[5], b[4], b[3], b[2], b[1], b[0]);
      var y = C255lmul8h((a[15] + a[7]), (a[14] + a[6]), (a[13] + a[5]), (a[12] + a[4]), (a[11] + a[3]), (a[10] + a[2]), (a[9] + a[1]), (a[8] + a[0]),
          (b[15] + b[7]), (b[14] + b[6]), (b[13] + b[5]), (b[12] + b[4]), (b[11] + b[3]), (b[10] + b[2]), (b[9] + b[1]), (b[8] + b[0]));

      Int64 v = 0;
      r[0] = (UInt16)((v = (Int64)(0x800000 + z[0] + (y[8] - x[8] - z[8] + x[0] - 0x80) * 38)) & 0xFFFF);
      r[1] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[1] + (y[9] - x[9] - z[9] + x[1]) * 38)) & 0xFFFF);
      r[2] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[2] + (y[10] - x[10] - z[10] + x[2]) * 38)) & 0xFFFF);
      r[3] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[3] + (y[11] - x[11] - z[11] + x[3]) * 38)) & 0xFFFF);
      r[4] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[4] + (y[12] - x[12] - z[12] + x[4]) * 38)) & 0xFFFF);
      r[5] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[5] + (y[13] - x[13] - z[13] + x[5]) * 38)) & 0xFFFF);
      r[6] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[6] + (y[14] - x[14] - z[14] + x[6]) * 38)) & 0xFFFF);
      r[7] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[7] + (y[15] - x[15] - z[15] + x[7]) * 38)) & 0xFFFF);
      r[8] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[8] + y[0] - x[0] - z[0] + x[8] * 38)) & 0xFFFF);
      r[9] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[9] + y[1] - x[1] - z[1] + x[9] * 38)) & 0xFFFF);
      r[10] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[10] + y[2] - x[2] - z[2] + x[10] * 38)) & 0xFFFF);
      r[11] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[11] + y[3] - x[3] - z[3] + x[11] * 38)) & 0xFFFF);
      r[12] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[12] + y[4] - x[4] - z[4] + x[12] * 38)) & 0xFFFF);
      r[13] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[13] + y[5] - x[5] - z[5] + x[13] * 38)) & 0xFFFF);
      r[14] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[14] + y[6] - x[6] - z[6] + x[14] * 38)) & 0xFFFF);
      Int64 r15 = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[15] + y[7] - x[7] - z[7] + x[15] * 38);
      C255lreduce(r, r15);
    }

    private static Int64[] C255lsqr8h(Int64 a7, Int64 a6, Int64 a5, Int64 a4, Int64 a3, Int64 a2, Int64 a1, Int64 a0)
    {
      var r = new Int64[16];
      Int64 v = 0;
      
      r[0] = ((v = (Int64)(a0 * a0)) & 0xFFFF);
      r[1] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a1)) & 0xFFFF);
      r[2] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a2 + a1 * a1)) & 0xFFFF);
      r[3] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a3 + 2 * a1 * a2)) & 0xFFFF);
      r[4] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a4 + 2 * a1 * a3 + a2 * a2)) & 0xFFFF);
      r[5] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a5 + 2 * a1 * a4 + 2 * a2 * a3)) & 0xFFFF);
      r[6] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a6 + 2 * a1 * a5 + 2 * a2 * a4 + a3 * a3)) & 0xFFFF);
      r[7] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a0 * a7 + 2 * a1 * a6 + 2 * a2 * a5 + 2 * a3 * a4)) & 0xFFFF);
      r[8] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a1 * a7 + 2 * a2 * a6 + 2 * a3 * a5 + a4 * a4)) & 0xFFFF);
      r[9] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a2 * a7 + 2 * a3 * a6 + 2 * a4 * a5)) & 0xFFFF);
      r[10] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a3 * a7 + 2 * a4 * a6 + a5 * a5)) & 0xFFFF);
      r[11] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a4 * a7 + 2 * a5 * a6)) & 0xFFFF);
      r[12] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a5 * a7 + a6 * a6)) & 0xFFFF);
      r[13] = ((v = (Int64)(((v / 0x10000) | 0) + 2 * a6 * a7)) & 0xFFFF);
      r[14] = ((v = (Int64)(((v / 0x10000) | 0) + a7 * a7)) & 0xFFFF);
      r[15] = (((v / 0x10000) | 0));
      return r;
    }

    private static void Sqr(UInt16[] r, UInt16[] a)
    {
      var x = C255lsqr8h(a[15], a[14], a[13], a[12], a[11], a[10], a[9], a[8]);
      var z = C255lsqr8h(a[7], a[6], a[5], a[4], a[3], a[2], a[1], a[0]);
      var y = C255lsqr8h(a[15] + a[7], a[14] + a[6], a[13] + a[5], a[12] + a[4], a[11] + a[3], a[10] + a[2], a[9] + a[1], a[8] + a[0]);

      Int64 v = 0;
      r[0] = (UInt16)((v = (Int64)(0x800000 + z[0] + (y[8] - x[8] - z[8] + x[0] - 0x80) * 38)) & 0xFFFF);
      r[1] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[1] + (y[9] - x[9] - z[9] + x[1]) * 38)) & 0xFFFF);
      r[2] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[2] + (y[10] - x[10] - z[10] + x[2]) * 38)) & 0xFFFF);
      r[3] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[3] + (y[11] - x[11] - z[11] + x[3]) * 38)) & 0xFFFF);
      r[4] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[4] + (y[12] - x[12] - z[12] + x[4]) * 38)) & 0xFFFF);
      r[5] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[5] + (y[13] - x[13] - z[13] + x[5]) * 38)) & 0xFFFF);
      r[6] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[6] + (y[14] - x[14] - z[14] + x[6]) * 38)) & 0xFFFF);
      r[7] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[7] + (y[15] - x[15] - z[15] + x[7]) * 38)) & 0xFFFF);
      r[8] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[8] + y[0] - x[0] - z[0] + x[8] * 38)) & 0xFFFF);
      r[9] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[9] + y[1] - x[1] - z[1] + x[9] * 38)) & 0xFFFF);
      r[10] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[10] + y[2] - x[2] - z[2] + x[10] * 38)) & 0xFFFF);
      r[11] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[11] + y[3] - x[3] - z[3] + x[11] * 38)) & 0xFFFF);
      r[12] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[12] + y[4] - x[4] - z[4] + x[12] * 38)) & 0xFFFF);
      r[13] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[13] + y[5] - x[5] - z[5] + x[13] * 38)) & 0xFFFF);
      r[14] = (UInt16)((v = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[14] + y[6] - x[6] - z[6] + x[14] * 38)) & 0xFFFF);
      Int64 r15 = (Int64)(0x7fff80 + ((v / 0x10000) | 0) + z[15] + y[7] - x[7] - z[7] + x[15] * 38);
      C255lreduce(r, r15);
    }

    private static void MulSmall(UInt16[] r, UInt16[] a, Int64 m)
    {
      Int64 v = 0;
      r[0] = (UInt16)((v = a[0] * m) & 0xFFFF);
      for (var i = 1; i <= 14; ++i)
        r[i] = (UInt16)((v = ((v / 0x10000) | 0) + a[i] * m) & 0xFFFF);

      var r15 = ((v / 0x10000) | 0) + a[15] * m;
      C255lreduce(r, r15);
    }

    /* B = 2 * Q   where
     *  X(B) = bx/bz
     *  X(Q) = (t3+t4)/(t3-t4)
     * clobbers t1 and t2, preserves t3 and t4  */
    private static void MontDbl(UInt16[] t1, UInt16[] t2, UInt16[] t3, UInt16[] t4, UInt16[] bx, UInt16[] bz)
    {
      Sqr(t1, t3);
      Sqr(t2, t4);
      Mul(bx, t1, t2);
      Sub(t2, t1, t2);
      MulSmall(bz, t2, 121665);
      Add(t1, t1, bz);
      Mul(bz, t1, t2);
    }

    private static void MontAdd(UInt16[] t1, UInt16[] t2, UInt16[] t3, UInt16[] t4, UInt16[] ax, UInt16[] az, UInt16[] dx)
    {
      Mul(ax, t2, t3);
      Mul(az, t1, t4);
      Add(t1, ax, az);
      Sub(t2, ax, az);
      Sqr(ax, t1);
      Sqr(t1, t2);
      Mul(az, t1, dx);
    }

    private static void Recip(UInt16[] y, UInt16[] x, Int64 sqrtassist)
    {
      UInt16[] t0 = CreateUnpackedArray();
      UInt16[] t1 = CreateUnpackedArray();
      UInt16[] t2 = CreateUnpackedArray();
      UInt16[] t3 = CreateUnpackedArray();
      UInt16[] t4 = CreateUnpackedArray();

      /* the chain for x^(2^255-21) is straight from djb's implementation */
      Int64 i;
      Sqr(t1, x); /*  2 === 2 * 1	*/
      Sqr(t2, t1); /*  4 === 2 * 2	*/
      Sqr(t0, t2); /*  8 === 2 * 4	*/
      Mul(t2, t0, x); /*  9 === 8 + 1	*/
      Mul(t0, t2, t1); /* 11 === 9 + 2	*/
      Sqr(t1, t0); /* 22 === 2 * 11	*/
      Mul(t3, t1, t2); /* 31 === 22 + 9 === 2^5   - 2^0	*/
      Sqr(t1, t3); /* 2^6   - 2^1	*/
      Sqr(t2, t1); /* 2^7   - 2^2	*/
      Sqr(t1, t2); /* 2^8   - 2^3	*/
      Sqr(t2, t1); /* 2^9   - 2^4	*/
      Sqr(t1, t2); /* 2^10  - 2^5	*/
      Mul(t2, t1, t3); /* 2^10  - 2^0	*/
      Sqr(t1, t2); /* 2^11  - 2^1	*/
      Sqr(t3, t1); /* 2^12  - 2^2	*/
      for (i = 1; i < 5; i++)
      {
        Sqr(t1, t3);
        Sqr(t3, t1);
      } /* t3 */ /* 2^20  - 2^10	*/
      Mul(t1, t3, t2); /* 2^20  - 2^0	*/
      Sqr(t3, t1); /* 2^21  - 2^1	*/
      Sqr(t4, t3); /* 2^22  - 2^2	*/
      for (i = 1; i < 10; i++)
      {
        Sqr(t3, t4);
        Sqr(t4, t3);
      } /* t4 */ /* 2^40  - 2^20	*/
      Mul(t3, t4, t1); /* 2^40  - 2^0	*/
      for (i = 0; i < 5; i++)
      {
        Sqr(t1, t3);
        Sqr(t3, t1);
      } /* t3 */ /* 2^50  - 2^10	*/
      Mul(t1, t3, t2); /* 2^50  - 2^0	*/
      Sqr(t2, t1); /* 2^51  - 2^1	*/
      Sqr(t3, t2); /* 2^52  - 2^2	*/
      for (i = 1; i < 25; i++)
      {
        Sqr(t2, t3);
        Sqr(t3, t2);
      } /* t3 */ /* 2^100 - 2^50 */
      Mul(t2, t3, t1); /* 2^100 - 2^0	*/
      Sqr(t3, t2); /* 2^101 - 2^1	*/
      Sqr(t4, t3); /* 2^102 - 2^2	*/
      for (i = 1; i < 50; i++)
      {
        Sqr(t3, t4);
        Sqr(t4, t3);
      } /* t4 */ /* 2^200 - 2^100 */
      Mul(t3, t4, t2); /* 2^200 - 2^0	*/
      for (i = 0; i < 25; i++)
      {
        Sqr(t4, t3);
        Sqr(t3, t4);
      } /* t3 */ /* 2^250 - 2^50	*/
      Mul(t2, t3, t1); /* 2^250 - 2^0	*/
      Sqr(t1, t2); /* 2^251 - 2^1	*/
      Sqr(t2, t1); /* 2^252 - 2^2	*/
      if (sqrtassist != 0)
      {
        Mul(y, x, t2); /* 2^252 - 3 */
      }
      else
      {
        Sqr(t1, t2); /* 2^253 - 2^3	*/
        Sqr(t2, t1); /* 2^254 - 2^4	*/
        Sqr(t1, t2); /* 2^255 - 2^5	*/
        Mul(y, t1, t0); /* 2^255 - 21	*/
      }
    }

    /* Y^2 = X^3 + 486662 X^2 + X
     * t is a temporary  */
    private static void XtoY2(UInt16[] t, UInt16[] y2, UInt16[] x)
    {
      // C1 = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

      Sqr(t, x);
      MulSmall(y2, x, 486662);
      Add(t, t, y2);
      Add(t, t, C1);
      Mul(y2, t, x);
    }

    /* Check if reduced-form input >= 2^255-19 */
    private static bool IsOverflow(UInt16[] x)
    {
      return (
            ((x[0] > P26 - 19)) &&
                ((x[1] & x[3] & x[5] & x[7] & x[9]) == P25) &&
                ((x[2] & x[4] & x[6] & x[8]) == P26)
            ) || (x[9] > P25);
    }

    /* checks if x is "negative", requires reduced input */
    private static Int64 IsNegative(UInt16[] x)
    {
      var isOverflowOrNegative = IsOverflow(x) || x[9] < 0;
      var leastSignificantBit = x[0] & 1;
      return ((isOverflowOrNegative ? 1 : 0) ^ leastSignificantBit) & 0xFFFFFFFF;
    }

    private static void Cpy32(byte[] d, byte[] s)
    {
      for (var i = 0; i < 32; i++)
        d[i] = s[i];
    }

    /* p[m..n+m-1] = q[m..n+m-1] + z * x */
    /* n is the size of x */
    /* n+m is the size of p and q */
    private static int MulaSmall(byte[] p, byte[] q, int m, byte[] x, int n, int z)
    {
      m = m | 0;
      n = n | 0;
      z = z | 0;

      var v = 0;
      for (var i = 0; i < n; ++i)
      {
        v += (q[i + m] & 0xFF) + z * (x[i] & 0xFF);
        p[i + m] = (byte)(v & 0xFF);
        v >>= 8;
      }

      return v;
    }

    private static void Core(byte[] Px, byte[] s, byte[] k, byte[] Gx)
    {
      var dx = CreateUnpackedArray();
      var t1 = CreateUnpackedArray();
      var t2 = CreateUnpackedArray();
      var t3 = CreateUnpackedArray();
      var t4 = CreateUnpackedArray();
      var x = new UInt16[2][] { CreateUnpackedArray(), CreateUnpackedArray() };
      var z = new UInt16[2][] { CreateUnpackedArray(), CreateUnpackedArray() };
      int i = 0;
      int j = 0;

      /* unpack the base */
      if (Gx != null)
        Unpack(dx, Gx);
      else
        Set(dx, 9);

      /* 0G = point-at-infinity */
      Set(x[0], 1);
      Set(z[0], 0);

      /* 1G = G */
      Cpy(x[1], dx);
      Set(z[1], 1);

      for (i = 32; i-- != 0;)
      {
        for (j = 8; j-- != 0;)
        {
          /* swap arguments depending on bit */
          var bit1 = (k[i] & 0xFF) >> j & 1;
          var bit0 = ~(k[i] & 0xFF) >> j & 1;
          var ax = x[bit0];
          var az = z[bit0];
          var bx = x[bit1];
          var bz = z[bit1];

          /* a' = a + b	*/
          /* b' = 2 b	*/
          MontPrep(t1, t2, ax, az);
          MontPrep(t3, t4, bx, bz);
          MontAdd(t1, t2, t3, t4, ax, az, dx);
          MontDbl(t1, t2, t3, t4, bx, bz);

        }
      }

      Recip(t1, z[0], 0);
      Mul(dx, x[0], t1);

      Pack(dx, Px);

      /* calculate s such that s abs(P) = G  .. assumes G is std base point */
      if (s != null)
      {
        XtoY2(t2, t1, dx); /* t1 = Py^2  */
        Recip(t3, z[1], 0); /* where Q=P+G ... */
        Mul(t2, x[1], t3); /* t2 = Qx  */
        Add(t2, t2, dx); /* t2 = Qx + Px  */
        Add(t2, t2, C486671); /* t2 = Qx + Px + Gx + 486662  */
        Sub(dx, dx, C9); /* dx = Px - Gx  */
        Sqr(t3, dx); /* t3 = (Px - Gx)^2  */
        Mul(dx, t2, t3); /* dx = t2 (Px - Gx)^2  */
        Sub(dx, dx, t1); /* dx = t2 (Px - Gx)^2 - Py^2  */
        Sub(dx, dx, C39420360); /* dx = t2 (Px - Gx)^2 - Py^2 - Gy^2  */
        Mul(t1, dx, BASE_R2Y); /* t1 = -Py  */

        if (IsNegative(t1) != 0)    /* sign is 1, so just copy  */
          Cpy32(s, k);
        else            /* sign is -1, so negate  */
          MulaSmall(s, ORDER_TIMES_8, 0, k, 32, -1);

        /* reduce s mod q
         * (is this needed?  do it just in case, it's fast anyway) */
        //divmod((dstptr) t1, s, 32, order25519, 32);

        /* take reciprocal of s mod q */
        var temp1 = new byte[32];
        var temp2 = new byte[64];
        var temp3 = new byte[64];
        Cpy32(temp1, ORDER);
        Cpy32(s, Egcd32(temp2, temp3, s, temp1));
        if ((s[31] & 0x80) != 0)
          MulaSmall(s, s, 0, ORDER, 32, 1);

      }
    }

    private static int Numsize(byte[] x, int n)
    {
      while (n-- != 0 && x[n] == 0) { }
      return n + 1;
    }

    /* divide r (size n) by d (size t), returning quotient q and remainder r
     * quotient is size n-t+1, remainder is size t
     * requires t > 0 && d[t-1] !== 0
     * requires that r[-1] and d[-1] are valid memory locations
     * q may overlap with r+t */
    private static void Divmod(byte[] q, byte[] r, int n, byte[] d, int t)
    {
      n = n | 0;
      t = t | 0;

      var rn = 0;
      var dt = (d[t - 1] & 0xFF) << 8;
      if (t > 1)
        dt |= (d[t - 2] & 0xFF);

      while (n-- >= t)
      {
        var z = (rn << 16) | ((r[n] & 0xFF) << 8);
        if (n > 0)
          z |= (r[n - 1] & 0xFF);

        var i = n - t + 1;
        z /= dt;
        rn += MulaSmall(r, r, i, d, t, -z);
        q[i] = (byte)((z + rn) & 0xFF);
        /* rn is 0 or -1 (underflow) */
        MulaSmall(r, r, i, d, t, -rn);
        rn = r[n] & 0xFF;
        r[n] = 0;
      }

      r[t - 1] = (byte)(rn & 0xFF);
    }

    /* p += x * y * z  where z is a small integer
     * x is size 32, y is size t, p is size 32+t
     * y is allowed to overlap with p+32 if you don't care about the upper half  */
    private static int Mula32(byte[] p, byte[] x, byte[] y, int t, int z)
    {
      t = t | 0;
      z = z | 0;

      var n = 31;
      var w = 0;
      var i = 0;
      for (; i < t; i++)
      {
        var zy = z * (y[i] & 0xFF);
        w += MulaSmall(p, p, i, x, n, zy) + (p[i + n] & 0xFF) + zy * (x[n] & 0xFF);
        p[i + n] = (byte)(w & 0xFF);
        w >>= 8;
      }
      p[i + n] = (byte)((w + (p[i + n] & 0xFF)) & 0xFF);
      return w >> 8;
    }

    /* Signature generation primitive, calculates (x-h)s mod q
     *   h  [in]  signature hash (of message, signature pub key, and context data)
     *   x  [in]  signature private key
     *   s  [in]  private key for signing
     * returns signature value on success, undefined on failure (use different x or h)
     */
    public static byte[] Sign(byte[] h, byte[] x, byte[] s)
    {
      // v = (x - h) s  mod q
      int w = 0;
      int i = 0;
      var h1 = new byte[32];
      var x1 = new byte[32];
      var tmp1 = new byte[64];
      var tmp2 = new byte[64];

      // Don't clobber the arguments, be nice!
      Cpy32(h1, h);
      Cpy32(x1, x);

      // Reduce modulo group order
      var tmp3 = new byte[32];
      Divmod(tmp3, h1, 32, ORDER, 32);
      Divmod(tmp3, x1, 32, ORDER, 32);

      // v = x1 - h1
      // If v is negative, add the group order to it to become positive.
      // If v was already positive we don't have to worry about overflow
      // when adding the order because v < ORDER and 2*ORDER < 2^256
      var v = new byte[32];
      MulaSmall(v, x1, 0, h1, 32, -1);
      MulaSmall(v, v, 0, ORDER, 32, 1);

      // tmp1 = (x-h)*s mod q
      Mula32(tmp1, v, s, 32, 1);
      Divmod(tmp2, tmp1, 64, ORDER, 32);

      for (w = 0, i = 0; i < 32; i++)
        w |= v[i] = tmp1[i];

      return w != 0 ? v : null;
    }

    /* Returns x if a contains the gcd, y if b.
     * Also, the returned buffer contains the inverse of a mod b,
     * as 32-byte signed.
     * x and y must have 64 bytes space for temporary use.
     * requires that a[-1] and b[-1] are valid memory locations  */
    private static byte[] Egcd32(byte[] x, byte[] y, byte[] a, byte[] b)
    {
      int an = 0;
      int bn = 32;
      int qn = 0;
      int i = 0;

      for (i = 0; i < 32; i++)
        x[i] = y[i] = 0;
      x[0] = 1;
      an = Numsize(a, 32);
      if (an == 0)
        return y; /* division by zero */
      var temp = new byte[32];
      while (true)
      {
        qn = bn - an + 1;
        Divmod(temp, b, bn, a, an);
        bn = Numsize(b, bn);
        if (bn == 0)
          return x;
        Mula32(y, x, temp, qn, -1);

        qn = an - bn + 1;
        Divmod(temp, a, an, b, bn);
        an = Numsize(a, an);
        if (an == 0)
          return y;
        Mula32(x, y, temp, qn, -1);
      }
    }

    /* Convert from internal format to little-endian byte format.  The
     * number must be in a reduced form which is output by the following ops:
     *     unpack, mul, sqr
     *     set --  if input in range 0 .. P25
     * If you're unsure if the number is reduced, first multiply it by 1.  */
    private static void Pack(UInt16[] x, byte[] m)
    {
      for (var i = 0; i < UNPACKED_SIZE; ++i)
      {
        m[2 * i] = (byte)(x[i] & 0x00FF);
        m[2 * i + 1] = (byte)((x[i] & 0xFF00) >> 8);
      }
    }

    public static Key Keygen(byte[] k)
    {
      var P = new byte[32];
      var s = new byte[32];
      Clamp(k);
      Core(P, s, k, null);

      return new Key
      {
        P = P,
        S = s,
        K = k
      };
    }
  }
}
