using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NBitcoin.Altcoins
{
	// Reference: https://github.com/polispay/polis/blob/master/src/chainparams.cpp
	public class Polis : NetworkSetBase
	{
		public static Polis Instance { get; } = new Polis();

		public override string CryptoCode => "POLIS";

		private Polis()
		{

		}
		public class PolisConsensusFactory : ConsensusFactory
		{
			private PolisConsensusFactory()
			{
			}

			public static PolisConsensusFactory Instance { get; } = new PolisConsensusFactory();

			public override BlockHeader CreateBlockHeader()
			{
				return new PolisBlockHeader();
			}
			public override Block CreateBlock()
			{
				return new PolisBlock(new PolisBlockHeader());
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public class PolisBlockHeader : BlockHeader
		{
			
			static byte[] CalculateHash(byte[] data, int offset, int count)
			{
				return new HashX11.X11().ComputeBytes(data.Skip(offset).Take(count).ToArray());
			}

			// https://github.com/polispay/polis/blob/e596762ca22d703a79c6880a9d3edb1c7c972fd3/src/primitives/block.cpp#L13
			protected override HashStreamBase CreateHashStream()
			{
				return BufferedHashStream.CreateFrom(CalculateHash, 80);
			}
		}

		public class PolisBlock : Block
		{
#pragma warning disable CS0612 // Type or member is obsolete
			public PolisBlock(PolisBlockHeader h) : base(h)
#pragma warning restore CS0612 // Type or member is obsolete
			{

			}
			public override ConsensusFactory GetConsensusFactory()
			{
				return PolisConsensusFactory.Instance;
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete


		protected override void PostInit()
		{
			RegisterDefaultCookiePath("PolisCore");
		}


		static uint256 GetPoWHash(BlockHeader header)
		{
			var headerBytes = header.ToBytes();
			var h = NBitcoin.Crypto.SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
			return new uint256(h);
		}

		protected override NetworkBuilder CreateMainnet()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 262800,
				MajorityEnforceBlockUpgrade = 750,
				MajorityRejectBlockOutdated = 950,
				MajorityWindow = 1000,
				BIP34Hash = new uint256("0x000001f35e70f7c5705f64c6c5cc3dea9449e74d5b5c7cf74dad1bcca14a8012"),
				PowLimit = new Target(new uint256("0x00000fffff000000000000000000000000000000000000000000000000000000")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000e72e9b868ce8efb7"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
				PowAllowMinDifficultyBlocks = false,
				CoinbaseMaturity = 15,
				PowNoRetargeting = false,
				RuleChangeActivationThreshold = 1916,
				MinerConfirmationWindow = 2016,
				ConsensusFactory = PolisConsensusFactory.Instance,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 55 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 56 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 60 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x03, 0xE2, 0x5D, 0x7E })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x03, 0xE2, 0x59, 0x45 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("polis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("polis"))
			.SetMagic(0xBD6B0CBF)
			.SetPort(24126)
			.SetRPCPort(24127)
			.SetMaxP2PVersion(70211)
			.SetName("polis-main")
			.AddAlias("polis-mainnet")
			.AddDNSSeeds(new[]
			{
				new DNSSeedData("dnsseed.poliscentral.org", "dnsseed.poliscentral.org"),
				new DNSSeedData("dnsseed2.poliscentral.org", "dnsseed2.poliscentral.org"),
				new DNSSeedData("dnsseed3.poliscentral.org", "dnsseed3.poliscentral.org"),
				new DNSSeedData("polis.seeds.mn.zone", "polis.seeds.mn.zone"),
				new DNSSeedData("polis.mnseeds.com", "polis.mnseeds.com")
			})
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000fc4b8cb903aed54e11e1ae8a5b7ad097ade34988a84500ad2d80e4d1f5bcc95d2bb73b5af0ff0f1edbff04000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff2404ffff001d01041c506f6c69732c2066726f6d2070656f706c6520746f2070656f706c65ffffffff0100f2052a01000000434104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac00000000");
			return builder;
		}

		protected override NetworkBuilder CreateTestnet()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 210240,
				MajorityEnforceBlockUpgrade = 51,
				MajorityRejectBlockOutdated = 75,
				MajorityWindow = 100,
				BIP34Hash = new uint256("0x0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1"),
				PowLimit = new Target(new uint256("0x00000fffff000000000000000000000000000000000000000000000000000000")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
				PowAllowMinDifficultyBlocks = true,
				CoinbaseMaturity = 15,
				PowNoRetargeting = false,
				RuleChangeActivationThreshold = 1512,
				MinerConfirmationWindow = 2016,
				ConsensusFactory = PolisConsensusFactory.Instance,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tpolis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tpolis"))
			.SetMagic(0xFFCAE2CE)
			.SetPort(24130)
			.SetRPCPort(24131)
			.SetMaxP2PVersion(70211)
		   	.SetName("polis-test")
		   	.AddAlias("polis-testnet")
		   	.AddSeeds(new NetworkAddress[0])
		   	.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000fc4b8cb903aed54e11e1ae8a5b7ad097ade34988a84500ad2d80e4d1f5bcc95d678f875af0ff0f1e94ba01000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff2404ffff001d01041c506f6c69732c2066726f6d2070656f706c6520746f2070656f706c65ffffffff0100f2052a01000000434104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac00000000");
			return builder;
		}

		protected override NetworkBuilder CreateRegtest()
		{
			var builder = new NetworkBuilder();
			builder.SetConsensus(new Consensus()
			{
				SubsidyHalvingInterval = 150,
				MajorityEnforceBlockUpgrade = 750,
				MajorityRejectBlockOutdated = 950,
				MajorityWindow = 1000,
				BIP34Hash = new uint256(),
				PowLimit = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
				MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
				PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
				PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
				PowAllowMinDifficultyBlocks = true,
				CoinbaseMaturity = 15,
				PowNoRetargeting = true,
				RuleChangeActivationThreshold = 108,
				MinerConfirmationWindow = 144,
				ConsensusFactory = PolisConsensusFactory.Instance,
				SupportSegwit = false
			})
			.SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
			.SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
			.SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
			.SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
			.SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
			.SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tpolis"))
			.SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tpolis"))
			.SetMagic(0xDCB7C1FC)
			.SetPort(19994)
			.SetRPCPort(19993)
			.SetMaxP2PVersion(70211)
			.SetName("polis-reg")
			.AddAlias("polis-regtest")
			.AddDNSSeeds(new DNSSeedData[0])
			.AddSeeds(new NetworkAddress[0])
			.SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000fc4b8cb903aed54e11e1ae8a5b7ad097ade34988a84500ad2d80e4d1f5bcc95d9a3b3b5af0ff0f1e3c8b0d000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff2404ffff001d01041c506f6c69732c2066726f6d2070656f706c6520746f2070656f706c65ffffffff0100f2052a01000000434104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac00000000");
			return builder;
		}

	}
}
