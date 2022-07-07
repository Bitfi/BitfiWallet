using System.Text;
using System.IO;
using System;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using System.Collections.Generic;

namespace DagLibrary
{
 class DagSigner
 {
  public static byte[] DagSign(byte[] key, string msg)
  {
   var hash = DagLibrary.Hash.Sha512(Encoding.UTF8.GetBytes(msg));
   EthereumLibrary.Signer.Crypto.ECKey eCKey = new EthereumLibrary.Signer.Crypto.ECKey(key, true);
   var sig = eCKey.Sign(hash);
   return sig.ToDER();
  }
  public static byte[] DagSign(byte[] key, byte[] msg)
  {
   var hash = DagLibrary.Hash.Sha512(msg);
   EthereumLibrary.Signer.Crypto.ECKey eCKey = new EthereumLibrary.Signer.Crypto.ECKey(key, true);
   var sig = eCKey.Sign(hash);
   return sig.ToDER();
  }

  public static bool DagVerify(byte[] pubkey, string msg, string sighex)
  {
   var hash = DagLibrary.Hash.Sha512(Encoding.UTF8.GetBytes(msg));
   EthereumLibrary.Signer.Crypto.ECKey eCKey = new EthereumLibrary.Signer.Crypto.ECKey(pubkey, false);

   var ecsig = EthereumLibrary.Signer.Crypto.ECDSASignature.FromDER(sighex.HexToByteArray());
   return eCKey.Verify(hash, ecsig);

  }
  public static byte[] HashPrefixedMessage(byte[] message)
  {
   var byteList = new List<byte>();
   var bytePrefix = "0x19".HexToByteArray();
   var textBytePrefix = Encoding.UTF8.GetBytes
    ("Constellation Signed Message:\n" + message.Length + "\n");
   byteList.AddRange(bytePrefix);
   byteList.AddRange(textBytePrefix);
   byteList.AddRange(message);

   return byteList.ToArray();

  }
 }
}