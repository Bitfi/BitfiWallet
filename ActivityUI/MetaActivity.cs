using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using static Android.OS.PowerManager;
using WalletLibrary.ActiveApe.ApeShift;
using WalletLibrary.ActiveApe;

namespace BitfiWallet
{

	[Activity(Name = "com.rokits.noxadmin.MetaActivity", Theme = "@style/FullscreenThemeMAS", Label = "", HardwareAccelerated = true, TurnScreenOn = true, NoHistory = true)]

	public class MetaActivity : Activity
	{

		WakeLock wakeLock;

		public CancellationTokenSource cancellationToken = null;

		Action ThisPosAction;
		Action ThisNegAction;
		Action SwitchAction;
		LinearLayout svrt;
		int LLHeight = 370;
		bool Created = false;
		bool Stopped = false;

		protected override void OnCreate(Bundle bundle)
		{

			base.OnCreate(bundle);
			SetContentView(Resource.Layout.metamask);
			this.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.TurnScreenOn);
			PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
			wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "com.rokits.noxadmin");
			CreateLayout();

			TextView tvStatus = FindViewById<TextView>(Resource.Id.metasessiontglStatus);
			tvStatus.Typeface = Typeface.CreateFromAsset(Assets, "Rubik-Bold.ttf");
			tvStatus.Text = "";

			TextView tvWalletID = FindViewById<TextView>(Resource.Id.meta_natwork_status1);
			tvWalletID.Typeface = Typeface.CreateFromAsset(Assets, "Rubik-Bold.ttf");
			tvWalletID.Text = NoxDPM.NoxT.noxDevice.GetWalletID().ToUpper();

			svrt = FindViewById<LinearLayout>(Resource.Id.metasessionmsgLayoutSCROLLER_ID);
			var llScroll = FindViewById<LinearLayout>(Resource.Id.metasessionllMessages);
			LLHeight = llScroll.LayoutParameters.Height;

			var btnPos = FindViewById<Button>(Resource.Id.metaBtnsend);
			var btnNeg = FindViewById<Button>(Resource.Id.metabtnCancel);
			var bwid = FindViewById<Button>(Resource.Id.metasessiontglBtn);
			Switch ds = FindViewById<Switch>(Resource.Id.meta_model_switch);

			if (Created)
				return;

			bwid.Click += Bwid_Click;
			ds.CheckedChange += Ds_CheckedChange;
			btnPos.Click += BtnPos_Click;
			btnNeg.Click += BtnNeg_Click;

			Created = true;

		}

		private void Ds_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{

			if (Stopped)
				return;

			SwitchAction.Invoke();
		}

		private void BtnNeg_Click(object sender, EventArgs e)
		{
			if (Stopped)
				return;

			ThisNegAction.Invoke();
		}

		private void BtnPos_Click(object sender, EventArgs e)
		{
			if (Stopped)
				return;

			if (ThisPosAction != null)
				ThisPosAction.Invoke();
		}

		private void Bwid_Click(object sender, EventArgs e)
		{
			if (Stopped)
				return;

			StopWallet();
		}

		private void _APESession__OnRequest(BroadcastRequestCompletedEventArgs e)
		{

			if (this.IsFinishing || this.IsDestroyed)
				return;

			if (e.Method == wallet_methods.status)
			{
				try
				{
					ToggleView(true, new List<string>() { e.Sender.ECStatus }, "START");

				}
				catch { }
				return;
			}


			Task.Run(async () =>
			{

				ToggleView(true, new List<string>() { e.Method + " request received" }, e.Method.ToString());

				try
				{
					var result = await NoxChannel.Current._ActiveProcessor.GetWalletRequest(e);

					if (this.IsFinishing || this.IsDestroyed)
						return;

					if (result.IsDisposed)
						return;

					if (result.IsError)
					{
						NoxChannel.Current._APESession.ErrorResonse(result.ErrorMessage, e.Sender);
						ToggleView(true, new List<string>() { "error: message sent for " + e.Method.ToString() }, e.Method.ToString());
						return;
					}

					if (result.UserPrompt)
					{
						UserPrompt(e.Method, result.Result, e.Request, e.Sender, result.PromptMessage, result.PromptInfo);
					}
					else
					{
						NoxChannel.Current._APESession.ResponseArrived(result.Result, e.Sender);
						ToggleView(true, new List<string>() { "success: result sent for " + e.Method.ToString() }, e.Method.ToString());
					}

				}
				catch (Exception ex)
				{
					NoxChannel.Current._APESession.ErrorResonse(ex.Message, e.Sender);
				}
			});

		}

		private void UserPrompt(wallet_methods method, object response, WalletRequest<object> request,
			MQDiffieResult sender, string msg, List<SatusListItem> items)
		{

				if (ThisPosAction != null)
					throw new Exception("Error, user maybe busy");
			

			string SuccessMsg = "SIGNATURE SENT TO PROVIDER";
			string PosBtn = "YES, SEND";
			string Title = string.Empty;

			if (method == wallet_methods.connect_wallet)
			{
				Title = "CONNECT WALLET";
				PosBtn = " YES, ALLOW";
				sender.ECDisplayCode = (string)response;
				SuccessMsg = sender.ECDisplayCode + " PROVIDER AUTHORIZED";
			}

			if (method == wallet_methods.transfer)
				Title = "TRANSFER ASSET";

			if (method == wallet_methods.sign_message)
				Title = "SIGN MESSAGE";


			ThisPosAction = new Action(() =>
			{
				ClosePrompt(true, SuccessMsg);

				NoxChannel.Current._APESession.ResponseArrived(response, sender);

			});


			ThisNegAction = new Action(() =>
			{
				ClosePrompt(false, "");

				NoxChannel.Current._APESession.ErrorResonse("REJECTED", sender);

			});


			AddTransaction(msg, PosBtn, "CANCEL", items, sender.ECDisplayCode, Title);

		}

		public override void OnBackPressed()
		{

		}

		private void StopWallet()
		{

			ThisPosAction = new Action(() =>
			{

				Finish();

			});

			ThisNegAction = new Action(() =>
			{

				ThisPosAction = null;

				FindViewById<LinearLayout>(Resource.Id.meta_ll_a1).Visibility = ViewStates.Visible;

				var llPrompt = FindViewById<LinearLayout>(Resource.Id.metallDialg);
				llPrompt.Visibility = ViewStates.Gone;

				var llScroll = FindViewById<LinearLayout>(Resource.Id.metasessionllMessages);
				llScroll.LayoutParameters.Height = LLHeight;

			});

			svrt.RemoveAllViews();

			MsgPrompt("CLOSE WALLET SESSION?", "YES, STOP", "CANCEL");

		}

		private void MsgPrompt(string Message, string PosBtnMsg, string NegBtnMsg)
		{

			var llScroll = FindViewById<LinearLayout>(Resource.Id.metasessionllMessages);
			llScroll.LayoutParameters.Height = 530;

			var llPrompt = FindViewById<LinearLayout>(Resource.Id.metallDialg);
			llPrompt.Visibility = ViewStates.Visible;

			var txtMsg = FindViewById<TextView>(Resource.Id.metalblsigntransfer);
			var btnPos = FindViewById<Button>(Resource.Id.metaBtnsend);
			var btnNeg = FindViewById<Button>(Resource.Id.metabtnCancel);

			txtMsg.Text = Message;
			btnPos.Text = PosBtnMsg;
			btnNeg.Text = NegBtnMsg;

		}

		bool Started = false;

		protected override void OnStart()
		{

			base.OnStart();

			try
			{
				SwitchAction = new Action(() =>
				{

					if (NoxChannel.Current._APESession.IsUsuerBlocking)
					{
						NoxChannel.Current._APESession.IsUsuerBlocking = false;
					}
					else
					{
						NoxChannel.Current._APESession.IsUsuerBlocking = true;
					}

				});


				if (!wakeLock.IsHeld) wakeLock.Acquire();
				NoxChannel.Current._APESession._OnRequest += _APESession__OnRequest;


				ThisPosAction = null;
				ThisNegAction = new Action(() => { });

				NoxChannel.Current.StartActivity(this, out cancellationToken);


			}
			catch { }

		}

		private void ClosePrompt(bool Success, string SuccessMsg)
		{

			Action uiaction = new Action(() =>
			{

				try
				{
					TextView tvStatus = FindViewById<TextView>(Resource.Id.metasessiontglStatus);
					tvStatus.Text = "";

					FindViewById<TextView>(Resource.Id.metasessiontglStatus2).Text = "";

					var llPrompt = FindViewById<LinearLayout>(Resource.Id.metallDialg);
					llPrompt.Visibility = ViewStates.Gone;

					var llScroll = FindViewById<LinearLayout>(Resource.Id.metasessionllMessages);
					llScroll.LayoutParameters.Height = LLHeight;

					FindViewById<LinearLayout>(Resource.Id.meta_ll_a1).Visibility = ViewStates.Visible;
					FindViewById<LinearLayout>(Resource.Id.meta_ll_a2).Visibility = ViewStates.Gone;

					svrt.RemoveAllViews();

					if (Success)
					{
						AddMessages(new List<string>() { SuccessMsg }, "SENT");
					}
					else
					{
						AddMessages(new List<string>() { "REJECTION ERROR SENT" }, "SENT");

					}

				}
				catch { }

					ThisPosAction = null;

			});

			RunOnUiThread(uiaction);

		}

		protected override void OnStop()
		{

			Stopped = true;

			base.OnStop();
			if (wakeLock.IsHeld) wakeLock.Release();
			try
			{

				SwitchAction = new Action(() => { });
				NoxChannel.Current._APESession._OnRequest -= _APESession__OnRequest;

				if (cancellationToken != null && !cancellationToken.IsCancellationRequested)
					cancellationToken.Cancel();

				Finish();

			}
			catch { }

		}

		private void ToggleView(bool UiThread, List<string> MsgList, string Title)
		{
			if (UiThread)
			{
				Action uiaction = new Action(() =>
				{
					AddMessages(MsgList, Title);
				});

				RunOnUiThread(uiaction);
			}
			else
			{
				AddMessages(MsgList, Title);
			}
		}

		private void AddTransaction(string Message, string PosBtnMsg, string NegBtnMsg, List<SatusListItem> items,
			string DisplayCode, string Title)
		{

			Action uiaction = new Action(() =>
			{

				if (!string.IsNullOrEmpty(DisplayCode))
				{
					TextView tvStatus = FindViewById<TextView>(Resource.Id.metasessiontglStatus);
					tvStatus.Text = DisplayCode.ToUpper();

					FindViewById<TextView>(Resource.Id.metasessiontglStatus2).Text = Title;

					FindViewById<LinearLayout>(Resource.Id.meta_ll_a2).Visibility = ViewStates.Visible;
					FindViewById<LinearLayout>(Resource.Id.meta_ll_a1).Visibility = ViewStates.Gone;

				}

				svrt.RemoveAllViews();

				LayoutInflater vi = (LayoutInflater)ApplicationContext.GetSystemService(Context.LayoutInflaterService);

				foreach (var _satusListItems in items)
				{

					View convertView = vi.Inflate(Resource.Layout.status_list, null);

					LinearLayout parent = convertView.FindViewById<LinearLayout>(Resource.Id.status_list_item_layout);
					LinearLayout.LayoutParams layout_params = (LinearLayout.LayoutParams)parent.LayoutParameters;
					layout_params.TopMargin = 8;
					layout_params.BottomMargin = 8;
					parent.LayoutParameters = layout_params;


					TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv1);
					TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv2);
					tv1.Typeface = TFView.typeface;
					tv2.Typeface = TFView.typeface;
					tv1.Text = _satusListItems.StatusTitle;
					tv2.Text = _satusListItems.StatusValue1;


					svrt.AddView(convertView, 0, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
						ViewGroup.LayoutParams.WrapContent));

				}

				MsgPrompt(Message, PosBtnMsg, NegBtnMsg);

			});

			RunOnUiThread(uiaction);

		}

		private void AddMessages(List<string> MsgList, string Title)
		{

			if (svrt == null)
			{
				svrt = FindViewById<LinearLayout>(Resource.Id.metasessionmsgLayoutSCROLLER_ID);
			}

			LayoutInflater vi = (LayoutInflater)ApplicationContext.GetSystemService(Context.LayoutInflaterService);

			for (int i = 0; i < MsgList.Count; i++)
			{
				View convertView = vi.Inflate(Resource.Layout.sessionLL, null);

				TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.sessionovt1);
				tv1.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);

				if (i == 0)
				{
					tv1.Text = "[VERBOSE] " + DateTime.Now.ToString();
				}
				else
				{
					tv1.Text = "";
				}

				string line = MsgList[i];

				TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.sessionovt2);
				tv2.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
				tv2.Text = line.ToUpper();

				int stackPos = 0;

				svrt.AddView(convertView, stackPos, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
					ViewGroup.LayoutParams.WrapContent));

			}

		}

		public void CreateLayout()
		{
			LoadInpViews();
			ScrollView scrollView = FindViewById<ScrollView>(Resource.Id.metasessionmsgSCROLLER_ID);
			scrollView.ScrollChange += ScrollViewScrolled;
		}

		private void ScrollViewScrolled(object sender, View.ScrollChangeEventArgs e)
		{

		}

		private void LoadInpViews()
		{
			LayoutInflater inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
			View dialoglayout = inflater.Inflate(Resource.Layout.splashmodel, null);

			AlertDialog.Builder builder = new AlertDialog.Builder(this, Resource.Style.FullscreenTheme);
			builder.SetCancelable(false);
			builder.SetView(dialoglayout);
		}

	}
}