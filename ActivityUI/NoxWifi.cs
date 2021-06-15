using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using System.Threading;

namespace BitfiWallet
{

 [Activity(Label = "", Name = "com.rokits.noxadmin.NoxWifi", Theme = "@style/FullscreenTheme", NoHistory = true)]
 public class NoxWifi : ListActivity
 {

  public CancellationTokenSource cancellationTokenMA = new CancellationTokenSource();

  NoxWifiReceiver noxWifiReceiver;

  protected override void OnCreate(Bundle bundle)
  {
   base.OnCreate(bundle);

   SetContentView(Resource.Layout.wifi_maina);

   ListAdapter = new WifiListAdapter(this, new List<WifiListItem>());

   var btn = FindViewById<Button>(Resource.Id.wifi_model_button_p);
   btn.Click += delegate { ScanWork(true, true); };

   var title = FindViewById<TextView>(Resource.Id.wifi_model_title);
   title.Text = "SELECT WIFI NETWORK TO CONFIGURE";

  }

  bool IsConnected()
  {
   WifiManager wifi = (WifiManager)GetSystemService(Context.WifiService);
   if (!wifi.IsWifiEnabled) wifi.SetWifiEnabled(true);

   ConnectivityManager cm = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
   NetworkInfo activeNetwork = cm.ActiveNetworkInfo;
   if (activeNetwork != null && activeNetwork.IsConnected) return true;

   return false;
  }

  private bool ShouldWeContinue(string Status)
  {
   switch (Status)
   {
    case "AUTHENTICATING":
    case "SCANNING":
    return true;

    case "OBTAINING_IPADDR":
    bool connected = IsConnected();
    if (connected)
    {
     return false;
    }
    else
    {
     return true;
    }

    default:
    return false;
   }
  }

  private void GetConfigurationStaus()
  {

   UpdateStatus("VALIDATING");

   var work = new RokitWorkDelegate(() =>
   {
    string Status = "SCANNING";

    try
    {
     while (ShouldWeContinue(Status))
     {

      var wifi = (WifiManager)GetSystemService(Context.WifiService);
      var wifiInfo = wifi.ConnectionInfo;
      NetworkInfo.DetailedState state = WifiInfo.GetDetailedStateOf(wifiInfo.SupplicantState);
      Status = state.Name();
      UpdateStatus(Status);

     }
    }
    catch (Exception) { Status = "NOT CONNECTED"; };

    return Status;

   });

   Task.Run(async () =>
   {

    await Task.Delay(2000);

    var resp = await work.ToRokit(30, cancellationTokenMA);

    await Task.Delay(1000);

    if (resp.Error == null)
     RunOnUiThread(delegate ()
        {
        var tv = FindViewById<TextView>(Resource.Id.wifi_model_tv_updatestatus);
        tv.Text = (string)resp.Content;

        ScanWork(true, true);

       });

   });

  }

  private void GetConnectionStatus()
  {
   var work = new RokitWorkDelegate(() =>
   {
    WifiManager wifi = (WifiManager)GetSystemService(Context.WifiService);
    if (!wifi.IsWifiEnabled) wifi.SetWifiEnabled(true);

    ConnectivityManager cm = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
    NetworkInfo activeNetwork = cm.ActiveNetworkInfo;

    string SSID = "";

    if (activeNetwork != null && activeNetwork.IsConnected)
    {

     try
     {
      WifiInfo wifiInfo = wifi.ConnectionInfo;

      if (wifiInfo != null)
      {
       NetworkInfo.DetailedState state = WifiInfo.GetDetailedStateOf(wifiInfo.SupplicantState);
       SSID = wifiInfo.SSID.Replace("\"", "").Trim();
       SSID = SSID.Replace(" ", "").ToUpper() + " CONNECTED";

      }
     }
     catch { }

     return new string[] { "CONNECTED", SSID };
    }


    return new string[] { "NOT CONNECTED", "NO ACTIVE NETWORK" };


   });

   Task.Run(async () =>
   {

    var resp = await work.ToRokit(5, cancellationTokenMA);

    if (resp.Error == null)
     RunOnUiThread(delegate ()
        {

        string[] content = (string[])resp.Content;

        var tv = FindViewById<TextView>(Resource.Id.wifi_model_tv_updatestatus);
        tv.Text = content[0];


        var tv2 = FindViewById<TextView>(Resource.Id.wifi_mania_natwork_name);
        tv2.Text = content[1];

       });
   });

  }

  private void GetNetworks()
  {
   var work = new RokitWorkDelegate(() =>
   {
    var sswifi = (WifiManager)GetSystemService(Context.WifiService);
    var filtered = new List<WifiListItem>();
    bool scan = sswifi.StartScan();
    List<ScanResult> wifiScanList = new List<ScanResult>();

    if (scan)
    {
     wifiScanList = sswifi.ScanResults.ToList();

     foreach (ScanResult scanResult in wifiScanList)
     {
      string Signal = "Marginal";
      int level = scanResult.Level;

      if (level <= 0 && level >= -50) Signal = "Excellent";
      else if (level < -50 && level >= -70) Signal = "Good";
      else if (level < -70 && level >= -80) Signal = "Fair";
      else if (level < -80 && level >= -100) Signal = "Weak";

      filtered.Add(new WifiListItem()
      {
       SSID = scanResult.Ssid,
       Signal = Signal + " [" + level.ToString() + "dBm]",
       Status = "",
       Level = level
      });
     }
    }

    return filtered;

   });

   Task.Run(async () =>
   {
    var resp = await work.ToRokit(5, cancellationTokenMA);

    if (resp.Error == null)
     RunOnUiThread(delegate ()
      {
      List<WifiListItem> content = (List<WifiListItem>)resp.Content;
      ListAdapter = new WifiListAdapter(this, content);
     });

   });

  }

  private void ScanWork(bool Start, bool SkipExtra = false)
  {

   if (noxWifiReceiver != null)
   {
    cancellationTokenMA.Cancel();
    UnregisterReceiver(noxWifiReceiver);
    noxWifiReceiver = null;
   }

   if (Start)
   {
    cancellationTokenMA = new CancellationTokenSource();
    noxWifiReceiver = new NoxWifiReceiver();

    IntentFilter intentFilter = new IntentFilter();
    intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
    RegisterReceiver(noxWifiReceiver, intentFilter);

    bool ConfigChange = Intent.GetBooleanExtra("newconfig", false);

    if (SkipExtra) ConfigChange = false;

    if (!ConfigChange)
    {
     GetConnectionStatus();
     GetNetworks();
    }
    else
    {
     GetConfigurationStaus();
    }

   }
  }

  private void UpdateStatus(string status)
  {
   Action workerThread = delegate ()
   {
    try
    {
     var tv = FindViewById<TextView>(Resource.Id.wifi_model_tv_updatestatus);
     tv.Text = status;
    }
    catch { }
   };

   RunOnUiThread(workerThread);
  }

  protected override void OnPause()
  {

   ScanWork(false);

   base.OnPause();
  }

  protected override void OnResume()
  {

   base.OnResume();


   ScanWork(true);

  }

  class WifiListItem
  {
   public string SSID { get; set; }
   public string Signal { get; set; }
   public string Status { get; set; }
   public int Level { get; set; }
  }

  class WifiListAdapter : BaseAdapter
  {
   Context context;
   List<WifiListItem> filtered;
   public WifiListAdapter(Context c, List<WifiListItem> _filtered)
   {
    context = c;
    filtered = _filtered;
   }
   public override Java.Lang.Object GetItem(int position)
   {
    return null;
   }
   public override long GetItemId(int position)
   {
    return position + 1;
   }
   public override int Count
   {
    get
    {
     return filtered.Count;
    }
   }

   public override View GetView(int position, View convertView, ViewGroup parent)
   {
    try
    {
     convertView = LayoutInflater.From(context).Inflate(Resource.Layout.status_list, parent, false);
     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv1);
     TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv2);
     TextView tv3 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv3);
     tv1.Typeface = TFView.typeface;
     tv2.Typeface = TFView.typeface;
     tv3.Typeface = TFView.typeface;
     tv1.Text = filtered[position].SSID;
     tv2.Text = filtered[position].Signal;
     tv3.Text = filtered[position].Status;

     convertView.Click += delegate
     {
      Task.Run(() =>
           {
        connectToWifi(filtered[position].SSID);

       });



     };

    }
    catch (Exception)
    {

    }

    return convertView;
   }

   private void connectToWifi(string wifiSSID)
   {
    string SSID = "";
    bool connected = false;
    bool hasEverConnected = false;
    int NetworkId = 0;
    try
    {

     Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)context.GetSystemService(Context.WifiService);

     if (wifi != null && wifi.IsWifiEnabled)
     {
      WifiInfo wifiInfo = wifi.ConnectionInfo;

      if (wifiInfo != null)
      {
       NetworkInfo.DetailedState state = WifiInfo.GetDetailedStateOf(wifiInfo.SupplicantState);

       if (state == NetworkInfo.DetailedState.Connected || state == NetworkInfo.DetailedState.ObtainingIpaddr)
       {
        SSID = wifiInfo.SSID.Replace("\"", "").Trim();
        SSID = SSID.Replace(" ", "");
        if (SSID == wifiSSID.Trim().Replace(" ", ""))
        {
         connected = true;
        }
       }

      }


      IList<WifiConfiguration> wifiScanList = wifi.ConfiguredNetworks;
      for (int i = 0; i < wifiScanList.Count; i++)
      {
       if (((wifiScanList[i]).ToString()).Contains("hasEverConnected: true"))
       {
        String cw = wifiScanList[i].Ssid.Replace("\"", "").Trim();
        cw = cw.Replace(" ", "");
        if (cw == wifiSSID.Trim().Replace(" ", ""))
        {
         NetworkId = wifiScanList[i].NetworkId;
         hasEverConnected = true;
         break;
        }
       }
      }
     }

    }
    catch (Exception e)
    {

    }

    var nxact = new Intent(Application.Context, typeof(ConnectActivity));
    nxact.PutExtra("SSID", wifiSSID);
    nxact.PutExtra("connected", connected);
    nxact.PutExtra("hasEverConnected", hasEverConnected);
    nxact.PutExtra("NetworkId", NetworkId);

    context.StartActivity(nxact);

   }

   Activity GetActivity(Context context)
   {
    if (context == null)
    {
     return null;
    }
    else if (context is ContextWrapper)
    {
     if (context is Activity)
     {
      return (Activity)context;
     }
     else
     {
      return GetActivity(((ContextWrapper)context).BaseContext);
     }
    }

    return null;
   }


  }


 }
}