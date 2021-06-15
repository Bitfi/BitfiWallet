using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using System.Threading;
using System.Net;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.StatusActivity", Label = "", Theme = "@style/FullscreenTheme", HardwareAccelerated = true)]
 class StatusActivity : Activity
 {

  protected override void OnCreate(Bundle bundle)
  {
   SetContentView(Resource.Layout.status_model);
   base.OnCreate(bundle);

   Load();

   var btnrokits = FindViewById<Button>(Resource.Id.status_model_button_rokits);

   btnrokits.Click += delegate
   {

    if (NoxDPM.IsRestartPending())
    {
     ShowRebootMsg();

     return;
    }

    var status = FindViewById<TextView>(Resource.Id.status_model_tv_updatestatusM2);
    status.Text = "SEARCHING";

    btnrokits.Enabled = false;
    Discover();

   };


   var btnb = FindViewById<Button>(Resource.Id.status_model_button_brightness);
   btnb.Typeface = TFView.typeface;

   FindViewById<TextView>(Resource.Id.status_model_button_brightnesstv).Typeface = TFView.typeface;

   btnb.Click += delegate
   {
    Intent intent = new Intent(this, typeof(BrightnessActivity));
    StartActivity(intent);
    BrightnessStarted = true;

   };

   var btnf = FindViewById<Button>(Resource.Id.status_model_button_firmaware);
   btnf.Typeface = TFView.typeface;

   FindViewById<TextView>(Resource.Id.status_model_button_firmawaretv).Typeface = TFView.typeface;

   btnf.Click += delegate
   {

    Intent intent = new Intent(this, typeof(FirmwareActivity));
    intent.PutExtra("update", false);
    StartActivity(intent);

   };


   var btnv = FindViewById<Button>(Resource.Id.status_model_button_vibe);
   btnv.Typeface = TFView.typeface;

   FindViewById<TextView>(Resource.Id.status_model_button_vibetv).Typeface = TFView.typeface;

   var btns = FindViewById<Button>(Resource.Id.status_model_button_download);
   btns.Typeface = TFView.typeface;

   FindViewById<TextView>(Resource.Id.status_model_button_downloadtv).Typeface = TFView.typeface;

   Switch ds = FindViewById<Switch>(Resource.Id.status_model_download_switch);
   Switch vs = FindViewById<Switch>(Resource.Id.status_model_vibe_switch1);

   ds.Checked = NoxDPM.IsAllowUpdate();
   vs.Checked = NoxDPM.IsAllowVibe();

   ds.CheckedChange += delegate
   {
    if (NoxDPM.NoxT.updateStatus.Progress == UpdateProgress.DOWNLOADING || NoxDPM.NoxT.updateStatus.Progress == UpdateProgress.QUEUED)
    {
     ds.Checked = true;
     return;
    }

    SetPref(ds, vs);
   };

   vs.CheckedChange += delegate
   {
    SetPref(ds, vs);
   };

  }

  private void ShowRebootMsg()
  {
   AlertDialog.Builder builder = new AlertDialog.Builder(this)
   .SetTitle("").SetMessage("The device management policy requires a restart before committing an update, please reboot then try again.")
   .SetCancelable(true);
   var zzalert = builder.Create();
   zzalert.Show();
  }

  void SetPref(Switch ds, Switch vs)
  {
   bool Update = ds.Checked;
   bool Vibe = vs.Checked;

   NoxDPM.UpdateSystemPref(Update, Vibe);

  }

  bool BrightnessStarted = false;

  public CancellationTokenSource cancellationTokenMA = new CancellationTokenSource();

  private void Discover()
  {

   Task.Run(async () =>
   {
    var resp = await RokitDiscovery.RunScan(cancellationTokenMA);


    RunOnUiThread(delegate ()
    {
     if (resp != null && resp.Count > 0)
     {
      string host = resp[0];
      string pubkey = resp[1];

      var nxact = new Intent(this, typeof(UpdatePrompt));
      nxact.PutExtra("pmtask", "rokits");
      nxact.PutExtra("host", host);
      nxact.PutExtra("pubkey", pubkey);
      nxact.AddFlags(ActivityFlags.NoAnimation);
      StartActivity(nxact);
      FinishAfterTransition();

     }
     else
     {
      var btn = FindViewById<Button>(Resource.Id.status_model_button_rokits);
      btn.Enabled = true;

      var status = FindViewById<TextView>(Resource.Id.status_model_tv_updatestatusM2);
      status.Text = "NO ROKITS";
     }

    });


   });

  }

  private void ContinueUpdate()
  {
   try
   {
    if (NoxDPM.IsRestartPending())
    {
     ShowRebootMsg();

     return;
    }

    var nxact = new Intent(this, typeof(FirmwareActivity));
    nxact.PutExtra("update", true);
    StartActivity(nxact);

   }
   catch { }
  }

  protected override void OnResume()
  {
   base.OnResume();

   if (BrightnessStarted)
   {
    OverridePendingTransition(0, 0);
    BrightnessStarted = false;
   }
  }

  protected override void OnDestroy()
  {
   base.OnDestroy();
   cancellationTokenMA.Cancel();
  }

  private void CheckAgain()
  {

   Action setupThread = delegate ()
   {
    Load();
   };

   Task t = Task.Run(async () =>
   {
    await UpdateSession.RequestUpdateAvailable();
    RunOnUiThread(setupThread);

   });

  }

  private void Load()
  {

   var btn = FindViewById<Button>(Resource.Id.status_model_button_p);
   var title = FindViewById<TextView>(Resource.Id.status_model_title);
   var status = FindViewById<TextView>(Resource.Id.status_model_tv_updatestatus);

   try
   {
    if (!NoxDPM.updateStatus.Available)
    {

     btn.Text = "CHECK AGAIN";
     btn.Click += delegate
     {

      btn.Text = "CHECKING...";
      CheckAgain();
     };

     if (NoxDPM.updateStatus.Progress == UpdateProgress.UNKNOWN)
     {
      title.Text = "UPDATE STATUS PENDING CONNECTIVITY";
     }
     else
     {
      title.Text = "YOU ARE RUNNING THE LATEST VERSION";
     }

     status.Text = NoxDPM.updateStatus.Progress.ToString();

    }
    else
    {

     if (NoxDPM.updateStatus.Progress == UpdateProgress.READY)
     {

      btn.Text = "YES, CONTINUE";
      btn.Click += delegate { ContinueUpdate(); };
      title.Text = "UPDATE NOW READY TO INSTALL";
      status.Text = NoxDPM.updateStatus.Progress.ToString();

     }
     else
     {

      if (!NoxDPM.IsAllowUpdate())
      {

       btn.Text = "TRY AGAIN";
       btn.Click += delegate
       {

        btn.Text = "";
        CheckAgain();
       };

       title.Text = "ALLOW DOWNLOAD TO UPDATE";
       status.Text = "UPDATE NEEDED";

       return;
      }

      var btns = FindViewById<Button>(Resource.Id.status_model_button_download);
      btns.Enabled = false;
      var btnv = FindViewById<Button>(Resource.Id.status_model_button_vibe);
      btnv.Enabled = false;


      btn.Text = "";
      title.Text = "UPDATE WILL BE READY SOON";
      status.Text = NoxDPM.updateStatus.Progress.ToString();

      if (NoxDPM.updateStatus.Progress == UpdateProgress.DOWNLOADING)
      {
       string prog = " " + NoxDPM.updateStatus.DownloadProgress.ToString() + "%";
       if (NoxDPM.updateStatus.DownloadProgress == 100)
       {
        status.Text = "VALIDATING";
       }
       else
       {
        status.Text = NoxDPM.updateStatus.Progress.ToString() + prog;
       }

      }
      else
      {
       NoxDPM.DownloadLockTask();

      }


      Action setupThread = delegate ()
      {
       Load();
      };

      Task t = Task.Run(async () =>
       {
        try
        {
         await Task.Delay(2000);

         if (!this.IsFinishing && this.HasWindowFocus) ;
         RunOnUiThread(setupThread);
        }
        catch { }

       });

     }

    }
   }
   catch { }

  }

 }

}