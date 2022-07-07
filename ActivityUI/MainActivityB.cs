using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.ComponentModel;
using System.Threading;
using Android.Graphics;
using Android.Views;
using System.Threading.Tasks;
using NoxService.NWS;
using Android.Runtime;
using Android.Content.Res;
using Android.Preferences;
using Android.Content.PM;
using Android;
using Android.App.Admin;
using static Android.OS.PowerManager;

namespace BitfiWallet
{

	[Activity(Name = "com.rokits.noxadmin.MainActivityB", Label = "", Theme = "@style/FullscreenTheme", HardwareAccelerated = true, LaunchMode = LaunchMode.SingleTop)]
	public class MainActivityB : Activity
	{

		public CancellationTokenSource cancellationToken = null;

		protected override void OnCreate(Bundle savedInstanceState)
		{

			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.walletmain);

			try
			{

				LoadVTypes();
				LoadClickViews();

			}
			catch { }

		}

		private void Current_OnBatteryChanged(object sender, EventArgs e)
		{
			RunOnUiThread(delegate ()
			{
				try
				{
					LoadBatStatus((BatStatus)sender);
				}
				catch { }
			});

		}

		protected override void OnStart()
		{


			base.OnStart();

			try
			{
				DeviceManager.NoxDevice.Current.OnBatteryChanged += Current_OnBatteryChanged;

				NoxNotification.Current.StartActivity(this, NoxEvent, out cancellationToken);
				cancellationToken.CancelAfter(TimeSpan.FromSeconds(45));
			}
			catch { }
		}

		protected override void OnStop()
		{


			base.OnStop();

			try
			{
				DeviceManager.NoxDevice.Current.OnBatteryChanged -= Current_OnBatteryChanged;
			}
			catch { }

			try
			{
				if (cancellationToken != null && !cancellationToken.IsCancellationRequested)
					cancellationToken.Cancel();
			}
			catch { }

		}

		protected override void OnResume()
		{

			base.OnResume();

			OverridePendingTransition(0, 0);

			try
			{
				if (!NoxDPM.NoxT.batStatus.IsError)
					LoadBatStatus(NoxDPM.NoxT.batStatus);
			}
			catch { }
		}

		void NetTask(string ConnectionStatus, bool nowifi)
		{
			try
			{
				var tbbWifi = FindViewById<Button>(Resource.Id.txtNoWifi);
				var w1 = FindViewById<ImageView>(Resource.Id.imageViewW1);
				var w2 = FindViewById<ImageView>(Resource.Id.imageViewW2);

				tbbWifi.Text = ConnectionStatus;

				if (nowifi)
				{
					tbbWifi.Text = "Waiting for WiFi";
					w1.Visibility = ViewStates.Gone;
					w2.Visibility = ViewStates.Visible;
					return;
				}

				w1.Visibility = ViewStates.Visible; w2.Visibility = ViewStates.Gone;

			}
			catch { }
		}

		public void NoxEvent(NoxMSGCompletedEventArgs e)
		{
			try
			{
				WorkResponse resp = e.Result;

				if (!HasWindowFocus)
					return;

				RunOnUiThread(delegate ()
				{

					switch (resp.Action)
					{

						case ResponseActionType.NOWIFI:
							NetTask("NO WIFI", true);
							break;

						case ResponseActionType.REQUEST:
							RequestPrompt(resp.UserInfo.ReqType, resp.UserInfo.SMSToken, resp.UserInfo.GMCToken);
							break;

						case ResponseActionType.SHOW_RATE:
							NetTask(resp.ValidationRate, false);
							break;

						case ResponseActionType.UPDATE:
							CheckUpdate();
							break;
					}

				});
			}
			catch { }

		}

		public void LoadBatStatus(BatStatus batStatus)
		{
			try
			{
				if (batStatus.IsError) return;

				TextView tbbStatus = FindViewById<TextView>(Resource.Id.txtBPercent);
				var b1 = FindViewById<ImageView>(Resource.Id.imageViewB1); var b2 = FindViewById<ImageView>(Resource.Id.imageViewB2);
				b1.Visibility = ViewStates.Visible; b2.Visibility = ViewStates.Gone; tbbStatus.Text = "";

				bool battercharging = batStatus.IsCharging; int batLevel = batStatus.Level;

				tbbStatus.Text = batLevel + "%";

				if (battercharging)
				{
					b1.Visibility = ViewStates.Gone; b2.Visibility = ViewStates.Visible;
				}
				else
				{
					b1.Visibility = ViewStates.Visible; b2.Visibility = ViewStates.Gone;
				}
			}
			catch { }
		}

		public override void OnBackPressed()
		{


		}

		private void StartSAct(string action, string task, string display_token = null)
		{

			var nxact = new Intent(this, typeof(SecretActivity));
			nxact.PutExtra("action", action);
			nxact.PutExtra("task", task);

			if (display_token != null)
			{
				nxact.PutExtra("display_token", display_token);
			}

			StartActivity(nxact);

		}

		private void RequestPrompt(string action, string SMSToken, string display_token)
		{

			try
			{
				var cntx = GetActionMsg(action, display_token);
				if (cntx == null) return;

				string msg = cntx[0];
				string title = "";


				if (action == "signin" || action == "register2fa" || action == "authorize2fa")
				{
					title = display_token;
				}


				AlertDialog.Builder builder = new AlertDialog.Builder(this).SetTitle(title).SetMessage(msg).SetCancelable(false).SetNegativeButton("NO",
					(EventHandler<DialogClickEventArgs>)null)
				.SetPositiveButton("Yes, Continue", (EventHandler<DialogClickEventArgs>)null);
				AlertDialog alert = builder.Create();

				alert.Show();
				TextView msgTxt = (TextView)alert.FindViewById(Android.Resource.Id.Message);
				msgTxt.TextSize = 20;
				msgTxt.SetTypeface(null, TypefaceStyle.Bold);
				var okBtn = alert.GetButton((int)DialogButtonType.Positive);
				var nokBtn = alert.GetButton((int)DialogButtonType.Negative);
				nokBtn.Click += (asender, args) =>
				{
					alert.Dismiss();
					NoxDPM.dpm.LockNow();

				};


				okBtn.Click += (asender, args) =>
				{
					StartSAct(action, SMSToken, display_token);
					alert.Dismiss();

				};
			}
			catch { }
		}

		private string[] GetActionMsg(string action, string DisplayToken)
		{
			switch (action)
			{
				case "address":
					return new string[2] { "ADD NEW ADDRESS?", "Add a public address to your profile." };
				case "signin":
					return new string[2] { "Sign-in to this request?", "Value should match what's in your browser." };
				case "txn":
					return new string[2] { "SIGN TRANSACTION?", "Sign then transmit a payment transaction." };
				case "swap":
					return new string[2] { "SWAP WITH CHANGELLY?", "Sign then transmit a payment transaction." };
				case "image":
					return new string[2] { "REBUILD XMR BALNCE?", "Calculate Monero image keys for rebuilding balance." };
				case "gas":
					return new string[2] { "CLAIM GAS?", "Used by NEO as fuel for sending assets." };
				case "message":
					return new string[2] { "SIGN MESSAGE?", "Create signature to prove ownership of a public address." };
				case "register2fa":
					return new string[2] { "REGISTER EXTENSION?", "" };
				case "authorize2fa":
					return new string[2] { "AUTHORIZE MFA REQUEST?", "" };



			}

			return null;
		}

		private void BalClick()
		{

			if (!HasWindowFocus)
				return;

			StartSAct("overview", "");
		}

		private void AdrClick()
		{
			if (!HasWindowFocus)
				return;

			StartSAct("accounts", "");
		}

		private void MsgClick()
		{

			if (!HasWindowFocus)
				return;

			StartPrivMsgs();
		}



		private void StartPrivMsgs(bool noani = false)
		{
			try
			{

				var nxact = new Intent(this, typeof(SecretActivity));
				nxact.PutExtra("action", "session_start");
				nxact.PutExtra("task", "");
				if (noani)
				{
					nxact.AddFlags(ActivityFlags.NoAnimation);
				}

				StartActivity(nxact);

			}
			catch { }

		}

		private void WifiClick()
		{
			var nxact = new Intent(this, typeof(NoxWifi));
			StartActivity(nxact);
		}

		private void CheckUpdate()
		{

			var nxact = new Intent(this, typeof(StatusActivity));
			StartActivity(nxact);

		}


		private void StartWalletClick()
		{
			try
			{

				var nxact = new Intent(this, typeof(ChannelActivity));

				StartActivity(nxact);

			}
			catch { }
		}

		private void LoadClickViews()
		{
			try
			{

				var xwid = FindViewById<LinearLayout>(Resource.Id.xbtnSystem);
				var bwid = FindViewById<Button>(Resource.Id.btnSystem);

				xwid.Click += delegate { CheckUpdate(); };
				bwid.Click += delegate { CheckUpdate(); };

				var xbtnWifi = FindViewById<LinearLayout>(Resource.Id.xbtnWifi);
				var xbtnAddresses = FindViewById<LinearLayout>(Resource.Id.xbtnAddresses);
				var xbtnBalances = FindViewById<LinearLayout>(Resource.Id.xbtnBalances);
				var xbtnMessage = FindViewById<LinearLayout>(Resource.Id.xbtnMessage);
				var btnWifi = FindViewById<Button>(Resource.Id.btnWifi);
				var btnAddresses = FindViewById<Button>(Resource.Id.btnAddresses);
				var btnBalances = FindViewById<Button>(Resource.Id.btnBalances);
				var btnMessage = FindViewById<Button>(Resource.Id.btnMessage);

				var btnStart = FindViewById<Button>(Resource.Id.main_connect_wallet);
				btnStart.Click += delegate { StartWalletClick(); };

				xbtnWifi.Click += delegate { WifiClick(); };
				xbtnBalances.Click += delegate { BalClick(); };
				xbtnAddresses.Click += delegate { AdrClick(); };
				xbtnMessage.Click += delegate { MsgClick(); };

				btnWifi.Click += delegate { WifiClick(); };
				btnBalances.Click += delegate { BalClick(); };
				btnAddresses.Click += delegate { AdrClick(); };
				btnMessage.Click += delegate { MsgClick(); };

			}
			catch { }
		}


		public void LoadVTypes()
		{
			try
			{

				var tbbWifi = FindViewById<Button>(Resource.Id.txtNoWifi);
				var w1 = FindViewById<ImageView>(Resource.Id.imageViewW1);
				var w2 = FindViewById<ImageView>(Resource.Id.imageViewW2);

				Button btnWifi = FindViewById<Button>(Resource.Id.btnWifi);
				var btnAddresses = FindViewById<Button>(Resource.Id.btnAddresses);
				var btnBalances = FindViewById<Button>(Resource.Id.btnBalances);
				var btnMessage = FindViewById<Button>(Resource.Id.btnMessage);
				var btnSystem = FindViewById<Button>(Resource.Id.btnSystem);

				w1.Visibility = ViewStates.Visible; w2.Visibility = ViewStates.Gone;

			tbbWifi.Typeface = TFView.typeface;
				btnWifi.Typeface = TFView.typeface;
				btnAddresses.Typeface = TFView.typeface; btnBalances.Typeface = TFView.typeface; btnMessage.Typeface = TFView.typeface;
				btnSystem.Typeface = TFView.typeface;

				TextView tvWalletID = FindViewById<TextView>(Resource.Id.btnWalletID);

				if (NoxDPM.NoxT.noxDevice.IsErrorKey())
				{
					tvWalletID.Text = "ERROR";
				}
				else
				{
					tvWalletID.Text = NoxDPM.NoxT.noxDevice.GetWalletID().ToUpper();
					tvWalletID.Typeface = TFView.typefaceB;
				}

				tvWalletID.Click += delegate { WalletIDClick(); };
			}
			catch { }
		}



		private void WalletIDClick()
		{

		}


	}
}


