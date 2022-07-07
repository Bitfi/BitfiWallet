using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.Signer;
using WalletLibrary.Core.Abstract;
using WalletLibrary.ActiveApe.ApeShift;

namespace WalletLibrary.ActiveApe
{

	public class MetaSession
	{

		private Dictionary<string, string> Challenges = new Dictionary<string, string>();
		private Dictionary<string, string> Authorizations = new Dictionary<string, string>();
	 private AesCryptoServiceProvider aes = new AesCryptoServiceProvider();


		ECPrivateKeyParameters _sessionKey;

		readonly byte[] PING_PREFIX = NBitcoin.DataEncoders.Encoders.ASCII.DecodeData("PING");
		readonly byte[] PONG_PREFIX = NBitcoin.DataEncoders.Encoders.ASCII.DecodeData("PONG");
		readonly byte[] CHALLENGE_PREFIX = "90c55055".HexToByteArray();
		readonly byte[] OAUTH_PREFIX = "e9d92935".HexToByteArray();

		public MetaSession()
		{
			var session = new NBitcoin.Key();

			aes.Padding = PaddingMode.PKCS7;
			aes.KeySize = 128;
			_sessionKey = session._ECKey.PrivateKey;
		}

		public void Dispose()
		{
			aes.Dispose();
		}

		public byte[] MyPublicKey()
		{
			return ECKEY.GetPubKey(_sessionKey, true);
		}

		private byte[] SharedSecret(byte[] PeerPublicKey)
		{
			using (SHA512Managed sha = new SHA512Managed())
			{
				return sha.ComputeHash(ECKEY.GetSharedPubkey(_sessionKey, ECKEY.GetECPublic(PeerPublicKey)));
			}
		}

		private string GetDataForSigning(string PubKey)
		{
			try
			{
				NBitcoin.PubKey test = new NBitcoin.PubKey(PubKey.HexToByteArray()).Compress(true);

				if (test.ToHex().ToLower() != PubKey.ToLower())
					throw new Exception("Invalid public key.");
			}
			catch (Exception ex)
			{
				throw new OrderStateException(ex.Message);
			}

			string msg = Guid.NewGuid().ToByteArray().ToHex();
			Challenges[msg] = PubKey;
			return msg;
		}

		public void AddAuthorization(string Code, string PubKey)
		{
			Authorizations[Code.ToLower()] = PubKey.ToLower();
		}

		private bool IsMagicAuthorized(string Code, string PubKey)
		{
			if (Authorizations.ContainsKey(Code.ToLower()))
			{
				if (Authorizations[Code.ToLower()] == PubKey.ToLower())
					return true;
			}

			return false;
		}

		public byte[] GetSignDataResponse(byte[] data)
		{
			return CHALLENGE_PREFIX.Concat(GetDataForSigning(data.ToHex()).HexToByteArray());
		}

		public byte[] GetSessionPubKeyResponse()
		{
			return OAUTH_PREFIX.Concat(MyPublicKey());
		}
		public string[] GetAuthorizationResponse(byte[] payload)
		{

			try
			{
				string Msg = payload.SafeSubarray(0, 16).ToHex();

				if (Challenges.ContainsKey(Msg))
				{
					string Signature = payload.SafeSubarray(16).ToHex();

					NBitcoin.Crypto.ECKey eCKey = new NBitcoin.Crypto.ECKey(
						Challenges[Msg].HexToByteArray(), false);
					NBitcoin.Crypto.ECDSASignature eCDSASignature = new NBitcoin.Crypto.ECDSASignature(
						Signature.HexToByteArray());

					byte[] msg = Msg.HexToByteArray();

					using (var sha = new SHA256Managed())
					{
						if (eCKey.Verify(new NBitcoin.uint256(sha.ComputeHash(msg)), eCDSASignature))
						{
							var code = GetDisplaySessionCode(Challenges[Msg].HexToByteArray(), msg).ToHex();
							return new string[] { code, Challenges[Msg] };
						}
					}
				}
			}
			catch { }

			throw new OrderStateException("403 - Invalid message or signature");
		}

		private byte[] GetDisplaySessionCode(byte[] pubKey, byte[] DataForSigning)
		{
			using (var rip = new RIPEMD160Managed())
			using (var sh = new SHA256Managed())
			using (var cs = MD5.Create())
			{
				var key160 = rip.ComputeHash(sh.ComputeHash(pubKey));

				byte[] data = key160.Concat(DataForSigning);
				byte[] hash = cs.ComputeHash(data);
				return hash.SafeSubarray(0, 4);
			}
		}

		public request_type GetRequestType(byte[] payload, out byte[] data)
		{
			if (payload == null || payload.Length == 0)
				throw new OrderStateException("Null payload.");


			try
			{
				if (payload.Length == PING_PREFIX.Length)
				{
					if (ArrayEqual(payload.SafeSubarray(0, PING_PREFIX.Length), PING_PREFIX))
					{
						data = NBitcoin.DataEncoders.Encoders.ASCII.DecodeData("PONG");
						return request_type.ping_pong;
					}
				}

				if (ArrayEqual(payload.SafeSubarray(0, CHALLENGE_PREFIX.Length), CHALLENGE_PREFIX))
				{
					data = payload.SafeSubarray(CHALLENGE_PREFIX.Length);
					return request_type.get_signing_data;
				}
				if (ArrayEqual(payload.SafeSubarray(0, OAUTH_PREFIX.Length), OAUTH_PREFIX))
				{
					data = payload.SafeSubarray(OAUTH_PREFIX.Length);
					return request_type.get_authorization;
				}

			}
			catch { }

			data = payload;
			return request_type.process_request;

		}

		public byte[] Decrypt(byte[] encrypted, MQDiffieResult sender)
		{

			try
			{

				if (encrypted.Length < 85)
					throw new Exception("Encrypted payload lenght less than 85.");

				var magic = encrypted.SafeSubarray(0, 4);
				var ephemeralPubkeyBytes = encrypted.SafeSubarray(4, 33);
				var cipherText = encrypted.SafeSubarray(37, encrypted.Length - 32 - 37);
				var mac = encrypted.SafeSubarray(encrypted.Length - 32);

				if (!IsMagicAuthorized(magic.ToHex(), ephemeralPubkeyBytes.ToHex()))
					throw new Exception("Invalid, unauthorized session code.");

				var sharedKey = SharedSecret(ephemeralPubkeyBytes);
				sender.ECPublicKey = sharedKey.ToHex();
				sender.ECDisplayCode = magic.ToHex();

				var iv = sharedKey.SafeSubarray(0, 16);
				var encryptionKey = sharedKey.SafeSubarray(16, 16);
				var hashingKey = sharedKey.SafeSubarray(32);

				var hashMAC = new HMACSHA256(hashingKey).ComputeHash(encrypted.SafeSubarray(0, encrypted.Length - 32));

				if (!ArrayEqual(mac, hashMAC))
					throw new Exception("Invalid.");

				return AesUnprotect(cipherText, encryptionKey, iv);

			}
			catch (Exception ex)
			{
				throw new OrderStateException("401");
			}
		}

		public byte[] Encrypt(string Msg, string SharedKey)
		{
			return Encrypt(System.Text.Encoding.UTF8.GetBytes(Msg), SharedKey.HexToByteArray());
		}

		public byte[] Encrypt(byte[] message, byte[] sharedKey)
		{
			var iv = sharedKey.SafeSubarray(0, 16);
			var encryptionKey = sharedKey.SafeSubarray(16, 16);
			var hashingKey = sharedKey.SafeSubarray(32);

			var cipherText = AesProtect(message, encryptionKey, iv);
			var ephemeralPubkeyBytes = MyPublicKey();
			var encrypted = NBitcoin.DataEncoders.Encoders.ASCII.DecodeData("AIE1").Concat(ephemeralPubkeyBytes, cipherText);
			var hashMAC = new HMACSHA256(hashingKey).ComputeHash(encrypted);
			return encrypted.Concat(hashMAC);

		}

	 byte[] AesProtect(byte[] bytes, byte[] KEY, byte[] IV)
		{
			using (var encryptorTransformer = aes.CreateEncryptor(KEY, IV))
				return encryptorTransformer.TransformFinalBlock(bytes, 0, bytes.Length);
		}

		byte[] AesUnprotect(byte[] bytes, byte[] KEY, byte[] IV)
		{
			using (var decryptorTransformer = aes.CreateDecryptor(KEY, IV))
				return decryptorTransformer.TransformFinalBlock(bytes, 0, bytes.Length);
		}

		static bool ArrayEqual(byte[] a, byte[] b)
		{
			if (a == null && b == null)
				return true;
			if (a == null)
				return false;
			if (b == null)
				return false;
			return ArrayEqual(a, 0, b, 0, Math.Max(a.Length, b.Length));
		}

		static bool ArrayEqual(byte[] a, int startA, byte[] b, int startB, int length)
		{
			if (a == null && b == null)
				return true;
			if (a == null)
				return false;
			if (b == null)
				return false;
			var alen = a.Length - startA;
			var blen = b.Length - startB;

			if (alen < length || blen < length)
				return false;

			for (int ai = startA, bi = startB; ai < startA + length; ai++, bi++)
			{
				if (a[ai] != b[bi])
					return false;
			}
			return true;
		}

	}


}
