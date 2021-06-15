using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Android.Views;
using Android.Text;
using Android.Util;
using Android.App.Admin;
using Android.Runtime;
using Android.Graphics;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.UpdatePrompt", Label = "UPDATE", Theme = "@style/FullscreenTheme")]
 class UpdatePrompt : Activity
 {
  protected override void OnCreate(Bundle bundle)
  {

   SetContentView(Resource.Layout.updateprompt);
   base.OnCreate(bundle);

   try
   {

    string task = Intent.GetStringExtra("pmtask");

    if (task == "prompt")
    {

     Prompt();
     return;
    }

    if (task == "rokits")
    {
     string pubkey = Intent.GetStringExtra("pubkey");
     string host = Intent.GetStringExtra("host");
     noxWSLocalUpdateClient = new NoxWSLocalUpdateClient();
     noxWSLocalUpdateClient.OnStatusChanged += RokitsEvent;
     Rokits(host, pubkey);
     return;
    }

   }
   catch { }
  }

  protected override void OnPause()
  {
   base.OnPause();
   OverridePendingTransition(0, 0);
  }

  public override void OnBackPressed()
  {

  }

  void RokitsEvent(DataMSGCompletedEventArgs e)
  {
   RunOnUiThread(delegate ()
   {

    TextView msgTxt = (TextView)wfShowPrivMsgAlert.FindViewById(Android.Resource.Id.Message);
    msgTxt.Text = e.Message;

    if (e.IsError)
    {
     var wfnokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Negative);
     wfnokBtn.Visibility = ViewStates.Visible;

    }

   });
  }

  AlertDialog wfShowPrivMsgAlert;
  NoxWSLocalUpdateClient noxWSLocalUpdateClient;
  public CancellationTokenSource cancellationTokenMA = new CancellationTokenSource();

  private void Rokits(string host, string pubkey)
  {

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(this, Resource.Style.MyAlertDialogThemeNox2)
   .SetMessage(pubkey.ToUpper()).SetTitle("DOES SSLKEY MATCH?").SetCancelable(false)
   .SetNegativeButton("CLOSE ", (EventHandler<DialogClickEventArgs>)null)
   .SetPositiveButton("YES, UPDATE", (EventHandler<DialogClickEventArgs>)null);
   wfShowPrivMsgAlert = wfbuilder.Create();
   wfShowPrivMsgAlert.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.TurnScreenOn);

   wfShowPrivMsgAlert.Show();

   var wfokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Positive);
   var wfnokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Negative);

   TextView msgTxt = (TextView)wfShowPrivMsgAlert.FindViewById(Android.Resource.Id.Message);
   msgTxt.TextSize = 17.9f;
   msgTxt.SetTextColor(new Color(wfokBtn.CurrentTextColor));

   wfokBtn.Click += (asender, args) =>
   {
    wfShowPrivMsgAlert.SetTitle("BITFI UPDATE");
    wfokBtn.Visibility = ViewStates.Gone;
    wfnokBtn.Visibility = ViewStates.Gone;
    var pi = CreatPEObject();

    Task.Run(async () =>
    {
     await noxWSLocalUpdateClient.ProcessWSUpdate(host, pubkey, cancellationTokenMA.Token, pi);

    });
   };


   wfnokBtn.Click += (asender, args) =>
   {
    Finish();
   };

  }

  private void Prompt()
  {

   var deviceAdmin = new ComponentName("com.rokits.noxadmin", "com.rokits.noxadmin.AdminReceiver");
   var dpm = (DevicePolicyManager)GetSystemService(Context.DevicePolicyService);

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(this, Resource.Style.MyAlertDialogThemeNox2)
   .SetMessage(NoxDPM.updateStatus.Session.message).SetTitle("BITFI UPDATE").SetCancelable(false)
   .SetNegativeButton("CLOSE ", (EventHandler<DialogClickEventArgs>)null)
   .SetPositiveButton("LET'S DO THIS", (EventHandler<DialogClickEventArgs>)null);

   wfShowPrivMsgAlert = wfbuilder.Create();
   wfShowPrivMsgAlert.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.TurnScreenOn);

   wfShowPrivMsgAlert.Show();

   var wfokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Positive);
   var wfnokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Negative);

   TextView msgTxt = (TextView)wfShowPrivMsgAlert.FindViewById(Android.Resource.Id.Message);
   msgTxt.TextSize = 17.9f;
   msgTxt.SetTextColor(new Color(wfokBtn.CurrentTextColor));


   wfokBtn.Click += (asender, args) =>
   {

    msgTxt.Text = "installing...";
    wfokBtn.Visibility = ViewStates.Gone;
    wfnokBtn.Visibility = ViewStates.Gone;

    string error = "Error installing update.";

    Action workerThread = delegate ()
    {
     msgTxt.Text = error;
     wfnokBtn.Visibility = ViewStates.Visible;
    };


    try
    {

     new Thread(() =>
     {
      try
      {

       ProcessCOSU();

      }
      catch (Exception ex)
      {
       UpdateSession.ClearStagedSession(this);
       error = "ERROR: " + ex.Message;
       RunOnUiThread(workerThread);
      }

     }).Start();

    }
    catch (Exception e)
    {
     msgTxt.Text = "ERROR: " + e.Message;
     wfnokBtn.Visibility = ViewStates.Visible;
    }


   };


   wfnokBtn.Click += (asender, args) =>
   {
    Finish();
   };

  }

  private PendingIntent CreatPEObject()
  {
   Intent nxact = new Intent(ApplicationContext, typeof(UpdatePrompt));
   nxact.PutExtra("pmtask", "update");
   int randRequId = new Random().Next();
   return CreatePendingResult(randRequId, nxact, PendingIntentFlags.UpdateCurrent);
  }

  protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
  {

   base.OnActivityResult(requestCode, resultCode, data);

   try
   {
    if (data.HasExtra("android.content.pm.extra.STATUS"))
    {

     var status = data.GetIntExtra("android.content.pm.extra.STATUS", -1);

     if (status != 0)
     {
      var wfnokBtn = wfShowPrivMsgAlert.GetButton((int)DialogButtonType.Negative);
      TextView msgTxt = (TextView)wfShowPrivMsgAlert.FindViewById(Android.Resource.Id.Message);

      var msg = data.GetStringExtra("android.content.pm.extra.STATUS_MESSAGE");
      msgTxt.Text = "ERROR: " + msg;
      wfnokBtn.Visibility = ViewStates.Visible;

     }

    }


    data.Extras.Clear();
   }
   catch { }

  }
  private void ProcessCOSU()
  {

   UpdateSession.Commit(this.BaseContext, CreatPEObject());
  }
 }

}