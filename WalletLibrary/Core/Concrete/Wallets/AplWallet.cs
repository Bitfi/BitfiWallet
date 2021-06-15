using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Apollo;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class AplWallet : CommonWallet, IWallet
  {
    public AplWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.APL)
      : base(keySets, product)
    { }

    public string GetLegacyAddress(uint index = 0)
    {
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        APLGen apl = new APLGen();
        return apl.GetAccountID(privateKey.Value);
      }
    }
    public string GetPubKey(uint index = 0)
    {
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        APLGen apl = new APLGen();
        return apl.GetPublicKey(privateKey.Value);
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
        APLGen apl = new APLGen();

        string fromadr = apl.GetPublicKey(privateKey.Value);

        if (fromadr != req.NoxAddress.BTCAddress)
          throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

        string check = apl.CheckTransactionError(req.MXTxn, req.ToAddress, req.Amount, req.FeeValue);
        if (!string.IsNullOrEmpty(check))
          throw new Exception(check);

        var serialized = req.MXTxn;
        var info = serialized.deserialize();

        var signedtxn = apl.SignTransaction(privateKey.Value, req.MXTxn);
        response.TxnHex = signedtxn;

        var aplDecimals = 8;
        var feeBtc = NumUtils.Utils.FromSatoshi(BigInteger.Parse(info.fee), aplDecimals);

        response.Addition = new Addition
        {
          FeeAmount = feeBtc
        };
        return response;
      }
    }
  }
}
