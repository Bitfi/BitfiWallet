using System;
using System.Numerics;
using System.Text;

namespace NoxKeys.MoneroWallet.Models
{
  public static class Converters
  {
    public static String ByteArrayToHex(byte[] ba)
    {
      string hex = BitConverter.ToString(ba);
      var res = hex.Replace("-", "");
      return res;
    }
    public static byte[] HexToByteArray(String hex)
    {
      if (!IsValidHex(hex))
      {
        throw new Exception("Invalid hex");
      }

      int NumberChars = hex.Length;
      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }

    public static bool IsValidHex(string hex)
    {
      if (hex == "" || hex == null)
      {
        return false;
      }

      var exp = new System.Text.RegularExpressions.Regex("[0-9a-fA-F]{" + hex.Length + "}");
      return exp.IsMatch(hex);
    }

  }
  public class Wallet : IDisposable
  {
    public enum Priority { Low = 1, Medium = 4, High = 20, Paranoid = 100 }
    public Priority currentPriority = Priority.Medium;
    public Keys Keys { get; private set; }
    public byte[] Seed { get; private set; }
    public string Address { get; private set; }
    public Wallet() { }

    private Wallet(Keys keys, byte[] seed, string address)
    {
      Keys = keys;
      Seed = seed;
      Address = address;
    }

    public static Wallet OpenWallet(byte[] seed)
    {

      MoneroWallet.XMRGen xmrgen = new MoneroWallet.XMRGen();
      var gwlt = xmrgen.GetWallet(Converters.ByteArrayToHex(seed));

      Keys keys = new Keys();
      keys.SpendPublic = Converters.HexToByteArray(gwlt.pub_spendKey_string);
      keys.SpendSecret = Converters.HexToByteArray(gwlt.sec_spendKey_string);
      keys.ViewPublic = Converters.HexToByteArray(gwlt.pub_viewKey_string);
      keys.ViewSecret = Converters.HexToByteArray(gwlt.sec_viewKey_string);

      string address = gwlt.address_string;

      return new Wallet(keys, seed, address);
    }
    public void Dispose()
    {
      for (int i = 0; i < Seed.Length; ++i)
      {
        Seed[i] = 0;
      }

      for (int i = 0; i < Keys.SpendSecret.Length; ++i)
      {
        Keys.SpendSecret[i] = 0;
      }

      for (int i = 0; i < Keys.SpendPublic.Length; ++i)
      {
        Keys.SpendPublic[i] = 0;
      }

      for (int i = 0; i < Keys.ViewSecret.Length; ++i)
      {
        Keys.ViewSecret[i] = 0;
      }
    }

  }
  public class Keys
  {
    //Spend
    public byte[] SpendPublic { get; set; }
    public byte[] SpendSecret { get; set; }
    //View 
    public byte[] ViewPublic { get; set; }
    public byte[] ViewSecret { get; set; }
  }
  public class Destination
  {
    public DecodedAddress Keys { get; set; }
    public string Address { get; set; }
    public string Amount { get; set; }
  }
  public class DecodedAddress
  {
    public string spend { get; set; }
    public string view { get; set; }
    public string intPaymentId { get; set; }
  }
  public class Rates
  {
    public Double AUD { get; set; }
    public Double BRL { get; set; }
    public Double BTC { get; set; }
    public Double CAD { get; set; }
    public Double CHF { get; set; }
    public Double CNY { get; set; }
    public Double EUR { get; set; }
    public Double GBP { get; set; }
    public Double HKD { get; set; }
    public Double INR { get; set; }
    public Double JPY { get; set; }
    public Double KRW { get; set; }
    public Double MXN { get; set; }
    public Double NOK { get; set; }
    public Double NZD { get; set; }
    public Double SEK { get; set; }
    public Double SGD { get; set; }
    public Double TRY { get; set; }
    public Double USD { get; set; }
    public Double RUB { get; set; }
    public Double ZAR { get; set; }
  }
  public class AddressInfo
  {
    public string lockedFunds { get; set; }
    public string totalReceived { get; set; }
    public string totalSent { get; set; }
    public string balance { get; set; }
    public Int64 scannedHeight { get; set; }
    public Int64 scannedBlockHeight { get; set; }
    public Int64 startHeight { get; set; }
    public Int64 transactionHeight { get; set; }
    public Int64 blockchainHeight { get; set; }
    public Output[] spentOutputs { get; set; }
    public Rates rates { get; set; }
  }
  public class LedgerData
  {
    public Destination[] destinations { get; set; }
    public Output[] usedOutputs { get; set; }
    public MixOutput[] mixOuts { get; set; }
    public Int64 mixin { get; set; }
    public BigInteger feeAmount { get; set; }
    public string paymentId { get; set; }
    public bool pidEncrypt { get; set; }
    public string realDestViewKey { get; set; }
    public UInt64 unlockTime { get; set; }
  }
  public class MixOutput
  {
    public string Amount { get; set; }
    public Output[] Outputs { get; set; }
  }
  public class Output
  {
    public string Key { get; set; }
    public string Amount { get; set; }
    public string GlobalIndex { get; set; }
    public UInt64 Height { get; set; }
    public UInt64 Index { get; set; }
    public string PublicKey { get; set; }
    public string Rct { get; set; }
    public string Commit { get; set; }
    public string[] SpendKeyImages { get; set; }
    public string KeyImage { get; set; }
    public Int32 outIndex { get; set; }
    public Int32 mixin { get; set; }
    public string Timestamp { get; set; }
    public string TxHash { get; set; }
    public UInt64 TxId { get; set; }
    public string TxPrefixHash { get; set; }
    public string TxPubKey { get; set; }
  }

  public class TxRecord
  {
    public bool coinbase { get; set; }
    public string hash { get; set; }
    public UInt64 height { get; set; }
    public UInt64 id { get; set; }
    public string amount { get; set; }
    public bool mempool { get; set; }
    public UInt64 mixin { get; set; }
    public string timestamp { get; set; }
    public string totalReceived { get; set; }
    public string totalSent { get; set; }
    public UInt64 unlockTime { get; set; }
    public Output[] spentOutputs { get; set; }
    public string approxFloatAmount { get; set; }
  }

  public class TxHistory
  {
    public UInt64 blockchainHeight { get; set; }
    public UInt64 scannedBlockchainHeight { get; set; }
    public UInt64 scannedHeight { get; set; }
    public UInt64 startHeight { get; set; }
    public string totalReceived { get; set; }
    public UInt64 transactionHeight { get; set; }
    public TxRecord[] transactions { get; set; }
  }

  public class TxToSubmit
  {
    public string rawTx { get; set; }
    public string[] keyImages { get; set; }
    public string error { get; set; }
  }
}