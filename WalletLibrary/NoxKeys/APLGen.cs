using Apollo;

namespace NoxKeys
{
  public class APLGen
  {
    public string GetPublicKey(byte[] keybytes)
    {
      var sec = ApolloLibrary.GetPassPhraseFromBytes(keybytes).StringToByteArray();

      return ApolloLibrary.GetPublicKey(sec).ByteArrayToHex();
    }

    public string GetAccountID(byte[] keybytes)
    {
      var sec = ApolloLibrary.GetPassPhraseFromBytes(keybytes).StringToByteArray();

      byte[] pubKey = ApolloLibrary.GetPublicKey(sec);
      return ApolloLibrary.GetAccountIdFromPublicKey(pubKey);
    }
    public string SignTransaction(byte[] keybytes, string Base64JsonTxnData)
    {
      var sec = ApolloLibrary.GetPassPhraseFromBytes(keybytes).StringToByteArray();

      var signedTx = ApolloLibrary.SignTransaction(Base64JsonTxnData.HexToByteArray(), sec).ByteArrayToHex();
      return signedTx;
    }
    public string CheckTransactionError(string Base64JsonTxnData, string ToAddress, string Amount, string Fee)
    {

      var serialized = Base64JsonTxnData;
      var info = serialized.deserialize();

      Amount = NumUtils.Utils.ToSatoshi(Amount, 8).ToString();
      //Fee = ToSatoshi(Fee, 8).ToString();
      if (info.amount != Amount) return "Invalid Amount.";
      if (info.to != ToAddress) return "Invalid Address.";
      if (info.fee != Fee) return "Invalid Fee.";

      return null;
    }

  }
}