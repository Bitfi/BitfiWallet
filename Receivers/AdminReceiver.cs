using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace BitfiWallet
{

 [BroadcastReceiver(Name = "com.rokits.noxadmin.AdminReceiver", Permission = "android.permission.BIND_DEVICE_ADMIN")]
 [Android.App.MetaDataAttribute("android.app.device_admin", Resource = "@xml/device_admin")]
 [IntentFilter(new string[] { "android.app.action.DEVICE_ADMIN_ENABLED", })]
 public class AdminReceiver : DeviceAdminReceiver
 {

  public override void OnEnabled(Context context, Intent intent)
  {
   base.OnEnabled(context, intent);

  }

 }

}