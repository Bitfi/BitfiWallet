using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using EthereumLibrary.Signer;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using NeoGasLibrary;
using NBitcoin;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using NoxService.SWS;
using NoxService.NWS;
using Org.BouncyCastle.Math;
using System.Security;
using WalletLibrary;
using static WalletLibrary.Core.Concrete.Wallets.CommonWallet;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete;

namespace NoxKeys
{
  struct NoxExtKeySet
  {
    public NoxManagedArray PrivateKey { get; set; }
    public NoxManagedArray ChainCode { get; set; }
  }

  public class NoxKeyGen
  {
    NoxExtKeySet MasterExtKeySet;
    List<NoxManagedArray> ArrayList;
    BitfiWallet.Device noxDevice;

    private KeySet[] KeySets;

    public NoxKeyGen(KeySet[] keySets)
    {
      KeySets = keySets;
    }

    public byte[] Native_GetSignature(PubKey pubKey, byte[] hash)
    {
      IKey key = KeySets.FirstOrDefault(k => k.Index == pubKey.NoxIndex).Key;

      using (ISigner signer = new NativeSecp256k1ECDSASigner(key))
      {
        var signature = signer.Sign(hash);

        if (!signer.Verify(hash, signature))
          throw new Exception("Unable to verify signed signature");

        return signature;
      }
    }
  }

}