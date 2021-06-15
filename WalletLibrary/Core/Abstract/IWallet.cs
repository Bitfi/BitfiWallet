using NoxKeys;
using NoxService.NWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Concrete;
using WalletLibrary.EosCore.ActionArgs;

namespace WalletLibrary.Core.Abstract
{
  public struct Addition
  {
    public string MaxFeeAllowed { get; set; }
    public string FeeAmount { get; set; }
    public string FeeUsd { get; set; }
    public string FeeWarning { get; set; }
    public string[] SpendKeyImages { get; set; }
    public string DestinationTag { get; set; }
    public string PaymentId { get; set; }
    public string TransactionType { get; set; }
    public string ContractAddress { get; set; }
    public string ContractType { get; set; }

  }

  public class SwapAddition
  {
    public string SwapType { get; set; }
    public string SwapToSymbol { get; set; }
    public string SwapToAmount { get; set; }
    public string SwapToAmountUSD { get; set; }
    public string SwapCostUSD { get; set; }
    public string SwapNetworkFee { get; set; }
    public bool FixedRate { get; set; }
  }
  public struct EosAddition
  {
    public BuyRamArgs BuyRamArgs { get; set; }
    public SellRamArgs SellRamArgs { get; set; }
    public EosUnstakeArgs EosUnstakeArgs { get; set; }
    public EosStakeArgs EosStakeArgs { get; set; }
    public TransferArgs TransferArgs { get; set; }
  }

  public struct PaymentRequestResponse
  {
    public string TxnHex { get; set; }
    public Addition Addition { get; set; }
    public EosAddition EosAddition { get; set; }
    public SwapAddition SwapAddition { get; set; }
    public string TransactionType { get; set; }
  }

  public interface IWallet : IDisposable
  {
    string Symbol { get; }
    
    string GetLegacyAddress(UInt32 index = 0);
    string GetSegwitAddress(UInt32 index = 0); //optional

    PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req);
    MsgTaskTransferResponse SignMessage(NoxService.NWS.NoxMsgProcess req); //optional

    ISigner GetSigner(NativeSecp256k1ECDSASigner.SignatureType type, UInt32 index = 0);
  }
}
