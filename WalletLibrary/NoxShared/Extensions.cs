using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.NoxShared
{

  public static class ListExtensions
  {

    public static string[] ListAddString(this string[] array, string value)
    {
 
      if (array == null) array = new string[0];

      if (string.IsNullOrEmpty(value)) return array;

      string[] destArray = new string[array.Length + 1];

      for (int i = 0; i < array.Length; i++)
      {
        destArray[i] = array[i].ToLower();
      }


      destArray[destArray.Length - 1] = value;

      return destArray;

    }
    public static bool ListContains(this string[] array, string value)
    {
    
      if (string.IsNullOrEmpty(value)) return true;

      for (int i = 0; i < array.Length; i++)
      {
        if (string.IsNullOrEmpty(array[i]))
        {
          return false;
        }

        if (array[i].Equals(value, StringComparison.CurrentCultureIgnoreCase))
        {
          return true;
        }
      }

      return false;
    }

    public static int ListContainsCount(this string[] array, string value)
    {

      int cnt = 0;

      if (string.IsNullOrEmpty(value)) return cnt;

      for (int i = 0; i < array.Length; i++)
      {
        if (string.IsNullOrEmpty(array[i]))
        {
          return cnt;
        }

        if (array[i].Equals(value, StringComparison.CurrentCultureIgnoreCase))
        {
          cnt++;
        }
      }

      return cnt;
    }

    public static string[] ListDistinct(this string[] array)
    {
      string[] newarray = new string[500];

      int cnt = 0;
      for (int i = 0; i < array.Length; i++)
      {

        if (!newarray.ListContains(array[i]))
        {
          newarray[cnt] = array[i];

          cnt++;
        }
      }

      int len = GetActualLength(newarray);
      return ACopy(newarray, len);
    }

    static int GetActualLength(string[] array)
    {
      int Length = 0;
      for (int i = 0; i < array.Length; i++)
      {
        if (!string.IsNullOrEmpty(array[i]))
        {
          Length++;
        }
        else
        {
          break;
        }

      }

      return Length;
    }

    static string[] ACopy(string[] srcArray, int len)
    {
      string[] destArray = new string[len];

      for (int i = 0; i < len; i++)
      {
        destArray[i] = srcArray[i].ToLower();
      }

      return destArray;
    }

  }
  internal static class ByteUtil
  {
    public static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];
    public static readonly byte[] ZERO_BYTE_ARRAY = { 0 };

    public static byte[] SHA256(byte[] data)
    {
      return SHA256(data, 0, data.Length);
    }
    public static byte[] SHA256(byte[] data, int offset, int count)
    {
      using (var sha = new SHA256Managed())
      {
        return sha.ComputeHash(data, offset, count);
      }
    }
    public static byte[] SHA512(byte[] data)
    {
      return SHA512(data, 0, data.Length);
    }
    public static byte[] SHA512(byte[] data, int offset, int count)
    {
      using (var sha = new SHA512Managed())
      {
        return sha.ComputeHash(data, offset, count);
      }
    }
    public static byte[] HMACSHA512(byte[] key, byte[] data)
    {
      return new HMACSHA512(key).ComputeHash(data);
    }
    public static byte[] HMACSHA256(byte[] key, byte[] data)
    {
      return new HMACSHA256(key).ComputeHash(data);
    }
    public static bool ArrayEqual(byte[] a, byte[] b)
    {
      if (a == null && b == null)
        return true;
      if (a == null)
        return false;
      if (b == null)
        return false;
      return ArrayEqual(a, 0, b, 0, Math.Max(a.Length, b.Length));
    }
    public static bool ArrayEqual(byte[] a, int startA, byte[] b, int startB, int length)
    {
      if (a == null && b == null)
        return true;
      if (a == null)
        return false;
      if (b == null)
        return false;
      var alen = a.Length - startA;
      var blen = b.Length - startB;

      if (alen < length || blen < length)
        return false;

      for (int ai = startA, bi = startB; ai < startA + length; ai++, bi++)
      {
        if (a[ai] != b[bi])
          return false;
      }
      return true;
    }
    public static byte[] AppendByte(byte[] bytes, byte b)
    {
      var result = new byte[bytes.Length + 1];
      Array.Copy(bytes, result, bytes.Length);
      result[result.Length - 1] = b;
      return result;
    }
    public static byte[] Slice(this byte[] org,
        int start, int end = int.MaxValue)
    {
      if (end < 0)
        end = org.Length + end;
      start = Math.Max(0, start);
      end = Math.Max(start, end);

      return org.Skip(start).Take(end - start).ToArray();
    }
    public static byte[] InitialiseEmptyByteArray(int length)
    {
      var returnArray = new byte[length];
      for (var i = 0; i < length; i++)
        returnArray[i] = 0x00;
      return returnArray;
    }
    public static IEnumerable<byte> MergeToEnum(params byte[][] arrays)
    {
      foreach (var a in arrays)
        foreach (var b in a)
          yield return b;
    }
    public static byte[] Merge(params byte[][] arrays)
    {
      return MergeToEnum(arrays).ToArray();
    }
    public static byte[] XOR(this byte[] a, byte[] b)
    {
      var length = Math.Min(a.Length, b.Length);
      var result = new byte[length];
      for (var i = 0; i < length; i++)
        result[i] = (byte)(a[i] ^ b[i]);
      return result;
    }
  }

  internal static class ByteArrayExtensions
  {
    public static bool StartWith(this byte[] data, byte[] versionBytes)
    {
      if (data.Length < versionBytes.Length)
        return false;
      for (int i = 0; i < versionBytes.Length; i++)
      {
        if (data[i] != versionBytes[i])
          return false;
      }
      return true;
    }
    public static byte[] SafeSubarray(this byte[] array, int offset, int count)
    {
      if (array == null)
        throw new ArgumentNullException(nameof(array));
      if (offset < 0 || offset > array.Length)
        throw new ArgumentOutOfRangeException("offset");
      if (count < 0 || offset + count > array.Length)
        throw new ArgumentOutOfRangeException("count");
      if (offset == 0 && array.Length == count)
        return array;
      var data = new byte[count];
      Buffer.BlockCopy(array, offset, data, 0, count);
      return data;
    }
    public static byte[] SafeSubarray(this byte[] array, int offset)
    {
      if (array == null)
        throw new ArgumentNullException(nameof(array));
      if (offset < 0 || offset > array.Length)
        throw new ArgumentOutOfRangeException("offset");

      var count = array.Length - offset;
      var data = new byte[count];
      Buffer.BlockCopy(array, offset, data, 0, count);
      return data;
    }
    public static byte[] Concat(this byte[] arr, params byte[][] arrs)
    {
      var len = arr.Length + arrs.Sum(a => a.Length);
      var ret = new byte[len];
      Buffer.BlockCopy(arr, 0, ret, 0, arr.Length);
      var pos = arr.Length;
      foreach (var a in arrs)
      {
        Buffer.BlockCopy(a, 0, ret, pos, a.Length);
        pos += a.Length;
      }
      return ret;
    }

  }
 

  internal class AesWrapper
  {

    private Aes _inner;
    private ICryptoTransform _transformer;
    private AesWrapper(Aes aes)
    {
      _inner = aes;
    }

    internal static AesWrapper Create()
    {
      var aes = Aes.Create();
      return new AesWrapper(aes);
    }

    public byte[] Process(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      return _transformer.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
    }

    internal void Initialize(byte[] key, byte[] iv, bool forEncryption)
    {
      if (_transformer != null)
        return;
      _inner.IV = iv;
      _inner.KeySize = key.Length * 8;
      _inner.Key = key;
      _transformer = forEncryption ? _inner.CreateEncryptor() : _inner.CreateDecryptor();
    }
  }

  internal class AesBuilder
  {
    private byte[] _key;
    private bool? _forEncryption;

    private byte[] _iv = new byte[16];


    public AesBuilder SetKey(byte[] key)
    {
      _key = key;
      return this;
    }

    public AesBuilder IsUsedForEncryption(bool forEncryption)
    {
      _forEncryption = forEncryption;
      return this;
    }

    public AesBuilder SetIv(byte[] iv)
    {
      _iv = iv;
      return this;
    }

    public AesWrapper Build()
    {
      var aes = AesWrapper.Create();
      var encrypt = !_forEncryption.HasValue || _forEncryption.Value;
      aes.Initialize(_key, _iv, encrypt);
      return aes;
    }
  }
}
