using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using NoxService.NWS;

namespace BitfiWallet
{

 public class NoxNotification
 {

  private string _BTCRateCache;
  private int _ActivityCount;
  private bool _UpdateShown;

  private HWS _SMSWebService;

  static DateTime _VersionRequestTime;

  public static NoxNotification Current { get; private set; }

  public NoxNotification()
  {
   if (Current != null)
    return;

   Current = this;
   _VersionRequestTime = new DateTime();

  }

  public void StartActivity(MainActivityB context, NoxMSGCompletedEventHandler StatusChanged, out CancellationTokenSource cancellation)
  {
   _SMSWebService = new HWS();
   CancellationTokenSource cancellationToken = new CancellationTokenSource();
   cancellation = cancellationToken;

   var work = new RokitWorkDelegate(() =>
   {
    return DoWork(context, StatusChanged, cancellationToken).Result;

   });

   Task.Run(async () =>
   {
    if (_ActivityCount == 0)
     await Task.Delay(1500);


    _ActivityCount++;

    var thread = await work.ToRokit(50, cancellationToken);

    if (thread.Error == null)
    {
     //var completed =(bool)thread.Content;
    }

   });
  }

  async Task<bool> DoWork(MainActivityB context, NoxMSGCompletedEventHandler StatusChanged, CancellationTokenSource cancellationToken)
  {
   bool completed = false;

   try
   {
    completed = await Work(context, cancellationToken.Token, StatusChanged).SetWaitCancellation(cancellationToken.Token);
   }
   catch (OperationCanceledException)
   {

    if (context.HasWindowFocus)
     NoxDPM.dpm.LockNow();
   }
   catch (Exception)
   {

   }
   finally
   {
    try
    {
     cancellationToken.Dispose();

    }
    catch { }

    cancellationToken = null;
    _SMSWebService = null;

   }

   return completed;
  }

  async Task<bool> Work(MainActivityB context, CancellationToken token, NoxMSGCompletedEventHandler StatusChanged)
  {


   if (IsNetworkConnected(context))
   {
    bool runsms = true;

    var rate = RecurringCaptiveRequest(context, token).Result;

    while (!context.HasWindowFocus && !token.IsCancellationRequested)
    {
     await Task.Delay(50);
    }

    if (token.IsCancellationRequested)
     return false;


    if (string.IsNullOrEmpty(rate))
    {
     runsms = false;
     rate = "WAITING FOR NETWORK";
    }

    if (NoxDPM.updateStatus.Available)
    {
     rate = "UPDATE AVAILABLE";
    }

    WorkResponse rateResponse = new WorkResponse();
    rateResponse.Action = ResponseActionType.SHOW_RATE;
    rateResponse.ValidationRate = rate;
    LocallyHandledWorkResponse(rateResponse, StatusChanged, context);

    if (runsms)
    {
     var info = await SMSRequest(context, token, StatusChanged);

     if (token.IsCancellationRequested)
      return false;

     if (info != null)
     {
      WorkResponse smsResponse = new WorkResponse();
      smsResponse.UserInfo = info;
      smsResponse.Action = ResponseActionType.REQUEST;
      LocallyHandledWorkResponse(smsResponse, StatusChanged, context);
     }
    }

   }
   else
   {
    while (!context.HasWindowFocus && !token.IsCancellationRequested)
    {
     await Task.Delay(50);
    }

    WorkResponse workResponse = new WorkResponse();
    workResponse.Action = ResponseActionType.NOWIFI;
    LocallyHandledWorkResponse(workResponse, StatusChanged, context);
   }

   return true;
  }

  private bool IsNetworkConnected(MainActivityB context)
  {
   ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
   NetworkInfo activeNetwork = cm.ActiveNetworkInfo;
   if (activeNetwork != null && activeNetwork.IsConnected) return true;

   return false;
  }

  async Task<FormUserInfo> SMSRequest(MainActivityB context, CancellationToken token, NoxMSGCompletedEventHandler StatusChanged)
  {

   if (NoxDPM.updateStatus.Available && NoxDPM.updateStatus.Progress != UpdateProgress.QUEUED)
   {
    await UpdateSession.RequestUpdateAvailable();
   }
   else
   {
    if (!NoxDPM.updateStatus.Available)
    {
     if (_VersionRequestTime < DateTime.Now && _ActivityCount > 1)
     {
      try
      {
       new Thread(() =>
       {
        try
        {
         UpdateSession.RequestUpdateAvailable().Wait();
        }
        catch { }

       }).Start();

       _VersionRequestTime = DateTime.Now.AddMinutes(10);
      }
      catch { }
     }
    }

   }

   try
   {
    while (!token.IsCancellationRequested && context.HasWindowFocus)
    {
     var hWSResponse = await _SMSWebService.GetCurrentSMSToken();

     if (hWSResponse.Notfify)
     {
      return hWSResponse.formUserInfo;
     }
     else
     {
      await Task.Delay(2000);
     }
    }
   }
   catch { }

   return null;
  }

  async Task<string> RecurringCaptiveRequest(MainActivityB context, CancellationToken token)
  {

   for (int i = 0; i < 3; i++)
   {
    string rate = await CaptiveRequest();
    if (!string.IsNullOrEmpty(rate)) return GetRateString(rate);

    if (string.IsNullOrEmpty(_BTCRateCache) && !token.IsCancellationRequested && context.HasWindowFocus)
    {
     await Task.Delay(500);
    }
    else
    {
     return GetRateString(_BTCRateCache);
    }
   }

   return null;
  }

  string GetRateString(string rate)
  {
   return rate + " BTC/USD";
  }

  async Task<string> CaptiveRequest()
  {
   try
   {
    using (var client = new System.Net.Http.HttpClient())
    {
     client.Timeout = TimeSpan.FromMilliseconds(3000);
     var response = await client.GetAsync("http://bitfi.dev/NoxMessages/DMA3Device.aspx?DomainRequest=1").ConfigureAwait(false);
     var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

     if (!string.IsNullOrEmpty(result))
     {
      NoxCaptive noxCaptive = Newtonsoft.Json.JsonConvert.DeserializeObject<NoxCaptive>(result);
      _BTCRateCache = noxCaptive.bTCRate.USD;
      return noxCaptive.bTCRate.USD;
     }
    }
   }
   catch (HttpRequestException) { }
   catch (Exception) { }

   return null;

  }

  void LocallyHandledWorkResponse(WorkResponse _WorkResponse, NoxMSGCompletedEventHandler StatusChanged, MainActivityB context)
  {
   try
   {
    if (StatusChanged == null)
     return;

    StatusChanged.Invoke(new NoxMSGCompletedEventArgs(_WorkResponse));
   }
   catch { }
  }

 }

 public delegate void NoxMSGCompletedEventHandler(NoxMSGCompletedEventArgs e);

 public class NoxMSGCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
 {
  public NoxMSGCompletedEventArgs(WorkResponse result) : base(null, false, null) { Result = result; }

  public WorkResponse Result { get; set; }
 }

}
