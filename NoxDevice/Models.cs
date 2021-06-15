using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WalletLibrary;

namespace BitfiWallet
{

 public interface IAppProvider
 {
  INoxDevice Device { get; }

 }

 public interface INoxDevice
 {
  event EventHandler OnBatteryChanged;
 }

 public class BatStatus
 {
  public bool IsCharging { get; set; }
  public Int32 Level { get; set; }
  public bool IsError { get; set; }

 }

 public class SystemPref
 {
  public bool Vibe { get; set; }

  public bool Update { get; set; }
 }

 public enum UpdateProgress
 {
  QUEUED = 0,
  DOWNLOADING = 1,
  READY = 2,
  //  ERROR = 3,
  CURRENT = 3,
  UNKNOWN = 4
 }

 public class UpdateStatus
 {
  public bool Available { get; set; }
  public bool Checking { get; set; }
  public bool Notified { get; set; }
  public UpdateProgress Progress { get; set; }
  public UpdateResponse Session { get; set; }

  public int DownloadProgress { get; set; }
  //  public string ErrorMessage { get; set; }
 }

 public class UpdateResponse
 {
  public int version { get; set; }
  public string message { get; set; }
  public string signature { get; set; }
  public string hash { get; set; }
  public int session { get; set; }
 }

 public enum ResponseActionType
 {
  REQUEST = 0,
  SHOW_RATE = 1,
  NOWIFI = 2,
  UPDATE = 3
 }

 public class WorkResponse
 {
  public NoxService.NWS.FormUserInfo UserInfo { get; set; }
  public bool CancellationRequested { get; set; }
  public string ValidationRate { get; set; }

  public ResponseActionType Action;
 }

 public class NoxCaptive
 {
  public BTCRate bTCRate { get; set; }
 }
 public class BTCRate
 {
  public string USD { get; set; }
 }

 public class NetStatus
 {
  public string ConnectionStatus { get; set; }
  public bool NoWifi { get; set; }

 }

 public class TFView
 {
  public static Typeface typeface;
  public static Typeface typefaceI;
  public static Typeface typefaceB;
 }
}