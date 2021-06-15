using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Apollo
{
  public static class AplAddress
  {
    private static readonly int[] Gexp = new int[] {
      1, 2, 4, 8, 16, 5, 10, 20,
      13, 26, 17, 7, 14, 28, 29, 31,
      27, 19, 3, 6, 12, 24, 21, 15,
      30, 25, 23, 11, 22, 9, 18, 1
    };

    private static readonly int[] Glog = new int[] {
      0, 0, 1, 18, 2, 5, 19, 11,
      3, 29, 6, 27, 20, 8, 12, 23,
      4, 10, 30, 17, 7, 22, 28, 26,
      21, 25, 9, 16, 13, 14, 24, 15
    };

    private static readonly int[] Cwmap = new int[] {
      3, 2, 1, 0, 7, 6, 5, 4, 13, 14, 15, 16, 12, 8, 9, 10, 11
    };

    public static readonly string AccountMask = "APL-";
    public static string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    private static int Gmult(int a, int b)
    {
      if (a == 0 || b == 0)
        return 0;

      var idx = (Glog[a] + Glog[b]) % 31;

      return Gexp[idx];
    }

    public static bool IsValidAddressRS(string address)
    {
      var regex = new System.Text.RegularExpressions.Regex(
        @"APL-[2-9A-HJ-NP-Z]{4}-[2-9A-HJ-NP-Z]{4}-[2-9A-HJ-NP-Z]{4}-[2-9A-HJ-NP-Z]{5}");
      return regex.IsMatch(address) && address.Length == 24;
    }

    private static void Encode(List<int> ls)
    {
      var p = new int[] { 0, 0, 0, 0 };

      for (var i = 12; i >= 0; i--)
      {
        var fb = ls[i] ^ p[3];

        p[3] = p[2] ^ Gmult(30, fb);
        p[2] = p[1] ^ Gmult(6, fb);
        p[1] = p[0] ^ Gmult(9, fb);
        p[0] = Gmult(17, fb);
      }

      ls.Add(p[0], 13);
      ls.Add(p[1], 14);
      ls.Add(p[2], 15);
      ls.Add(p[3], 16);
    }

    public static string NumericToRSAccountFormat(string accountId)
    {
      var inp = new List<int>();
      var o = new List<int>();
      var codeword = new List<int>();
      int pos = 0;
      int len = accountId.Length;

      if (len == 20 && accountId[0] != '1')
        throw new Exception(String.Format("len ('{0}') == 20 && acc.charAt(0) ('{1}') != '1'",
          len, accountId[0]));

      for (var i = 0; i < len; i++)
      {
        inp.Add(accountId[i] - '0', i);
      }

      int divide = 0;
      int newlen = 0;

      do // base 10 to base 32 conversion
      {
        divide = 0;
        newlen = 0;

        for (int i = 0; i < len; i++)
        {
          divide = divide * 10 + inp[i];

          if (divide >= 32)
          {
            inp.Add(divide >> 5, newlen);
            newlen++;
            divide &= 31;
          }
          else if (newlen > 0)
          {
            inp.Add(0, newlen);
            newlen++;
          }
        }

        len = newlen;
        o.Add(divide, pos);
        pos++;
      }
      while (newlen != 0);

      for (int i = 0; i < 13; i++) // copy to codeword in reverse, pad with 0's
      {
        var v = (--pos >= 0 ? o[i] : 0);
        codeword.Add(v, i);
      }

      Encode(codeword);

      var res = AccountMask;

      for (var i = 0; i < 17; i++)
      {
        res += Alphabet[codeword[Cwmap[i]]];

        if ((i & 3) == 3 && i < 13)
          res += '-';
      }

      return res;
    }
  }
}
