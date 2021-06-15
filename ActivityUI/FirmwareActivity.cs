using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Runtime;
using Java.Util;
using System.Net;
using System.Net.Sockets;

namespace BitfiWallet
{


 [Activity(Name = "com.rokits.noxadmin.FirmwareActivity", Label = "", Theme = "@style/FullscreenTheme")]
 public class FirmwareActivity : ListActivity
 {

  protected override void OnCreate(Bundle bundle)
  {

   base.OnCreate(bundle);
   SetContentView(Resource.Layout.status_model_list);


   var btn = FindViewById<Button>(Resource.Id.status_model_list_button_p);

   bool Update = Intent.GetBooleanExtra("update", false);

   if (Update)
   {
    btn.Text = "YES, REVIEW";

    var tv = FindViewById<TextView>(Resource.Id.status_model_list_title2);
    tv.Text = "PROCEED WITH THIS UPDATE?";

    var ltv2 = FindViewById<TextView>(Resource.Id.status_model_list_tv2);
    ltv2.Text = "UPDATE PACKAGE";

    btn.Click += delegate
    {
     ContinueUpdate();

    };
   }
   else
   {

    btn.Click += delegate
    {

     Finish();

    };
   }


   Load(Update);

  }

  private void ContinueUpdate()
  {
   try
   {

    var nxact = new Intent(this, typeof(UpdatePrompt));
    nxact.PutExtra("pmtask", "prompt");
    nxact.AddFlags(ActivityFlags.NoAnimation);
    StartActivity(nxact);

    Finish();

   }
   catch { }
  }

  protected override void OnResume()
  {
   base.OnResume();

  }

  public override void OnBackPressed()
  {
   base.OnBackPressed();

   Finish();
  }

  public void Load(bool Update)
  {
   List<SatusListItem> items = new List<SatusListItem>();

   if (Update)
   {
    try
    {
     items.Add(new SatusListItem() { StatusTitle = "Update Version", StatusValue1 = "DMA-4 V" + NoxDPM.updateStatus.Session.version.ToString() });
     items.Add(new SatusListItem() { StatusTitle = "SHA256 Hash", StatusValue1 = NoxDPM.updateStatus.Session.hash.ToString() });
     items.Add(new SatusListItem() { StatusTitle = "Signature", StatusValue1 = NoxDPM.updateStatus.Session.signature.ToString() });

    }
    catch { }

    ListAdapter = new StatusListAdapter(this, items);

    return;
   }

   try
   {
    var inDate = FromJTime(GetUpdateDate());
    var tsuptime = TimeSpan.FromMilliseconds(GetUpTime());
    var upTime = tsuptime.Hours + ":" + tsuptime.Minutes + ":" + tsuptime.Seconds;
    tsuptime = TimeSpan.FromMilliseconds(GetRealTime());
    var realTime = tsuptime.Hours + ":" + tsuptime.Minutes + ":" + tsuptime.Seconds;

    ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
    ActivityManager activityManager = (ActivityManager)this.GetSystemService(Context.ActivityService);
    activityManager.GetMemoryInfo(mi);
    long availableMegs = mi.AvailMem / 1048576L;
    //decimal megsPercent = (availableMegs / 1000) * 100;

    items.Add(new SatusListItem() { StatusTitle = "TEE PubKey", StatusValue1 = NoxDPM.NoxT.noxDevice.DevicePubHash() });

    items.Add(new SatusListItem() { StatusTitle = "Installed Version", StatusValue1 = "Running DMA-4 V" + NoxDPM.ThisVersion.ToString() });
    items.Add(new SatusListItem() { StatusTitle = "Available Memory", StatusValue1 = availableMegs.ToString() + "MB OF 1GB" });

    items.Add(new SatusListItem() { StatusTitle = "Network Time", StatusValue1 = GetNetTime().ToString() + " EST" });
    items.Add(new SatusListItem() { StatusTitle = "Host Address", StatusValue1 = GetHost() });

    items.Add(new SatusListItem() { StatusTitle = "Uptime Elapsed", StatusValue1 = realTime });
    items.Add(new SatusListItem() { StatusTitle = "Uptime Awake", StatusValue1 = upTime });


    //items.Add(new SatusListItem() { StatusTitle = "Hardware ID", StatusValue1 = NoxDPM.NoxT.noxDevice.GetHardwareID() });

   }
   catch { }

   ListAdapter = new StatusListAdapter(this, items);

  }

  string GetHost()
  {
   try
   {
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
     if (ip.AddressFamily == AddressFamily.InterNetwork)
     {
      if (ip.ToString() == "127.0.0.1")
       return "Network Unavailable";

      return ip.ToString();
     }
    }
   }
   catch { }

   return "NA";
  }

  long GetInstallDate()
  {
   var pi = PackageManager.GetPackageInfo("com.rokits.noxadmin", 0);
   return pi.FirstInstallTime;
  }

  long GetUpdateDate()
  {
   var pi = PackageManager.GetPackageInfo("com.rokits.noxadmin", 0);
   return pi.LastUpdateTime;
  }

  long GetUpTime()
  {
   return SystemClock.UptimeMillis();
  }

  long GetRealTime()
  {
   return SystemClock.ElapsedRealtime();
  }

  DateTime GetNetTime()
  {
   return DateTime.Now.ToLocalTime();
  }

  DateTime FromJTime(long javaMS)
  {
   DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
   DateTime dt = UTCBaseTime.Add(new TimeSpan(javaMS *
   TimeSpan.TicksPerMillisecond));
   return dt;
  }

  class SatusListItem
  {
   public string StatusTitle { get; set; }
   public string StatusValue1 { get; set; }
  }

  class StatusListAdapter : BaseAdapter
  {
   Context context;
   List<SatusListItem> _satusListItems;

   public StatusListAdapter(Context c, List<SatusListItem> satusListItems)
   {
    context = c;
    _satusListItems = satusListItems;

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
     return _satusListItems.Count;
    }
   }
   public override View GetView(int position, View convertView, ViewGroup parent)
   {
    try
    {
     convertView = LayoutInflater.From(context).Inflate(Resource.Layout.status_list, parent, false);
     TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv1);
     TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv2);
     tv1.Typeface = TFView.typeface;
     tv2.Typeface = TFView.typeface;
     tv1.Text = _satusListItems[position].StatusTitle;
     tv2.Text = _satusListItems[position].StatusValue1;

    }
    catch (Exception)
    {

    }
    return convertView;
   }


  }

 }

}
