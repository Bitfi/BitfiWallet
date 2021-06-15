using Android.App;
using Android.Content;
using Android.OS;
using static Android.OS.PowerManager;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.MessageActivity", Theme = "@style/FullscreenTheme", Label = "")]
 public class MessageActivity : Activity
 {
  private NoxMessageActivity noxMessageActivity = new NoxMessageActivity();

  WakeLock wakeLock;
  protected override void OnCreate(Bundle bundle)
  {
   base.OnCreate(bundle);
   SetContentView(Resource.Layout.session);

   PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
   wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "com.rokits.noxadmin");

   noxMessageActivity.Init(this);
  }
  protected override void OnStop()
  {
   if (!wakeLock.IsHeld) wakeLock.Acquire();
   noxMessageActivity.HandleStop();
   base.OnStop();
  }
  protected override void OnStart()
  {
   base.OnStart();
   if (wakeLock.IsHeld) wakeLock.Release();
   noxMessageActivity.HandleStart();
  }
  protected override void OnDestroy()
  {
   if (wakeLock.IsHeld) wakeLock.Release();
   noxMessageActivity.HandleDstry();
   base.OnDestroy();
  }
  public override void OnBackPressed()
  {
   noxMessageActivity.HandleBP();
   OverridePendingTransition(0, 0);
  }
  protected override void OnPause()
  {
   base.OnPause();
   OverridePendingTransition(0, 0);
  }
 }
}