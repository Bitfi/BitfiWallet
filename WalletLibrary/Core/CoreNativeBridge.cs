using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.Core
{
  public static class CoreNativeBridge
  {
    public const int BTC_ECKEY_PKEY_OFFSET = 12 + BTC_BIP32_CHAINCODE_SIZE;
    public const int BTC_ECKEY_COMPRESSED_OFFSET = 12 + BTC_BIP32_CHAINCODE_SIZE + BTC_ECKEY_PKEY_LENGTH;

    public const int DUMMY_BYTES_ALIGNMENT = 3; //3 dummy bytes, required for alignment
    public const int BTC_BIP32_CHAINCODE_SIZE = 32;
    public const int BTC_ECKEY_PKEY_LENGTH = 32;
    public const int BTC_ECKEY_COMPRESSED_LENGTH = 33;
    public const int BTC_ECKEY_UNCOMPRESSED_LENGTH = 65;
    public const int SECP256K1_ECDSA_SIGNATURE_SIZE = 64;
    public const int SECP256K1_COMPACT_ECDSA_SIGNATURE_SIZE = 64;
    private const string DllPath = "libnativelib.so";

    [StructLayout(LayoutKind.Explicit)]
    public struct BtcNode
    {
      [FieldOffset(0)]
      public UInt32 depth;
      [FieldOffset(4)]
      public UInt32 fingerprint;
      [FieldOffset(8)]
      public UInt32 child_num;

      [FieldOffset(12)]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = BTC_BIP32_CHAINCODE_SIZE, ArraySubType = UnmanagedType.U1)]
      public byte[] chain_code;

      [FieldOffset(12 + BTC_BIP32_CHAINCODE_SIZE)]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = BTC_ECKEY_PKEY_LENGTH, ArraySubType = UnmanagedType.U1)]
      public byte[] private_key;

      [FieldOffset(12 + BTC_BIP32_CHAINCODE_SIZE + BTC_ECKEY_PKEY_LENGTH)]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = BTC_ECKEY_COMPRESSED_LENGTH + DUMMY_BYTES_ALIGNMENT, ArraySubType = UnmanagedType.U1)]
      public byte[] public_key;
    }

    //bip32 methods
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_from_seed_bridge")]
    public static extern bool BtcHDNodeFromSeed(byte[] seed, int seedLen, IntPtr node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_free_bridge")]
    public static extern void BtcHDNodeFree(IntPtr node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_copy_bridge")]
    public static extern IntPtr BtcHDNodeCopy(IntPtr node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_public_ckd_bridge")]
    public static extern bool BtcHDNodePublicCkd(IntPtr node, UInt32 i);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_private_ckd_bridge")]
    public static extern bool BtcHDNodePrivateCkd(IntPtr node, UInt32 i);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_new_bridge")]
    public static extern IntPtr BtcHDNodeNew();

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "btc_hdnode_from_seed_with_key_bridge")]
    public static extern bool BtcHDNodeFromSeedWithKey(byte[] seed, int seedLen, byte[] key, int keyLen, IntPtr node);

    private const UInt32 SECP256K1_FLAGS_BIT_CONTEXT_VERIFY = 1 << 8;
    private const UInt32 SECP256K1_FLAGS_BIT_CONTEXT_SIGN = 1 << 9;
    private const UInt32 SECP256K1_FLAGS_TYPE_CONTEXT = 1 << 0;

    /** Flags to pass to secp256k1_context_create. */
    public const UInt32 SECP256K1_CONTEXT_VERIFY = SECP256K1_FLAGS_TYPE_CONTEXT | SECP256K1_FLAGS_BIT_CONTEXT_VERIFY;
    public const UInt32 SECP256K1_CONTEXT_SIGN = SECP256K1_FLAGS_TYPE_CONTEXT | SECP256K1_FLAGS_BIT_CONTEXT_SIGN;

    //secp256k1 methods
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_wallet_pubkey_create")]
    public static extern bool Secp256k1WalletPubkeyCreate(IntPtr ctx, byte[] pubKey, byte[] outputSer, int compressed);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_context_create_bridge")]
    public static extern IntPtr Secp256k1ContextCreate(UInt32 flags);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_context_destroy_bridge")]
    public static extern void Secp256k1ContextDestroy(IntPtr ctx);


    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_sign_bridge")]
    public static extern bool Secp256k1ECDSASign(IntPtr ctx, byte[] secKey, byte[] data, byte[] sig);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_verify_bridge")]
    public static extern bool Secp256k1ECDSAVerify(IntPtr ctx, byte[] sig, byte[] msg32, byte[] pubkey);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_signature_serialize_der_bridge")]
    public static extern bool Secp256k1ECDSASignatureSerializeDer(IntPtr ctx, byte[] output, ref UInt32 outputLen, byte[] sig);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_signature_parse_der_bridge")]
    public static extern bool Secp256k1ECDSASignatureParseDer(IntPtr ctx, byte[] sig, byte[] input, int inputLen);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_signature_serialize_compact_bridge")]
    public static extern bool Secp256k1ECDSASerializeCompact(IntPtr ctx, byte[] output64, byte[] sig);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "secp256k1_ecdsa_signature_parse_compact_bridge")]
    public static extern bool Secp256k1ECDSAParseCompact(IntPtr ctx, byte[] sig, byte[] input64);


  }
}
