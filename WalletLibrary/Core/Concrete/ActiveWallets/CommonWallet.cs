using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.ActiveWallets
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

  public NoxKeys.NoxKeyGen NoxKeyGen { get; set; }

  public static Int64 Counter = 0;
  

  public string Symbol { get; private set; }
  public Dictionary<uint, IKey> KeySets = new Dictionary<uint, IKey>();

  public void Dispose()
  {
   foreach (var keySet in KeySets)
    keySet.Value.Dispose();

   Deinit();
  }

  public ISigner GetSigner(NativeSecp256k1ECDSASigner.SignatureType type, uint i)
  {
   if (!HasIndex(i))
    throw new Exception(String.Format("Unable to find key set with given : {0} index", i));

   var keySet = KeySets[i];

   return new NativeSecp256k1ECDSASigner(keySet, type);
  }

  protected CommonWallet(WalletActiveFactory.Products product)
  {
   Symbol = product.ToString().ToLower();

   Init();
  }

  public void AddKey(IKey keySet, UInt32 index)
		{
   if (HasIndex(index))
    return;

   KeySets.TryAdd(index, keySet);
		}

  public bool HasIndex(UInt32 index)
  { 
   return KeySets.ContainsKey(index);
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