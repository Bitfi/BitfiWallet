using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletLibrary.Core.Abstract;
using Newtonsoft.Json;

namespace NoxKeys
{

  public class TokenResponse
  {
    public int Decimals { get; set; }
    public decimal Rate { get; set; }
  }
  public class BCUnspent
  {
    public string Amount { get; set; }
    public string TxHash { get; set; }
    public int OutputN { get; set; }
    public string Address { get; set; }
  }
  public class AdrCollection
  {
    public string BTC { get; set; }
    public string LTC { get; set; }
    public string[] XMR { get; set; }
    public string ETH { get; set; }
    public string NEO { get; set; }
  }
  public class SignTransferResponse
  {
    public string ToAddress { get; set; }
    public string LineID { get; set; }
    public string Blk { get; set; }
    public string BlkDisplayName { get; set; }
    public string Amount { get; set; }
    public string AmountUSD { get; set; }
    public double _AmountUSD { get; set; }
    public PaymentRequestResponse paymentRequestResponse { get; set; }

  }
  public struct XMRTaskTransferResponse
  {
    public string TxnHex { get; set; }
    public string[] SpendKeyImages { get; set; }
  }
  public struct XMRTaskImageResponse
  {
    public string WalletAddress { get; set; }
    public string[] SpendKeyImages { get; set; }
  }
  public struct RipTaskTransferResponse
  {
    public string TxnHex { get; set; }
  }
  public struct MsgTaskTransferResponse
  {
    public string MsgSig { get; set; }
  }
  public struct ApolloTaskTransferResponse
  {
    public string TxnHex { get; set; }
  }
  public struct NEOTaskTransferResponse
  {
    public string TxnHex { get; set; }
  }
  public struct ETHTaskTransferResponse
  {
    public string TxnHex { get; set; }
  }
  public struct AltoCoinTaskTransferResponse
  {
    public string FeeAmount { get; set; }

    public string FeeWarning { get; set; }
    public string TxnHex { get; set; }
  }
  public struct NEONeoscanUnspent
  {
    public string txid { get; set; }
    public decimal value { get; set; }
    public int n { get; set; }
  }
  public class NEONeoscanUnclaimed
  {
    public NEONeoscanClaimable[] claimable { get; set; }
  }

  public struct NEONeoscanClaimable
  {
    public string txid { get; set; }
    public decimal unclaimed { get; set; }
    public int n { get; set; }

  }

  public static class DataModels
  {
    public static string SerializeResponse(AuthResponse response)
    {
      var json = JsonConvert.SerializeObject(response);
      return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    }
    public static AuthRequest DeserializeRequest(string Json)
    {
      if (string.IsNullOrEmpty(Json)) throw new Exception("[WS] Error getting signature request.");

      byte[] bts = Convert.FromBase64String(Json);
      Json = System.Text.Encoding.UTF8.GetString(bts);

      var AuthRequest = JsonConvert.DeserializeObject<AuthRequest>(Json);

      if (!string.IsNullOrEmpty(AuthRequest.ErrorMessage)) throw new Exception(AuthRequest.ErrorMessage);

      if (string.IsNullOrEmpty(AuthRequest.DerivationIndex)) AuthRequest.DerivationIndex = "0";

      try
      {
        uint index = Convert.ToUInt32(AuthRequest.DerivationIndex);
      }
      catch
      {
        throw new Exception("[WS] Invalid derivation index.");
      }

      switch (AuthRequest.authType)
      {
        case AuthType.bfa_registration:
        case AuthType.bfa_authorization:
          AuthRequest.BFA_GuidDdata = FormatDataForSigning(AuthRequest.BFA_GuidDdata);
          return AuthRequest;

        case AuthType.bfa_registration_bitfi_profile:
          AuthRequest.BITFI_GuidData = FormatDataForSigning(AuthRequest.BITFI_GuidData);
          AuthRequest.BFA_GuidDdata = FormatDataForSigning(AuthRequest.BFA_GuidDdata);
          return AuthRequest;

        case AuthType.bitfi_authorization:
          AuthRequest.BITFI_GuidData = FormatDataForSigning(AuthRequest.BITFI_GuidData);
          return AuthRequest;

        default:
          throw new Exception("Invalid request.");
      }
    }
    static string FormatDataForSigning(string guid_data)
    {
      try
      {
        Guid guidMessage = Guid.Parse(guid_data);
        if (guidMessage == Guid.Empty) throw new Exception("Invalid data for signing.");
        using (var sha = new System.Security.Cryptography.SHA256Managed())
        {
          byte[] hash = sha.ComputeHash(guidMessage.ToByteArray());
          return hash.ByteToHex();
        }
      }
      catch (Exception)
      {
        throw new Exception("Invalid data for signing.");
      }
    }
    public static string ByteToHex(this byte[] data)
    {
      return BitConverter.ToString(data).Replace("-", "");
    }
  }
  public class AuthRequest
  {
    public AuthType authType { get; set; }
    public string BFA_GuidDdata { get; set; }
    public string BITFI_GuidData { get; set; }
    public string DerivationIndex { get; set; }
    public string BFA_PublicKey { get; set; }
    public string ErrorMessage { get; set; }
  }
  public class AuthResponse
  {
    public string SMSToken { get; set; }
    public string BFA_PublicKey { get; set; }
    public string BFA_Signature { get; set; }
    public string BITFI_PublicKey { get; set; }
    public string BITFI_Signature { get; set; }
    public string DisplayToken { get; set; }
  }
  public enum AuthType
  {
    bitfi_authorization = 0,
    bfa_registration = 1,
    bfa_registration_bitfi_profile = 2,
    bfa_authorization = 3
  }
}