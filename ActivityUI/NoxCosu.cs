using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using Android.Graphics;
using WalletLibrary;
using System.Threading.Tasks;
using System.Threading;
using Android;
using System.Collections.Generic;
using Android.App.Admin;
using Android.Runtime;
using Android.Net.Wifi;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.NoxCosu", Label = "", HardwareAccelerated = true, AlwaysRetainTaskState = true)]
 [IntentFilter(actions: new string[] { "android.intent.action.MAIN" }, Categories = new string[] { "android.intent.category.DEFAULT", "android.intent.category.HOME" })]
 class NoxCosu : Activity
 {

  protected override void OnCreate(Bundle savedInstanceState)
  {

   base.OnCreate(savedInstanceState);

   string launcher = string.Empty;

   try
   {
    launcher = Intent.GetStringExtra("launcher");
   }
   catch { }

   Action workerThread = delegate ()
   {
    SetContentView(Resource.Layout.Main);

    TextView tvLabel = FindViewById<TextView>(Resource.Id.txtLauncherMain);
    tvLabel.SetTypeface(TFView.typeface, TypefaceStyle.Normal);

    if (launcher == "com.rokits.direct.MainActivity") tvLabel.Text = "Welcome to Bitfi";

    if (launcher == "com.rokits.noxadmin.ApplicationReceiver") tvLabel.Text = "Update complete!";

   };


   Task.Run(async () =>
   {

    try
    {
     if (launcher == "com.rokits.direct.MainActivity") NoxDPM.IsDirectBoot = true;

     TFView.typeface = Typeface.CreateFromAsset(Assets, "Rubik-Regular.ttf");
     RunOnUiThread(workerThread);

     TFView.typefaceI = Typeface.CreateFromAsset(Assets, "Rubik-Italic.ttf");
     TFView.typefaceB = Typeface.CreateFromAsset(Assets, "Rubik-Bold.ttf");

     await StartDevice();

    }
    catch
    {

    }
    finally
    {
     NoxDPM.COUSU_Created = true;

     if (NoxDPM.DirectAPKInstalled)
     {
      OpenWallet();
     }

     //FindViewById<LinearLayout>(Resource.Id.llLauncherMain).Visibility = ViewStates.Visible;
     //SetTheme(Resource.Style.FullscreenTheme);
    }

   });

  }


  protected override void OnResume()
  {


   base.OnResume();

   StartLockTask();

   if (NoxDPM.COUSU_Created && NoxDPM.DirectAPKInstalled)
   {
    OpenWallet();
   }

  }

  private async Task StartDevice()
  {

   NoxDPM.Init();

   NoxDPM.DirectAPKInstalled = UpdateSession.NoxSplashInstalled(NoxDPM.GetContext());
   NoxDPM.RunPolicy(NoxDPM.DirectAPKInstalled);

   Task t2 = Task.Run(async () =>
   {

    var kStore = KeyEnc.GetKeyStore(NoxDPM.GetContext());
    var DeviceID = kStore[0];
    var DeviceKey = kStore[1];

    NoxDPM.SetDeviceKey(DeviceKey, DeviceID);

    PackageReceiver packageReceiver = new PackageReceiver();
    IntentFilter lockFilter = new IntentFilter();
    lockFilter.AddAction(Intent.ActionScreenOff);
    lockFilter.AddAction(Intent.ActionBatteryChanged);
    ApplicationContext.RegisterReceiver(packageReceiver, lockFilter);


    DeviceManager.NoxDevice.Current.LoadSystemPref();

    NoxDPM.ThisVersion = UpdateSession.ThisVersion(NoxDPM.GetContext());

    var session = UpdateSession.GetStagedSession(NoxDPM.GetContext());

    if (session != null)
    {
     NoxDPM.updateStatus.Available = true;
     NoxDPM.updateStatus.Progress = UpdateProgress.READY;
     NoxDPM.updateStatus.Session = session;
    }

   });

   Task t3 = Task.Run(async () =>
   {

    if (!NoxDPM.DirectAPKInstalled)
    {
     StartLockTask();

     Action setupThread = delegate ()
     {

      TextView tvLabel = FindViewById<TextView>(Resource.Id.txtLauncherMain);
      tvLabel.Text = "restarting...";
     };

     var pi = CreatPEObject();

     RunOnUiThread(setupThread);

     UpdateSession.SetDirectAPK(NoxDPM.GetContext(), pi);

    }


    SPValidator.Initialize();

   });

   Task t4 = Task.Run(async () =>
   {
    Sclear.Initiate();

    var noxWifiReceiver = new NoxWifiReceiver();
    var sswifi = (WifiManager)GetSystemService(Context.WifiService);

    try
    {
     while (!HasWindowFocus)
     {
      await Task.Delay(50);
     }

     IntentFilter intentFilter = new IntentFilter();
     intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
     RegisterReceiver(noxWifiReceiver, intentFilter);

     var scan = sswifi.StartScan();
    }
    catch { }
    finally
    {
     UnregisterReceiver(noxWifiReceiver);

    }

   });


   await Task.WhenAll(new Task[] { t2, t3, t4 });



  }

  private PendingIntent CreatPEObject()
  {
   Intent ni = new Intent(this, typeof(NoxCosu));
   ni.PutExtra("pmtask", "update");
   int randRequId = new Random().Next();
   return CreatePendingResult(randRequId, ni, PendingIntentFlags.UpdateCurrent);
  }

  protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
  {

   base.OnActivityResult(requestCode, resultCode, data);

   try
   {
    if (data.HasExtra("android.content.pm.extra.STATUS"))
    {

     var status = data.GetIntExtra("android.content.pm.extra.STATUS", -1);

     if (status == 0)
     {

      NoxDPM.RunPolicy(true);
      NoxDPM.dpm.Reboot(NoxDPM.deviceAdmin);

     }
     else
     {
      OpenWallet();
     }


    }

    data.Extras.Clear();
   }
   catch { }

  }

  public override void OnBackPressed()
  {

  }

  void OpenWallet()
  {

   Intent nxact = new Intent(this, typeof(MainActivityB));
   nxact.AddFlags(ActivityFlags.NoAnimation);
   StartActivity(nxact);
  }

 }

}