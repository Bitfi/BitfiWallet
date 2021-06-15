using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Android.App.Admin;
using Android.Preferences;
using System.Threading;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using System.Net;
using System.Web;
using System.ComponentModel;

namespace BitfiWallet
{

 class AsyncDownloadClient
 {
  WebClient client = new WebClient();
  RokitProxy rokitProxy = new RokitProxy();
  string _address;
  public AsyncDownloadClient(string address)
  {
   _address = address;
   client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadFileCompletedCallback);
   client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
  }

  void DownloadFile()
  {
   try
   {
    Uri uri = new Uri(_address);
    client.DownloadDataAsync(uri);
   }
   catch (WebException)
   {
    rokitProxy.LocallyHandledObjectArrived(null);
   }
   catch (Exception ex)
   {
    rokitProxy.LocallyHandledObjectArrived(null);
   }
  }
  public async Task<byte[]> GetDownload()
  {
   var result = await GetRokitsTask();
   return result.RokitResult;
  }
  Task<RokitsCompletedEventArgs> GetRokitsTask()
  {

   TaskCompletionSource<RokitsCompletedEventArgs> ntask = new TaskCompletionSource<RokitsCompletedEventArgs>();
   rokitProxy.TaskCompleted += (e) => CompletedEvent(ntask, e, () => e);

   DownloadFile();
   return ntask.Task;
  }

  void CompletedEvent<T>(TaskCompletionSource<T> tcs,
   System.ComponentModel.AsyncCompletedEventArgs e, Func<T> getResult)
  {
   try
   {
    if (e.Error != null) tcs.TrySetException(e.Error);
    else if (e.Cancelled) tcs.TrySetCanceled();
    else tcs.TrySetResult(getResult());
   }
   catch (Exception)
   {
    tcs.TrySetCanceled();
   }
  }
  void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
  {
   NoxDPM.updateStatus.DownloadProgress = e.ProgressPercentage;
  }
  void DownloadFileCompletedCallback(object sender, DownloadDataCompletedEventArgs e)
  {
   try
   {
    if (!e.Cancelled && e.Error == null)
    {
     rokitProxy.LocallyHandledObjectArrived(e.Result);
    }
    else
    {
     rokitProxy.LocallyHandledObjectArrived(null);
    }
   }
   catch { }
  }

  class RokitProxy
  {
   public event RokitsCompletedEventHandler TaskCompleted;
   public void LocallyHandledObjectArrived(byte[] result)
   {
    if (TaskCompleted != null)
    {
     try
     {
      TaskCompleted.Invoke(new RokitsCompletedEventArgs(result, null));
     }
     catch { }
    }
   }
  }

  delegate void RokitsCompletedEventHandler(RokitsCompletedEventArgs e);
  partial class RokitsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
  {
   public RokitsCompletedEventArgs(byte[] result, object error) : base(null, false, null)
   {
    RokitResult = result;
   }
   public byte[] RokitResult { get; }

  }
 }
 class UpdateSession
 {

  const string UPDATE_URI_BASE = "https://bitfi.com/NoxUpdateECDSA/DMA4Device.aspx";
  static Uri GetVersionUri()
  {
   string PublicKey = NoxDPM.NoxT.noxDevice.PubTEE().ToHex();
   string DeviceID = NoxDPM.NoxT.noxDevice.GetDeviceID();

   var msgRequest = GetVersionMessageUri(PublicKey, DeviceID);
   var msg = GetMessageForSigning(msgRequest);
   if (msg == null || msg.Length == 0) return null;

   string Signature = NoxDPM.NoxT.noxDevice.GetUpdateSignature(msg);
   return GetVersionUri(Signature, PublicKey);
  }
  static Uri GetDownloadUri()
  {
   string PublicKey = NoxDPM.NoxT.noxDevice.PubTEE().ToHex();
   string DeviceID = NoxDPM.NoxT.noxDevice.GetDeviceID();

   var msgRequest = GetDownloadMessageUri(PublicKey, DeviceID);
   var msg = GetMessageForSigning(msgRequest);
   if (msg == null) return null;

   string Signature = NoxDPM.NoxT.noxDevice.GetUpdateSignature(msg);
   return GetDownloadUri(Signature, PublicKey);
  }
  static Uri GetVersionUri(string Signature, string PublicKey)
  {
   UriBuilder uriBuilder = new UriBuilder(UPDATE_URI_BASE);
   var query = HttpUtility.ParseQueryString(uriBuilder.Query);
   query["AuthVersionRequest"] = Signature;
   query["DIDRequest"] = PublicKey;
   query["DIDVersion"] = NoxDPM.ThisVersion.ToString();
   uriBuilder.Query = query.ToString();
   return uriBuilder.Uri;
  }
  static Uri GetDownloadUri(string Signature, string PublicKey)
  {
   UriBuilder uriBuilder = new UriBuilder(UPDATE_URI_BASE);
   var query = HttpUtility.ParseQueryString(uriBuilder.Query);
   query["AuthWalletRequest"] = Signature;
   query["DIDRequest"] = PublicKey;
   uriBuilder.Query = query.ToString();
   return uriBuilder.Uri;
  }

  static Uri GetVersionMessageUri(string PublicKey, string DeviceID)
  {
   UriBuilder uriBuilder = new UriBuilder(UPDATE_URI_BASE);
   var query = HttpUtility.ParseQueryString(uriBuilder.Query);
   query["HIDVersionRequest"] = DeviceID;
   query["DIDRequest"] = PublicKey;
   query["DIDVersion"] = NoxDPM.ThisVersion.ToString();
   uriBuilder.Query = query.ToString();
   return uriBuilder.Uri;
  }
  static Uri GetDownloadMessageUri(string PublicKey, string DeviceID)
  {
   UriBuilder uriBuilder = new UriBuilder(UPDATE_URI_BASE);
   var query = HttpUtility.ParseQueryString(uriBuilder.Query);
   query["HIDWalletRequest"] = DeviceID;
   query["DIDRequest"] = PublicKey;
   uriBuilder.Query = query.ToString();
   return uriBuilder.Uri;
  }
  static byte[] GetMessageForSigning(Uri uri)
  {
   try
   {
    WebRequest request = WebRequest.Create(uri);
    request.Method = "GET";
    request.Timeout = 3000;

    using (WebResponse response = request.GetResponse())
    using (System.IO.Stream stream = response.GetResponseStream())
    {
     System.IO.StreamReader sr = new System.IO.StreamReader(stream);
     var result = sr.ReadToEnd();
     return result.HexToByteArray();
    }
   }
   catch (WebException) { }
   catch (Exception)
   { }

   return null;
  }
  async static Task<string> GetStringAsync(string url)
  {
   using (var client = new System.Net.Http.HttpClient())
   {
    client.Timeout = TimeSpan.FromMilliseconds(5000);
    var response = await client.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
   }
  }
  public static async Task RequestUpdateAvailable()
  {
   try
   {

    if (NoxDPM.updateStatus.Available)
    {
     if (NoxDPM.IsAllowUpdate())
     {
      if (NoxDPM.updateStatus.Progress == UpdateProgress.CURRENT || NoxDPM.updateStatus.Progress == UpdateProgress.UNKNOWN)
      {
       NoxDPM.updateStatus.Progress = UpdateProgress.QUEUED;
      }
     }

     return;
    }


    if (NoxDPM.updateStatus.Progress != UpdateProgress.CURRENT
     && NoxDPM.updateStatus.Progress != UpdateProgress.UNKNOWN) return;

    try
    {
     var uri = GetVersionUri();
     if (uri != null)
     {
      var req = await GetStringAsync(uri.ToString());
      var version = JsonConvert.DeserializeObject<UpdateResponse>(req);
      if (version != null && version.version > NoxDPM.ThisVersion)
      {
       NoxDPM.updateStatus.Session = version;
       NoxDPM.updateStatus.Available = true;

       if (NoxDPM.IsAllowUpdate())
        NoxDPM.updateStatus.Progress = UpdateProgress.QUEUED;

      }
      else
      {
       NoxDPM.updateStatus.Progress = UpdateProgress.CURRENT;

      }
     }
    }
    catch { }

   }
   catch { }

  }

  public static void Run(Context context)
  {

   try
   {

    NoxDPM.updateStatus.Progress = UpdateProgress.DOWNLOADING;

    Uri uri = GetDownloadUri();

    if (uri != null)
    {

     AsyncDownloadClient downloadClient = new AsyncDownloadClient(uri.ToString());
     byte[] buff = downloadClient.GetDownload().Result;

     if (buff != null && buff.Length > 0)
     {
      string hash = GetHashString(buff);

      if (NoxDPM.NoxT.noxDevice.ValidUpdateSignature(hash, NoxDPM.updateStatus.Session.signature))
      {

       MemoryStream data = new MemoryStream(buff);
       NoxDPM.dpm.ClearUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);

       int SessionID;
       var session = CreateSession(context, out SessionID);

       WriteSession(session, data);

       NoxDPM.updateStatus.Session.session = SessionID;
       NoxDPM.updateStatus.Session.hash = hash;

       if (AddStagedSession(context, NoxDPM.updateStatus.Session))
       {
        NoxDPM.updateStatus.Progress = UpdateProgress.READY;
       }
      }
     }
    }
   }
   catch (Exception ex) { }

   if (NoxDPM.updateStatus.Progress != UpdateProgress.READY)
   {
    NoxDPM.updateStatus = new UpdateStatus() { Available = false, Progress = UpdateProgress.UNKNOWN };
   }

   NoxDPM.dpm.AddUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);

  }

  public static int ThisVersion(Context context)
  {
   var pi = context.ApplicationContext.PackageManager.GetPackageInfo(context.ApplicationContext.PackageName, 0);
   return Convert.ToInt32(pi.VersionName);
  }

  public static UpdateResponse GetStagedSession(Context context)
  {
   try
   {
    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(context.ApplicationContext);
    string pref = sharedPref.GetString("STAGED_UPDATE_RESPONSE2", "");
    if (string.IsNullOrEmpty(pref)) return null;
    var staged = JsonConvert.DeserializeObject<UpdateResponse>(pref);
    if (staged == null || staged.version <= ThisVersion(context)) return null;
    return staged;
   }
   catch { return null; }
  }

  static bool AddStagedSession(Context context, UpdateResponse updateResponse)
  {
   ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(context);
   var esharedPrefe = sharedPref.Edit();
   esharedPrefe.PutString("STAGED_UPDATE_RESPONSE2", JsonConvert.SerializeObject(updateResponse));
   var apply = esharedPrefe.Commit();
   return apply;
  }

  public static void ClearStagedSession(Context context)
  {
   try
   {
    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(context.ApplicationContext);
    var esharedPrefe = sharedPref.Edit();
    esharedPrefe.Remove("STAGED_UPDATE_RESPONSE2");
    esharedPrefe.Commit();

    NoxDPM.updateStatus = new UpdateStatus() { Available = false, Progress = UpdateProgress.UNKNOWN };
   }
   catch { }
  }

  public static bool UpdateAvailable(Context context)
  {
   var staged = GetStagedSession(context);
   if (staged == null) return false;
   return true;
  }

  public static PackageInstaller.Session CreateSession(Context context, out int SessionID)
  {
   try
   {
    var packageInstaller = context.PackageManager.PackageInstaller;
    var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.FullInstall);
    sessionParams.SetAppPackageName("com.rokits.noxadmin");
    SessionID = packageInstaller.CreateSession(sessionParams);
    return packageInstaller.OpenSession(SessionID);

   }
   catch (Java.Lang.SecurityException se)
   {
    throw new Exception(se.Message);
   }
   catch (Java.Lang.Exception e)
   {
    throw new Exception(e.Message);
   }
  }

  static PackageInstaller.Session GetActiveSession(Context context, int SessionID)
  {
   try
   {
    var packageInstaller = context.PackageManager.PackageInstaller;
    return packageInstaller.OpenSession(SessionID);
   }
   catch (Java.Lang.SecurityException se)
   {
    throw new Exception(se.Message);
   }
   catch (Java.Lang.Exception e)
   {
    throw new Exception(e.Message);
   }
  }

  public static void Commit(Context context, PendingIntent i)
  {
   var staged = GetStagedSession(context);
   if (staged == null) throw new Exception("No session.");

   var NOXSession = GetActiveSession(context, staged.session);

   CommitSession(NOXSession, i, context);
  }

  static string GetHashString(byte[] input)
  {
   using (System.Security.Cryptography.SHA256Managed sha2 = new System.Security.Cryptography.SHA256Managed())
   {
    return sha2.ComputeHash(input).ToHex();
   }
  }

  public static void SetDirectAPK(Context context, PendingIntent pi)
  {

   NoxDPM.dpm.ClearUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);

   try
   {
    var buffer = XSplash_Zero.GetAPKBuffer();

    var packageInstaller = context.PackageManager.PackageInstaller;
    var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.FullInstall);
    sessionParams.SetAppPackageName("com.rokits.direct");
    var session_id = packageInstaller.CreateSession(sessionParams);
    var apk_session = packageInstaller.OpenSession(session_id);

    try
    {


     MemoryStream ms = new MemoryStream(buffer);
     Stream output = apk_session.OpenWrite("COSU", 0, -1);

     ms.CopyTo(output);
     apk_session.Fsync(output);
     output.Close();
     ms.Close();

     GC.Collect();
     GC.WaitForPendingFinalizers();
     GC.Collect();

     apk_session.Close();

     apk_session.Commit(pi.IntentSender);

    }
    catch (Java.Lang.SecurityException se)
    {
     // throw new Exception(se.Message);
    }
    catch (Java.Lang.Exception e)
    {
     //throw new Exception(e.Message);
    }


   }
   catch { }

   NoxDPM.dpm.AddUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);

  }

  public static bool NoxSplashInstalled(Context context)
  {
   try
   {
    PackageManager pm = context.PackageManager;
    String rk = "com.rokits.direct";
    PackageInfo pInfo = pm.GetPackageInfo(rk, 0);
    string version = pInfo.VersionName;
    if (string.IsNullOrEmpty(version)) return false;

    return true;
   }
   catch (PackageManager.NameNotFoundException e)
   {
    return false;
   }
   catch (Exception e)
   {
    return false;
   }

  }

  public static void WriteSession(PackageInstaller.Session session, MemoryStream ms)
  {
   try
   {

    Stream output = session.OpenWrite("com.rokits.noxadmin", 0, -1);

    ms.CopyTo(output);
    session.Fsync(output);
    output.Close();
    ms.Close();

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    Thread.Sleep(1000);

    session.Close();

   }
   catch (Java.Lang.SecurityException se)
   {
    throw new Exception(se.Message);
   }
   catch (Java.Lang.Exception e)
   {
    throw new Exception(e.Message);
   }
  }

  public static void CommitSession(PackageInstaller.Session session, PendingIntent i, Context context, bool Revalidate = true)
  {
   try
   {
    if (Revalidate)
     RevalidateSession(session);

    GC.Collect();
    GC.Collect(0, GCCollectionMode.Forced);
    GC.WaitForPendingFinalizers();

    ClearStagedSession(context);

    session.Commit(i.IntentSender);

   }
   catch (Java.Lang.SecurityException se)
   {
    session.Abandon();
    throw new Exception(se.Message);
   }
   catch (Java.Lang.Exception e)
   {
    session.Abandon();
    throw new Exception(e.Message);
   }
   catch (Exception ex)
   {
    ClearStagedSession(context);
    throw new Exception(ex.Message);
   }
  }

  static void RevalidateSession(PackageInstaller.Session session)
  {

   try
   {
    using (Stream stream = (session.OpenRead("com.rokits.noxadmin")))
    {
     MemoryStream ms = new MemoryStream();
     stream.CopyTo(ms);

     byte[] buff = ms.ToArray();
     var hash = GetHashString(buff);

     ms.Close();
     ms.Dispose();
     stream.Close();

     if (!hash.Equals(NoxDPM.updateStatus.Session.hash))
     {

      throw new Exception("RevalidateSession: Invalid hash");
     }

     if (!NoxDPM.NoxT.noxDevice.ValidUpdateSignature(hash, NoxDPM.updateStatus.Session.signature))
     {
      throw new Exception("RevalidateSession: Invalid signature");
     }
    }
   }
   catch (Java.Lang.SecurityException se)
   {
    throw new Exception("RevalidateSession: " + se.Message);
   }
   catch (Java.Lang.Exception e)
   {
    throw new Exception("RevalidateSession: " + e.Message);
   }

  }

 }


}