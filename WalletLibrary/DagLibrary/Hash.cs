using Org.BouncyCastle.Crypto.Digests;

namespace DagLibrary
{
  public static class Hash
  {
  public static byte[] Sha256(byte[] bytes)
  {
   using (var sha = System.Security.Cryptography.SHA256Managed.Create())
    return sha.ComputeHash(bytes);
  }

  public static byte[] Sha512(byte[] bytes)
  {
   using (var sha = System.Security.Cryptography.SHA512Managed.Create())
    return sha.ComputeHash(bytes);
  }
 }
}