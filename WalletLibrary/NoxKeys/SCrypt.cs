using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace NoxKeys
{
  internal struct UintPoint
  {
    public uint[] array;
    public int len { get; set; }

    public void Dispose()
    {
      if (array == null) return;

      array.NoxWriteArray();
    }
  }
  public static class SCrypt
  {
    public static async Task<NoxManagedArray> ComputeDerivedKey(NoxManagedArray key, NoxManagedArray salt, int cost, int blockSize, int parallel, int maxThreads, int derivedKeyLength)
    {
      using (Pbkdf2 kdf = await GetStream(key, salt, cost, blockSize, parallel, maxThreads))
      {
        return kdf.Read(derivedKeyLength);

      }
    }

    private static async Task<NoxManagedArray> GetEffectivePbkdf2Salt(NoxManagedArray key, NoxManagedArray salt, int cost, int blockSize, int parallel, int maxThreads)
    {
      return await MFcrypt(key, salt, cost, blockSize, parallel, maxThreads);
    }

    private static async Task<Pbkdf2> GetStream(NoxManagedArray key, NoxManagedArray salt, int cost, int blockSize, int parallel, int maxThreads)
    {
      var B = await GetEffectivePbkdf2Salt(key, salt, cost, blockSize, parallel, maxThreads);
      Pbkdf2 kdf = new Pbkdf2(new HMACSHA256(key.Value), B, 1);
      B.Dispose();
      return kdf;
    }


    static async Task<NoxManagedArray> MFcrypt(NoxManagedArray P, NoxManagedArray S, int cost, int blockSize, int parallel, int maxThreads)
    {
      List<GCHandle> handles = new List<GCHandle>();

      int MFLen = blockSize * 128;
      if (maxThreads == null)
      {
        maxThreads = int.MaxValue;
      }

      NoxManagedArray B = Pbkdf2.ComputeDerivedKey(new HMACSHA256(P.Value), S, 1, parallel * MFLen);

      uint[] sB0 = new uint[B.Value.Length / 4];
      handles.Add(GCHandle.Alloc(sB0, GCHandleType.Pinned));

      UintPoint B0 = new UintPoint() { array = sB0, len = (B.Value.Length / 4) };

      for (int i = 0; i < B0.len; i++)
      {
        B0.array[i] = BitPacking.UInt32FromLEBytes(B.Value, i * 4);
      }

      await ThreadSMixCalls(B0, MFLen, cost, blockSize, parallel, maxThreads);

      for (int i = 0; i < B0.len; i++)
      {
        BitPacking.LEBytesFromUInt32(B0.array[i], B.Value, i * 4);
      }

      B0.Dispose();
      foreach (var handle in handles) handle.Free();
      return B;

    }

    static async Task ThreadSMixCalls(UintPoint B0, int MFLen, int cost, int blockSize, int parallel, int maxThreads)
    {

      int current = 0;

      List<Task> tList = new List<Task>();

      for (int i = 0; i < maxThreads; i++)
      {

        var ut = Task.Run(async () =>
        {

          while (true)
          {

          int j = Interlocked.Increment(ref current) - 1;
          if (j >= parallel)
          {
            break;
          }

          SMix(B0, j * MFLen / 4, B0, j * MFLen / 4, (uint)cost, blockSize);

          }

        });

        tList.Add(ut);

      }

      await Task.WhenAll(tList);

    }

    static void xxxxThreadSMixCalls(UintPoint B0, int MFLen, int cost, int blockSize, int parallel, int maxThreads)
    {
      int current = 0;

      ThreadStart workerThread = delegate ()
      {
        while (true)
        {
          int j = Interlocked.Increment(ref current) - 1;
          if (j >= parallel)
          {
            break;
          }

          SMix(B0, j * MFLen / 4, B0, j * MFLen / 4, (uint)cost, blockSize);
        }
      };

      int threadCount = maxThreads;

      Thread[] threads = new Thread[threadCount - 1];
      for (int i = 0; i < threads.Length; i++)
      {
        (threads[i] = new Thread(workerThread, 8192)).Start();

      }

      for (int i = 0; i < threads.Length; i++)
      {
        threads[i].Join();
      }

    }

    static bool IsBusy(Task[] threads)
    {
      for (int i = 0; i < threads.Length; i++)
      {
        if (!threads[i].IsCompleted) return true;
      }

      return false;
    }

    static void SMix(UintPoint B, int Boffset, UintPoint Bp, int Bpoffset, uint N, int r)
    {
      List<GCHandle> handles = new List<GCHandle>();

      uint Nmask = N - 1;
      int Bs = 16 * 2 * r;

      uint[] sr1 = new uint[16];
      handles.Add(GCHandle.Alloc(sr1, GCHandleType.Pinned));
      UintPoint scratch1 = new UintPoint() { array = sr1, len = 16 };

      uint[] srX = new uint[16];
      handles.Add(GCHandle.Alloc(srX, GCHandleType.Pinned));
      UintPoint scratchX = new UintPoint() { array = srX, len = 16 };

      uint[] srY = new uint[Bs];
      handles.Add(GCHandle.Alloc(srY, GCHandleType.Pinned));
      UintPoint scratchY = new UintPoint() { array = srY, len = Bs };

      uint[] srZ = new uint[Bs];
      handles.Add(GCHandle.Alloc(srZ, GCHandleType.Pinned));
      UintPoint scratchZ = new UintPoint() { array = srZ, len = Bs };

      uint[] sx = new uint[Bs];
      handles.Add(GCHandle.Alloc(sx, GCHandleType.Pinned));
      UintPoint x = new UintPoint() { array = sx, len = Bs };

      uint[][] v = new uint[N][];


      for (int i = 0; i < v.Length; i++)
      {
        v[i] = new uint[Bs];
        var handle = GCHandle.Alloc(v[i], GCHandleType.Pinned);
        handles.Add(handle);
      }

      UCopy(B, Boffset, x, 0, Bs);

      for (uint i = 0; i < N; i++)
      {

        SCopy(x, 0, v[i], 0, Bs);

        BlockMix(x, 0, x, 0, scratchX, scratchY, scratch1, r);

      }

      for (uint i = 0; i < N; i++)
      {
        uint j = x.array[Bs - 16] & Nmask;
        for (int k = 0; k < scratchZ.len; k++)
        {

          scratchZ.array[k] = x.array[k] ^ v[j][k];
        }

        BlockMix(scratchZ, 0, x, 0, scratchX, scratchY, scratch1, r);
      }


      UCopy(x, 0, Bp, Bpoffset, Bs);


      Task.Run(() =>
      {
        for (int i = 0; i < N; i++)
        {
          uint[] clear = new uint[v[i].Length];
          ACopy(clear, 0, v[i], 0, clear.Length);
        }

        x.Dispose();
        scratch1.Dispose();
        scratchX.Dispose();
        scratchY.Dispose();
        scratchZ.Dispose();


        foreach (var handle in handles) handle.Free();
      });
    

    }

    static void BlockMix(UintPoint B, int Boffset, UintPoint Bp, int Bpoffset, UintPoint x, UintPoint y, UintPoint scratch, int r)
    {
      int k = Boffset, m = 0, n = 16 * r;

      UCopy(B, (2 * r - 1) * 16, x, 0, 16);

      for (int i = 0; i < r; i++)
      {
        for (int j = 0; j < scratch.len; j++)
        {
          scratch.array[j] = x.array[j] ^ B.array[j + k];
        }

        Salsa20Core.Compute2(8, scratch.array, 0, x.array, 0);


        UCopy(x, 0, y, m, 16);

        k += 16;

        for (int j = 0; j < scratch.len; j++)
        {
          scratch.array[j] = x.array[j] ^ B.array[j + k];
        }

        Salsa20Core.Compute2(8, scratch.array, 0, x.array, 0);

        UCopy(x, 0, y, m + n, 16);
        k += 16;

        m += 16;
      }


      UCopy(y, 0, Bp, Boffset, y.len);
    }

    static void SCopy(UintPoint srcArray, int srcOffset, uint[] destArray, int destOffset, int copyLength)
    {

      int cnt = 0;
      for (int i = srcOffset; i < srcArray.len; i++)
      {

        if (copyLength > cnt)
        {
          int dpos = (i - srcOffset) + destOffset;
          destArray[dpos] = srcArray.array[i];
          cnt++;
        }
      }
    }

    static void UCopy(UintPoint srcArray, int srcOffset, UintPoint destArray, int destOffset, int copyLength)
    {


      int cnt = 0;
      for (int i = srcOffset; i < srcArray.len; i++)
      {

        if (copyLength > cnt)
        {
          int dpos = (i - srcOffset) + destOffset;
          destArray.array[dpos] = srcArray.array[i];
          cnt++;
        }
      }
    }

    static void ACopy(uint[] srcArray, int srcOffset, uint[] destArray, int destOffset, int copyLength)
    {
      int cnt = 0;
      for (int i = srcOffset; i < srcArray.Length; i++)
      {

        if (copyLength > cnt)
        {
          int dpos = (i - srcOffset) + destOffset;
          destArray[dpos] = srcArray[i];
          cnt++;
        }
      }
    }

  }

  public class Pbkdf2 : Stream
  {
    NoxManagedArray _saltBuffer, _digest, _digestT1;
    KeyedHashAlgorithm _hmacAlgorithm;
    int _iterations;

    public Pbkdf2(KeyedHashAlgorithm hmacAlgorithm, NoxManagedArray salt, int iterations)
    {
      int hmacLength = hmacAlgorithm.HashSize / 8;
      _saltBuffer = new NoxManagedArray(salt.Value.Length + 4);

      ACopy(salt.Value, 0, _saltBuffer.Value, 0, salt.Value.Length);

      _iterations = iterations; _hmacAlgorithm = hmacAlgorithm;

      _digest = new NoxManagedArray(hmacLength);

      _digestT1 = new NoxManagedArray(hmacLength);
    }

    public NoxManagedArray Read(int count)
    {

      NoxManagedArray buffer = new NoxManagedArray(count);
      int bytes = Read(buffer.Value, 0, count);

      return buffer;
    }
    public static NoxManagedArray ComputeDerivedKey(KeyedHashAlgorithm hmacAlgorithm, NoxManagedArray salt, int iterations, int derivedKeyLength)
    {
      using (Pbkdf2 kdf = new Pbkdf2(hmacAlgorithm, salt, iterations))
      {
        return kdf.Read(derivedKeyLength);
      }
    }
    protected override void Dispose(bool disposing)
    {
      _saltBuffer.Dispose();
      _digest.Dispose();
      _digestT1.Dispose();

      DisposeHmac();
    }
    private void DisposeHmac()
    {

      _hmacAlgorithm.Clear();

    }

    void ComputeBlock(uint pos)
    {
      BitPacking.BEBytesFromUInt32(pos, _saltBuffer.Value, _saltBuffer.Value.Length - 4);
      ComputeHmac(_saltBuffer.Value, _digestT1.Value);
      ACopy(_digestT1.Value, 0, _digest.Value, 0, _digestT1.Value.Length);

      for (int i = 1; i < _iterations; i++)
      {
        ComputeHmac(_digestT1.Value, _digestT1.Value);
        for (int j = 0; j < _digest.Value.Length; j++)
        {
          _digest.Value[j] ^= _digestT1.Value[j];
        }
      }

      byte[] clear = new byte[_digestT1.Value.Length];
      ACopy(clear, 0, _digestT1.Value, 0, clear.Length);


    }
    void ComputeHmac(byte[] input, byte[] output)
    {
      _hmacAlgorithm.Initialize();
      _hmacAlgorithm.TransformBlock(input, 0, input.Length, input, 0);
      _hmacAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
      ACopy(_hmacAlgorithm.Hash, 0, output, 0, output.Length);
    }


    static void ACopy(byte[] srcArray, int srcOffset, byte[] destArray, int destOffset, int copyLength)
    {
      int cnt = 0;
      for (int i = srcOffset; i < srcArray.Length; i++)
      {

        if (copyLength > cnt)
        {
          int dpos = (i - srcOffset) + destOffset;
          destArray[dpos] = srcArray[i];
          cnt++;
        }
      }
    }


    long _blockStart, _blockEnd, _pos;

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int bytes = 0;

      while (count > 0)
      {
        if (Position < _blockStart || Position >= _blockEnd)
        {
          if (Position >= Length)
          {
            break;
          }

          long pos = Position / _digest.Value.Length;
          ComputeBlock((uint)(pos + 1));
          _blockStart = pos * _digest.Value.Length;
          _blockEnd = _blockStart + _digest.Value.Length;
        }

        int bytesSoFar = (int)(Position - _blockStart);
        int bytesThisTime = (int)Math.Min(_digest.Value.Length - bytesSoFar, count);
        ACopy(_digest.Value, bytesSoFar, buffer, bytes, bytesThisTime);
        count -= bytesThisTime;
        bytes += bytesThisTime;
        Position += bytesThisTime;
      }

      return bytes;
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
      long pos;

      switch (origin)
      {
        case SeekOrigin.Begin:
          pos = offset;
          break;
        case SeekOrigin.Current:
          pos = Position + offset;
          break;
        case SeekOrigin.End:
          pos = Length + offset;
          break;
        default:
          throw new Exception("origin, Unknown seek type.");
      }

      if (pos < 0)
      {
        throw new Exception("offset, Can't seek before the stream start.");
      }
      Position = pos;
      return pos;
    }

    public override void SetLength(long value)
    {
      throw new Exception("not supported");
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new Exception("not supported");
    }
    public override bool CanRead
    {
      get
      {
        return true;
      }
    }
    public override bool CanSeek
    {
      get
      {
        return true;
      }
    }
    public override bool CanWrite
    {
      get
      {
        return false;
      }
    }
    public override long Length
    {
      get
      {
        return (long)_digest.Value.Length * uint.MaxValue;
      }
    }
    public override long Position
    {
      get
      {
        return _pos;
      }
      set
      {
        if (_pos < 0)
        {
          throw new Exception("Can't seek before the stream start.");
        }
        _pos = value;
      }
    }
  }


  public static class BitPacking
  {


    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //  unsafe internal static ulong ToUInt64(this byte[] value, int startIndex)
    //  {
    //      fixed (byte* pbyte = &value[startIndex])
    //      {
    //          return *((ulong*)pbyte);
    //     }
    // }


    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe public static uint UInt32FromLEBytes(byte[] value, int startIndex)
    {
      fixed (byte* pbyte = &value[startIndex])
      {
        return *((uint*)pbyte);
      }
    }

    //  public static uint UInt24FromLEBytes(byte[] bytes, int offset)
    //   {
    //       return
    //           (uint)bytes[offset + 2] << 16 |
    //           (uint)bytes[offset + 1] << 8 |
    //           (uint)bytes[offset + 0];
    //  }

    //     public static uint UInt32FromLEBytes(byte[] bytes, int offset)
    //    {
    //      return
    ////           (uint)bytes[offset + 3] << 24 |
    //         UInt24FromLEBytes(bytes, offset);
    //}

    public static void BEBytesFromUInt32(uint value, byte[] bytes, int offset)
    {
      bytes[offset + 0] = (byte)(value >> 24);
      bytes[offset + 1] = (byte)(value >> 16);
      bytes[offset + 2] = (byte)(value >> 8);
      bytes[offset + 3] = (byte)(value);
    }

    public static void LEBytesFromUInt24(uint value, byte[] bytes, int offset)
    {
      bytes[offset + 2] = (byte)(value >> 16);
      bytes[offset + 1] = (byte)(value >> 8);
      bytes[offset + 0] = (byte)(value);
    }

    public static void LEBytesFromUInt32(uint value, byte[] bytes, int offset)
    {
      bytes[offset + 3] = (byte)(value >> 24);
      LEBytesFromUInt24(value, bytes, offset);
    }
  }


  public static class Salsa20Core
  {
    static uint R(uint a, int b)
    {
      return (a << b) | (a >> (32 - b));
    }
    public static void Compute2(int rounds, uint[] input, int inputOffset, uint[] output, int outputOffset)
    {

      uint x0 = input[inputOffset + 0];
      uint x1 = input[inputOffset + 1];
      uint x2 = input[inputOffset + 2];
      uint x3 = input[inputOffset + 3];
      uint x4 = input[inputOffset + 4];
      uint x5 = input[inputOffset + 5];
      uint x6 = input[inputOffset + 6];
      uint x7 = input[inputOffset + 7];
      uint x8 = input[inputOffset + 8];
      uint x9 = input[inputOffset + 9];
      uint x10 = input[inputOffset + 10];
      uint x11 = input[inputOffset + 11];
      uint x12 = input[inputOffset + 12];
      uint x13 = input[inputOffset + 13];
      uint x14 = input[inputOffset + 14];
      uint x15 = input[inputOffset + 15];

      for (int i = rounds; i > 0; i -= 2)
      {
        x4 ^= R(x0 + x12, 7);
        x8 ^= R(x4 + x0, 9);
        x12 ^= R(x8 + x4, 13);
        x0 ^= R(x12 + x8, 18);
        x9 ^= R(x5 + x1, 7);
        x13 ^= R(x9 + x5, 9);
        x1 ^= R(x13 + x9, 13);
        x5 ^= R(x1 + x13, 18);
        x14 ^= R(x10 + x6, 7);
        x2 ^= R(x14 + x10, 9);
        x6 ^= R(x2 + x14, 13);
        x10 ^= R(x6 + x2, 18);
        x3 ^= R(x15 + x11, 7);
        x7 ^= R(x3 + x15, 9);
        x11 ^= R(x7 + x3, 13);
        x15 ^= R(x11 + x7, 18);
        x1 ^= R(x0 + x3, 7);
        x2 ^= R(x1 + x0, 9);
        x3 ^= R(x2 + x1, 13);
        x0 ^= R(x3 + x2, 18);
        x6 ^= R(x5 + x4, 7);
        x7 ^= R(x6 + x5, 9);
        x4 ^= R(x7 + x6, 13);
        x5 ^= R(x4 + x7, 18);
        x11 ^= R(x10 + x9, 7);
        x8 ^= R(x11 + x10, 9);
        x9 ^= R(x8 + x11, 13);
        x10 ^= R(x9 + x8, 18);
        x12 ^= R(x15 + x14, 7);
        x13 ^= R(x12 + x15, 9);
        x14 ^= R(x13 + x12, 13);
        x15 ^= R(x14 + x13, 18);
      }

      output[outputOffset + 0] = input[inputOffset + 0] + x0;
      x0 = 0;
      output[outputOffset + 1] = input[inputOffset + 1] + x1;
      x1 = 0;
      output[outputOffset + 2] = input[inputOffset + 2] + x2;
      x2 = 0;
      output[outputOffset + 3] = input[inputOffset + 3] + x3;
      x3 = 0;
      output[outputOffset + 4] = input[inputOffset + 4] + x4;
      x4 = 0;
      output[outputOffset + 5] = input[inputOffset + 5] + x5;
      x5 = 0;
      output[outputOffset + 6] = input[inputOffset + 6] + x6;
      x6 = 0;
      output[outputOffset + 7] = input[inputOffset + 7] + x7;
      x7 = 0;
      output[outputOffset + 8] = input[inputOffset + 8] + x8;
      x8 = 0;
      output[outputOffset + 9] = input[inputOffset + 9] + x9;
      x9 = 0;
      output[outputOffset + 10] = input[inputOffset + 10] + x10;
      x10 = 0;
      output[outputOffset + 11] = input[inputOffset + 11] + x11;
      x11 = 0;
      output[outputOffset + 12] = input[inputOffset + 12] + x12;
      x12 = 0;
      output[outputOffset + 13] = input[inputOffset + 13] + x13;
      x13 = 0;
      output[outputOffset + 14] = input[inputOffset + 14] + x14;
      x14 = 0;
      output[outputOffset + 15] = input[inputOffset + 15] + x15;
      x15 = 0;

    }

  }


}

