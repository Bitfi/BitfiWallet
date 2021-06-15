using Newtonsoft.Json.Linq;
using Ripple.Signing;
using Ripple.TxSigning;
using System;
using System.Security.Cryptography;

namespace NoxKeys
{
  public class RipGen
  {
    private byte[] PublicKeyHash(byte[] bytes)
    {
      var hash = SHA256.Create();
      var riper = RIPEMD160.Create();
      bytes = hash.ComputeHash(bytes, 0, bytes.Length);
      return riper.ComputeHash(bytes, 0, bytes.Length);
    }

    private string GetAddressId(IKeyPair keypair)
    {

      return Ripple.Address.AddressCodec.EncodeAddress(PublicKeyHash(keypair.CanonicalPubBytes()));
    }

    public string GetAddress(byte[] seed)
    {
      var walletIndex = 0;
      var keypair = Ripple.Signing.K256.K256KeyGenerator.From128Seed(seed, walletIndex);
      return GetAddressId(keypair);
    }

    public string CreateTxn(byte[] seed, string JsonTxn)
    {
      var walletIndex = 0;
      var keypair = Ripple.Signing.K256.K256KeyGenerator.From128Seed(seed, walletIndex);

      var alt = TxSigner.FromKeyPair(keypair).SignJson(JObject.Parse(JsonTxn));

      return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(alt.Hash + "|" + alt.TxBlob + "|" + alt.TxJson));

    }
  }

  public class RipTxnModel
  {
    public string TransactionType { get; set; }
    public string Account { get; set; }
    public string Destination { get; set; }
    public string Amount { get; set; }
    public string Flags { get; set; }
    public string LastLedgerSequence { get; set; }
    public string Fee { get; set; }
    public string Sequence { get; set; }
    public string DestinationTag { get; set; }
  }
}