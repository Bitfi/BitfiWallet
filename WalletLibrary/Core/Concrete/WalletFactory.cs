using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete.Wallets;
using static WalletLibrary.Core.Concrete.Wallets.CommonWallet;

namespace WalletLibrary.Core.Concrete
{
  public class WalletFactory : IDisposable
  {
    public enum Products { BTC = 0, LTC, GRS, DGB, DOGE, DASH, ETH, NEO, EOS, APL, XRP, XMR, QTUM, FTC, VIA, BTG, MONA, ZCL, STRAT, BFA, BCH, XDC, DAG }
    public static Int64 Counter = 0;
    
    private IKey MasterKey;
    private bool Initialized = false;
    private static int SALT_MIN_LENGTH = 5;
    private static int SECRET_MIN_LENGTH = 29;
    private const UInt32[] DEFAULT_INDEXES = null;

    WSTask statusTask = new WSTask();
    WalletEventProxy _eventProxy;


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

    public static WalletFactory.Products SymbolToWalletProduct(string symbol)
    {
      if (symbol.ToLower() == "gas")
        symbol = "neo";

      return (Products)Enum.Parse(typeof(Products), symbol.ToUpper());
    }

    public WalletFactory(NoxManagedArray secret, NoxManagedArray salt, WalletEventProxy eventProxy)
    {

      _eventProxy = eventProxy;

      if (salt.Value.Length <= SALT_MIN_LENGTH)
      {
        Thread.Sleep(500);
        throw new Exception("Invalid length. Salt should be at least 6 characters.");
      }
      
        if (secret.Value.Length <= SECRET_MIN_LENGTH)
        {
          Thread.Sleep(500);
          throw new Exception("Invalid length. Secret phrase should be at least 30 characters.");
        }

      statusTask.Transition = UITransition.DlgModel;
      statusTask.StatusMsg = "Deriving master keys";
      _eventProxy.HandleStatusEvent(statusTask);

      MasterKey = new NativeSecp256k1Key(secret, salt);
      Init();
    }

    public IWallet ConstructWallet(WalletFactory.Products product, UInt32[] indexes = DEFAULT_INDEXES, bool Authenticating = false)
    {
      indexes = indexes ?? new UInt32[] { 0 };

      if (Authenticating)
      {
        statusTask.StatusMsg = "Calculating authenticator address for signing";
      }
      else
      {
        statusTask.StatusMsg = "Calculating address for " + product.ToString().ToUpper();
        if (indexes.Length > 1)
          statusTask.StatusMsg = "Calculating " + indexes.Length.ToString() + " addresses for " + product.ToString().ToUpper();
      }
      _eventProxy.HandleStatusEvent(statusTask);

      var productStr = product.ToString().ToLower();
      switch (product)
      {
        case Products.BFA:
          return new B2FAWallet(GenerateKeys(productStr, indexes));
        case Products.BTC:
        case Products.DGB:
        case Products.DOGE:
        case Products.GRS:
        case Products.LTC:
        case Products.QTUM:
        case Products.DASH:
        case Products.FTC:
        case Products.VIA:
        case Products.BTG:
        case Products.MONA:
        case Products.ZCL:
        case Products.STRAT:
        case Products.BCH:
          return new BtcWallet(GenerateKeys(productStr, indexes), product);
        case Products.ETH:
          return new EthWallet(GenerateKeys(productStr, indexes));
        case Products.XDC:
          return new XDCWallet(GenerateKeys(productStr, indexes));
        case Products.APL:
          return new AplWallet(GenerateKeys(productStr, indexes));
        case Products.NEO:
          return new NeoWallet(GenerateKeys(productStr, indexes));
        case Products.XMR:
          return new XmrWallet(GenerateKeys(productStr, indexes));
        case Products.XRP:
          return new XrpWallet(GenerateKeys(productStr, indexes));
        case Products.EOS:
          return new EosWallet(GenerateKeys(productStr, indexes));

        case Products.DAG:
          return new DagWallet(GenerateKeys(productStr, indexes));

        default:
          throw new Exception(String.Format("Invalid currencySymbol: {0}", productStr));
      }
    }

    public IKey GetMasterKeyCopy()
    {
      return MasterKey.Clone();
    }

    public string GetTestHash()
    {
      var index = (uint)Sclear.GetCurrencyIndex("test");

      using (var hmac = new System.Security.Cryptography.SHA256Managed())
      using (var derivedKey = MasterKey.DerivePrivate(index))
      using (var privateKey = derivedKey.GetPrivateKey())
      {
        return hmac.ComputeHash(privateKey.Value).ByteToHex();
      }
    }

    private CommonWallet.KeySet[] GenerateKeys(string currencySymbol, UInt32[] indexes)
    {
      CommonWallet.KeySet[] keySets = new CommonWallet.KeySet[indexes.Length];

      try
      {
        using (IKey parent = MasterKey.DeriveCurrency(currencySymbol))
        {
          for (int i = 0; i < indexes.Length; ++i)
          {
            IKey key = parent.DerivePrivate(indexes[i]);
            
            var keySet = new CommonWallet.KeySet
            {
              Index = indexes[i],
              Key = key,
            };
            keySets[i] = keySet;
          }
        }

        return keySets;
      }
      catch (Exception exc)
      {
        for (int i = 0; i < keySets.Length; ++i)
          keySets[i].Key.Dispose();

        throw new Exception(String.Format("Unable to generate keys for {0}", currencySymbol));
      }
    }
    
    public void Dispose()
    {
      if (Initialized)
      {
        MasterKey.Dispose();
        Deinit();
      }
    }
  }
}
