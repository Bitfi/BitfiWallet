using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.Signer.Crypto;
using EthereumLibrary.Util;

namespace EthereumLibrary
{
  public class MsgSigning
  {
    public string ETHMsgSign(byte[] message, EthereumLibrary.Signer.EthECKey key)
    {
      var msg_formt = HashPrefixedMessage(message);
      var signature = key.SignAndCalculateV(msg_formt);
      return ETHCreateStringSignature(signature);
    }
    private string ETHCreateStringSignature(EthereumLibrary.Signer.EthECDSASignature signature)
    {
      return EthereumLibrary.Signer.EthECDSASignature.CreateStringSignature(signature);
    }

    private byte[] HashPrefixedMessage(byte[] message)
    {
      var byteList = new List<byte>();
      var bytePrefix = "0x19".HexToByteArray();
      var textBytePrefix = Encoding.UTF8.GetBytes("Ethereum Signed Message:\n" + message.Length);

      byteList.AddRange(bytePrefix);
      byteList.AddRange(textBytePrefix);
      byteList.AddRange(message);

      var hash = new Sha3Keccack().CalculateHash(byteList.ToArray());
      return hash;
    }
  }
}