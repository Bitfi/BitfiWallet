using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using Android.Net.Wifi;

namespace BitfiWallet
{
 [Activity(Label = "", Name = "com.rokits.noxadmin.ConnectActivity", Theme = "@style/FullscreenTheme", NoHistory = true)]
 public class ConnectActivity : Activity
 {

  public string secr;
  public string kbstr;
  public bool capson;
  AlertDialog bwalert;

  protected override void OnStop()
  {

   base.OnStop();
  }
  public void DisplayMSG(string msg, bool Close = false)
  {
   AlertDialog.Builder builder = new AlertDialog.Builder(this).SetTitle("").SetMessage(msg).SetCancelable(false).SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
   AlertDialog alert = builder.Create();
   alert.Show();
   var okBtn = alert.GetButton((int)DialogButtonType.Positive);
   okBtn.Click += (asender, args) =>
   {
    alert.Dismiss();
    if (Close)
    {
     Finish();

    }

   };
  }


  protected override void OnCreate(Bundle bundle)
  {
   base.OnCreate(bundle);
   SetContentView(Resource.Layout.nconnect);

   Bundle extras = Intent?.Extras;
   if (extras == null)
   {
    Finish();
    return;
   }

   AlertDialog.Builder builder = new AlertDialog.Builder(this, Resource.Style.MyAlertDialogTheme).SetTitle("< />  working, please wait...").SetMessage("").SetCancelable(false);
   bwalert = builder.Create();

   string SSID = extras.GetString("SSID", "");
   bool connected = extras.GetBoolean("connected", false);
   bool hasEverConnected = extras.GetBoolean("hasEverConnected", false);
   int NetworkId = extras.GetInt("NetworkId", 0);

   if (string.IsNullOrEmpty(SSID))
   {
    Finish();
    return;
   }

   Button ok = (Button)FindViewById(Resource.Id.okButton);
   Button forgetButton = (Button)FindViewById(Resource.Id.forgetButton);
   TextView pass = (TextView)FindViewById(Resource.Id.textviewPassword);

   if (!hasEverConnected)
   {

    forgetButton.Text = "RETURN";
   }

   if (connected)
   {
    ok.Text = "(CONNECTED)";
    ok.SetBackgroundResource(Resource.Color.black);
    ok.Enabled = false;
   }

   if (hasEverConnected || connected)
   {
    pass.Text = "***********";
    pass.Enabled = false;

   }
   else
   {
    LoadBClicks();

   }

   ok.Click += delegate
   {
    if (hasEverConnected)
    {
     try
     {
      using (Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)ApplicationContext.GetSystemService(Context.WifiService))
      {
       if (wifi != null && wifi.IsWifiEnabled)
       {
        wifi.Disconnect();
        wifi.EnableNetwork(NetworkId, true);
        wifi.Reconnect();
       }
      }
     }
     catch { }
    }
    else
    {
     finallyConnect(pass.Text, SSID);
    }

    Intent nxact = new Intent(this, typeof(NoxWifi));
    nxact.PutExtra("newconfig", true);
    nxact.AddFlags(ActivityFlags.NoAnimation);
    StartActivity(nxact);

       //Finish();
      };

   forgetButton.Click += delegate
   {

    try
    {
     using (Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)ApplicationContext.GetSystemService(Context.WifiService))
     {
      if (wifi != null && wifi.IsWifiEnabled)
      {
       if (hasEverConnected)
       {
        wifi.RemoveNetwork(NetworkId);
       }
      }
     }
    }
    catch { }

    Intent nxact = new Intent(this, typeof(NoxWifi));
    nxact.PutExtra("newconfig", true);
    nxact.AddFlags(ActivityFlags.NoAnimation);
    StartActivity(nxact);

       //Finish();
      };

  }
  private void finallyConnect(string networkPass, string networkSSID)
  {

   try
   {
    using (Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)ApplicationContext.GetSystemService(Context.WifiService))
    {
     if (wifi != null && wifi.IsWifiEnabled)
     {
      using (WifiConfiguration wifiConfig = new WifiConfiguration())
      {
       wifiConfig.Ssid = "\"" + networkSSID + "\""; //string.Format("\"%s\"", networkSSID);
       wifiConfig.PreSharedKey = "\"" + networkPass + "\""; // string.Format("\"%s\"", networkPass);
       int netId = wifi.AddNetwork(wifiConfig);
       wifi.Disconnect();
       wifi.EnableNetwork(netId, true);
       wifi.Reconnect();
       using (WifiConfiguration conf = new WifiConfiguration())
       {
        conf.Ssid = "\"" + networkSSID + "\"";
        conf.PreSharedKey = "\"" + networkPass + "\"";
        wifi.AddNetwork(conf);
       }
      }
     }
    }
   }
   catch
   {


   }
  }

  private Button mButton1;
  private Button mButton2;
  private Button mButton3;
  private Button mButton4;
  private Button mButton5;
  private Button mButton6;
  private Button mButton7;
  private Button mButton8;
  private Button mButton9;
  private Button mButton0;
  private Button mButtonA;
  private Button mButtonB;
  private Button mButtonC;
  private Button mButtonD;
  private Button mButtonE;
  private Button mButtonF;
  private Button mButtonG;
  private Button mButtonH;
  private Button mButtonI;
  private Button mButtonJ;
  private Button mButtonK;
  private Button mButtonL;
  private Button mButtonM;
  private Button mButtonN;
  private Button mButtonO;
  private Button mButtonP;
  private Button mButtonQ;
  private Button mButtonR;
  private Button mButtonS;
  private Button mButtonT;
  private Button mButtonU;
  private Button mButtonV;
  private Button mButtonW;
  private Button mButtonX;
  private Button mButtonY;
  private Button mButtonZ;
  private Button mButtonDelete;
  private Button mButtonEnter;
  private Button mButtonSpace;
  private Button mButtonShift;
  private Button mButtonCaps;

  private Button mButtonSemiC;
  private Button mButtonQuote;
  private Button mButtonUnderS;
  private Button mButtonAT;

  List<Button> buttonLIst;


  private void LoadBClicks()
  {

   buttonLIst = new List<Button>();

   mButton1 = (Button)FindViewById(Resource.Id.wbutton_1);
   mButton2 = (Button)FindViewById(Resource.Id.wbutton_2);
   mButton3 = (Button)FindViewById(Resource.Id.wbutton_3);
   mButton4 = (Button)FindViewById(Resource.Id.wbutton_4);
   mButton5 = (Button)FindViewById(Resource.Id.wbutton_5);
   mButton6 = (Button)FindViewById(Resource.Id.wbutton_6);
   mButton7 = (Button)FindViewById(Resource.Id.wbutton_7);
   mButton8 = (Button)FindViewById(Resource.Id.wbutton_8);
   mButton9 = (Button)FindViewById(Resource.Id.wbutton_9);
   mButton0 = (Button)FindViewById(Resource.Id.wbutton_0);
   mButtonA = (Button)FindViewById(Resource.Id.wbutton_A);
   mButtonB = (Button)FindViewById(Resource.Id.wbutton_B);
   mButtonC = (Button)FindViewById(Resource.Id.wbutton_C);
   mButtonD = (Button)FindViewById(Resource.Id.wbutton_D);
   mButtonE = (Button)FindViewById(Resource.Id.wbutton_E);
   mButtonF = (Button)FindViewById(Resource.Id.wbutton_F);
   mButtonG = (Button)FindViewById(Resource.Id.wbutton_G);
   mButtonH = (Button)FindViewById(Resource.Id.wbutton_H);
   mButtonI = (Button)FindViewById(Resource.Id.wbutton_I);
   mButtonJ = (Button)FindViewById(Resource.Id.wbutton_J);
   mButtonK = (Button)FindViewById(Resource.Id.wbutton_K);
   mButtonL = (Button)FindViewById(Resource.Id.wbutton_L);
   mButtonM = (Button)FindViewById(Resource.Id.wbutton_M);
   mButtonN = (Button)FindViewById(Resource.Id.wbutton_N);
   mButtonO = (Button)FindViewById(Resource.Id.wbutton_O);
   mButtonP = (Button)FindViewById(Resource.Id.wbutton_P);
   mButtonQ = (Button)FindViewById(Resource.Id.wbutton_Q);
   mButtonR = (Button)FindViewById(Resource.Id.wbutton_R);
   mButtonS = (Button)FindViewById(Resource.Id.wbutton_S);
   mButtonT = (Button)FindViewById(Resource.Id.wbutton_T);
   mButtonU = (Button)FindViewById(Resource.Id.wbutton_U);
   mButtonV = (Button)FindViewById(Resource.Id.wbutton_V);
   mButtonW = (Button)FindViewById(Resource.Id.wbutton_W);
   mButtonX = (Button)FindViewById(Resource.Id.wbutton_X);
   mButtonY = (Button)FindViewById(Resource.Id.wbutton_Y);
   mButtonZ = (Button)FindViewById(Resource.Id.wbutton_Z);

   mButtonSemiC = (Button)FindViewById(Resource.Id.wbutton_SemiC);
   mButtonQuote = (Button)FindViewById(Resource.Id.wbutton_Quote);
   mButtonUnderS = (Button)FindViewById(Resource.Id.wbutton_UnderS);
   mButtonAT = (Button)FindViewById(Resource.Id.wbutton_AT);




   buttonLIst.Add(mButton0);
   buttonLIst.Add(mButton1);
   buttonLIst.Add(mButton2);
   buttonLIst.Add(mButton3);
   buttonLIst.Add(mButton4);
   buttonLIst.Add(mButton5);
   buttonLIst.Add(mButton6);
   buttonLIst.Add(mButton7);
   buttonLIst.Add(mButton8);
   buttonLIst.Add(mButton9);
   buttonLIst.Add(mButtonA);
   buttonLIst.Add(mButtonB);
   buttonLIst.Add(mButtonC);
   buttonLIst.Add(mButtonD);
   buttonLIst.Add(mButtonE);
   buttonLIst.Add(mButtonF);
   buttonLIst.Add(mButtonG);
   buttonLIst.Add(mButtonH);
   buttonLIst.Add(mButtonI);
   buttonLIst.Add(mButtonJ);
   buttonLIst.Add(mButtonK);
   buttonLIst.Add(mButtonL);
   buttonLIst.Add(mButtonM);
   buttonLIst.Add(mButtonN);
   buttonLIst.Add(mButtonO);
   buttonLIst.Add(mButtonP);
   buttonLIst.Add(mButtonQ);
   buttonLIst.Add(mButtonR);
   buttonLIst.Add(mButtonS);
   buttonLIst.Add(mButtonT);
   buttonLIst.Add(mButtonU);
   buttonLIst.Add(mButtonV);
   buttonLIst.Add(mButtonW);
   buttonLIst.Add(mButtonX);
   buttonLIst.Add(mButtonY);
   buttonLIst.Add(mButtonZ);

   buttonLIst.Add(mButtonUnderS);
   buttonLIst.Add(mButtonQuote);
   buttonLIst.Add(mButtonSemiC);
   buttonLIst.Add(mButtonAT);

   TextView tbsecr = FindViewById<TextView>(Resource.Id.textviewPassword);

   tbsecr.SetRawInputType(Android.Text.InputTypes.TextFlagNoSuggestions);

   kbstr = tbsecr.Text;
   tbsecr.SetBackgroundResource(Resource.Color.colorPrimary);

   Button forgetButton = (Button)FindViewById(Resource.Id.forgetButton);

   mButtonSpace = (Button)FindViewById(Resource.Id.wbutton_SPACE);
   mButtonSpace.Click += delegate
   {
    if (kbstr.Length > 0)
    {
     kbstr = kbstr + " ";


     tbsecr.Text = kbstr;

    }

   };

   mButtonDelete = (Button)FindViewById(Resource.Id.wbutton_delete);
   mButtonDelete.Click += delegate
   {
    if (kbstr.Length > 0)
    {
     kbstr = kbstr.Substring(0, kbstr.Length - 1);


     tbsecr.Text = kbstr;

    }
   };



   mButtonCaps = (Button)FindViewById(Resource.Id.wbutton_CAPS);


   mButtonCaps.Visibility = ViewStates.Visible;
   forgetButton.Visibility = ViewStates.Visible;

   mButtonUnderS.Visibility = ViewStates.Gone;
   mButtonQuote.Visibility = ViewStates.Gone;
   mButtonSemiC.Visibility = ViewStates.Gone;
   mButtonAT.Visibility = ViewStates.Gone;

   mButtonCaps.Click += delegate
   {

    if (mButtonCaps.Text == "CAPS")
    {
     mButtonCaps.Text = "SCAP";
     capson = true;
    }
    else
    {
     mButtonCaps.Text = "CAPS";
     capson = false;
    }

    foreach (Button b in buttonLIst)
    {


     if (capson)
     {
      b.Text = b.Text.ToUpper();
     }
     else
     {
      b.Text = b.Text.ToLower();
     }
    }

   };


   foreach (Button b in buttonLIst)
   {


    if (capson)
    {
     b.Text = b.Text.ToUpper();
    }
    else
    {
     b.Text = b.Text.ToLower();
    }

    b.Click += delegate
    {
     string val = b.Text;

     if (capson)
     {
      val = val.ToUpper();
     }
     else
     {
      val = val.ToLower();
     }

     kbstr = kbstr + val;


     tbsecr.Text = kbstr;

    };
   }


   mButtonShift = (Button)FindViewById(Resource.Id.wbutton_SHIFT);
   mButtonShift.Click += delegate
   {

    if (mButtonShift.Text == "^")
    {
     mButtonShift.Text = "<";

     mButtonA.Text = "~";
     mButtonB.Text = "-";
     mButtonC.Text = "+";
     mButtonD.Text = "=";
     mButtonE.Text = "{";
     mButtonF.Text = "}";
     mButtonG.Text = "[";
     mButtonH.Text = "]";
     mButtonI.Text = "/";
     mButtonJ.Text = ">";
     mButtonK.Text = "<";
     mButtonL.Text = "?";
     mButtonM.Text = ".";
     mButtonN.Text = ",";
     mButtonO.Text = ")";
     mButtonP.Text = "(";
     mButtonQ.Text = "!";
     mButtonR.Text = "\\";
     mButtonS.Text = "#";
     mButtonT.Text = "$";
     mButtonU.Text = "%";
     mButtonV.Text = "^";
     mButtonW.Text = "&";
     mButtonX.Text = "*";
     mButtonY.Text = ";";
     mButtonZ.Text = "\"";

        //  mButtonCaps.Visibility = ViewStates.Gone;
        mButtonUnderS.Visibility = ViewStates.Visible;
     mButtonQuote.Visibility = ViewStates.Visible;
     mButtonSemiC.Visibility = ViewStates.Visible;

        //  forgetButton.Visibility = ViewStates.Gone;
        mButtonAT.Visibility = ViewStates.Visible;
     mButtonSpace.Visibility = ViewStates.Gone;

    }
    else
    {
     mButtonShift.Text = "^";

     mButtonA.Text = "A";
     mButtonB.Text = "B";
     mButtonC.Text = "C";
     mButtonD.Text = "D";
     mButtonE.Text = "E";
     mButtonF.Text = "F";
     mButtonG.Text = "G";
     mButtonH.Text = "H";
     mButtonI.Text = "I";
     mButtonJ.Text = "J";
     mButtonK.Text = "K";
     mButtonL.Text = "L";
     mButtonM.Text = "M";
     mButtonN.Text = "N";
     mButtonO.Text = "O";
     mButtonP.Text = "P";
     mButtonQ.Text = "Q";
     mButtonR.Text = "R";
     mButtonS.Text = "S";
     mButtonT.Text = "T";
     mButtonU.Text = "U";
     mButtonV.Text = "V";
     mButtonW.Text = "W";
     mButtonX.Text = "X";
     mButtonY.Text = "Y";
     mButtonZ.Text = "Z";


     foreach (Button b in buttonLIst)
     {


      if (capson)
      {
       b.Text = b.Text.ToUpper();
      }
      else
      {
       b.Text = b.Text.ToLower();
      }
     }

     mButtonUnderS.Visibility = ViewStates.Gone;
     mButtonQuote.Visibility = ViewStates.Gone;
     mButtonSemiC.Visibility = ViewStates.Gone;
     mButtonAT.Visibility = ViewStates.Gone;
     mButtonSpace.Visibility = ViewStates.Visible;
    }

   };

  }

 }
}