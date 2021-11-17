using Org.BouncyCastle.Crypto.Digests;

namespace DagLibrary
{
  public static class Hash
  {
    public static byte[] Sha256(byte[] bytes)
    {
      var encData = bytes;
      Sha256Digest myHash = new Sha256Digest();
      myHash.BlockUpdate(encData, 0, encData.Length);
      byte[] compArr = new byte[myHash.GetDigestSize()];
      myHash.DoFinal(compArr, 0);
      return compArr;
    }

    public static byte[] Sha512(byte[] bytes)
    {
      var encData = bytes;
      Sha512Digest myHash = new Sha512Digest();
      myHash.BlockUpdate(encData, 0, encData.Length);
      byte[] compArr = new byte[myHash.GetDigestSize()];
      myHash.DoFinal(compArr, 0);
      return compArr;
    }
  }
}