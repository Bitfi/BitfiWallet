using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.Collections.Generic;
using Newtonsoft.Json;
using ZXing;
using NoxService.SWS;
using Android.Text;
using NoxService.DEVWS;
using NoxKeys;

namespace BitfiWallet
{

 [Activity(Label = "", Name = "com.rokits.noxadmin.NoxViewModel", Theme = "@style/FullscreenTheme")]
 public class NoxViewModel : ListActivity
 {
  private string action;
  private string token;
  private string addresses;
  string wallet;
  BackgroundWorker xbw = new BackgroundWorker();
  public ProgressBar pb;

  private WalletList[] filtered = new WalletList[0];
  private NoxAddresses[] NWSfiltered = new NoxAddresses[0];

  SWS WS;
  NoxService.NWS.NWS WSTxn;


  SignTransferResponse signTransferResponse;
  protected override void OnCreate(Bundle bundle)
  {
   try
   {

    WS = new SWS();
    WSTxn = new NoxService.NWS.NWS();
    xbw.DoWork += new DoWorkEventHandler(xbw_DoWork);
    xbw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(xbw_RunWorkerCompleted);


    RequestWindowFeature(WindowFeatures.NoTitle);
    SetContentView(Resource.Layout.viewmodel_maina);

    token = Intent.GetStringExtra("token");
    action = Intent.GetStringExtra("action");

    Button send = (Button)FindViewById(Resource.Id.btnlik64);
    Button sendTxn = (Button)FindViewById(Resource.Id.btnlik66);
    Button close = (Button)FindViewById(Resource.Id.btnlik65);
    TextView lbl = (TextView)FindViewById(Resource.Id.tvliktv64);
    TextView lblTxn = (TextView)FindViewById(Resource.Id.tvliktv65);

    if (action == "txn")
    {
     byte[] jhash = Intent.GetByteArrayExtra("SignTransferResponse");
     string json = JsonConvert.SerializeObject(NoxDPM.NoxT.noxDevice.signTransferResponse);
     using (var hmac = new System.Security.Cryptography.SHA256Managed())
     {

      if (Convert.ToBase64String(jhash) != Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json))))
      {
       return;
       Finish();
      }
     }

     signTransferResponse = JsonConvert.DeserializeObject<SignTransferResponse>(json);

     lblTxn.Visibility = ViewStates.Visible;
     sendTxn.Visibility = ViewStates.Visible;
     close.Visibility = ViewStates.Visible;
     lbl.Visibility = ViewStates.Gone;
     send.Visibility = ViewStates.Gone;

     sendTxn.Click += delegate
     {

      SendTxn(signTransferResponse);

     };

     close.Click += delegate
     {
      signTransferResponse = null;
      Finish();

     };
    }
    else
    {
     if (action == "overview")
     {

      TextView stLbl = (TextView)FindViewById(Resource.Id.ovt1);
      stLbl.Text = "Getting balance overview";

      send.Text = "REFRESH";
      lbl.Text = "BALANCE";

      send.Click += delegate
      {
       Recreate();
      };

     }

     if (action == "accounts")
     {
      send.Text = "VIEW BALANCES";
      lbl.Text = "ADDRESSES";
      addresses = Intent.GetStringExtra("adrlist");

      send.Click += delegate
      {

       action = "overview";

       Intent rint = new Intent();
       rint.PutExtra("action", action);
       rint.PutExtra("token", token);
       Intent.ReplaceExtras(rint);

       Recreate();
      };

     }

     if (action == "noxnews")
     {
      send.Visibility = ViewStates.Gone;
      lbl.Text = "SUPPORT & FAQ <| ";

     }
    }

    xbw.RunWorkerAsync();
    base.OnCreate(bundle);
    RequestWindowFeature(WindowFeatures.NoTitle);
   }
   catch
   { }
  }

  private void SendTxn(SignTransferResponse SendResponse)
  {

   LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
   View dialoglayout = inflater.Inflate(Resource.Layout.splashmodel, null);

   AlertDialog.Builder builder = new AlertDialog.Builder(this, Resource.Style.FullscreenTheme);
   builder.SetCancelable(false);
   builder.SetView(dialoglayout);
   var bwalert = builder.Create();
   // bwalert.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.TurnScreenOn);
   bwalert.Show();

   Task t = Task.Factory.StartNew(() =>
   {

    string resp;

    try
    {
     if (SendResponse.Blk != "xmr")
     {
      resp = WSTxn.SubmitTxnResponse(SendResponse.LineID, SendResponse.paymentRequestResponse.TxnHex);

     }
     else
     {
      resp = WSTxn.SubmitTxnResponseMX(SendResponse.LineID, SendResponse.paymentRequestResponse.TxnHex, SendResponse.paymentRequestResponse.Addition.SpendKeyImages);

     }
    }
    catch (Exception ex)
    {
     resp = ex.Message;
    }

    Action workerThread = delegate ()
       {

      string title = "";
      string pbtn = "";

      if (resp == "0")
      {
       resp = "";
       title = "Success! " + SendResponse.BlkDisplayName.ToUpper() + " transaction has been sent.";
       pbtn = "OK, CLOSE";
      }
      else
      {
       title = "ERROR";
       pbtn = "OK, RETURN";
      }

      DisplayMSG(resp, title, pbtn, true);

     };

    RunOnUiThread(workerThread);

   });

  }

  public void DisplayMSG(string msg, string title, string pbtn, bool Close = false)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(this, Resource.Style.MyAlertDialogThemeD).SetTitle(title).SetMessage(msg).SetCancelable(false).SetPositiveButton(pbtn, (EventHandler<DialogClickEventArgs>)null);

   AlertDialog alert = builder.Create();
   alert.Show();

   if (!string.IsNullOrEmpty(msg))
   {

    TextView msgTxt = (TextView)alert.FindViewById(Android.Resource.Id.Message);
    msgTxt.TextSize = 16;
    msgTxt.SetTextColor(Color.WhiteSmoke);

    msgTxt.SetPadding(msgTxt.PaddingLeft, msgTxt.PaddingTop + 20, msgTxt.PaddingRight, msgTxt.PaddingBottom + 40);
    msgTxt.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
   }

   var okBtn = alert.GetButton((int)DialogButtonType.Positive);
   okBtn.Click += (asender, args) =>
   {

    Finish();

   };

  }
  protected override void OnPause()
  {
   base.OnPause();
   OverridePendingTransition(0, 0);

  }
  public override void OnBackPressed()
  {


   action = null;
   token = null;
   addresses = null;
   filtered = null;
   NWSfiltered = null;

   NoxDPM.NoxT.noxDevice.signTransferResponse = null;


   try
   {
    Intent.Extras.Clear();
    if (!this.IsFinishing) Finish();
   }
   catch { }
   base.OnBackPressed();

  }
  private void xbw_DoWork(object sender, DoWorkEventArgs e)
  {

   try
   {
    e.Result = 0;

    filtered = new WalletList[0];
    NWSfiltered = new NoxAddresses[0];


    FindViewById<LinearLayout>(Resource.Id.linlaHeaderProgress).Visibility = ViewStates.Visible;
    pb = (ProgressBar)FindViewById(Resource.Id.progressBar);
    pb.Indeterminate = true;
    pb.Max = 100;
    pb.Min = 0;
    pb.SetProgress(0, true);

    if (action == "txn")
    {
     List<NoxAddresses> noxViews = new List<NoxAddresses>();

     if (signTransferResponse.paymentRequestResponse.SwapAddition != null)
     {

      noxViews.Add(new NoxAddresses() { ViewKey = "asset action", TXNLineID = "changelly swap" });
      noxViews.Add(new NoxAddresses() { ViewKey = "sending value", TXNLineID = signTransferResponse.Amount + " " + signTransferResponse.BlkDisplayName.ToUpper(), BTCAddress = signTransferResponse.AmountUSD });
      noxViews.Add(new NoxAddresses() { ViewKey = "swapping for", TXNLineID = signTransferResponse.paymentRequestResponse.SwapAddition.SwapToAmount + " " + signTransferResponse.paymentRequestResponse.SwapAddition.SwapToSymbol, BTCAddress = signTransferResponse.paymentRequestResponse.SwapAddition.SwapToAmountUSD });
      noxViews.Add(new NoxAddresses() { ViewKey = "estimated cost", TXNLineID = signTransferResponse.paymentRequestResponse.SwapAddition.SwapType, BTCAddress = signTransferResponse.paymentRequestResponse.SwapAddition.SwapCostUSD });

      if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.SwapAddition.SwapNetworkFee))
      {
       noxViews.Add(new NoxAddresses() { ViewKey = "network fee", TXNLineID = "-" + signTransferResponse.paymentRequestResponse.SwapAddition.SwapNetworkFee + " " + signTransferResponse.paymentRequestResponse.SwapAddition.SwapToSymbol, BTCAddress = "included in amount" });
      }

      if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.FeeAmount)) noxViews.Add(new NoxAddresses() { ViewKey = "sending fee", TXNLineID = signTransferResponse.paymentRequestResponse.Addition.FeeAmount + " " + signTransferResponse.Blk.ToUpper(), BTCAddress = signTransferResponse.paymentRequestResponse.Addition.FeeUsd });
      if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.MaxFeeAllowed)) noxViews.Add(new NoxAddresses() { ViewKey = "max-send fee", TXNLineID = signTransferResponse.paymentRequestResponse.Addition.MaxFeeAllowed, BTCAddress = signTransferResponse.paymentRequestResponse.Addition.FeeUsd });
     }
     else
     {
      string typt = " [transfer]";
      if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.TransactionType)) typt = " [" + signTransferResponse.paymentRequestResponse.TransactionType + "]";

      noxViews.Add(new NoxAddresses() { ViewKey = "asset action", TXNLineID = signTransferResponse.BlkDisplayName.ToUpper() + typt });

      if (!string.IsNullOrEmpty(signTransferResponse.Amount))
      {
       noxViews.Add(new NoxAddresses() { ViewKey = "sending value", TXNLineID = signTransferResponse.Amount, BTCAddress = signTransferResponse.AmountUSD });
      }



      if (signTransferResponse.Blk.ToLower() == "eos")
      {
       var eosobj = signTransferResponse.paymentRequestResponse.EosAddition;

       noxViews.Add(new NoxAddresses() { ViewKey = "contract", TXNLineID = signTransferResponse.paymentRequestResponse.Addition.ContractType });

       switch (signTransferResponse.paymentRequestResponse.Addition.TransactionType)
       {
        case "delegatebw":
        noxViews.Add(new NoxAddresses() { ViewKey = "stake_cpu_qty", TXNLineID = eosobj.EosStakeArgs.stake_cpu_quantity });
        noxViews.Add(new NoxAddresses() { ViewKey = "stake_net_qty", TXNLineID = eosobj.EosStakeArgs.stake_net_quantity });
        noxViews.Add(new NoxAddresses() { ViewKey = "receiver", TXNLineID = eosobj.EosStakeArgs.receiver });
        break;
        case "undelegatebw":
        noxViews.Add(new NoxAddresses() { ViewKey = "unstake_cpu_qty", TXNLineID = eosobj.EosUnstakeArgs.unstake_cpu_quantity });
        noxViews.Add(new NoxAddresses() { ViewKey = "unstake_net_qty", TXNLineID = eosobj.EosUnstakeArgs.unstake_net_quantity });
        noxViews.Add(new NoxAddresses() { ViewKey = "receiver", TXNLineID = eosobj.EosUnstakeArgs.receiver });
        break;
        case "sellram":
        noxViews.Add(new NoxAddresses() { ViewKey = "ram to sell", TXNLineID = eosobj.SellRamArgs.bytes.ToString() });
        noxViews.Add(new NoxAddresses() { ViewKey = "user account", TXNLineID = eosobj.SellRamArgs.account });
        break;
        case "buyrambytes":
        noxViews.Add(new NoxAddresses() { ViewKey = "ram to buy", TXNLineID = eosobj.BuyRamArgs.bytes.ToString() });
        noxViews.Add(new NoxAddresses() { ViewKey = "receiver", TXNLineID = eosobj.BuyRamArgs.receiver });
        break;
        case "transfer":
        noxViews.Add(new NoxAddresses() { ViewKey = "receiver", TXNLineID = eosobj.TransferArgs.to });
        if (!string.IsNullOrEmpty(eosobj.TransferArgs.memo)) noxViews.Add(new NoxAddresses() { ViewKey = "memo", BTCAddress = eosobj.TransferArgs.memo });
        break;
       }
      }
      else
      {
       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.FeeAmount)) noxViews.Add(new NoxAddresses() { ViewKey = "fee amount", TXNLineID = signTransferResponse.paymentRequestResponse.Addition.FeeAmount, BTCAddress = signTransferResponse.paymentRequestResponse.Addition.FeeUsd });
       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.MaxFeeAllowed)) noxViews.Add(new NoxAddresses() { ViewKey = "maximum fee", TXNLineID = signTransferResponse.paymentRequestResponse.Addition.MaxFeeAllowed, BTCAddress = signTransferResponse.paymentRequestResponse.Addition.FeeUsd });


       noxViews.Add(new NoxAddresses() { ViewKey = "receiver", BTCAddress = signTransferResponse.ToAddress });


       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.ContractAddress)) noxViews.Add(new NoxAddresses() { ViewKey = "contract", BTCAddress = signTransferResponse.paymentRequestResponse.Addition.ContractAddress });
       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.DestinationTag)) noxViews.Add(new NoxAddresses() { ViewKey = "destination tag", BTCAddress = signTransferResponse.paymentRequestResponse.Addition.DestinationTag });
       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.PaymentId)) noxViews.Add(new NoxAddresses() { ViewKey = "payment id", BTCAddress = signTransferResponse.paymentRequestResponse.Addition.PaymentId });
       if (!string.IsNullOrEmpty(signTransferResponse.paymentRequestResponse.Addition.FeeWarning)) noxViews.Add(new NoxAddresses() { ViewKey = "warning", BTCAddress = signTransferResponse.paymentRequestResponse.Addition.FeeWarning });


      }
     }

     NWSfiltered = noxViews.ToArray();



    }
    else
    {
     if (action == "noxnews")
     {
      try
      {
       DEVWS dEVWS = new DEVWS();
       var tagList = dEVWS.GetTagList();

       NWSfiltered = new NoxAddresses[0];

       if (tagList != null && tagList.Length > 0)
       {
        List<NoxAddresses> lnoxAddresses = new List<NoxAddresses>();

        foreach (DevMsgForm.BITFIDEV.NoxMessagesBasic item in tagList)
        {
         NoxAddresses noxAddr = new NoxAddresses();
         noxAddr.BTCAddress = item.ID.ToString();
         noxAddr.BlkNet = item.MobMessage;
         lnoxAddresses.Add(noxAddr);
        }

        NWSfiltered = lnoxAddresses.ToArray();
        Thread.Sleep(1000);
       }
       else
       {
        e.Result = 2;
       }
      }
      catch
      {
       e.Result = 3;
      }

      return;
     }

     if (action == "accounts")
     {
      try
      {
       NWSfiltered = JsonConvert.DeserializeObject<NoxAddresses[]>(addresses);
       if (NWSfiltered != null && NWSfiltered.Length > 1)
       {
        //   Thread.Sleep(1000);

       }
       else
       {
        e.Result = 2;
       }
      }
      catch
      {
       e.Result = 3;
      }
     }
     else
     {
      try
      {


       var VModel = WS.GetOverviewModel(token);
       if (VModel != null && VModel.OverviewTableViewModel != null && VModel.OverviewTableViewModel.Wallets != null)
       {
        wallet = VModel.OverviewTableViewModel.BtcRate;
        filtered = VModel.OverviewTableViewModel.Wallets;

       }
       else
       {
        e.Result = 1;
       }
      }
      catch
      {
       e.Result = 3;
      }
     }
    }
   }
   catch
   {
    e.Result = 3;
   }

  }
  private void xbw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
  {

   pb.Indeterminate = false;
   pb.Max = 100;

   pb.SetProgress(100, true);

   FindViewById<LinearLayout>(Resource.Id.linlaHeaderProgress).Visibility = ViewStates.Gone;
   ListAdapter = new ViewListAdapter(this, action, filtered, NWSfiltered);

   if ((int)e.Result == 1)
   {
    DisplayMSG("Network error, please check connection.");
    return;
   }
   if ((int)e.Result == 2)
   {
    DisplayMSG("Connection error, please try again");
    return;
   }
   if ((int)e.Result == 3)
   {
    DisplayMSG("Error, please try again");
    return;
   }
   if (!string.IsNullOrEmpty(wallet))
   {
    TextView lbl = (TextView)FindViewById(Resource.Id.tvliktv64);
    lbl.Text = wallet;
    lbl.SetTextSize(Android.Util.ComplexUnitType.Sp, 18);
    lbl.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
   }

  }



  public void DisplayMSG(string msg)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(this).SetTitle("")
       .SetMessage(msg).SetCancelable(false)
       .SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
   AlertDialog alert = builder.Create();
   alert.Show();
   var okBtn = alert.GetButton((int)DialogButtonType.Positive);
   okBtn.Click += (asender, args) =>
   {

    alert.Dismiss();

    Finish();

   };

  }
 }

 class ViewListAdapter : BaseAdapter
 {
  Context context;
  string action;
  WalletList[] filtered;
  NoxAddresses[] NWSfiltered;
  Typeface typeface;
  Typeface tpfB;
  public ViewListAdapter(Context c, string _action, WalletList[] _filtered, NoxAddresses[] _NWSfiltered)
  {
   context = c;
   action = _action;

   filtered = _filtered;
   NWSfiltered = _NWSfiltered;

   typeface = Typeface.CreateFromAsset(c.Assets, "Rubik-Medium.ttf");
   tpfB = Typeface.CreateFromAsset(c.Assets, "Rubik-Bold.ttf");
  }
  public override Java.Lang.Object GetItem(int position)
  {
   return null;
  }
  public override long GetItemId(int position)
  {
   return position + 1;
  }
  public override int Count
  {
   get
   {
    if (action == "overview")
    {
     if (filtered == null)
     {
      return 0;

     }
     else
     {
      int count = filtered.Length;
      return count;
     }
    }
    else
    {
     if (NWSfiltered == null)
     {
      return 0;

     }
     else
     {
      int count = NWSfiltered.Length;
      return count;
     }
    }
   }
  }

  public override View GetView(int position, View convertView, ViewGroup parent)
  {
   try
   {

    convertView = LayoutInflater.From(context).Inflate(Resource.Layout.overview, parent, false);
    convertView.FindViewById<TextView>(Resource.Id.ovt2).Typeface = typeface;
    convertView.FindViewById<TextView>(Resource.Id.ovt3).Typeface = typeface;

    if (action == "txn")
    {
     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
     tv1.Text = NWSfiltered[position].ViewKey;
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Text = NWSfiltered[position].BTCAddress;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).Text = NWSfiltered[position].TXNLineID;

     if (string.IsNullOrEmpty(NWSfiltered[position].ViewKey))
     {
      convertView.FindViewById<TextView>(Resource.Id.ovt2).Visibility = ViewStates.Gone;
      convertView.FindViewById<TextView>(Resource.Id.ovt1b).Visibility = ViewStates.Gone;
      tv1.Visibility = ViewStates.Visible;
     }

    }
    else
    {


    }

    if (action == "overview")
    {

     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
     tv1.Text = filtered[position].Currency;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).Text = filtered[position].Balance;
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Text = filtered[position].USD;
     convertView.FindViewById<TextView>(Resource.Id.ovt1b).Visibility = ViewStates.Gone;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).Visibility = ViewStates.Visible;
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Visibility = ViewStates.Visible;


    }

    if (action == "noxnews")
    {

     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
     tv1.Visibility = ViewStates.Gone;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).Text = NWSfiltered[position].BlkNet.ToUpper();
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Text = NWSfiltered[position].BTCAddress;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Visibility = ViewStates.Gone;
     convertView.FindViewById<ImageView>(Resource.Id.imageView1_ov).Visibility = ViewStates.Visible;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).SetTypeface(tpfB, TypefaceStyle.Normal);

     convertView.FindViewById(Resource.Id.ovinf).Click += delegate
     {

      var nxact = new Intent(context, typeof(PageActivity));
      nxact.PutExtra("ID", NWSfiltered[position].BTCAddress);

      context.StartActivity(nxact);

     };
    }

    if (action == "accounts")
    {

     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);

     tv1.Text = NWSfiltered[position].BlkNet.ToUpper();
     convertView.FindViewById<TextView>(Resource.Id.ovt2).Text = "";
     convertView.FindViewById<TextView>(Resource.Id.ovt3).Text = NWSfiltered[position].BTCAddress;
     convertView.FindViewById<TextView>(Resource.Id.ovt2).SetTextSize(Android.Util.ComplexUnitType.Sp, 16);
     convertView.FindViewById<TextView>(Resource.Id.ovt3).SetTextSize(Android.Util.ComplexUnitType.Sp, 13);

     convertView.FindViewById(Resource.Id.ovinf).Click += delegate
     {

      ImageView image = new ImageView(context);
      var bmp = ConvertQr(NWSfiltered[position].BTCAddress);
      image.SetImageBitmap(bmp);

      AlertDialog.Builder builder = new AlertDialog.Builder(context).SetTitle(NWSfiltered[position].BlkNet.ToUpper()).SetMessage(NWSfiltered[position].BTCAddress).SetCancelable(true).SetPositiveButton("CLOSE", (EventHandler<DialogClickEventArgs>)null);
      AlertDialog alert = builder.Create();
      alert.SetView(image);
      alert.Show();

      var okBtn = alert.GetButton((int)DialogButtonType.Positive);

      okBtn.Click += (asender, args) =>
           {

        alert.Dismiss();
       };


     };
    }

   }
   catch (Exception)
   {

   }
   return convertView;
  }

  public void DisplayMSG(string msg)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(context).SetTitle("").SetMessage(msg).SetCancelable(false).SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
   AlertDialog alert = builder.Create();
   alert.Show();
   var okBtn = alert.GetButton((int)DialogButtonType.Positive);
   okBtn.Click += (asender, args) =>
   {
    alert.Dismiss();

   };

  }

  public Bitmap GenerateQrCodeRaw(string url, int height = 300, int width = 300, int margin = 10)
  {
   BarcodeWriter qrWriter = new BarcodeWriter();
   qrWriter.Format = BarcodeFormat.QR_CODE;
   qrWriter.Options = new ZXing.Common.EncodingOptions()
   {
    Height = height,
    Width = width,
    Margin = margin
   };

   var barcode = qrWriter.Write(url);
   return barcode;


  }

  public Bitmap ConvertQr(string address)
  {
   return GenerateQrCodeRaw(address);
  }


 }


}
