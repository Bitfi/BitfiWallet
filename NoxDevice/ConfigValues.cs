using System;
using Android.OS;

namespace BitfiWallet
{
 class ConfigValues
 {
  public static readonly string[] restrictions = new string[] {

   UserManager.DisallowCrossProfileCopyPaste,
   UserManager.DisallowAddUser,
   UserManager.DisallowAppsControl,
   UserManager.DisallowUninstallApps,
   UserManager.DisallowUsbFileTransfer,
   UserManager.DisallowDebuggingFeatures,
   UserManager.DisallowInstallApps,
   UserManager.DisallowMountPhysicalMedia,
   UserManager.DisallowSafeBoot,
   UserManager.DisallowCreateWindows,
   UserManager.DisallowConfigCellBroadcasts,
   UserManager.DisallowAdjustVolume,
   UserManager.DisallowBluetooth,
   UserManager.DisallowBluetoothSharing,
   UserManager.DisallowConfigBluetooth,
   UserManager.DisallowConfigTethering,
   UserManager.DisallowOutgoingCalls,
   UserManager.DisallowOutgoingBeam,
   UserManager.DisallowFun,
   UserManager.DisallowSms,
   UserManager.DisallowConfigMobileNetworks,
   UserManager.DisallowConfigCredentials,
   UserManager.DisallowConfigWifi

 };

  public static readonly string[] package_restrictions = new string[] {

      "com.android.settings",
      "com.android.shell",
      "com.android.launcher3",
   "com.android.inputmethod.latin",
   "com.android.smspush",
   "com.android.phone",
   "com.android.webview",
   "com.android.captiveportallogin",
"com.android.providers.downloads.ui",
"com.android.browser",
"com.android.providers.downloads",
"com.android.storagemanager",
"com.android.deskclock",
"com.android.printservice.recommendation",
"com.android.printspooler",
"com.android.calculator2",
"com.android.calllogbackup",
"com.android.carrierconfig",
"com.android.mms.service",
"com.android.providers.contacts",
"com.android.providers.media",
   "com.valmul.defcontainer",
    "com.adups.fota.sysoper",
    "com.adups.fota",
    "com.mediatek.engineermode",
    "com.mediatek.omacp",
    "com.mediatek.providers.drm",
    "com.mediatek.batterywarning",
    "com.mediatek",
    "com.mediatek.duraspeed",
    "com.valmedia.fdelux",
    "com.svox.pico",
    "com.mediatek.calendarimporter",
    "com.mediatek.thermalmanager",
    "com.mediatek.callrecorder",
    "com.mediatek.webview",
    "com.mediatek.factorymode",
    "com.mtk.telephony",
    "com.baidu.map.location",
    "jp.co.omronsoft.openwnn",
    "com.mediatek.location.mtknlp",
    "com.mediatek.filemanager",
    "com.mediatek.mtklogger",
    "com.mediatek.mtklogger.proxy",
  };


  public const string SMSTokenMsg = "92115a14ce29ffaaab1852ea46f8733ff7375d3d";


 }

}
