using System;
using Android.App;
using Android.App.Admin;
using Android.Content;
using WalletLibrary;
using BitfiWallet.DeviceManager;


namespace BitfiWallet
{
 public class NoxDPM
 {

  private readonly IAppProvider _Provider;

  public INoxDevice Device => _Provider.Device;

  public static NoxDPM DeviceInit(IAppProvider Provider)
  {
   return new NoxDPM(Provider);
  }

  private NoxDPM(IAppProvider Provider)
  {
   _Provider = Provider;

   Device.OnBatteryChanged += Device_OnBatteryChanged;
  }

  private void Notification_OnStatusChanged(NoxMSGCompletedEventArgs e) { }

  internal static ComponentName deviceAdmin
  {
   get
   {
    return NoxDevice.Current.deviceAdmin;
   }
  }

  internal static DevicePolicyManager dpm
  {
   get
   {
    return NoxDevice.Current.dpm;
   }
  }

  internal static NoxTManager NoxT
  {
   get
   {
    return NoxDevice.Current.NoxT;
   }
  }

  internal static void SetDeviceKey(string DeviceKey, string DeviceID)
  {
   NoxDevice.Current.SetTEE(DeviceKey, DeviceID);
  }

  public static int ThisVersion
  {
   get
   {
    return NoxDevice.Current.ThisVersion;
   }
   set
   {
    NoxDevice.Current.ThisVersion = value;
   }
  }

  public static bool IsDirectBoot
  {
   get
   {
    return NoxDevice.Current.IsDirectBoot;
   }
   set
   {
    NoxDevice.Current.IsDirectBoot = value;
   }
  }

  public static bool COUSU_Created
  {
   get
   {
    return NoxDevice.Current.COUSU_Created;
   }
   set
   {
    NoxDevice.Current.COUSU_Created = value;
   }
  }

  public static bool DirectAPKInstalled
  {
   get
   {
    return NoxDevice.Current.DirectAPKInstalled;
   }
   set
   {
    NoxDevice.Current.DirectAPKInstalled = value;
   }
  }

  public static UpdateStatus updateStatus
  {
   get
   {
    return NoxDevice.Current.NoxT.updateStatus;
   }
   set
   {
    NoxDevice.Current.NoxT.updateStatus = value;
   }
  }

  public static bool IsRestartPending()
  {
   return NoxDevice.Current.IsRestartPending();
  }

  public static bool PkgRestrictionsPending()
  {
   return NoxDevice.Current.PkgRestrictionsPending();
  }

  public static bool IsAllowVibe()
  {
   return NoxDevice.Current.PrefVibe();
  }

  public static bool IsAllowUpdate()
  {
   return NoxDevice.Current.PrefUpdate();
  }

  public static void UpdateSystemPref(bool Update, bool Vibe)
  {
   NoxDevice.Current.UpdateSystemPref(Update, Vibe);
  }

  private void Device_OnBatteryChanged(object sender, EventArgs e)
  {
   NoxT.batStatus = (BatStatus)sender;
  }

  public static Context GetContext()
  {
   return NoxDevice.Current.GetContext();
  }

  public static void AddActivity(Activity activity)
  {
   NoxDevice.Current.AddActivity(activity);
  }

  public static void LockNow()
  {
   NoxDevice.Current.LockNow();
  }

  public static void DownloadLockTask()
  {
   NoxDevice.Current.DownloadLockTask();
  }

  public static void Init()
  {
   NoxDevice.Current.StartDPM();
  }

  public static void RunPolicy(bool DirectAPKInstalled)
  {
   NoxDevice.Current.RunPolicy(DirectAPKInstalled);
  }

  public static void Vibe()
  {
   NoxDevice.Current.Vibe();
  }

 }

}