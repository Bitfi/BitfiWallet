using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete
{
  public class NativeSecp256k1Bip32Node : IBip32Node
  {
    public static Int64 Counter = 0;
    private IntPtr NodePtr = IntPtr.Zero;
    public bool Initialized { get; private set; }
    public UInt64 Child { get; private set; }
    public UInt64 Depth { get; private set; }

    private void Init(UInt64 child, UInt64 depth)
    {
      Initialized = true;
      Counter++;
      Child = child;
      Depth = depth;
    }

    private void Deinit()
    {
      Initialized = false;
      Counter--;
      Child = 0;
      Depth = 0;
    }

    private NativeSecp256k1Bip32Node(IntPtr ptr, UInt64 child, UInt64 depth)
    {
      NodePtr = ptr;
      Init(child, depth);
    }
    
    public NativeSecp256k1Bip32Node(NoxManagedArray seed, NoxManagedArray key = null)
    {
      try
      {
        if (key == null)
        {
          key = new NoxManagedArray(System.Text.Encoding.UTF8.GetBytes("Bitcoin seed"));
        }
        
        NodePtr = CoreNativeBridge.BtcHDNodeNew();
        Init(0, 0);

        var res = CoreNativeBridge.BtcHDNodeFromSeedWithKey(seed.Value, seed.Value.Length,
          key.Value, key.Value.Length, NodePtr);

        if (!res)
          throw new Exception("Error while generating bip32 node from secret and phrase");
      }
      catch (Exception exc)
      {
        if (Initialized)
        {
          this.Dispose();
          throw new Exception("Unable to create IBipKey");
        }
      }
    }
    public IBip32Node Clone()
    {
      var nodePtrCopy = CoreNativeBridge.BtcHDNodeCopy(NodePtr);
      var copy = new NativeSecp256k1Bip32Node(nodePtrCopy, Child, Depth);
      return copy;
    }
    public bool DerivePrivate(uint child)
    {
      try
      {
        var res = CoreNativeBridge.BtcHDNodePrivateCkd(NodePtr, child);

        if (!res)
          throw new Exception("Error while deriving key process");

        Child = child;
        Depth++;
        return true;
      }
      catch (Exception exc)
      {
        return false;
      }
    }

    public bool DerivePublic(uint child)
    {
      try
      {
        var res = CoreNativeBridge.BtcHDNodePublicCkd(NodePtr, child);

        if (!res)
          throw new Exception("Error while deriving key process");

        Child = child;
        Depth++;
        return true;
      }
      catch (Exception exc)
      {
        return false;
      }
    }

    public void Dispose()
    {
      if (Initialized)
      {
        CoreNativeBridge.BtcHDNodeFree(NodePtr);
        NodePtr = IntPtr.Zero;
        Deinit();
      }
    }
    
    public NoxManagedArray GetPrivateKey()
    {
      byte[] managedArray = new byte[CoreNativeBridge.BTC_ECKEY_PKEY_LENGTH];
      Marshal.Copy(
        IntPtr.Add(NodePtr, CoreNativeBridge.BTC_ECKEY_PKEY_OFFSET),
        managedArray, 0, CoreNativeBridge.BTC_ECKEY_PKEY_LENGTH
      );

      return new NoxManagedArray(managedArray);
    }

    public NoxManagedArray GetPublicKey(bool isCompressed)
    {
      byte[] pubKeyCompressed = new byte[CoreNativeBridge.BTC_ECKEY_COMPRESSED_LENGTH];
      Marshal.Copy(
        IntPtr.Add(NodePtr, CoreNativeBridge.BTC_ECKEY_COMPRESSED_OFFSET),
        pubKeyCompressed, 0, CoreNativeBridge.BTC_ECKEY_COMPRESSED_LENGTH
      );
      
      if (!isCompressed)
      {
        byte[] pubKeyUncompressed = new byte[CoreNativeBridge.BTC_ECKEY_UNCOMPRESSED_LENGTH];
        var status = CoreNativeBridge.Secp256k1WalletPubkeyCreate(NodePtr, pubKeyCompressed, pubKeyUncompressed, 0);
        return new NoxManagedArray(pubKeyUncompressed);
      }
      else
      {
        return new NoxManagedArray(pubKeyCompressed);
      }
    }

    public NoxManagedArray GetPublicKey()
    {
      byte[] managedArray = new byte[CoreNativeBridge.BTC_ECKEY_COMPRESSED_LENGTH];
      Marshal.Copy(
        IntPtr.Add(NodePtr, CoreNativeBridge.BTC_ECKEY_COMPRESSED_OFFSET),
        managedArray, 0, CoreNativeBridge.BTC_ECKEY_COMPRESSED_LENGTH
      );
      return new NoxManagedArray(managedArray);
    }
  }
}
