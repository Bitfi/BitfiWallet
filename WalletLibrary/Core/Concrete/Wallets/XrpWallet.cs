using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class XrpWallet : CommonWallet, IWallet
  {
    public XrpWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.XRP)
      : base(keySets, product)
    { }

    public string GetLegacyAddress(uint index = 0)
    {
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        RipGen ripGen = new RipGen();
        return ripGen.GetAddress(privateKey.Value);
      }
    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      throw new NotImplementedException();
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        byte[] bts = Convert.FromBase64String(req.MXTxn);
        string MXTxn = System.Text.Encoding.UTF8.GetString(bts);

        RipGen ripGen = new RipGen();

        string fromadr = ripGen.GetAddress(privateKey.Value);

        if (fromadr != req.NoxAddress.BTCAddress)
          throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

        JsonSerializerSettings ser = new JsonSerializerSettings();
        ser.MissingMemberHandling = MissingMemberHandling.Ignore;
        ser.NullValueHandling = NullValueHandling.Ignore;
        ser.ObjectCreationHandling = ObjectCreationHandling.Auto;
        ser.TypeNameHandling = TypeNameHandling.All;
        var data = JsonConvert.DeserializeObject<RipTxnModel>(MXTxn, ser);

        if (!string.IsNullOrEmpty(data.Amount))
        {
          var chamount = Convert.ToDecimal(data.Amount) * 0.000001M;
          if (chamount > Convert.ToDecimal(req.Amount))
            throw new Exception("Error parsing amount.");
        }

        if (!string.IsNullOrEmpty(data.Destination))
        {
          if (data.Destination.ToUpper() != req.ToAddress.ToUpper())
            throw new Exception("Error parsing destination.");
        }

        string txn = ripGen.CreateTxn(privateKey.Value, MXTxn);
        
        response.TxnHex = txn;
        var xrpDecimals = 6;
        var feeBtc = NumUtils.Utils.FromSatoshi(BigInteger.Parse(data.Fee), xrpDecimals);

        response.Addition = new Addition
        {
          FeeAmount = feeBtc,
          DestinationTag = data.DestinationTag,
          TransactionType = data.TransactionType
        };
        return response;
      }
    }
  }
}
