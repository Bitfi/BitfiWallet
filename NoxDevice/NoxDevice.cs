using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using NoxService.NWS;
using WalletLibrary;
using Android.Preferences;
using Newtonsoft.Json;

namespace BitfiWallet.DeviceManager
{

 public class NoxDevice : INoxDevice
 {

  public event EventHandler OnBatteryChanged;

  private readonly Context context;
  private readonly Vibrator v;
  private readonly PowerManager.WakeLock wakeLock;

  private List<WeakReference<Activity>> acts = new List<WeakReference<Activity>>();

  public static NoxDevice Current { get; private set; }

  public readonly ComponentName deviceAdmin;
  public readonly DevicePolicyManager dpm;

  private NoxTManager _NoxT = null;

  public int ThisVersion { get; set; }
  public bool IsDirectBoot { get; set; }
  public bool COUSU_Created { get; set; }
  public bool DirectAPKInstalled { get; set; }

  private bool _RestartPending;

  private bool _PrefUpdate;
  private bool _PrefVibe;

  public bool IsRestartPending()
  {
   return _RestartPending;
  }

  public void SetTEE(string DeviceKey, string DeviceID)
  {
   if (_NoxT != null)
    return;

   _NoxT = new NoxTManager(DeviceKey, DeviceID);
  }

  public NoxTManager NoxT
  {
   get
   {
    return _NoxT;
   }
  }

  public NoxDevice(Context appContext)
  {
   if (Current != null)
    return;

   Current = this;

   context = appContext;

   deviceAdmin = new ComponentName(context.PackageName, "com.rokits.noxadmin.AdminReceiver");
   dpm = (DevicePolicyManager)context.GetSystemService("device_policy");
   wakeLock = ((PowerManager)context.GetSystemService("power")).NewWakeLock(WakeLockFlags.Partial, "com.rokits.noxadmin");
   v = (Vibrator)context.GetSystemService("vibrator");

  }

  public bool PrefUpdate()
  {
   return _PrefUpdate;
  }

  public bool PrefVibe()
  {
   return _PrefVibe;
  }

  ISharedPreferencesEditor preferencesEditor;

  public void LoadSystemPref()
  {
   try
   {
    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(context.ApplicationContext);
    preferencesEditor = sharedPref.Edit();
    string pref = sharedPref.GetString("USER_STATUS_SYSTEM_PREF", "");
    if (string.IsNullOrEmpty(pref))
    {
     _PrefVibe = true;
     _PrefUpdate = true;

     return;
    }

    SystemPref systemPref = JsonConvert.DeserializeObject<SystemPref>(pref);
    _PrefUpdate = systemPref.Update;
    _PrefVibe = systemPref.Vibe;
   }
   catch { }
  }

  public void UpdateSystemPref(bool Update, bool Vibe)
  {
   try
   {
    if (preferencesEditor == null)
     return;



    if (Update != _PrefUpdate || Vibe != _PrefVibe)
    {
     SystemPref systemPref = new SystemPref() { Update = Update, Vibe = Vibe };

     preferencesEditor.PutString("USER_STATUS_SYSTEM_PREF", JsonConvert.SerializeObject(systemPref));
     preferencesEditor.Apply();

     _PrefUpdate = Update;
     _PrefVibe = Vibe;

    }

   }
   catch { }
  }

  public void BatteryChanged(BatStatus status)
  {
   try
   {
    if (OnBatteryChanged == null)
     return;

    OnBatteryChanged?.Invoke(status, null);
   }
   catch { }
  }

  public Context GetContext()
  {
   return context;
  }

  public void AddActivity(Activity activity)
  {
   acts.Add(new WeakReference<Activity>(activity));
  }

  public void LockNow()
  {

   if (!COUSU_Created)
    return;

   if (!DirectAPKInstalled)
    return;


   CloseAll();

   DownloadLockTask();

  }

  public void DownloadLockTask()
  {
   if (NoxT == null) return;
   if (wakeLock.IsHeld) return;


   new Thread(() =>
   {
    wakeLock.Acquire();

    try
    {

     if (NoxT.updateStatus.Available && NoxT.updateStatus.Progress == UpdateProgress.QUEUED)
     {
      UpdateSession.Run(context);
     }
     else
     {
      if (PkgRestrictionsPending() && IsDirectBoot)
      {
       DisablePackages();
      }

      CheckLgcyPkgs();
     }


     GC.Collect(0, GCCollectionMode.Forced);
     GC.WaitForPendingFinalizers();
     GC.Collect();

    }
    catch { }

    wakeLock.Release();


   }).Start();
  }

  void CloseAll()
  {
   try
   {
    foreach (var activity in GetActivities())
    {
     try
     {

      activity.Finish();

     }
     catch { }
    }
   }
   catch { }
  }

  List<Activity> GetActivities()
  {
   List<Activity> activities = new List<Activity>();

   foreach (var wr in acts)
   {
    Activity activity;
    if (wr.TryGetTarget(out activity))
    {
     activities.Add(activity);
    }
   }

   acts.Clear();
   return activities;
  }

  public void StartDPM()
  {


   _RestartPending = PkgRestrictionsPending();

   dpm.SetPermissionPolicy(deviceAdmin, PermissionPolicy.AutoDeny);
   dpm.SetScreenCaptureDisabled(deviceAdmin, true);
   dpm.SetNetworkLoggingEnabled(deviceAdmin, false);
   dpm.SetSecurityLoggingEnabled(deviceAdmin, false);
   dpm.SetBackupServiceEnabled(deviceAdmin, false);
   dpm.SetStatusBarDisabled(deviceAdmin, true);
   dpm.SetAutoTimeRequired(deviceAdmin, true);

   dpm.SetPermissionGrantState(deviceAdmin, context.PackageName, "android.permission.ACCESS_FINE_LOCATION", PermissionGrantState.Granted);

   dpm.SetPermissionGrantState(deviceAdmin, context.PackageName, "android.permission.BIND_VPN_SERVICE", PermissionGrantState.Granted);


   try
   {
    dpm.SetGlobalSetting(deviceAdmin, "WIFI_DEVICE_OWNER_CONFIGS_LOCKDOWN".ToLower(), "1");
   }
   catch { }

   try
   {
    dpm.SetGlobalSetting(deviceAdmin, "DEVICE_PROVISIONED".ToLower(), "1");
   }
   catch { }

   AddRestrictions();

  }

  public void RunPolicy(bool DirectAPKInstalled)
  {

   if (DirectAPKInstalled)
   {

    List<string> scopes = new List<string>();
    scopes.Add(DevicePolicyManager.DelegationAppRestrictions);
    scopes.Add(DevicePolicyManager.DelegationPackageAccess);
    scopes.Add(DevicePolicyManager.DelegationPermissionGrant);
    dpm.SetDelegatedScopes(deviceAdmin, "com.rokits.direct", scopes);

    var xcomponentName = new ComponentName("com.rokits.direct", "com.rokits.direct.MainActivity");
    var xintentFilter = new IntentFilter(Intent.ActionMain);
    xintentFilter.AddCategory(Intent.CategoryLauncher);
    dpm.AddPersistentPreferredActivity(deviceAdmin, xintentFilter, xcomponentName);

    dpm.SetLockTaskPackages(deviceAdmin, new string[] { context.PackageName, "com.rokits.direct", "com.android.systemui" });
   }
   else
   {
    dpm.SetLockTaskPackages(deviceAdmin, new string[] { context.PackageName, "com.android.systemui" });
   }

   ComponentName componentName = new ComponentName(context.PackageName, "com.rokits.noxadmin.NoxCosu");
   IntentFilter intentFilter = new IntentFilter(Intent.ActionMain);
   intentFilter.AddCategory(Intent.CategoryHome);
   dpm.AddPersistentPreferredActivity(deviceAdmin, intentFilter, componentName);

  }

  public bool PkgRestrictionsPending()
  {
   try
   {
    if (dpm.IsPackageSuspended(deviceAdmin, "com.adups.fota"))
    {
     return false;
    }
   }
   catch { }

   return true;
  }

  private void CheckLgcyPkgs()
  {
   try
   {
    if (_RestartPending)
     return;

    if (!IsDirectBoot)
     return;

    if (!dpm.IsPackageSuspended(deviceAdmin, "app.rokits.android"))
    {
     List<string> pkgs = ConfigValues.package_restrictions.ToList();
     pkgs.Add("app.rokits.android");

     try
     {
      dpm.SetPackagesSuspended(deviceAdmin, pkgs.ToArray(), true);
     }
     catch { }

     foreach (var pkg in pkgs)
     {
      try
      {
       dpm.SetApplicationHidden(deviceAdmin, pkg, true);
      }
      catch { }
     }

     ActivityManager am = (ActivityManager)context.GetSystemService(Activity.ActivityService);

     foreach (var pkg in pkgs)
     {
      try
      {
       am.KillBackgroundProcesses(pkg);
      }
      catch { }
     }
    }
   }
   catch { }

  }

  void DisablePackages()
  {
   try
   {
    dpm.SetPackagesSuspended(deviceAdmin, ConfigValues.package_restrictions, true);

    foreach (var pkg in ConfigValues.package_restrictions)
    {
     dpm.SetApplicationHidden(deviceAdmin, pkg, true);
    }

   }
   catch { }

  }

  public void setAllowUserRestriction(string restriction)
  {
   try
   {
    dpm.ClearUserRestriction(deviceAdmin,
         restriction);
   }
   catch { }
  }

  public void setDis_AllowUserRestriction(string restriction)
  {
   try
   {
    dpm.AddUserRestriction(deviceAdmin,
         restriction);
   }
   catch { }
  }

  public void AddRestrictions()
  {
   foreach (var ur in ConfigValues.restrictions)
   {
    setDis_AllowUserRestriction(ur);
   }
  }

  public void clearRestrictions()
  {
   foreach (var ur in ConfigValues.restrictions)
   {
    setAllowUserRestriction(ur);
   }
  }

  public void Vibe()
  {
   if (!_PrefVibe)
    return;

   v.Vibrate(VibrationEffect.CreateOneShot(70, 100));
  }

 }



}

