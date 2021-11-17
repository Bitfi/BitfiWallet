using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using NoxService.SWS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.Core.Concrete;
using WalletLibrary.Core.Concrete.Wallets;

namespace WalletLibrary
{
 public delegate void WalletEventHandler(WalletEventArgs e);

 public class WalletEventProxy
 {
  public event WalletEventHandler TaskCompleted;

  public event WalletEventHandler OnStatusChanged;

  public void HandleWalletEvent(WSTask arg)
  {
   TaskCompleted.Invoke(new WalletEventArgs(arg));
  }

  public void HandleStatusEvent(WSTask arg)
  {
   OnStatusChanged.Invoke(new WalletEventArgs(arg));
  }
 }

 public class WalletEventArgs : System.ComponentModel.AsyncCompletedEventArgs
 {
  WSTask _obj;
  public WalletEventArgs(WSTask result) : base(null, false, null) { _obj = result; }
  public WSTask GetResult() { return _obj; }
 }

 public abstract class Wallet
 {

  public WalletEventProxy _eventProxy;

  public WSTask statusTask;

  protected Wallet(string _action, string _actiontask)
  {
   action = _action; actiontask = _actiontask;

   MaxArrayLength = 250;
   NoxSalt = new NoxManagedArray(MaxArrayLength);
   NoxSecret = new NoxManagedArray(MaxArrayLength);

   WS = new NWS();
  }

  protected Wallet() { }

  protected abstract void NoxEvent(WalletEventArgs e);

  public async Task<WSTask> RunWalletAsync()
  {
   TaskCompletionSource<WalletEventArgs> ntask = new TaskCompletionSource<WalletEventArgs>();

   WalletEventProxy walletEventProxy = new WalletEventProxy();
   walletEventProxy.TaskCompleted += (e) => APM_Delegate(ntask, () => e);

   Task.Run(() =>
   {
    StartHandle(walletEventProxy);

   });

   var res = await ntask.Task;
   return res.GetResult();
  }

  async Task StartHandle(WalletEventProxy walletEventProxy)
  {
   walletEventProxy.HandleWalletEvent(EnterAction());
  }
  void APM_Delegate<T>(TaskCompletionSource<T> tcs, Func<T> getResult)
  {
   tcs.TrySetResult(getResult());
  }

  BitfiWallet.Device noxDevice
  {
   get
   {
    return BitfiWallet.NoxDPM.NoxT.noxDevice;
   }
  }

  NWS WS = new NWS();

  NoxManagedArray NoxSalt;

  NoxManagedArray NoxSecret;

  public string action; string actiontask; string display_token; string TryAddressHash;

  public bool SaltSelected; int MaxArrayLength;

  public void JavaClassInit(string _action, string _actiontask, string _display_token = null)
  {
   action = _action; actiontask = _actiontask;

   display_token = _display_token;

   MaxArrayLength = 250;
   NoxSalt = new NoxManagedArray(MaxArrayLength);
   NoxSecret = new NoxManagedArray(MaxArrayLength);
  }

  public void NoxStopArrays()
  {
   NoxSalt.Dispose();
   NoxSecret.Dispose();
   action = null;
   actiontask = null;
   display_token = null;
  }
  public void NoxResetArrays()
  {
   NoxSalt.Value.NoxWriteArray();
   NoxSecret.Value.NoxWriteArray();
  }
  private void NXArrayUpdate(byte addval, bool SaltSelected, bool Remove)
  {
   if (SaltSelected)
   {
    UpdateValue(NoxSalt.Value, addval, Remove);
   }
   else
   {
    UpdateValue(NoxSecret.Value, addval, Remove);
   }
  }
  private void UpdateValue(byte[] currentArray, byte addval, bool Remove)
  {
   int Length = GetActualLength(currentArray);
   if (Remove)
   {
    if (Length == 0) return;
    currentArray[Length - 1] = 0;
   }
   else
   {
    if (Length == MaxArrayLength) return;
    currentArray[Length] = addval;
   }
  }
  public static int GetActualLength(byte[] array)
  {
   int Length = 0;
   for (int i = 0; i < array.Length; i++)
   {
    if (array[i] != 0 && array[i] != 1)
    {
     Length = i + 1;
    }
    else
    {
     break;
    }

   }

   return Length;
  }
  public void NoxAdd(byte cval)
  {
   if (cval == 0) return;
   NXArrayUpdate(cval, SaltSelected, false);
   UpdateSelected();
  }
  public void NoxRemove()
  {
   NXArrayUpdate(0, SaltSelected, true);
   UpdateSelected();
  }

  public void UpdateSelected()
  {
   if (SaltSelected)
   {
    NoxUpdateView(NoxSalt.Value);
   }
   else
   {
    NoxUpdateView(NoxSecret.Value);
   }
  }
  public string WalletAction
  {
   get
   {
    return action;
   }

   internal set { }
  }
  public string WalletActionTask
  {
   get
   {
    return actiontask;
   }

   internal set { }
  }
  public string WalletDisplayToken
  {
   get
   {
    return display_token;
   }

   internal set { }
  }
  public virtual void NoxUpdateView(byte[] inputs) { }
  public List<byte> Get3DGSaltHash()
  {
   return noxDevice.Get3DGSaltHash(NoxSalt.Value);
  }
  public void NoxFinish()
  {
   NoxStopArrays();
  }

  public string GetInstrctionText()
  {
   using (NoxManagedArray _ResizedNoxSecret = new NoxManagedArray(NoxSecret.Value, GetActualLength(NoxSecret.Value)))
   {
    return System.Text.Encoding.UTF8.GetString(_ResizedNoxSecret.Value);
   }

  }
  public NoxManagedArray GetUserMessage()
  {
   return new NoxManagedArray(NoxSecret.Value, GetActualLength(NoxSecret.Value));
  }

  private WorkerTask DoWalletTask(string action)
  {

   using (NoxManagedArray _ResizedNoxSalt = new NoxManagedArray(NoxSalt.Value, GetActualLength(NoxSalt.Value)))
   {
    using (NoxManagedArray _ResizedNoxSecret = new NoxManagedArray(NoxSecret.Value, GetActualLength(NoxSecret.Value)))
    {
     NoxSalt.Value.NoxWriteArray(); NoxSecret.Value.NoxWriteArray();

     WorkerTask workerTask = new WorkerTask();


     try
     {
      using (var walletFactory = new WalletFactory(_ResizedNoxSecret, _ResizedNoxSalt, _eventProxy))
      {
       switch (action)
       {
        case "address":
        workerTask.Result = AddressTask(walletFactory);
        break;

        case "authorize2fa":
        case "register2fa":
        case "signin":

        statusTask.StatusMsg = "Getting authenticator message";
        _eventProxy.HandleStatusEvent(statusTask);

        workerTask.Result = DMA2SignInTask(walletFactory);
        break;
        case "overview":

        statusTask.StatusMsg = "Getting authenticator message";
        _eventProxy.HandleStatusEvent(statusTask);

        workerTask.Result = DMA2BalancesTask(walletFactory);
        break;
        case "accounts":

        statusTask.StatusMsg = "Getting authenticator message";
        _eventProxy.HandleStatusEvent(statusTask);

        workerTask.Result = DMA2AddressDisplayTask(walletFactory);
        break;
        case "txn":
        workerTask.Result = TxnProcessTask(walletFactory);
        break;
        case "swap":
        workerTask.Result = TxnProcessSwapTask(walletFactory);
        break;
        case "image":
        workerTask.Result = XMRImagesTask(walletFactory);
        break;
        case "gas":
        workerTask.Result = NEOGasTask(walletFactory);
        break;
        case "message":
        workerTask.Result = MessageTask(walletFactory);
        break;
        case "session_start":
        workerTask.Result = SessionSharedTask(walletFactory);
        break;

       }
      }
     }
     catch (Exception ex)
     {
      workerTask.Ex = ex;
     }



     return workerTask;
    }
   }
  }


  private WSTask EnterAction()
  {

   WorkerTask workerTask = DoWalletTask(action);

   if (workerTask.Ex != null) return new WSTask() { Success = false, ErrorMsg = workerTask.Ex.Message };
   var result = workerTask.Result;

   try
   {
    switch (action)
    {
     case "address":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      if (!string.IsNullOrEmpty(TryAddressHash))
      {
       return new WSTask() { Success = true, Prompt = true, PromptMsg = Result };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }


     case "register2fa":
     case "authorize2fa":
     case "signin":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      if (!string.IsNullOrEmpty(TryAddressHash))
      {
       return new WSTask() { Success = true, Prompt = true, PromptMsg = Result };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }


     case "overview":
     {

      string[] resp = new string[2] { result as string, result as string };
      return new WSTask() { Success = true, Result = resp, Prompt = true };

     }

     case "accounts":
     {


      return new WSTask() { Success = true, Result = result as string[], Prompt = true };
     }

     case "session_start":
     {

      return new WSTask() { Success = true, Result = result as string, Prompt = true };
     }

     case "txn":
     case "swap":
     {
      SignTransferResponse signTransferResponse = result as SignTransferResponse;
      string msg = "Send " + signTransferResponse.Amount + " to " + signTransferResponse.ToAddress + "?";

      return new WSTask() { Success = true, Prompt = true, PromptMsg = msg, Result = signTransferResponse };
     }

     case "image":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }

     case "gas":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }
     case "message":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }

     case "test":
     {
      string Result = result as string;
      if (Result == "0")
      {
       return new WSTask() { Success = true };
      }

      return new WSTask() { Success = false, ErrorMsg = Result };
     }

     default:
     return new WSTask() { Success = false, ErrorMsg = "Invalid Method." };

    }
   }
   catch (Exception ex)
   {
    return new WSTask() { Success = false, ErrorMsg = ex.Message };
   }

  }

  string TestTask(WalletFactory factory)
  {
   using (var btcWallet = factory.ConstructWallet(WalletFactory.Products.BTC))
   {
    return btcWallet.GetLegacyAddress();
   }

  }

  string XMRImagesTask(WalletFactory factory)
  {
   NoxImageRequests req = WS.GetImageRequest(actiontask);
   if (req == null) return "Error getting request, please retry.";

   var product = WalletFactory.Products.XMR;
   using (XmrWallet wallet = (XmrWallet)factory.ConstructWallet(product))
   {
    var resp = wallet.GetImages(req.PublicTable);
    return WS.RespondImageRequest(req.TXNLineID, resp.SpendKeyImages, resp.WalletAddress);
   }

  }

  string MessageTask(WalletFactory factory)
  {
   NoxMsgProcess req = WS.GetMsgRequest(actiontask);
   if (req == null) return "Error getting request, please retry.";

   var index = (uint)req.NoxAddress.HDIndex;
   uint[] indexes = new uint[] { index };
   var product = WalletFactory.SymbolToWalletProduct(req.BlkNet);

   using (var wallet = factory.ConstructWallet(product, indexes))
   {
    var msgTaskTransferResponse = wallet.SignMessage(req);
    return WS.SubmitMsgResponse(req.TXNLineID, msgTaskTransferResponse.MsgSig);
   }

  }

  string NEOGasTask(WalletFactory factory)
  {
   NoxGasRequests req = WS.GetGasRequest(actiontask);
   if (req == null) return "Error getting request, please retry.";

   var product = WalletFactory.Products.NEO;
   using (NeoWallet wallet = (NeoWallet)factory.ConstructWallet(product))
   {
    var resp = wallet.ClaimGas(req);
    return WS.SubmitGasResponse(req.TXNLineID, resp.TxnHex);
   }

  }

  SignTransferResponse TxnProcessSwapTask(WalletFactory factory)
  {

   SGAAuthenticator authenticator = new SGAAuthenticator(factory, _eventProxy);
   var signature = authenticator.SignTask(new Guid(actiontask));

   var req = WS.GetSwapRequest(actiontask, signature);
   if (req == null) throw new Exception("Error getting request.");
   if (!string.IsNullOrEmpty(req.ErrorMessage)) throw new Exception(req.ErrorMessage);


   statusTask.StatusMsg = "Creating Swap directly with Changelly";
   _eventProxy.HandleStatusEvent(statusTask);

   ChangellySwap.CreateTxnResp swapInfo;
   var swapbase = ChangellySwap.Changelly.CreateSwap(req.RequestObj, req.RequestSignature, req.APIAccount, out swapInfo);

   string CalculatedPayoutAddress = null;
   string CalculatedRefundAddress = null;

   var product = WalletFactory.SymbolToWalletProduct(req.ToBlkNet);
   using (var wallet = factory.ConstructWallet(product, new uint[] { (uint)req.PayoutAddressIndex }))
   {
    CalculatedPayoutAddress = req.PayoutAddressDoSegwit ? wallet.GetSegwitAddress((uint)req.PayoutAddressIndex) : wallet.GetLegacyAddress((uint)req.PayoutAddressIndex);
   }

   product = WalletFactory.SymbolToWalletProduct(req.FromBlkNet);
   using (var wallet = factory.ConstructWallet(product, new uint[] { (uint)req.RefundAddressIndex }))
   {
    CalculatedRefundAddress = req.RefundAddressDoSegwit ? wallet.GetSegwitAddress((uint)req.RefundAddressIndex) : wallet.GetLegacyAddress((uint)req.RefundAddressIndex);
   }

   if (swapInfo.refundAddress != CalculatedRefundAddress) throw new Exception("Invalid refund address.");
   if (swapInfo.payoutAddress != CalculatedPayoutAddress) throw new Exception("Invalid payout address.");

   statusTask.StatusMsg = "Building transaction";
   _eventProxy.HandleStatusEvent(statusTask);

   var resp = WS.GetSwapTransaction(actiontask, swapbase);
   if (resp == null) throw new Exception("Error creating txn1.");
   if (!string.IsNullOrEmpty(resp.ErrorMessage)) throw new Exception(resp.ErrorMessage);
   if (resp.Response.ToAddress != swapInfo.payinAddress) throw new Exception("Invalid payment address.");

   ValidateSwapAmount(resp.Response, swapInfo);

   var signTransferResponse = TxnProcessTask(factory, resp.Response);
   var paymentRequestResponse = signTransferResponse.paymentRequestResponse;
   paymentRequestResponse.SwapAddition = CalculateSwapAdditions(swapInfo, req.FixedRate, signTransferResponse, req.Tracking, req.CreateResponse);
   signTransferResponse.paymentRequestResponse = paymentRequestResponse;
   return signTransferResponse;

  }

  private void ValidateSwapAmount(NoxTxnProcess noxTxn, ChangellySwap.CreateTxnResp swapInfo)
  {

   double? Amount = null;

   if (noxTxn.BlkNet.ToUpper() == "ETH")
   {
    if (!string.IsNullOrEmpty(noxTxn.ETCToken))
    {
     try
     {
      TokenResponse tokenResponse = new TokenResponse();
      var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(noxTxn.ProcessH1));
      tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

      if (tokenResponse.Decimals > 0)
      {
       Amount = Convert.ToDouble(NumUtils.Utils.FromSatoshi(NumUtils.Utils.ToSatoshi(noxTxn.Amount, 18), tokenResponse.Decimals));
      }
     }
     catch
     {
      throw new Exception("Error validating amount.");
     }
    }
   }

   try
   {
    if (Amount.Equals(null)) Amount = Convert.ToDouble(noxTxn.Amount);

    if (!NumUtils.Utils.AmountEquals(Amount ?? 0, Convert.ToDouble(swapInfo.amountExpectedFrom), 1))
    {
     throw new Exception("Invalid transfer amount.");
    }
   }
   catch
   {
    throw new Exception("Error validating amount.");
   }
  }

  private Core.Abstract.SwapAddition CalculateSwapAdditions(ChangellySwap.CreateTxnResp swapInfo, bool FixedRate, SignTransferResponse signTransferResponse, string ToUSDRate, string SubNetworkFee)
  {

   var swapAddition = new Core.Abstract.SwapAddition();

   try
   {
    swapAddition.FixedRate = true;
    string swapType = "[fixed rate]";

    if (!FixedRate)
    {
     swapType = "[floating rate]";
     swapAddition.FixedRate = false;
    }

    swapAddition.SwapType = swapType;
    swapAddition.SwapToSymbol = swapInfo.currencyTo.ToUpper();

    double NetFee = Convert.ToDouble(SubNetworkFee);

    if (NetFee > 0) swapAddition.SwapNetworkFee = SubNetworkFee;

    double AmountTo = Convert.ToDouble(swapInfo.amountExpectedTo) - NetFee;

    swapAddition.SwapToAmount = AmountTo.ToString();

    double SwapToAmountUSD = Convert.ToDouble(ToUSDRate) * AmountTo;
    swapAddition.SwapToAmountUSD = SwapToAmountUSD.ToString("C2");

    double SwapCostUSD = signTransferResponse._AmountUSD - SwapToAmountUSD;

    if (SwapCostUSD <= 0) throw new Exception("Unable to estimate cost.");

    swapAddition.SwapCostUSD = SwapCostUSD.ToString("C2");

   }
   catch (Exception ex)
   {
    throw new Exception("Error calculating cost > " + ex.Message);
   }

   return swapAddition;
  }

  SignTransferResponse TxnProcessTask(WalletFactory factory, NoxTxnProcess tproc = null)
  {

   SignTransferResponse signTransferResponse = new SignTransferResponse();

   if (tproc == null)
   {
    statusTask.StatusMsg = "Requesting outputs";
    _eventProxy.HandleStatusEvent(statusTask);

    tproc = WS.GetTxnRequest(actiontask);

    if (tproc == null) throw new Exception("[WS] Error getting request.");

    statusTask.StatusMsg = "Building transaction";
    _eventProxy.HandleStatusEvent(statusTask);
   }


   signTransferResponse.LineID = tproc.TXNLineID;
   signTransferResponse.Amount = tproc.Amount;
   signTransferResponse.Blk = tproc.BlkNet;
   signTransferResponse.BlkDisplayName = tproc.BlkNet;
   signTransferResponse.ToAddress = tproc.ToAddress;

   uint[] indexes = new uint[] { 0 };
   if (tproc.HDIndexList != null && tproc.HDIndexList.Length > 0)
   {
    indexes = new uint[tproc.HDIndexList.Length];

    for (int i = 0; i < tproc.HDIndexList.Length; i++)
    {
     indexes[i] = Convert.ToUInt32(tproc.HDIndexList[i]);

    }
   }

   var product = WalletFactory.SymbolToWalletProduct(tproc.BlkNet);
   using (var wallet = factory.ConstructWallet(product, indexes))
   {
    var response = wallet.SignPaymentRequest(tproc);
    signTransferResponse.paymentRequestResponse = response;

    if (tproc.BlkNet.ToLower() == "gas")
    {
     var gresp = signTransferResponse.paymentRequestResponse;
     gresp.TransactionType = "unfreeze gas";
     signTransferResponse.paymentRequestResponse = gresp;
     return signTransferResponse;
    }
    bool isToken = false;
    double Token_USDRate = 0;
    double USDRate = Convert.ToDouble(tproc.USDRate);

    if (product == WalletFactory.Products.ETH)
    {
     if (!string.IsNullOrEmpty(tproc.ETCTokenName) && !string.IsNullOrEmpty(tproc.ETCToken))
     {
      signTransferResponse.BlkDisplayName = tproc.ETCTokenName;
      isToken = true;

      try
      {
       TokenResponse tokenResponse = new TokenResponse();
       var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tproc.ProcessH1));
       tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

       if (tokenResponse.Decimals > 0)
       {
        signTransferResponse.Amount = NumUtils.Utils.FromSatoshi(NumUtils.Utils.ToSatoshi(tproc.Amount, 18), tokenResponse.Decimals);
        Token_USDRate = Convert.ToDouble(tokenResponse.Rate);
       }
      }
      catch { }

     }
    }



    CalculateAdditions(signTransferResponse, USDRate, isToken, Token_USDRate);
   }

   return signTransferResponse;
  }

  private void CalculateAdditions(SignTransferResponse signTransferResponse, double USDRate, bool isToken, double Token_USDRate)
  {
   if (USDRate <= 0) return;

   try
   {

    if (string.IsNullOrEmpty(signTransferResponse.Amount)) return;

    double Amount = Convert.ToDouble(signTransferResponse.Amount);

    if (!isToken)
    {
     signTransferResponse._AmountUSD = (USDRate * Amount);
    }
    else
    {
     if (Token_USDRate > 0) signTransferResponse._AmountUSD = (Token_USDRate * Amount);

    }

    signTransferResponse.AmountUSD = signTransferResponse._AmountUSD.ToString("C2");

    var paymentRequestResponse = signTransferResponse.paymentRequestResponse;
    var addition = paymentRequestResponse.Addition;

    if (!string.IsNullOrEmpty(addition.FeeAmount))
    {
     double FeeAmount = Convert.ToDouble(addition.FeeAmount);

     double FeeUSD = USDRate * FeeAmount;

     addition.FeeUsd = FeeUSD.ToString("C2");

     if (FeeUSD >= 4) addition.FeeWarning = "HIGH FEE ALERT";

     if (FeeAmount > (Amount * .04d))
     {
      double feePer = (FeeAmount / Amount) * 100;
      addition.FeeWarning = "FEE IS " + feePer.ToString("N2") + "% OF PAYMENT";
     }

     paymentRequestResponse.Addition = addition;
     signTransferResponse.paymentRequestResponse = paymentRequestResponse;
     return;
    }

    if (!string.IsNullOrEmpty(addition.MaxFeeAllowed))
    {
     double FeeAmount = Convert.ToDouble(addition.MaxFeeAllowed);

     double FeeUSD = USDRate * FeeAmount;

     addition.FeeUsd = FeeUSD.ToString("C2");

     if (isToken) addition.MaxFeeAllowed = addition.MaxFeeAllowed + " ETH";

     paymentRequestResponse.Addition = addition;
     signTransferResponse.paymentRequestResponse = paymentRequestResponse;
    }
   }
   catch { }
  }


  string SessionSharedTask(WalletFactory factory)
  {
   SGAAuthenticator authenticator = new SGAAuthenticator(factory, _eventProxy);

   SWS WSS = new SWS();
   string sgamsg = WSS.GetSGAMessage();
   if (string.IsNullOrEmpty(sgamsg)) throw new Exception("[WS] Error getting signature request.");
   return authenticator.SignSGASessionShared(sgamsg).ToString();

  }

  string DMA2SignInTask(WalletFactory factory)
  {
   SGAAuthenticator authenticator = new SGAAuthenticator(factory, _eventProxy);
   SWS WSS = new SWS();

   var _request = WSS.GetSGAMessageV2(actiontask);

   var Request = DataModels.DeserializeRequest(_request);

   AuthResponse authResponse = new AuthResponse();
   authResponse.DisplayToken = display_token;
   authResponse.SMSToken = actiontask;

   Core.Abstract.DerResponse BFA_derResponse;
   Core.Abstract.DerResponse BITFI_derResponse;

   switch (Request.authType)
   {
    case AuthType.bfa_registration:
    return BFARegistrationTask(factory, Request, authenticator, WSS, authResponse);
    break;

    case AuthType.bfa_authorization:
    BFA_derResponse = authenticator.SignSGAMessageV2(Request.BFA_GuidDdata, WalletFactory.Products.BFA, Convert.ToUInt32(Request.DerivationIndex));
    authResponse.BFA_Signature = BFA_derResponse.signature.ByteToHex();
    authResponse.BFA_PublicKey = BFA_derResponse.public_key.ByteToHex();
    break;

    case AuthType.bfa_registration_bitfi_profile:
    BFA_derResponse = authenticator.SignSGAMessageV2(Request.BFA_GuidDdata, WalletFactory.Products.BFA, Convert.ToUInt32(Request.DerivationIndex));
    authResponse.BFA_Signature = BFA_derResponse.signature.ByteToHex();
    authResponse.BFA_PublicKey = BFA_derResponse.public_key.ByteToHex();

    BITFI_derResponse = authenticator.SignSGAMessageV2(Request.BITFI_GuidData, WalletFactory.Products.BTC, 0);
    authResponse.BITFI_Signature = BITFI_derResponse.signature.ByteToHex();
    authResponse.BITFI_PublicKey = BITFI_derResponse.public_key.ByteToHex();
    break;

    case AuthType.bitfi_authorization:
    BITFI_derResponse = authenticator.SignSGAMessageV2(Request.BITFI_GuidData, WalletFactory.Products.BTC, 0);
    authResponse.BITFI_Signature = BITFI_derResponse.signature.ByteToHex();
    authResponse.BITFI_PublicKey = BITFI_derResponse.public_key.ByteToHex();
    break;

    default:
    throw new Exception("Invalid request.");
   }

   var formattedResp = DataModels.SerializeResponse(authResponse);

   if (string.IsNullOrEmpty(formattedResp))
   {
    throw new Exception("Error building response.");
   }

   var tkn = WSS.GetSGATokenForSigninV2(formattedResp);

   if (string.IsNullOrEmpty(tkn))
    throw new Exception("[WS] Communication error.");

   return tkn;
  }
  private string BFARegistrationTask(WalletFactory factory, AuthRequest Request, SGAAuthenticator authenticator, SWS WSS, AuthResponse authResponse)
  {
   if (string.IsNullOrEmpty(TryAddressHash))
   {
    TryAddressHash = factory.GetTestHash();

    return ("Now repeat the passphrase and salt exactly as you have just entered, you will need to remember both when using Bitfi 2FA on the platform where you have initiated this registration.");
   }
   else
   {
    if (TryAddressHash != factory.GetTestHash())
    {
     throw new Exception("Information does not match, repeat the passphrase and salt exactly as you entered or start over by initiating a new 2FA registration request.");
    }

    TryAddressHash = null;

    Core.Abstract.DerResponse BFA_derResponse;

    BFA_derResponse = authenticator.SignSGAMessageV2(Request.BFA_GuidDdata, WalletFactory.Products.BFA, Convert.ToUInt32(Request.DerivationIndex));
    authResponse.BFA_Signature = BFA_derResponse.signature.ByteToHex();
    authResponse.BFA_PublicKey = BFA_derResponse.public_key.ByteToHex();

    var tkn = WSS.GetSGATokenForSigninV2(DataModels.SerializeResponse(authResponse));

    if (string.IsNullOrEmpty(tkn))
     throw new Exception(Sclear.MSG_ERROR_PROFILE);

    return tkn;
   }
  }


  string[] DMA2AddressDisplayTask(WalletFactory factory)
  {
   SGAAuthenticator authenticator = new SGAAuthenticator(factory, _eventProxy);

   SWS WSS = new SWS();
   string sgamsg = WSS.GetSGAMessage();
   if (string.IsNullOrEmpty(sgamsg)) throw new Exception("[WS] Error getting signature request.");
   return authenticator.SignSGAMessageOut(sgamsg);
  }
  string DMA2BalancesTask(WalletFactory factory)
  {
   SGAAuthenticator authenticator = new SGAAuthenticator(factory, _eventProxy);

   SWS WSS = new SWS();
   string sgamsg = WSS.GetSGAMessage();
   if (string.IsNullOrEmpty(sgamsg)) throw new Exception("[WS] Error getting signature request.");
   var resp = authenticator.SignSGAMessage(sgamsg);
   if (resp.Length < 10)
    throw new Exception(Sclear.MSG_ERROR_PROFILE);

   return resp;
  }
  string AddressTask(WalletFactory factory)
  {


   NoxAddressRequests req = WS.GetRequest(actiontask);
   if (req == null) throw new Exception("[WS] Communication error.");
   var afst = WS.GetFirstAddress(req.TXNLineID);
   if (afst == null) throw new Exception("[WS] Communication error.");

   string fst = afst[0];

   if (!string.IsNullOrEmpty(fst))
   {
    var index = (uint)req.HDIndex;
    uint[] indexes = new uint[] { index };
    string adr = "";

    if (index == 0)
    {
     if (Sclear.GetBLKNetworkAlt(req.BlkNet) == null && req.BlkNet != "apl" && req.BlkNet != "xrp" && req.BlkNet != "eos" &&
            req.BlkNet != "xdc" && req.BlkNet != "dag")
      throw new Exception(String.Format("Address feature is not supported"));
    }
    else
    {
     if (Sclear.GetBLKNetworkAlt(req.BlkNet) == null && req.BlkNet != "apl")
      throw new Exception(String.Format("Multiaddress feature is not supported"));

    }

    using (var btcWallet = factory.ConstructWallet(WalletFactory.Products.BTC, new UInt32[] { 0 }, true))
    {
     string btcadr = btcWallet.GetLegacyAddress();

     if (btcadr != fst) throw new Exception(Sclear.MSG_ERROR_ADDRESS);
    }

    if (req.BlkNet.ToLower() == "apl")
    {
     using (AplWallet aplWallet = (AplWallet)factory.ConstructWallet(WalletFactory.Products.APL, indexes))
     {
      adr = aplWallet.GetPubKey(index);
     }
    }
    else
    {
     var product = WalletFactory.SymbolToWalletProduct(req.BlkNet);
     using (var wallet = factory.ConstructWallet(product, indexes))
     {
      adr = req.DoSegwit ? wallet.GetSegwitAddress(index) : wallet.GetLegacyAddress(index);
     }
    }

    var send = WS.RespondAddressRequest(req.TXNLineID, adr);
    return send;
   }
   else
   {
    return NewProfileTask(factory, req);
   }
  }

  private string NewProfileTask(WalletFactory factory, NoxAddressRequests req)
  {
   if (string.IsNullOrEmpty(TryAddressHash))
   {
    TryAddressHash = factory.GetTestHash();

    return ("Now repeat the passphrase and salt exactly as you have just entered, it's very important that you remember both since this is your first address.");
   }
   else
   {
    if (TryAddressHash != factory.GetTestHash())
    {
     throw new Exception("Information does not match, repeat the passphrase and salt exactly as you entered or start over by initiating a new address request in your dashboard.");
    }

    AdrCollection adrCollection = new AdrCollection();

    var products = new WalletFactory.Products[] {
     WalletFactory.Products.BTC,
     WalletFactory.Products.LTC,
     WalletFactory.Products.NEO,
     WalletFactory.Products.ETH,
     WalletFactory.Products.XMR
    };

    using (var btcWallet = factory.ConstructWallet(WalletFactory.Products.BTC))
    using (var ltcWallet = factory.ConstructWallet(WalletFactory.Products.LTC))
    using (var neoWallet = factory.ConstructWallet(WalletFactory.Products.NEO))
    using (var ethWallet = factory.ConstructWallet(WalletFactory.Products.ETH))
    using (var xmrWallet = factory.ConstructWallet(WalletFactory.Products.XMR))
    {
     TryAddressHash = null;
     string btcadr = btcWallet.GetLegacyAddress();
     string ltcadr = ltcWallet.GetLegacyAddress();
     string ethadr = ethWallet.GetLegacyAddress();
     string neoaddress = neoWallet.GetLegacyAddress();
     string[] moneroadr = new string[] {
      xmrWallet.GetLegacyAddress(),
      ((XmrWallet)xmrWallet).GetViewKey()
     };

     FirstAdrCollection Addresses = new FirstAdrCollection();
     Addresses.BTC = btcadr;
     Addresses.LTC = ltcadr;
     Addresses.ETH = ethadr;
     Addresses.NEO = neoaddress;
     Addresses.Monero = moneroadr;
     return WS.RespondFirstAddressRequest(req.TXNLineID, Addresses);
    }
   }

  }

 }

 public class WorkerTask
 {
  public object Result { get; set; }
  public Exception Ex { get; set; }
 }

 public class WSTask
 {
  public bool Success { get; set; }
  public bool Prompt { get; set; }
  public string PromptMsg { get; set; }
  public object Result { get; set; }
  public string ErrorMsg { get; set; }

  public string StatusMsg { get; set; }
  public UITransition Transition { get; set; }
 }

 public enum UITransition
 {
  Activity = 0,
  DlgModel = 1
 }

}
