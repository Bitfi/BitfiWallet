using System;
using System.Linq;
using Org.BouncyCastle.Security;

namespace DagLibrary
{
  public static class Utils
  {
    public static string ByteArray2hex(byte[] ba)
    {
      return BitConverter.ToString(ba).Replace("-", "");
    }

    public static byte[] Hex2ByteArray(string hex)
    {
      return Enumerable.Range(0, hex.Length)
        .Where(x => x % 2 == 0)
        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
        .ToArray();
    }

    public static Int32 Utf8Length(string str)
    {
      return str.Length;
    }

    public static UInt64 BytesToUint64(byte[] bytes)
    {
      byte[] value = new byte[8];
      Array.Reverse(bytes);
      Array.Copy(bytes, value, 6);
      UInt64 result = BitConverter.ToUInt64(value, 0);
      return result;
    }

    public static byte[] GenRandom(UInt32 len)
    {
      var rand = new SecureRandom();
      var bytes = new byte[len];
      rand.NextBytes(bytes);
      return bytes;
    }

    public static string removeDecimals(decimal amount)
    {
      var tmp = amount.ToString();
      int index = tmp.IndexOf(".");
      return index >= 0 ? tmp.Substring(0, index) : tmp;
    }

    public static byte[] Utf8Length(UInt32 value)
    {
      var buffer = new byte[] { };
      var position = 0;

      if (value >> 6 == 0)
      {
        buffer = new byte[1];
        buffer[position] = (byte)(value | 0x80); // Set bit 8.
      }
      else if (value >> 13 == 0)
      {
        buffer = new byte[2];
        buffer[position] = (byte)(value | 0x40 | 0x80); // Set bit 7 and 8.
        position++;
        buffer[position] = (byte)(value >> 6);
      }
      else if (value >> 20 == 0)
      {
        buffer = new byte[3];
        buffer[position] = (byte)(value | 0x40 | 0x80); // Set bit 7 and 8.
        position++;
        buffer[position] = (byte)((value >> 6) | 0x80); // Set bit 8.
        position++;
        buffer[position] = (byte)(value >> 13);
      }
      else if (value >> 27 == 0)
      {
        buffer = new byte[4];
        buffer[position] = (byte)(value | 0x40 | 0x80); // Set bit 7 and 8.
        position++;
        buffer[position] = (byte)((value >> 6) | 0x80); // Set bit 8.
        position++;
        buffer[position] = (byte)((value >> 13) | 0x80); // Set bit 8.
        position++;
        buffer[position] = (byte)((value >> 20) | 0x80); // Set bit 8.
        position++;
        buffer[position] = (byte)(value >> 27);
      }
      else
      {
        throw new Exception("UTF8 length is too big");
      }

      return buffer;
    }
  }
}