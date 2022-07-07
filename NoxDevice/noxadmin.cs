using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.App.Application;
using Android.Content;
using Android.Content.Res;
using Java.Interop;
using Android.App.Admin;


namespace BitfiWallet.DeviceManager
{

 [Application(Name = "rokits.noxadmin", LargeHeap = true, SupportsRtl = true, HardwareAccelerated = true, Label = "NoxAdmin", Theme = "@style/DEREXCFullscreenTheme")]
 public class noxadmin : Application, IActivityLifecycleCallbacks, IAppProvider
 {

  private NoxDPM manager { get; set; }

  public INoxDevice Device { get; }

  public NoxNotification NoxNotification { get; }

  public NoxChannel NoxChannel { get; }

  public noxadmin(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
  {
   Device = new NoxDevice(this);
   NoxNotification = new NoxNotification();
   NoxChannel = new NoxChannel();
  }

  public override void OnCreate()
  {

   base.OnCreate();

   manager = NoxDPM.DeviceInit(this);

   RegisterActivityLifecycleCallbacks(this);

  }

  public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
  {


   try
   {
    activity.Window.AddFlags(WindowManagerFlags.LayoutNoLimits | WindowManagerFlags.LayoutInOverscan | WindowManagerFlags.Fullscreen | WindowManagerFlags.Secure);

   }
   catch { }

   if (activity.LocalClassName.ToLower().Contains("secret"))
    return;

   if (activity.LocalClassName.ToLower().Contains("updateprompt"))
    return;

   if (activity.LocalClassName.ToLower().Contains("brightness"))
    return;

   if (activity.LocalClassName.ToLower().Contains("mainactivityb"))
    return;

   if (activity.LocalClassName.ToLower().Contains("noxviewmodel"))
    return;

   if (activity.LocalClassName.ToLower().Contains("messageactivity"))
    return;

   if (activity.LocalClassName.ToLower().Contains("metaactivity"))
    return;

   NoxDPM.AddActivity(activity);

  }

  public void OnBackPressed()
  {

  }

  public void OnActivityDestroyed(Activity activity)
  {

  }

  public void OnActivityPaused(Activity activity)
  {

  }

  public void OnActivityResumed(Activity activity)
  {


  }

  public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
  {

  }

  public void OnActivityStarted(Activity activity)
  {

  }

  public void OnActivityStopped(Activity activity)
  {


  }

 }


}