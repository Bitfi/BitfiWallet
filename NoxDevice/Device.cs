using NBitcoin;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;
using NoxKeys;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;

namespace BitfiWallet
{
	public class Device
	{
		BitcoinPubKeyAddress WS_PUB;

		readonly Key TEEKey;

		readonly Guid DeviceID;

		private string WALLETID;

		readonly PubKey TEEPubKey;

		public Device(Key _TEEKey, Guid _DeviceID)
		{

			TEEKey = _TEEKey;
			DeviceID = _DeviceID;
			TEEPubKey = TEEKey.PubKey;
			WALLETID = DeviceID.ToString().ToUpper().Replace("-", "").Substring(0, 6);
			WS_PUB = new BitcoinPubKeyAddress(new KeyId(new byte[] { 146, 17, 90, 20, 206, 41, 255, 170, 171, 24, 82, 234, 70, 248, 115, 63, 247, 55, 93, 61 }), Network.Main);
			MsgCount = 0;

			try
			{
				byte[] vch = TEEKey.ToBytes();
				eCPrivate = WalletLibrary.NoxShared.ECKEY.GetECPrivate(new NoxManagedArray(vch, vch.Length));
				TEEPubCompressed = new NBitcoin.PubKey(TEEPubKey.ToBytes()).Compress(true);
			}
			catch { }
		}

		private readonly ECPrivateKeyParameters eCPrivate;

		public PubKey TEEPubCompressed { get; private set; }

		static readonly byte[] pkgSigner = new byte[] { 4, 153, 122, 107, 94, 140, 29, 86, 213, 157, 43, 169, 181, 243, 252, 221, 40, 227, 11, 44, 178, 46, 202, 76, 5, 58, 41, 38, 25, 182, 168, 92, 145, 178, 26, 210, 214, 34, 166, 223, 240, 107, 95, 174, 4, 91, 137, 160, 153, 242, 39, 140, 188, 132, 31, 42, 134, 6, 174, 125, 198, 106, 48, 234, 152 };

		const string dummyKey = "5Jcs7yFWtMBaKesMhQMaKVXmbnLuS6LMaWxrrjuzW8b7BT1B8wi";

		public Device(string _B58TEEKey, string _DeviceID) : this(new BitcoinSecret(_B58TEEKey).PrivateKey, new Guid(_DeviceID)) { }
		public Device(string _HexTEEKey, Guid _DeviceID) : this(new Key(_HexTEEKey.HexToByteArray(), -1, false), _DeviceID) { }

		public int MsgCount { get; set; }

		public string DevicePubHash() { return TEEKey.GetBitcoinSecret(Network.Main).GetAddress().Hash.ToString(); }
		public string WS_PubHash() { return WS_PUB.Hash.ToString(); }
		public PubKey PubTEE() { return TEEPubKey; }
		public string GetWalletID() { return WALLETID; }
		public string GetDeviceID() { return DeviceID.ToString(); }

		public bool IsErrorKey()
		{
			try
			{
				if (DevicePubHash().ToUpper() == "A7D702365ACF44FE1D2ACE958ACC127FC85DB920")
				{
					return true;
				}
			}
			catch { }
			return false;
		}

		public string GetHardwareID()
		{
			return DeviceID.ToByteArray().ToHex();
		}

		public string SignMsg(string msg) { return TEEKey.SignMessage(msg); }

		public bool ValidMsg(string msg, string signature)
		{
			string hash = SHashSHA1(msg);
			return WS_PUB.VerifyMessage(hash, signature);
		}

		public string SHashSHA1(string msg)
		{
			byte[] input = System.Text.Encoding.ASCII.GetBytes(msg);

			using (System.Security.Cryptography.SHA1Managed sha1 = new System.Security.Cryptography.SHA1Managed())
			{
				return sha1.ComputeHash(input).ToHex().ToUpper();
			}
		}

		public string signerPubKeyHex
		{
			get
			{
				return pkgSigner.ToHex();
			}
		}

		public bool ValidUpdateSignature(byte[] buffer, string Signature)
		{
			using (var sha = new SHA256Managed())
			{
				NBitcoin.Crypto.ECKey eCKey = new NBitcoin.Crypto.ECKey(signerPubKeyHex.HexToByteArray(), false);
				NBitcoin.Crypto.ECDSASignature eCDSASignature = new NBitcoin.Crypto.ECDSASignature(Signature.HexToByteArray());
				return eCKey.Verify(new uint256(sha.ComputeHash(buffer)), eCDSASignature);
			}
		}

		public bool ValidUpdateSignature(string hash, string Signature)
		{
			try
			{
				NBitcoin.Crypto.ECKey eCKey = new NBitcoin.Crypto.ECKey(signerPubKeyHex.HexToByteArray(), false);
				NBitcoin.Crypto.ECDSASignature eCDSASignature = new NBitcoin.Crypto.ECDSASignature(Signature.HexToByteArray());
				return eCKey.Verify(new uint256(hash.HexToByteArray()), eCDSASignature);
			}
			catch
			{
				return false;
			}
		}

		public string GetUpdateSignature(string Message)
		{
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(Message);
			using (var sha = new SHA256Managed())
			{
				var sig = TEEKey._ECKey.Sign(sha.ComputeHash(msg));
				var der = sig.ToDER();
				return der.ToHex().ToLower();
			}
		}

		public string GetUpdateSignature(byte[] Message)
		{
			using (var sha = new SHA256Managed())
			{
				var sig = TEEKey._ECKey.Sign(Message);
				var der = sig.ToDER();
				return der.ToHex().ToLower();
			}
		}

		public List<byte> Get3DGSaltHash(byte[] _noxSalt)
		{
			using (NoxManagedArray _Resized3DGSalt = new NoxManagedArray(_noxSalt, 3))
			{
				using (var hmac = new System.Security.Cryptography.HMACSHA256(TEEKey.ToBytes()))
				{
					NoxManagedArray khash = new NoxManagedArray(hmac.ComputeHash(_Resized3DGSalt.Value));
					NoxManagedArray shash = new NoxManagedArray(hmac.ComputeHash(DeviceID.ToByteArray()));

					var scrypt = SCrypt.ComputeDerivedKey(khash, shash, 1024, 1, 1, 2, 4).Result;

					return BitfiWallet.Nox.Sclear.EncodeB58NoxHash(scrypt.Value, 0, scrypt.Value.Length);

					khash.Dispose();
					shash.Dispose();
					scrypt.Dispose();

				}
			}
		}

		public Dictionary<Guid, WalletLibrary.NoxShared.SharedSession> sharedSessions = new Dictionary<Guid, WalletLibrary.NoxShared.SharedSession>();
		public NoxKeys.SignTransferResponse signTransferResponse { get; set; }


		public byte[] GetSharedSecret(byte[] pubKey)
		{
			using (SHA256Managed sha = new SHA256Managed())
			{
				return sha.ComputeHash(WalletLibrary.NoxShared.ECKEY.GetSharedPubkey(eCPrivate, WalletLibrary.NoxShared.ECKEY.GetECPublic(pubKey)));

			}
		}

	}
}
