using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System;

namespace BitfiWallet
{

 [BroadcastReceiver(Name = "com.rokits.noxadmin.PackageReceiver")]
 [IntentFilter(new string[] { Intent.ActionScreenOff, Intent.ActionBatteryChanged })]
 public class PackageReceiver : BroadcastReceiver
 {

  public override void OnReceive(Context context, Intent intent)
  {


   if (Intent.ActionScreenOff == intent.Action)
   {

    NoxDPM.LockNow();
    return;
   }

   try
   {
    if (Intent.ActionBatteryChanged == intent.Action)
    {
     int status = intent.GetIntExtra(BatteryManager.ExtraStatus, -1);
     int pluggedIn = intent.GetIntExtra(BatteryManager.ExtraPlugged, 0);
     int BatLevel = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);

     int BatCharging = 0;
     if (pluggedIn > 0 || status == 2)
     {
      BatCharging = 1;
     }


     RelayBatteryStatus(BatLevel, BatCharging);

     return;
    }
   }
   catch { }

  }

  void RelayBatteryStatus(int Level, int Charging)
  {
   try
   {

    bool IsCharging = false;
    if (Charging > 0) IsCharging = true;

    BatStatus bat = new BatStatus();
    bat.Level = Level;
    bat.IsCharging = IsCharging;

    DeviceManager.NoxDevice.Current.BatteryChanged(bat);

   }
   catch { }
  }
 }

}