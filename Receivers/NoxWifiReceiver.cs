using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BitfiWallet
{

 [BroadcastReceiver(Enabled = true)]
 [IntentFilter(new[] { WifiManager.ScanResultsAvailableAction })]
 public class NoxWifiReceiver : BroadcastReceiver
 {
  public override void OnReceive(Context context, Intent intent)
  {

   if (intent.Action == WifiManager.ScanResultsAvailableAction)
   {
    var success = intent.GetBooleanExtra(WifiManager.ExtraResultsUpdated, false);

   }
  }
 }

}