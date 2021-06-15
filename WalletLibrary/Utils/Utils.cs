using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Numerics;

namespace NumUtils
{
  public static class RestHelper
  {
    public static string PostRequest(string url, JObject obj)
    {

      return null;
    }

    public static string GetRequest(string url)
    {

      return null;
    }
  }

  public static class Utils
  {
    public static BigInteger Exp10(this BigInteger b, Int32 n)
    {
      return BigInteger.Multiply(b, BigInteger.Pow(new BigInteger(10), n));
    }

    public static string FromSatoshi(BigInteger v, int decimals)
    {
      var f = TrimRight(FromSatoshiFull(v, decimals), '0');
      if (f[f.Length - 1] == '.')
      {
        return f.Substring(0, f.Length - 1);
      }
      return f;
    }
    
    private static string TrimRight(string str, char c)
    {
      while (str[str.Length - 1] == c)
        str = str.Substring(0, str.Length - 1);
      return str;
    }

    private static string PadLeft(string str, Int32 len, char c)
    {
      while (str.Length < len)
      {
        str = c + str;
      }
      return str;
    }

    public static bool AmountEquals(double value1, double value2, int units)
    {
      long lValue1 = BitConverter.DoubleToInt64Bits(value1);
      long lValue2 = BitConverter.DoubleToInt64Bits(value2);

      if ((lValue1 >> 63) != (lValue2 >> 63))
      {
        if (value1 == value2)
          return true;

        return false;
      }

      long diff = Math.Abs(lValue1 - lValue2);

      if (diff <= (long)units)
        return true;

      return false;
    }

    private static string FromSatoshiFull(BigInteger money, int decimals)
    {
      string units = money.ToString();
      bool isNeg = units[0] == '-';
      if (isNeg)
      {
        units = units.Substring(1);
      }
      string dec = "";
      if (units.Length >= decimals)
      {
        dec = units.Substring(units.Length - decimals, decimals);
      }
      else
      {
        dec = PadLeft(units, decimals, '0');
      }

      string whole = "0";
      if (units.Length - decimals > 0)
      {
        whole = units.Substring(0, units.Length - decimals);
      }
      string prefix = isNeg ? "-" : "";
      return prefix + whole + '.' + dec;
    }
    public static BigInteger ToSatoshi(string str, int decimals)
    {
      var coinUnits = BigInteger.Pow(new BigInteger(10), decimals);
      if (str == null)
      {
        return BigInteger.Zero;
      }

      bool negative = str[0] == '-';
      if (negative)
      {
        str = str.Substring(1);
      }
      int decimalIndex = str.ToList().IndexOf('.');
      if (decimalIndex == -1)
      {
        if (negative)
        {
          BigInteger val = BigInteger.Multiply(new BigInteger(Double.Parse(str)), coinUnits);
          return BigInteger.Negate(val);
        }
        return BigInteger.Multiply(new BigInteger(Double.Parse(str)), coinUnits);
      }
      if (decimalIndex + decimals + 1 < str.Length)
      {
        str = str.Substring(0, decimalIndex + decimals + 1);
      }
      BigInteger.TryParse(str.Substring(0, decimalIndex), out BigInteger d);
      BigInteger.TryParse(str.Substring(decimalIndex + 1), out BigInteger tmp);

      BigInteger v = BigInteger.Add(
          d.Exp10(decimals),
          tmp.Exp10(decimalIndex + decimals - str.Length + 1)
      );

      return negative ? BigInteger.Negate(v) : v;
    }
  }
}