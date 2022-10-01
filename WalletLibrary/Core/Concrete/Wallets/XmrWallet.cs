using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class XmrWallet : CommonWallet, IWallet
  {
    NoxKeys.MoneroWallet.Models.Wallet xmrWallet;

    public XmrWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.XMR)
      : base(keySets, product)
    {
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        xmrWallet = NoxKeys.MoneroWallet.Models.Wallet.OpenWallet(privateKey.Value);
      }
    }

    public string GetLegacyAddress(uint index = 0)
    {
      return xmrWallet.Address;
    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();
      var addition = new Addition();
      

      if (xmrWallet.Address != req.NoxAddress.BTCAddress)
        throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

      NoxKeys.MoneroWallet.XMRGen xMRGen = new NoxKeys.MoneroWallet.XMRGen(xmrWallet);
      var send = xMRGen.XMR_Sign(req.ToAddress, req.Amount, req.MXTxn, req.NoxAddress.BTCAddress);

      if (!string.IsNullOrEmpty(send.Error))
      {
        throw new Exception(send.Error);
      }

      addition.SpendKeyImages = send.SpendKeyImages;

      try
      {
        var MoneroDecimals = 12;
        if (!string.IsNullOrEmpty(send.FeeSatUsed))
        {
          addition.FeeAmount = NumUtils.Utils.FromSatoshi(BigInteger.Parse(send.FeeSatUsed), MoneroDecimals);
        }
      }
      catch { }

    var deserialized = xMRGen.ConvertFromString(req.MXTxn);

      if (deserialized != null) addition.PaymentId = deserialized.PaymentIdString ?? "";


      try
      {
        var MoneroDecimals = 12;

          addition.MaxFeeAllowed = NumUtils.Utils.FromSatoshi(BigInteger.Parse(deserialized.MaxAllowedFee), MoneroDecimals);
        
      }
      catch { }

      response.TxnHex = send.TxnHex;
      response.Addition = addition;
      return response;
    }

    public XMRTaskImageResponse GetImages(ImageRequestTable[] requestTable)
    {
     throw new NotImplementedException();
    }
    
    public string GetViewKey()
    {
      return xmrWallet.Keys.ViewSecret.ToHex();
    }
    
    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      throw new NotImplementedException();
    }
  }
}
