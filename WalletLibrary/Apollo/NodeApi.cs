using Apollo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo
{
  public class NodeApi
  {
    private string BaseUrl = ""; 

    public NodeApi(string url = "https://apollowallet.org")
    {
      BaseUrl = url + "/apl?requestType={0}&{1}";
    }

    public UnsignedTx ConstructSendMoneyTransaction(string recipientId, string publicKey, 
      string amountATMsatoshi,  string feeATMsatoshi = "100000000", int deadline = 60)
    {
      var completeUrl = String.Format(BaseUrl, "sendMoney", String.Format(
        "publicKey={0}&recipient={1}&amountATM={2}&feeATM={3}&deadline={4}",
        publicKey, recipientId, amountATMsatoshi, feeATMsatoshi, deadline
      ));

      JObject o = (JObject)JToken.FromObject(new { });
            return null;
    }

    public BroadcastResponse BroadcastTransaction(string transactionBytes)
    {
      var completeUrl = String.Format(BaseUrl, "broadcastTransaction", 
        String.Format("transactionBytes={0}", transactionBytes));
      JObject o = (JObject)JToken.FromObject(new { });
            return null;
    }

    public AccountTxes GetAccountTransactions(string accountId)
    {
      var completeUrl = String.Format(BaseUrl, "getBlockchainTransactions", String.Format("account={0}", accountId));
            return null;
    }

    public Account GetAccount(string accountId)
    {
      var completeUrl = String.Format(BaseUrl, "getAccount", String.Format("account={0}", accountId));
            return null;
    }
  }
}
