using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete
{
  public class NativeSecp256k1Key : IKey
  {
    public static Int64 Counter = 0;
    private IBip32Node BipNode = null;
    private bool Initialized = false;
    private void Init()
    {
      Initialized = true;
      Counter++;
    }
    private void Deinit()
    {
      Initialized = false;
      Counter--;
    }
    public NativeSecp256k1Key(NoxManagedArray secret, NoxManagedArray salt)
    {

      ComputeAsync(secret, salt).Wait();
    }

    async Task ComputeAsync(NoxManagedArray secret, NoxManagedArray salt)
    {
      try
      {
        using (var der = await SCrypt.ComputeDerivedKey(secret, salt, 32768, 8, 4, 4, 64))
        using (var shacrypt = new System.Security.Cryptography.SHA256Managed())
        using (NoxManagedArray HashKey = new NoxManagedArray(shacrypt.ComputeHash(salt.Value)))
        {
          BipNode = new NativeSecp256k1Bip32Node(der, HashKey);
          Init();
        }
      }
      catch (Exception exc)
      {
        this.Dispose();
        throw new Exception("Unable to create IKey");
      }
    }


    public IKey DerivePrivate(uint child)
    {
      IBip32Node bipNodeCopy = null;
      try
      {
        bipNodeCopy = BipNode.Clone();
        var res = bipNodeCopy.DerivePrivate(child);

        if (res == false)
          throw new Exception("Unable to derive private key");

        var keyCopy = new NativeSecp256k1Key(bipNodeCopy);
        return keyCopy;
      }
      catch (Exception exc)
      {
        if (bipNodeCopy != null)
        {
          bipNodeCopy.Dispose();
        }
        return null;
      }
    }

    public IKey DerivePublic(uint child)
    {
      IBip32Node bipNodeCopy = null;
      try
      {
        bipNodeCopy = BipNode.Clone();
        var res = bipNodeCopy.DerivePublic(child);

        if (res == false)
          throw new Exception("Unable to derive public key");

        var keyCopy = new NativeSecp256k1Key(bipNodeCopy);
        return keyCopy;
      }
      catch (Exception exc)
      {
        if (bipNodeCopy != null)
        {
          bipNodeCopy.Dispose();
        }
        return null;
      }
    }

    public void Dispose()
    {
      if (Initialized)
      {
        BipNode.Dispose();
        Deinit();
      }
    }

    public IKey Clone()
    {
      var bipNodeCopy = BipNode.Clone();
      return new NativeSecp256k1Key(bipNodeCopy);
    }

    public NoxManagedArray GetPublicKey(bool isCompressed = true)
    {
      return BipNode.GetPublicKey(isCompressed);
    }

    public NoxManagedArray GetPrivateKey()
    {
      return BipNode.GetPrivateKey();
    }

    public IKey DeriveKeyForCurrency(uint hdIndex, string symbol)
    {
      var currencyIndex = (UInt32)Sclear.GetCurrencyIndex(symbol) | 0x80000000u; //is hardened
      using (IKey masterKeyForCurrency = DerivePrivate(currencyIndex))
      {
        return masterKeyForCurrency.DerivePrivate(hdIndex);
      }
    }

    public IKey DeriveCurrency(string symbol)
    {
      var currencyIndex = (UInt32)Sclear.GetCurrencyIndex(symbol) | 0x80000000u; //is hardened

      return DerivePrivate(currencyIndex);
    }

    private NativeSecp256k1Key(IBip32Node bipNode)
    {
      BipNode = bipNode;
      Init();
    }
  }
}
