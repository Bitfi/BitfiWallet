using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Apollo
{
  public static class ApolloLibrary
  {
    public static byte[] SimpleHash(byte[] data, byte[] addition = null)
    {
      Org.BouncyCastle.Crypto.Digests.Sha256Digest myHash = new Org.BouncyCastle.Crypto.Digests.Sha256Digest();
      myHash.BlockUpdate(data, 0, data.Length);
      if (addition != null)
      {
        myHash.BlockUpdate(addition, 0, addition.Length);
      }
      byte[] compArr = new byte[myHash.GetDigestSize()];
      myHash.DoFinal(compArr, 0);
      return compArr;
    }

    public static string GetPassPhraseFromBytes(byte[] arr)
    {
      var bytes = arr.ToUInt32Arr();
      var n = Utils.EngWords.Length;
      var phraseWords = new List<string>();
      UInt32 x = 0;
      Int64 w1 = 0;
      Int64 w2 = 0;
      Int64 w3 = 0;

      for (var i = 0; i < bytes.Length; i++)
      {
        x = bytes[i];
        w1 = x % n;
        w2 = (((x / n) >> 0) + w1) % n;
        w3 = (((((x / n) >> 0) / n) >> 0) + w2) % n;

        phraseWords.Add(Utils.EngWords[w1]);
        phraseWords.Add(Utils.EngWords[w2]);
        phraseWords.Add(Utils.EngWords[w3]);
      }

      bytes.Clear();
      return String.Join(" ", phraseWords.ToArray());
    }

    public static string GetAccountIdFromPublicKey(byte[] publicKey, bool isRsFormat = true)
    {
      //var hex = converters.hexStringToByteArray(publicKey);
      var account = SimpleHash(publicKey);
      var sliced = account.SubArray(0, 8);
      var accountId = sliced.ToBigInteger().ToString();

      return isRsFormat ? AplAddress.NumericToRSAccountFormat(accountId) : accountId;
    }

    public static byte[] GetPublicKey(byte[] secretPhrase)
    {
      var digest = SimpleHash(secretPhrase);
      return Curve25519.Keygen(digest).P;
    }

    public static bool IsValidAddress(string addr)
    {
      return AplAddress.IsValidAddressRS(addr);
    }

    public static byte[] SignTransaction(byte[] unsignedBytes, byte[] secretPhrase)
    {
      var signature = SignBytes(unsignedBytes, secretPhrase);
      var signedTx = new byte[unsignedBytes.Length];
      Buffer.BlockCopy(unsignedBytes, 0, signedTx, 0, unsignedBytes.Length);

      //put signature in transaction
      Buffer.BlockCopy(
        signature, 0, signedTx,
        //other fields, like atm amount, fee and etc
        1 + 1 + 4 + 2 + 32 + 8 + 8 + 8 + 32,
      64);

      return signedTx;
    }

    public static byte[] SignBytes(byte[] message, byte[] secretPhrase)
    {
      var digest = SimpleHash(secretPhrase);
      var s = Curve25519.Keygen(digest).S;
      var m = SimpleHash(message);
      var x = SimpleHash(m, s);
      var y = Curve25519.Keygen(x).P;
      var h = SimpleHash(m, y);
      var v = Curve25519.Sign(h, x, s);
      return v.Concat(h).ToArray();
    }
  }
}
