using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace DagLibrary
{
  public static class Account
  {

    public static string GetDagAddressFromUncompressedPublicKey(string uncompressedPk)
    {
      var PKCS_PREFIX = "3056301006072a8648ce3d020106052b8104000a034200";
      var pubKeyHex = uncompressedPk;
      if (uncompressedPk.Length != 130)
      {
        throw new Exception("Invalid public key.");
      }

      char c = 'B';
      var a = Regex.IsMatch($"{(char)c}", @"^\d+$");

      pubKeyHex = PKCS_PREFIX + pubKeyHex;
      var sha256str = Hash.Sha256(Utils.Hex2ByteArray(pubKeyHex));
      var t = Utils.ByteArray2hex(sha256str);
      var base58 = Base58.FromByteArray(sha256str);
      var end = base58.Substring(base58.Length - 36);

      var sum = end
        .ToCharArray()
        .Select(v => (int)v)
        .AsEnumerable()
        .Aggregate<int, int>(0, (acc, x) =>
          Regex.IsMatch($"{(char)x}", @"^\d+$") ? acc + int.Parse($"{(char)x}") : acc);

      var par = sum % 9;
      return $"DAG{par}{end}";
    }

    public static string GetDagAddressFromPrivateKey(byte[] pk)
    {
      return GetDagAddressFromUncompressedPublicKey(GetPublicKeyFromPrivatekey(pk, false));
    }

    public static string GetPublicKeyFromPrivatekey(byte[] sk, Boolean compressed = false)
    {
      var skPoint = new BigInteger(1, sk);
      var curve = ECNamedCurveTable.GetByName("secp256k1");
      var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
      var pkPoint = domainParams.G.Multiply(skPoint);
      var publicParams = new ECPublicKeyParameters(pkPoint, domainParams);

      var pk = Utils.ByteArray2hex(publicParams.Q.GetEncoded(compressed));
      return pk;
    }
  }
}