﻿using System;
using System.IO;
using System.Linq;

namespace NeoGasLibrary.Cryptography
{
  public abstract class UIntBase : IEquatable<UIntBase>
  {
    private byte[] data_bytes;

    public int Size => data_bytes.Length;

    protected UIntBase(int bytes, byte[] value)
    {
      if (value == null)
      {
        this.data_bytes = new byte[bytes];
        return;
      }
      if (value.Length != bytes)
        throw new ArgumentException();
      this.data_bytes = value;
    }

    public bool Equals(UIntBase other)
    {
      if (ReferenceEquals(other, null))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (data_bytes.Length != other.data_bytes.Length)
        return false;
      return data_bytes.SequenceEqual(other.data_bytes);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(obj, null))
        return false;
      if (!(obj is UIntBase))
        return false;
      return this.Equals((UIntBase)obj);
    }


    public byte[] ToArray()
    {
      return data_bytes;
    }

    public override string ToString()
    {
      return "0x" + data_bytes.Reverse().ToHexString();
    }

    public static bool operator ==(UIntBase left, UIntBase right)
    {
      if (ReferenceEquals(left, right))
        return true;
      if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        return false;
      return left.Equals(right);
    }

    public static bool operator !=(UIntBase left, UIntBase right)
    {
      return !(left == right);
    }
  }
}
