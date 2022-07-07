using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
 public class CommonWallet
 {
  protected JsonSerializerSettings commonSerializer = new JsonSerializerSettings()
  {
   MissingMemberHandling = MissingMemberHandling.Ignore,
   NullValueHandling = NullValueHandling.Ignore,
   ObjectCreationHandling = ObjectCreationHandling.Auto,
   TypeNameHandling = TypeNameHandling.All
  };

  protected T DeserializeMXTxn<T>(string MXTxn)
  {
   byte[] bts = Convert.FromBase64String(MXTxn);
   string tmp = System.Text.Encoding.UTF8.GetString(bts);
   var data = JsonConvert.DeserializeObject<T>(tmp, commonSerializer);
   return data;
  }

  public static Int64 Counter = 0;
  public struct KeySet
  {
   public IKey Key;
   public UInt32 Index;
  }

  public string Symbol { get; private set; }
  protected KeySet[] KeySets;

  public void Dispose()
  {
   foreach (var keySet in KeySets)
    keySet.Key.Dispose();

   Deinit();
  }

  public ISigner GetSigner(NativeSecp256k1ECDSASigner.SignatureType type, UInt32 i)
  {
   if (!HasIndex(i))
    throw new Exception(String.Format("Unable to find key set with given : {0} index", i));

   var keySet = KeySets.FirstOrDefault(ks => ks.Index == i);

   return new NativeSecp256k1ECDSASigner(keySet.Key, type);
  }

  protected CommonWallet(KeySet[] keySets, WalletFactory.Products product)
  {
   KeySets = keySets;
   Symbol = product.ToString().ToLower();
   Init();
  }

  protected bool HasIndex(UInt32 index)
  {
   foreach (var keySet in KeySets)
    if (keySet.Index == index)
     return true;

   return false;
  }

  private void Init()
  {
   Counter++;
  }

  private void Deinit()
  {
   Counter--;
  }
 }
}