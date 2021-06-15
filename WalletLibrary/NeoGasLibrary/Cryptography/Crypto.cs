using NeoGasLibrary.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NeoGasLibrary.Cryptography
{
  public static class CryptoHelper
  {
    private static ThreadLocal<SHA256> _sha256 = new ThreadLocal<SHA256>(() => SHA256.Create());
    private static ThreadLocal<RIPEMD160Managed> _ripemd160 = new ThreadLocal<RIPEMD160Managed>(() => new RIPEMD160Managed());

    public static UInt160 ToScriptHash(this byte[] script)
    {
      return new UInt160(Crypto.Default.Hash160(script));
    }

    public static string ToAddress(this UInt160 scriptHash)
    {
      byte[] data = new byte[21];
      data[0] = 23;
      Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
      return data.Base58CheckEncode();
    }

    public static byte[] AES256Decrypt(this byte[] block, byte[] key)
    {
      using (Aes aes = Aes.Create())
      {
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        using (ICryptoTransform decryptor = aes.CreateDecryptor())
        {
          return decryptor.TransformFinalBlock(block, 0, block.Length);
        }
      }
    }

    public static byte[] AES256Encrypt(this byte[] block, byte[] key)
    {
      using (Aes aes = Aes.Create())
      {
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        using (ICryptoTransform encryptor = aes.CreateEncryptor())
        {
          return encryptor.TransformFinalBlock(block, 0, block.Length);
        }
      }
    }

    public static byte[] Sha256(this IEnumerable<byte> value)
    {
      return _sha256.Value.ComputeHash(value.ToArray());
    }

    public static byte[] Sha256(this byte[] value, int offset, int count)
    {
      return _sha256.Value.ComputeHash(value, offset, count);
    }

    public static byte[] AesDecrypt(this byte[] data, byte[] key, byte[] iv)
    {
      if (data == null || key == null || iv == null) throw new ArgumentNullException();
      if (data.Length % 16 != 0 || key.Length != 32 || iv.Length != 16) throw new ArgumentException();
      using (Aes aes = Aes.Create())
      {
        aes.Padding = PaddingMode.None;
        using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
        {
          return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
      }
    }

    public static byte[] AesEncrypt(this byte[] data, byte[] key, byte[] iv)
    {
      if (data == null || key == null || iv == null) throw new ArgumentNullException();
      if (data.Length % 16 != 0 || key.Length != 32 || iv.Length != 16) throw new ArgumentException();
      using (Aes aes = Aes.Create())
      {
        aes.Padding = PaddingMode.None;
        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
        {
          return encryptor.TransformFinalBlock(data, 0, data.Length);
        }
      }
    }

    public static byte[] ToAesKey(this string password)
    {
      using (SHA256 sha256 = SHA256.Create())
      {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] passwordHash = sha256.ComputeHash(passwordBytes);
        byte[] passwordHash2 = sha256.ComputeHash(passwordHash);
        Array.Clear(passwordBytes, 0, passwordBytes.Length);
        Array.Clear(passwordHash, 0, passwordHash.Length);
        return passwordHash2;
      }
    }

    public static byte[] Base58CheckDecode(this string input)
    {
      byte[] buffer = Base58.Decode(input);
      if (buffer.Length < 4) throw new FormatException();
      byte[] checksum = buffer.Sha256(0, buffer.Length - 4).Sha256();
      if (!buffer.Skip(buffer.Length - 4).SequenceEqual(checksum.Take(4)))
        throw new FormatException();
      return buffer.Take(buffer.Length - 4).ToArray();
    }

    public static string Base58CheckEncode(this byte[] data)
    {
      byte[] checksum = data.Sha256().Sha256();
      byte[] buffer = new byte[data.Length + 4];
      Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
      Buffer.BlockCopy(checksum, 0, buffer, data.Length, 4);
      return Base58.Encode(buffer);
    }

    public static byte[] RIPEMD160(this IEnumerable<byte> value)
    {
      return _ripemd160.Value.ComputeHash(value.ToArray());
    }

  }

  public class Crypto
  {
    public static readonly Crypto Default = new Crypto();

    public byte[] Hash160(byte[] message)
    {
      return message.Sha256().RIPEMD160();
    }

    public byte[] Hash256(byte[] message)
    {
      return message.Sha256().Sha256();
    }

    public byte[] Sign(byte[] message, byte[] prikey, byte[] pubkey)
    {
      X9ECParameters ecParams = NistNamedCurves.GetByName("P-256");
      ECDomainParameters domainParameters = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N,
        ecParams.H, ecParams.GetSeed());
      ECPrivateKeyParameters privKeyParam = new ECPrivateKeyParameters(
          new BigInteger(1, prikey), // d
          domainParameters);
      ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
      signer.Init(true, privKeyParam);
      signer.BlockUpdate(message, 0, message.Length);

      //https://crypto.stackexchange.com/questions/1795/how-can-i-convert-a-der-ecdsa-signature-to-asn-1
      var signature = signer.GenerateSignature();
      var resSignature = new byte[64];
      int i = signature[3] == 33 ? 5 : 4;
      Array.Copy(signature, i, resSignature, 0, 32);
      i += 32 + 1;
      i = signature[i] == 33 ? i + 2 : i + 1;
      Array.Copy(signature, i, resSignature, 32, 32);
      return resSignature;
    }

    public bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey)
    {
      if (pubkey.Length == 33 && (pubkey[0] == 0x02 || pubkey[0] == 0x03))
      {
        try
        {
          pubkey = ECPoint.DecodePoint(pubkey, ECCurve.Secp256r1).EncodePoint(false).Skip(1).ToArray();
        }
        catch
        {
          return false;
        }
      }
      else if (pubkey.Length == 65 && pubkey[0] == 0x04)
      {
        pubkey = pubkey.Skip(1).ToArray();
      }
      else if (pubkey.Length != 64)
      {
        throw new ArgumentException();
      }

      BigInteger x = new BigInteger(1, pubkey.Take(32).ToArray());
      BigInteger y = new BigInteger(1, pubkey.Skip(32).ToArray());

      X9ECParameters ecParams = NistNamedCurves.GetByName("P-256");
      ECDomainParameters domainParameters = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N,
        ecParams.H, ecParams.GetSeed());
      var G = ecParams.G;
      Org.BouncyCastle.Math.EC.ECCurve curve = ecParams.Curve;
      Org.BouncyCastle.Math.EC.ECPoint q = curve.CreatePoint(x, y);

      ECPublicKeyParameters pubkeyParam = new ECPublicKeyParameters(q, domainParameters);

      var verifier = SignerUtilities.GetSigner("SHA-256withECDSA");
      
      verifier.Init(false, pubkeyParam);
      verifier.BlockUpdate(message, 0, message.Length);
      // expected format is SEQUENCE {INTEGER r, INTEGER s}
      var derSignature = new DerSequence(
          // first 32 bytes is "r" number
          new DerInteger(new BigInteger(1, signature.Take(32).ToArray())),
          // last 32 bytes is "s" number
          new DerInteger(new BigInteger(1, signature.Skip(32).ToArray())))
          .GetDerEncoded();

      ///old verify method
      ///
      /*
      const int ECDSA_PUBLIC_P256_MAGIC = 0x31534345;
      pubkey = BitConverter.GetBytes(ECDSA_PUBLIC_P256_MAGIC).Concat(BitConverter.GetBytes(32)).Concat(pubkey).ToArray();
      using (CngKey key = CngKey.Import(pubkey, CngKeyBlobFormat.EccPublicBlob))
      using (ECDsaCng ecdsa = new ECDsaCng(key))
      {
        var result = ecdsa.VerifyData(message, signature, HashAlgorithmName.SHA256);
      }
      */
      ///////////////////
      return verifier.VerifySignature(derSignature);
    }
  }
}
