using NBitcoin;
using NBitcoin.Crypto;
using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete
{
  public class NativeSecp256k1ECDSASigner : ISigner
  {
    public enum SignatureType { ECDSA, COMPACT, DER, RECOVERABLE_COMPACT };

    public static Int64 Counter = 0;
    private SignatureType ECDSASignatureType;
    private IKey Key;
    private IntPtr Secp256k1Ptr = IntPtr.Zero;
    private bool Initialized = false;
    static private UInt32 SIGNATURE_BYTES_SIZE_DEFAULT = 2 << 6;
    
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
    public NativeSecp256k1ECDSASigner(IKey key, SignatureType signatureType = SignatureType.DER)
    {
      try
      {
        ECDSASignatureType = signatureType;
        Key = key;
        var flags = CoreNativeBridge.SECP256K1_CONTEXT_SIGN | CoreNativeBridge.SECP256K1_CONTEXT_VERIFY;
        Secp256k1Ptr = CoreNativeBridge.Secp256k1ContextCreate(flags);
        Init();
      }
      catch (Exception exc)
      {
        this.Dispose();
        throw new Exception("Unable to create ISigner");
      }
    }
    public void Dispose()
    {
      if (Initialized)
      {
        CoreNativeBridge.Secp256k1ContextDestroy(Secp256k1Ptr);
        Secp256k1Ptr = IntPtr.Zero;
        Deinit();
      }
    }
    private byte[] FormatBecauseOfBug(byte[] signature)
    {
      var incorrectLength = 72;
      var formatted = new byte[incorrectLength];
      if (signature.Length < incorrectLength)
      {
        for (int i = 0; i < formatted.Length; ++i)
        {
          formatted[i] = (i < signature.Length) ? signature[i] : (byte)(0);
        }
      }
      else
      {
        Array.Copy(signature, formatted, incorrectLength);
      }
      return formatted;
    }

    public byte[] GetBFASecret()
    {
      return Key.GetPrivateKey().Value;
    }
    public DerResponse GetDerResponse(byte[] mes)
    {
      return new DerResponse() { public_key = Key.GetPublicKey(false).Value, signature = Sign(mes) };
    }

    public byte[] Sign(byte[] mes)
    {
      if (mes.Length != 32)
        throw new Exception("It's possible to sign only 32 byte messages");

      using (var privateKey = Key.GetPrivateKey())
      {
        var ecdsaSignature = new byte[CoreNativeBridge.SECP256K1_ECDSA_SIGNATURE_SIZE];
        var status = CoreNativeBridge.Secp256k1ECDSASign(Secp256k1Ptr, privateKey.Value, mes, ecdsaSignature);

        if (!status)
          throw new Exception("Unable to sign message properly");

        if (ECDSASignatureType == SignatureType.ECDSA)
        {
          return ecdsaSignature;
        }
        else if (ECDSASignatureType == SignatureType.DER || ECDSASignatureType == SignatureType.RECOVERABLE_COMPACT)
        {
          UInt32 len = SIGNATURE_BYTES_SIZE_DEFAULT;
          byte[] output = new byte[len];
          status = CoreNativeBridge.Secp256k1ECDSASignatureSerializeDer(Secp256k1Ptr, output, ref len, ecdsaSignature);

          if (!status && len != SIGNATURE_BYTES_SIZE_DEFAULT)
          {
            //it means that we supplied little array, we need to resize it make it bigger
            output = new byte[len];
            status = CoreNativeBridge.Secp256k1ECDSASignatureSerializeDer(Secp256k1Ptr, output, ref len, ecdsaSignature);
          }

          if (!status)
            throw new Exception("Unable to convert ECDSA signature to der format");

          var derSignature = new byte[len];
          Array.Copy(output, derSignature, len);

          if (ECDSASignatureType == SignatureType.RECOVERABLE_COMPACT)
          {
            return DerToRecoverableCompact(derSignature, mes);
          }

          return derSignature;
        }
        else if (ECDSASignatureType == SignatureType.COMPACT)
        {
          var compactSignature = new byte[CoreNativeBridge.SECP256K1_COMPACT_ECDSA_SIGNATURE_SIZE];
          status = CoreNativeBridge.Secp256k1ECDSASerializeCompact(Secp256k1Ptr, compactSignature, ecdsaSignature);

          if (!status)
            throw new Exception("Unable to convert ECDSA signature to Compact format");

          return compactSignature;
        }
        else
        {
          throw new Exception(String.Format("Not supported ECDSA signature type: {0}", ECDSASignatureType));
        }
      }
    }

    private byte[] DerToRecoverableCompact(byte[] derSignature, byte[] message)
    {
      var IsCompressed = false;
      var hash = new uint256(message);
      using (var publicKey = Key.GetPublicKey(IsCompressed))
      {
        var pubKey = new PubKey(publicKey.Value, 0, "btc");
        ECDSASignature eCDSASignature = new ECDSASignature(derSignature);
        var sig = eCDSASignature;
        
        // Now we have to work backwards to figure out the recId needed to recover the signature.
        int recId = -1;
        for (int i = 0; i < 4; i++)
        {
          ECKey k = ECKey.RecoverFromSignature(i, sig, hash, IsCompressed);
          if (k != null && k.GetPubKey(IsCompressed).ToHex() == pubKey.ToHex())
          {
            recId = i;
            break;
          }
        }

        if (recId == -1)
          throw new InvalidOperationException("Could not construct a recoverable key. This should never happen.");

        int headerByte = recId + 27 + (IsCompressed ? 4 : 0);

        byte[] sigData = new byte[65];  // 1 header + 32 bytes for R + 32 bytes for S

        sigData[0] = (byte)headerByte;

        Array.Copy(NBitcoin.Utils.BigIntegerToBytes(sig.R, 32), 0, sigData, 1, 32);
        Array.Copy(NBitcoin.Utils.BigIntegerToBytes(sig.S, 32), 0, sigData, 33, 32);
        return sigData;
      }
    }

    public bool Verify(byte[] message, byte[] signature)
    {
      using (var publicKey = Key.GetPublicKey())
      {
        var ecdsaSignature = new byte[CoreNativeBridge.SECP256K1_ECDSA_SIGNATURE_SIZE];
        if (ECDSASignatureType == SignatureType.DER)
        {
          var status = CoreNativeBridge.Secp256k1ECDSASignatureParseDer(Secp256k1Ptr, ecdsaSignature, signature, signature.Length);

          if (!status)
            throw new Exception("Unable to convert Der signature to ECDSA format");

          return CoreNativeBridge.Secp256k1ECDSAVerify(Secp256k1Ptr, ecdsaSignature, message, publicKey.Value);
        }
        else if (ECDSASignatureType == SignatureType.ECDSA)
        {
          ecdsaSignature = signature;
        }
        else if (ECDSASignatureType == SignatureType.COMPACT)
        {
          var status = Core.CoreNativeBridge.Secp256k1ECDSAParseCompact(Secp256k1Ptr, ecdsaSignature, signature);

          if (!status)
            throw new Exception("Unable to convert Compact signature to ECDSA format");
        }
        else if (ECDSASignatureType == SignatureType.RECOVERABLE_COMPACT)
        {
          return true;
        }
        else
        {
          throw new Exception(String.Format("Not supported ECDSA signature type: {0}", ECDSASignatureType));
        }

        return CoreNativeBridge.Secp256k1ECDSAVerify(Secp256k1Ptr, ecdsaSignature, message, publicKey.Value);
      }
    }
  }
}
