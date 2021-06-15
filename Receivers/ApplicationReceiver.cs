using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace BitfiWallet
{

 [BroadcastReceiver(Name = "com.rokits.noxadmin.ApplicationReceiver", Enabled = true)]
 [IntentFilter(new string[] { Intent.ActionMyPackageReplaced })]
 public class ApplicationReceiver : BroadcastReceiver
 {
  public override void OnReceive(Context context, Intent intent)
  {

   var nxact = new Intent(context, typeof(NoxCosu));
   nxact.AddFlags(ActivityFlags.NoAnimation);
   nxact.PutExtra("launcher", "com.rokits.noxadmin.ApplicationReceiver");
   context.StartActivity(nxact);


  }

 }
}