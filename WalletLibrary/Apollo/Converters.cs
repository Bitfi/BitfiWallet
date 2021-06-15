using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Apollo
{
  public struct WordArray
  {
    public UInt32[] Words { get; set; }
    public int SigBytes { get; set; }
  }

  public static class Converters
  {
    public class DeserializedTx
    {
      public string amount { get; set; }
      public string fee { get; set; }
      public string to { get; set; }
    }

    public static DeserializedTx deserialize(this String serialized)
    {
      var amount = serialized.Substring(80 + 2 * 8, 2 * 8).ToDecimalStringFromHex();
      var fee = serialized.Substring(80 + 2 * 2 * 8, 2 * 8).ToDecimalStringFromHex();
      var recipient = serialized.Substring(80, 2 * 8).ToDecimalStringFromHex();
      var to = AplAddress.NumericToRSAccountFormat(recipient.ToString());

      return new DeserializedTx
      {
        amount = amount,
        fee = fee,
        to = to
      };
    }

    public static string ToDecimalStringFromHex(this String hex)
    {
      var asd = "0" + hex.ReverseHex();
      var res = BigInteger.Parse("0" + hex.ReverseHex(), NumberStyles.HexNumber).ToString();
      return BigInteger.Parse("0" + hex.ReverseHex(), NumberStyles.HexNumber).ToString();
    }

    public static string ReverseHex(this String hex)
    {
      if (hex.Length % 2 != 0)
        throw new Exception("Hex is in invalid format");

      var res = "";

      for (int i = hex.Length - 2; i >= 0; i -= 2)
      {
        res += hex.Substring(i, 2);
      }

      var j = 0;
      while (res[j] == '0')
      {
        ++j;
      }

      return res.Substring(j);
    }

    public static byte[] HexToByteArray(this String hex)
    {
      int NumberChars = hex.Length;
      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }

    public static string ByteArrayToHex(this byte[] ba)
    {
      StringBuilder hex = new StringBuilder(ba.Length * 2);
      foreach (byte b in ba)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }

    public static byte[] StringToByteArray(this string str)
    {
      return Encoding.UTF8.GetBytes(str);
    }

    public static WordArray ByteArrayToWordArray(byte[] byteArray)
    {
      int i = 0;
      int offset = 0;
      UInt32 word = 0;
      int len = byteArray.Length;

      var wordsLen = ((len / 4) | 0) + (len % 4 == 0 ? 0 : 1);
      var words = new UInt32[wordsLen];

      while (i < (len - (len % 4)))
      {
        words[offset++] =
          ((UInt32)byteArray[i++] << 24) |
          ((UInt32)byteArray[i++] << 16) |
          ((UInt32)byteArray[i++] << 8) |
          ((UInt32)byteArray[i++]);
      }
      if (len % 4 != 0)
      {
        word = (UInt32)byteArray[i++] << 24;
        if (len % 4 > 1)
        {
          word = word | (UInt32)byteArray[i++] << 16;
        }
        if (len % 4 > 2)
        {
          word = word | (UInt32)byteArray[i++] << 8;
        }
        words[offset] = word;
      }
      var wordArray = new WordArray();
      wordArray.SigBytes = len;
      wordArray.Words = words;

      return wordArray;
    }

    public static byte[] WordArrayToByteArray(WordArray wordArray, bool isFirstByteHasSign = true)
    {
      var len = wordArray.Words.Length;
      if (len == 0)
      {
        return new byte[] { };
      }

      var byteArray = new byte[wordArray.SigBytes];
      int offset = 0;
      UInt32 word = 0;
      int i = 0;

      for (i = 0; i < len - 1; i++)
      {
        word = wordArray.Words[i];
        byteArray[offset++] = (byte)(isFirstByteHasSign ? word >> 24 : (word >> 24) & 0xff);
        byteArray[offset++] = (byte)((word >> 16) & 0xff);
        byteArray[offset++] = (byte)((word >> 8) & 0xff);
        byteArray[offset++] = (byte)(word & 0xff);
      }
      word = wordArray.Words[len - 1];
      byteArray[offset++] = (byte)(isFirstByteHasSign ? word >> 24 : (word >> 24) & 0xff);
      if (wordArray.SigBytes % 4 == 0)
      {
        byteArray[offset++] = (byte)((word >> 16) & 0xff);
        byteArray[offset++] = (byte)((word >> 8) & 0xff);
        byteArray[offset++] = (byte)(word & 0xff);
      }
      if (wordArray.SigBytes % 4 > 1)
      {
        byteArray[offset++] = (byte)((word >> 16) & 0xff);
      }
      if (wordArray.SigBytes % 4 > 2)
      {
        byteArray[offset++] = (byte)((word >> 8) & 0xff);
      }
      return byteArray;
    }
  }
}
