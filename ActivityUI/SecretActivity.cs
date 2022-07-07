using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Android.Graphics;
using Android.Views;
using Newtonsoft.Json;
using Android.Runtime;
using WalletLibrary;
using NoxKeys;
using Android.Content.PM;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.SecretActivity", Label = "", Theme = "@style/FullscreenTheme", HardwareAccelerated = true)]
 class SecretActivity : Activity
 {
  private NoxSecretActivity noxSecretActivity = new NoxSecretActivity();
  protected override void OnCreate(Bundle bundle)
  {

   SetContentView(Resource.Layout.Secret);
   base.OnCreate(bundle);
  }
  protected override void OnPause()
  {
   base.OnPause();
   OverridePendingTransition(0, 0);
  }
  public override void OnBackPressed()
  {
   noxSecretActivity.FinishA();

   base.OnBackPressed();
  }

  protected override void OnStart()
  {
   base.OnStart();
   if (noxSecretActivity.dgashowing) return;

   noxSecretActivity.Init(this);
   noxSecretActivity.CreateLayout();
  }
  protected override void OnDestroy()
  {
   noxSecretActivity.NoxFinish();
   base.OnDestroy();
  }
  protected override void OnStop()
  {
   if (!noxSecretActivity.bwalert.IsShowing)
   {
    try
    {
     noxSecretActivity.NoxResetArrays();
     if (!this.IsFinishing) noxSecretActivity.FinishA();
    }
    catch { }
   }

   base.OnStop();
  }
 }

 class NoxSecretActivity : Wallet
 {
  Activity _activity;
  public bool dgashowing;

  public NoxSecretActivity()
  {
   statusTask = new WSTask();
   statusTask.Transition = UITransition.DlgModel;

   _eventProxy = new WalletEventProxy();
   _eventProxy.OnStatusChanged += NoxEvent;

  }

  public void Init(Activity activity)
  {
   this._activity = activity;

   string display_token = null;

   if (_activity.Intent.HasExtra("display_token"))
   {
    display_token = _activity.Intent.GetStringExtra("display_token");
   }

   JavaClassInit(_activity.Intent.GetStringExtra("action"),
     _activity.Intent.GetStringExtra("task"), display_token);
  }

  public void CreateLayout()
  {
   SaltSelected = true;
   LoadBtns();
   LoadInpViews();

  }

  public TextView _splashModelTV;

  protected override void NoxEvent(WalletEventArgs e)
  {
   try
   {
    var resp = e.GetResult();
    if (resp == null)
     return;

    if (resp.Transition != UITransition.DlgModel)
     return;

    if (_splashModelTV == null)
     return;

    _activity.RunOnUiThread(delegate ()
    {
     try
     {
      if (!bwalert.IsShowing) return;

      _splashModelTV.Text = resp.StatusMsg;

     }
     catch { }
    });
   }
   catch { }

  }

  public override void NoxUpdateView(byte[] inputs)
  {
   LinearLayout box;

   if (SaltSelected)
   {
    box = tbsalt;
   }
   else
   {
    box = tbsecr;
   }

   box.RemoveAllViews();

   int cnt = 0;
   int pos = 1;
   int gg = GetActualLength(inputs) - 1;

   for (int i = gg; i >= 0; i--)
   {
    if (cnt < 30)
    {
     ImageView imageView = new ImageView(_activity.ApplicationContext);
     imageView.SetImageBitmap(Sclear.GetKeyDictionary(inputs[i]));
     box.AddView(imageView, 0);
     cnt = cnt + 1;
    }
    else
    {
     break;
    }
   }

   if (!SaltSelected)
   {
    if (tbshash.ChildCount > 3) tbshash.RemoveAllViews();
    llhash.Visibility = ViewStates.Gone;

    return;
   }

   if (gg == 2 || gg == 3)
   {

    Task t = Task.Factory.StartNew(() =>
    {
     List<byte> chl_hash = new List<byte>();

     Action workerThread = delegate ()
         {
       tbshash.RemoveAllViews();
       llhash.Visibility = ViewStates.Visible;
       if (chl_hash != null)
       {
        foreach (var ch in chl_hash)
        {
         ImageView imageView = new ImageView(_activity.ApplicationContext);
         imageView.SetImageBitmap(Sclear.GetKeyDictionary(ch));
         tbshash.AddView(imageView, 0);
        }
       }
      };

     try
     {
      chl_hash = Get3DGSaltHash();
     }
     catch
     {
     }
     finally
     {

      _activity.RunOnUiThread(workerThread);
     }

    });

   }
   else
   {
    tbshash.RemoveAllViews();
    llhash.Visibility = ViewStates.Gone;
   }

  }
  public void FinishA()
  {

   Task t = Task.Factory.StartNew(() =>
   {
    try
    {
     _activity.Finish();
     Thread.Sleep(300);
    }
    catch
    {
     Action workerThread = delegate ()
        {
        _activity.Finish();
       };

     _activity.RunOnUiThread(workerThread);
    }
    finally
    {
     try
     {
      Action workerThread = delegate ()
         {
          tbsecr.RemoveAllViews();
          tbsalt.RemoveAllViews();

         };

      _activity.RunOnUiThread(workerThread);
     }
     catch { }
    }

   });

  }
  private void PromptTxn(WSTask task)
  {
   SignTransferResponse signTransferResponse = (SignTransferResponse)task.Result;

   string json = JsonConvert.SerializeObject(signTransferResponse);
   using (var hmac = new System.Security.Cryptography.SHA256Managed())
   {
    byte[] jhash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));

    NoxDPM.NoxT.noxDevice.signTransferResponse = signTransferResponse;
    var nxact = new Intent(_activity.ApplicationContext, typeof(NoxViewModel));
    nxact.PutExtra("action", "txn");
    nxact.PutExtra("SignTransferResponse", jhash);
    nxact.AddFlags(ActivityFlags.NoAnimation | ActivityFlags.NoHistory);
    _activity.StartActivity(nxact);
   }
   FinishA();
  }
  private void StartViewModel(WSTask task)
  {
   string[] result = (string[])task.Result;

   var nxact = new Intent(_activity.ApplicationContext, typeof(NoxViewModel));
   nxact.PutExtra("action", WalletAction);
   nxact.PutExtra("token", result[0]);
   nxact.PutExtra("adrlist", result[1]);

   nxact.AddFlags(ActivityFlags.NoAnimation | ActivityFlags.NoHistory);

   _activity.StartActivity(nxact);
   FinishA();
  }
  private void ContWSTask(WSTask task)
  {
   switch (WalletAction)
   {
    case "register2fa":
    string Rmsg = task.PromptMsg;
    string Rtitle = "NEW 2FA PROFILE";
    string Rpbtn = "ok, continue";
    DisplayMSG(Rmsg, Rtitle, Rpbtn, false);
    break;

    case "address":
    string msg = task.PromptMsg;
    string title = "FIRST ADDRESS ALERT";
    string pbtn = "ok, continue";
    DisplayMSG(msg, title, pbtn, false);
    break;

    case "txn":
    case "swap":
    PromptTxn(task);
    break;

    case "overview":
    StartViewModel(task);
    break;

    case "accounts":
    StartViewModel(task);
    break;

    case "session_start":
    var nxact = new Intent(_activity, typeof(MessageActivity));
    nxact.PutExtra("action", "session_factory");
    nxact.PutExtra("task", (string)task.Result);
    nxact.AddFlags(ActivityFlags.NoAnimation);
    _activity.StartActivity(nxact);
    FinishA();
    break;


    case "wallet_start":
     var wnxact = new Intent(_activity, typeof(MetaActivity));
     wnxact.AddFlags(ActivityFlags.NoAnimation);
     _activity.StartActivity(wnxact);
     FinishA();
     break;

   }
  }
  private async void EnterEvent()
  {
   if (bwalert.IsShowing) return;

   if (_splashModelTV != null)
    _splashModelTV.Text = "";

   bwalert.Show();

   dgashowing = true;

   _RunAsync();

  }

  void _RunAsync()
  {


   CancellationTokenSource cancellationTokenMA = new CancellationTokenSource();

   Task.Run(async () =>
   {
    var work = new RokitWorkDelegate(() =>
       {
      _activity.RunOnUiThread(delegate ()
         {

        tbsecr.RemoveAllViews();
        tbsalt.RemoveAllViews();
        toptbsalt.SetBackgroundResource(Resource.Color.space);
        tagsalt.SetBackgroundResource(Resource.Color.space);
        toptbsecr.SetBackgroundResource(Resource.Color.black);
        tagsecr.SetBackgroundResource(Resource.Color.black);
        ResetTags();
       });

      return RunWalletAsync().Result;

     });

    var resp = await work.ToRokit(500, cancellationTokenMA, false);

    WSTask task;

    if (resp.Error != null)
    {
     task = new WSTask() { Success = false, ErrorMsg = "Unable to start work, please try again." };

     _activity.RunOnUiThread(delegate ()
        {

        tbsecr.RemoveAllViews();
        tbsalt.RemoveAllViews();
        toptbsalt.SetBackgroundResource(Resource.Color.space);
        tagsalt.SetBackgroundResource(Resource.Color.space);
        toptbsecr.SetBackgroundResource(Resource.Color.black);
        tagsecr.SetBackgroundResource(Resource.Color.black);
        ResetTags();
       });
    }
    else
    {
     task = (WSTask)resp.Content;
    }

    _activity.RunOnUiThread(delegate ()
       {

      if (!task.Success)
      {
       bool cls = false;
       string msg = task.ErrorMsg;
       string title = "";
       string pbtn = "ok, retry";

       if (msg.IndexOf("[WS SESSION ERROR]") > -1)
       {
        msg = msg.Replace("[WS SESSION ERROR]", "");
        cls = true;
        pbtn = "ok, start over";
       }

       DisplayMSG(msg, title, pbtn, cls);

      }
      else
      {
       if (task.Success && !task.Prompt)
       {
        string msg = "";
        string title = "Success!";
        string pbtn = "ok, close";

        DisplayMSG(msg, title, pbtn, true);
       }
       else
       {
        ContWSTask(task);
       }
      }


     });



   });

  }


  private void ResetTags()
  {
   mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25; mButton0.Tag = 52; mButton1.Tag = 53; mButton2.Tag = 54; mButton3.Tag = 55; mButton4.Tag = 56; mButton5.Tag = 57; mButton6.Tag = 58; mButton7.Tag = 59; mButton8.Tag = 60; mButton9.Tag = 61;
   mButtonShift.Text = "^";
   mButtonCaps.Text = "CAPS";
   capson = false;
   foreach (Button b in buttonLIst)
   {
    b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
   }
  }
  public void DisplayMSG(string msg, string title, string pbtn, bool Close = false)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeD)
    .SetTitle(title).SetMessage(msg).SetCancelable(false).SetPositiveButton(pbtn, (EventHandler<DialogClickEventArgs>)null);

   if (!Close)
   {
    builder.SetNegativeButton("CANCEL ", (EventHandler<DialogClickEventArgs>)null);
   }

   AlertDialog alert = builder.Create();
   alert.Show();
   dgashowing = true;

   if (!string.IsNullOrEmpty(msg))
   {
    TextView msgTxt = (TextView)alert.FindViewById(Android.Resource.Id.Message);
    msgTxt.TextSize = 18;
    msgTxt.SetTextColor(new Color(200, 228, 230));
    if (msg.IndexOf("INVALID INFO.") == 0)
    {
     msgTxt.SetLineSpacing(0, 1.2f);
     msgTxt.LetterSpacing = 0.08f;
     msgTxt.JustificationMode = Android.Text.JustificationMode.InterWord;
    }
    msgTxt.SetPadding(msgTxt.PaddingLeft, msgTxt.PaddingTop + 20, msgTxt.PaddingRight, msgTxt.PaddingBottom + 40);
    msgTxt.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
   }

   var okBtn = alert.GetButton((int)DialogButtonType.Positive);
   okBtn.Click += (asender, args) =>
   {
    dgashowing = false;

    if (Close)
    {
     FinishA();
    }
    else
    {
     try
     {
      if (bwalert.IsShowing) bwalert.Dismiss();
     }
     catch { }
     alert.Dismiss();
    }
   };

   if (!Close)
   {
    var nBtn = alert.GetButton((int)DialogButtonType.Negative);
    nBtn.Click += (asender, args) =>
    {
     dgashowing = false;
     FinishA();
    };
   }

  }

  List<Button> buttonLIst; bool capson;
  LinearLayout tbsalt; private LinearLayout tbsecr;
  LinearLayout tbshash; private LinearLayout llhash;
  public AlertDialog bwalert;

  Button mButton1; Button mButton2; Button mButton3; Button mButton4; Button mButton5; Button mButton6; Button mButton7; Button mButton8; Button mButton9; Button mButton0; Button mButtonA; Button mButtonB; Button mButtonC; Button mButtonD; Button mButtonE; Button mButtonF; Button mButtonG; Button mButtonH; Button mButtonI; Button mButtonJ; Button mButtonK; Button mButtonL; Button mButtonM; Button mButtonN; Button mButtonO; Button mButtonP; Button mButtonQ; Button mButtonR; Button mButtonS; Button mButtonT; Button mButtonU; Button mButtonV; Button mButtonW; Button mButtonX; Button mButtonY; Button mButtonZ; Button mButtonDelete;

  Button mButtonEnter; Button mButtonSpace; Button mButtonShift; Button mButtonCaps;

  LinearLayout toptbsalt; LinearLayout toptbsecr; TextView tagsalt; TextView tagsecr;
  private void LoadBtns()
  {

   toptbsalt = _activity.FindViewById<LinearLayout>(Resource.Id.ttag1);
   toptbsecr = _activity.FindViewById<LinearLayout>(Resource.Id.ttag2);
   tagsalt = _activity.FindViewById<TextView>(Resource.Id.tag1);
   tagsecr = _activity.FindViewById<TextView>(Resource.Id.tag2);
   tbsalt = _activity.FindViewById<LinearLayout>(Resource.Id.pos1);
   tbsecr = _activity.FindViewById<LinearLayout>(Resource.Id.pos2);
   tbshash = _activity.FindViewById<LinearLayout>(Resource.Id.pos3);
   llhash = _activity.FindViewById<LinearLayout>(Resource.Id.llhash);

   buttonLIst = new List<Button>();

   mButton1 = (Button)_activity.FindViewById(Resource.Id.button_1); mButton2 = (Button)_activity.FindViewById(Resource.Id.button_2); mButton3 = (Button)_activity.FindViewById(Resource.Id.button_3); mButton4 = (Button)_activity.FindViewById(Resource.Id.button_4); mButton5 = (Button)_activity.FindViewById(Resource.Id.button_5); mButton6 = (Button)_activity.FindViewById(Resource.Id.button_6); mButton7 = (Button)_activity.FindViewById(Resource.Id.button_7); mButton8 = (Button)_activity.FindViewById(Resource.Id.button_8); mButton9 = (Button)_activity.FindViewById(Resource.Id.button_9); mButton0 = (Button)_activity.FindViewById(Resource.Id.button_0); mButtonA = (Button)_activity.FindViewById(Resource.Id.button_A); mButtonB = (Button)_activity.FindViewById(Resource.Id.button_B); mButtonC = (Button)_activity.FindViewById(Resource.Id.button_C); mButtonD = (Button)_activity.FindViewById(Resource.Id.button_D); mButtonE = (Button)_activity.FindViewById(Resource.Id.button_E); mButtonF = (Button)_activity.FindViewById(Resource.Id.button_F); mButtonG = (Button)_activity.FindViewById(Resource.Id.button_G); mButtonH = (Button)_activity.FindViewById(Resource.Id.button_H); mButtonI = (Button)_activity.FindViewById(Resource.Id.button_I); mButtonJ = (Button)_activity.FindViewById(Resource.Id.button_J); mButtonK = (Button)_activity.FindViewById(Resource.Id.button_K); mButtonL = (Button)_activity.FindViewById(Resource.Id.button_L); mButtonM = (Button)_activity.FindViewById(Resource.Id.button_M); mButtonN = (Button)_activity.FindViewById(Resource.Id.button_N); mButtonO = (Button)_activity.FindViewById(Resource.Id.button_O); mButtonP = (Button)_activity.FindViewById(Resource.Id.button_P); mButtonQ = (Button)_activity.FindViewById(Resource.Id.button_Q); mButtonR = (Button)_activity.FindViewById(Resource.Id.button_R); mButtonS = (Button)_activity.FindViewById(Resource.Id.button_S); mButtonT = (Button)_activity.FindViewById(Resource.Id.button_T); mButtonU = (Button)_activity.FindViewById(Resource.Id.button_U); mButtonV = (Button)_activity.FindViewById(Resource.Id.button_V); mButtonW = (Button)_activity.FindViewById(Resource.Id.button_W); mButtonX = (Button)_activity.FindViewById(Resource.Id.button_X); mButtonY = (Button)_activity.FindViewById(Resource.Id.button_Y); mButtonZ = (Button)_activity.FindViewById(Resource.Id.button_Z);
   buttonLIst.Add(mButton0); buttonLIst.Add(mButton1); buttonLIst.Add(mButton2); buttonLIst.Add(mButton3); buttonLIst.Add(mButton4); buttonLIst.Add(mButton5); buttonLIst.Add(mButton6); buttonLIst.Add(mButton7); buttonLIst.Add(mButton8); buttonLIst.Add(mButton9); buttonLIst.Add(mButtonA); buttonLIst.Add(mButtonB); buttonLIst.Add(mButtonC); buttonLIst.Add(mButtonD); buttonLIst.Add(mButtonE); buttonLIst.Add(mButtonF); buttonLIst.Add(mButtonG); buttonLIst.Add(mButtonH); buttonLIst.Add(mButtonI); buttonLIst.Add(mButtonJ); buttonLIst.Add(mButtonK); buttonLIst.Add(mButtonL); buttonLIst.Add(mButtonM); buttonLIst.Add(mButtonN); buttonLIst.Add(mButtonO); buttonLIst.Add(mButtonP); buttonLIst.Add(mButtonQ); buttonLIst.Add(mButtonR); buttonLIst.Add(mButtonS); buttonLIst.Add(mButtonT); buttonLIst.Add(mButtonU); buttonLIst.Add(mButtonV); buttonLIst.Add(mButtonW); buttonLIst.Add(mButtonX); buttonLIst.Add(mButtonY); buttonLIst.Add(mButtonZ);
   mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25; mButton0.Tag = 52; mButton1.Tag = 53; mButton2.Tag = 54; mButton3.Tag = 55; mButton4.Tag = 56; mButton5.Tag = 57; mButton6.Tag = 58; mButton7.Tag = 59; mButton8.Tag = 60; mButton9.Tag = 61;

   mButtonEnter = (Button)_activity.FindViewById(Resource.Id.button_enter);
   mButtonEnter.Tag = 1005;
   mButtonEnter.Click += BtnEnterClick;

   mButtonSpace = (Button)_activity.FindViewById(Resource.Id.button_SPACE);
   mButtonSpace.Tag = 88;
   mButtonSpace.Click += SpaceAddClick;

   mButtonDelete = (Button)_activity.FindViewById(Resource.Id.button_delete);
   mButtonDelete.Tag = 1002;
   mButtonDelete.Click += DelClick;

   mButtonShift = (Button)_activity.FindViewById(Resource.Id.button_SHIFT);
   mButtonShift.Tag = 1001;
   mButtonShift.Click += ShiftModClick;

   mButtonCaps = (Button)_activity.FindViewById(Resource.Id.button_CAPS);
   mButtonCaps.Tag = 1000;
   mButtonCaps.Click += CapsModClick;


   tbsalt.Click += TBSaltClick;
   tagsalt.Click += TBSaltClick;
   tbsecr.Click += TBSecretClick;
   tagsecr.Click += TBSecretClick;

   llhash.Visibility = ViewStates.Gone;

   toptbsalt.SetBackgroundResource(Resource.Color.space);
   tagsalt.SetBackgroundResource(Resource.Color.space);
   toptbsecr.SetBackgroundResource(Resource.Color.black);
   tagsecr.SetBackgroundResource(Resource.Color.black);

   foreach (Button b in buttonLIst)
   {
    b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
    b.Click += BtnClick;
   }

  }
  private void LoadInpViews()
  {
   LayoutInflater inflater = (LayoutInflater)_activity.GetSystemService(Context.LayoutInflaterService);
   View dialoglayout = inflater.Inflate(Resource.Layout.splashmodel, null);

   _splashModelTV = (TextView)dialoglayout.FindViewById(Resource.Id.ovt1S);

   AlertDialog.Builder builder = new AlertDialog.Builder(_activity, Resource.Style.FullscreenTheme);
   builder.SetCancelable(false);
   builder.SetView(dialoglayout);
   bwalert = builder.Create();
   //  bwalert.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.TurnScreenOn);

  }
  private void SpaceAddClick(object sender, System.EventArgs e)
  {
   var btn = (Button)sender;
   NoxAdd(Sclear.GetConsoleDictionaryFromIndex((int)btn.Tag));
   Vibe();

  }
  void DelClick(object sender, System.EventArgs e)
  {
   NoxRemove();
  }
  private void CapsModClick(object sender, System.EventArgs e)
  {

   mButtonShift.Text = "^";

   if (mButtonCaps.Text == "CAPS")
   {
    mButtonCaps.Text = "SCAP";
    capson = true;

    mButtonA.Tag = 26; mButtonB.Tag = 27; mButtonC.Tag = 28; mButtonD.Tag = 29; mButtonE.Tag = 30; mButtonF.Tag = 31; mButtonG.Tag = 32; mButtonH.Tag = 33; mButtonI.Tag = 34; mButtonJ.Tag = 35; mButtonK.Tag = 36; mButtonL.Tag = 37; mButtonM.Tag = 38; mButtonN.Tag = 39; mButtonO.Tag = 40; mButtonP.Tag = 41; mButtonQ.Tag = 42; mButtonR.Tag = 43; mButtonS.Tag = 44; mButtonT.Tag = 45; mButtonU.Tag = 46; mButtonV.Tag = 47; mButtonW.Tag = 48; mButtonX.Tag = 49; mButtonY.Tag = 50; mButtonZ.Tag = 51;
   }
   else
   {

    mButtonCaps.Text = "CAPS";
    capson = false;

    mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25;
   }

   foreach (Button b in buttonLIst)
   {
    b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
   }
  }

  private void ShiftModClick(object sender, System.EventArgs e)
  {
   if (mButtonShift.Text == "^")
   {
    mButtonShift.Text = "<";

    mButtonA.Tag = 62; mButtonB.Tag = 63; mButtonC.Tag = 64; mButtonD.Tag = 65; mButtonE.Tag = 66; mButtonF.Tag = 67; mButtonG.Tag = 68; mButtonH.Tag = 69; mButtonI.Tag = 70; mButtonJ.Tag = 71; mButtonK.Tag = 72; mButtonL.Tag = 73; mButtonM.Tag = 74; mButtonN.Tag = 75; mButtonO.Tag = 76; mButtonP.Tag = 77; mButtonQ.Tag = 78; mButtonR.Tag = 79; mButtonS.Tag = 80; mButtonT.Tag = 81; mButtonU.Tag = 82; mButtonV.Tag = 83; mButtonW.Tag = 84; mButtonX.Tag = 85; mButtonY.Tag = 86; mButtonZ.Tag = 87;

   }
   else
   {
    mButtonShift.Text = "^";

    if (capson)
    {
     mButtonA.Tag = 26; mButtonB.Tag = 27; mButtonC.Tag = 28; mButtonD.Tag = 29; mButtonE.Tag = 30; mButtonF.Tag = 31; mButtonG.Tag = 32; mButtonH.Tag = 33; mButtonI.Tag = 34; mButtonJ.Tag = 35; mButtonK.Tag = 36; mButtonL.Tag = 37; mButtonM.Tag = 38; mButtonN.Tag = 39; mButtonO.Tag = 40; mButtonP.Tag = 41; mButtonQ.Tag = 42; mButtonR.Tag = 43; mButtonS.Tag = 44; mButtonT.Tag = 45; mButtonU.Tag = 46; mButtonV.Tag = 47; mButtonW.Tag = 48; mButtonX.Tag = 49; mButtonY.Tag = 50; mButtonZ.Tag = 51;
    }
    else
    {
     mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25;
    }
   }


   foreach (Button b in buttonLIst)
   {
    b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);

   }
  }

  private void BtnClick(object sender, System.EventArgs e)
  {
   var b = (Button)sender;

   if ((int)b.Tag < 52)
   {
    if (capson)
    {
     if ((int)b.Tag < 26) b.Tag = (int)b.Tag + 26;

    }
    else
    {
     if ((int)b.Tag > 25) b.Tag = (int)b.Tag - 26;

    }
   }

   NoxAdd(Sclear.GetConsoleDictionaryFromIndex((int)b.Tag));
   Vibe();
  }
  private void BtnEnterClick(object sender, System.EventArgs e)
  {

   if (SaltSelected)
   {
    SaltSelected = false;
    toptbsecr.SetBackgroundResource(Resource.Color.space);
    tagsecr.SetBackgroundResource(Resource.Color.space);

    toptbsalt.SetBackgroundResource(Resource.Color.black);
    tagsalt.SetBackgroundResource(Resource.Color.black);

    tbshash.RemoveAllViews();
    llhash.Visibility = ViewStates.Gone;
   }
   else
   {
    SaltSelected = true;
    EnterEvent();
   }
  }
  private void TBSaltClick(object sender, System.EventArgs e)
  {
   SaltSelected = true;
   toptbsalt.SetBackgroundResource(Resource.Color.space);
   tagsalt.SetBackgroundResource(Resource.Color.space);

   toptbsecr.SetBackgroundResource(Resource.Color.black);
   tagsecr.SetBackgroundResource(Resource.Color.black);
  }
  private void TBSecretClick(object sender, System.EventArgs e)
  {
   SaltSelected = false;
   toptbsecr.SetBackgroundResource(Resource.Color.space);
   tagsecr.SetBackgroundResource(Resource.Color.space);

   toptbsalt.SetBackgroundResource(Resource.Color.black);
   tagsalt.SetBackgroundResource(Resource.Color.black);

   tbshash.RemoveAllViews();
   llhash.Visibility = ViewStates.Gone;
  }
  private void Vibe()
  {

   NoxDPM.Vibe();
  }

 }
}