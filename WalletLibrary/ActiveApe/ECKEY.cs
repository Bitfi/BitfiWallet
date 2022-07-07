using System;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using NoxKeys;

namespace WalletLibrary.ActiveApe
{
	internal class ECKEY
	{
		static readonly BigInteger HALF_CURVE_ORDER;
		static readonly BigInteger CURVE_ORDER;
		static readonly ECDomainParameters CURVE;
		static readonly X9ECParameters _Secp256k1;

		static X9ECParameters Secp256k1 => _Secp256k1;
		static ECDomainParameters _DomainParameter;
		static ECKEY()
		{
			_Secp256k1 = SecNamedCurves.GetByName("secp256k1");
			CURVE = new ECDomainParameters(_Secp256k1.Curve, _Secp256k1.G, _Secp256k1.N, _Secp256k1.H);
			HALF_CURVE_ORDER = _Secp256k1.N.ShiftRight(1);
			CURVE_ORDER = _Secp256k1.N;
		}
		public static byte[] GetSharedPubkey(ECPrivateKeyParameters privKey, ECPublicKeyParameters pub)
		{
			var q = pub.Q.Multiply(privKey.D).Normalize();
			if (q.IsInfinity) throw new InvalidOperationException("Infinity is not a valid agreement value for ECDH");

			var pubkey = Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger());
			pubkey = pubkey.Normalize();
			var param = GetECPublic(pubkey.GetEncoded(false));
			return GetPubKey(param, true);
		}
		public static ECPrivateKeyParameters GetECPrivate(NoxManagedArray vch)
		{
			return new ECPrivateKeyParameters(new BigInteger(1, vch.Value), DomainParameter);
		}
		public static ECPrivateKeyParameters GetECPrivate(byte[] vch)
		{
			return new ECPrivateKeyParameters(new BigInteger(1, vch), DomainParameter);
		}
		public static ECPublicKeyParameters GetECPublic(byte[] vch)
		{
			var q = Secp256k1.Curve.DecodePoint(vch);
			return new ECPublicKeyParameters("EC", q, DomainParameter);
		}
		static ECDomainParameters DomainParameter
		{
			get
			{
				if (_DomainParameter == null) _DomainParameter = new ECDomainParameters(
						Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H);

				return _DomainParameter;
			}
		}
		public static byte[] GetPubKey(ECPublicKeyParameters param, bool isCompressed)
		{
			var q = param.Q;
			q = q.Normalize();
			return Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(isCompressed);
		}
		public static byte[] GetPubKey(ECPrivateKeyParameters param, bool isCompressed)
		{
			var q = GetPublicKeyParameters(param).Q;
			q = q.Normalize();
			return Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(isCompressed);
		}
		static ECPublicKeyParameters GetPublicKeyParameters(ECPrivateKeyParameters eCPrivateKeyParameters)
		{
			var q = Secp256k1.G.Multiply(eCPrivateKeyParameters.D);
			return new ECPublicKeyParameters("EC", q, DomainParameter);
		}
		static Org.BouncyCastle.Math.EC.ECPoint DecompressKey(BigInteger xBN, bool yBit)
		{
			var curve = Secp256k1.Curve;
			var compEnc = X9IntegerConverter.IntegerToBytes(xBN, 1 + X9IntegerConverter.GetByteLength(curve));
			compEnc[0] = (byte)(yBit ? 0x03 : 0x02);
			return curve.DecodePoint(compEnc);
		}
	}

}
