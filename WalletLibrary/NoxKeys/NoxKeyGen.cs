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
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete;

namespace NoxKeys
{

	public class NoxKeyGen
	{

		private WalletLibrary.Core.Concrete.Wallets.CommonWallet.KeySet[] KeySets;

		private Dictionary<UInt32, IKey> ActiveKeySets;
		private bool IsActiveWallet = false;

		public NoxKeyGen(Dictionary<UInt32, IKey> keySets)
		{
			ActiveKeySets = keySets;
			IsActiveWallet = true;
		}

		public NoxKeyGen(WalletLibrary.Core.Concrete.Wallets.CommonWallet.KeySet[] keySets)
		{
			KeySets = keySets;
		}

		public byte[] Native_GetSignature(PubKey pubKey, byte[] hash)
		{
			IKey key;

			if (IsActiveWallet)
			{
				key = ActiveKeySets[(uint)pubKey.NoxIndex]; 
			}
			else
			{
				key = KeySets.FirstOrDefault(k => k.Index == pubKey.NoxIndex).Key;
			}

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