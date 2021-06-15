using EthereumLibrary.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoxKeys
{

  public class NoxManagedArray : IDisposable
  {
    byte[] nox_array;
    GCHandle handle;
    public NoxManagedArray() { }
    public NoxManagedArray(byte[] array)
    {
      nox_array = array;
      handle = GCHandle.Alloc(nox_array, GCHandleType.Pinned);
    }
    public NoxManagedArray(int Size)
    {
      nox_array = new byte[Size];
      handle = GCHandle.Alloc(nox_array, GCHandleType.Pinned);
    }
    public NoxManagedArray(byte[] src, int count)
    {
      nox_array = new byte[count];
      handle = GCHandle.Alloc(nox_array, GCHandleType.Pinned);
      Buffer.BlockCopy(src, 0, nox_array, 0, count);
    }
    public NoxManagedArray(byte[] src, int srcOffset, int dstOffSet, int count)
    {
      nox_array = new byte[count];
      handle = GCHandle.Alloc(nox_array, GCHandleType.Pinned);
      Buffer.BlockCopy(src, srcOffset, nox_array, dstOffSet, count);
    }
    public byte[] Value
    {
      get
      {
        return nox_array;
      }
      set
      {
        nox_array = value;
        handle = GCHandle.Alloc(value, GCHandleType.Pinned);
      }
    }

    public void Dispose()
    {
      if (nox_array == null) return;

      try
      {

          nox_array.NoxWriteArray();
          handle.Free();


      }
      catch { }
    

  
    }

  }

  public static class NoxArrayExtensions
  {
    public static void NoxWriteArray(this byte[] array)
    {
      int WriteInt = 0;

      for (int i = 0; i < array.Length; i++)
      {
        array[i] = (byte)WriteInt;
        if (WriteInt == 0)
        {
          WriteInt = 1;
        }
        else
        {
          WriteInt = 0;
        }
      }

    }

    public static void NoxWriteArray(this uint[] array)
    {
      int WriteInt = 0;

      for (int i = 0; i < array.Length; i++)
      {
        array[i] = (byte)WriteInt;
        if (WriteInt == 0)
        {
          WriteInt = 1;
        }
        else
        {
          WriteInt = 0;
        }
      }

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



  }
}
