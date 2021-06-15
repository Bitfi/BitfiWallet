using NBitcoin;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBitcoin.Altcoins
{
  public class Qtum : NetworkSetBase
  {
    public static Qtum Instance { get; } = new Qtum();

    public override string CryptoCode => "QTUM";

    private Qtum()
    { }

    protected override void PostInit()
    {
      RegisterDefaultCookiePath("Qtum");
    }

    protected override NetworkBuilder CreateMainnet()
    {
      var builder = new NBitcoin.NetworkBuilder();
      builder.CopyFrom(Network.Main);

      var qtumMainnetBuilder = builder
        .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("qt"))
        .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("qc"))
        .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 0x3a })
        .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 0x32 })
        .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 128 })
        .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x88, 0xB2, 0x1E })
        .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
        .SetName("qtum-mainnet");

      return qtumMainnetBuilder;
    }

    protected override NetworkBuilder CreateTestnet()
    {
      var builder = new NBitcoin.NetworkBuilder();
      builder.CopyFrom(Network.TestNet);

      var qtumTestnetBuilder = builder
        .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tq"))
        .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tq"))
        .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 0x78 })
        .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 0x6e })
        .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 0xef })
        .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xcf })
        .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
        .SetName("qtum-testnet");

      return qtumTestnetBuilder;
    }

    protected override NetworkBuilder CreateRegtest()
    {
      return null;
    }
  }
}