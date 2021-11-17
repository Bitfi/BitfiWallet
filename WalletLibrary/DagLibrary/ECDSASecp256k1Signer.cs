using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1.X9;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Asn1;
using System.IO;
using System;

namespace DagLibrary
{
  class ECDSASecp256k1Signer
  {
    private X9ECParameters curve;
    private ECDomainParameters domainParams;

    public ECDSASecp256k1Signer()
    {
      this.curve = ECNamedCurveTable.GetByName("secp256k1");
      this.domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
    }

    public byte[] Sign(byte[] sk, string msg)
    {

      var sha512hashBytes = Hash.Sha512(Encoding.UTF8.GetBytes(msg));
      var sha515hash = Utils.ByteArray2hex(sha512hashBytes).ToLower();

      var skPoint = new BigInteger(1, sk);
      ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(skPoint, this.domainParams);

      var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));

      signer.Init(true, privKey);
      var sig = signer.GenerateSignature(sha512hashBytes);

      byte[] dersig;
      using (MemoryStream ms = new MemoryStream())
      using (Asn1OutputStream asn1stream = new Asn1OutputStream(ms))
      {
        DerSequenceGenerator seq = new DerSequenceGenerator(asn1stream);
        seq.AddObject(new DerInteger(sig[0]));
        seq.AddObject(new DerInteger(sig[1]));
        seq.Close();
        dersig = ms.ToArray();
      }

      var pkPoint = domainParams.G.Multiply(skPoint);
      var res = Verify(pkPoint, msg, sig);
      if (!res)
      {
        throw new System.Exception("Signature is not valid!");
      }

      return dersig;
    }

    public Boolean Verify(Org.BouncyCastle.Math.EC.ECPoint pubKey, string msg, BigInteger[] sig)
    {
      var sha512hashBytes = Hash.Sha512(Encoding.UTF8.GetBytes(msg));

      var publicParams = new ECPublicKeyParameters(pubKey, domainParams);
      var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));

      signer.Init(false, publicParams);
      var res = signer.VerifySignature(sha512hashBytes, sig[0], sig[1]);

      return res;
    }
  }
}