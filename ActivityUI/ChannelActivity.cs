using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace BitfiWallet
{
	[Activity(Name = "com.rokits.noxadmin.ChannelActivity", Theme = "@style/FullscreenTheme", Label = "", HardwareAccelerated = true, NoHistory = true)]

	public class ChannelActivity : Activity
	{


		protected override void OnCreate(Bundle bundle)
		{

			base.OnCreate(bundle);
			SetContentView(Resource.Layout.channel_model);

			try
			{
				CreateLayout();
			
				var btnStart = FindViewById<Button>(Resource.Id.chan_model_button_start);
				var btnMsg = FindViewById<Button>(Resource.Id.chan_model_button_messaging);


				btnStart.Click += delegate
				{

					if (string.IsNullOrEmpty(NoxChannel.Current.APE_CONNECTION_STRING))
					{

						ToggleView(true, new List<string>() { "CHECK NETWORK CONNECTION" }, "SETUP");
						return;
					}
						StartPrivMsgs("wallet_start");
				};

				btnMsg.Click += delegate
				{
				//	StartPrivMsgs("session_start");
				};

	
				Task.Run(async () =>
				{
					if (string.IsNullOrEmpty(NoxChannel.Current.APE_CONNECTION_STRING))
					{

						ToggleView(true, new List<string>() { "BUILDING CONFIGURATION" }, "SETUP");

						var setup = await WalletLibrary.ActiveApe.ApeShift.ApeAPI.WSRequest();

						if (setup != null)
						{
							if (string.IsNullOrEmpty(setup.error_message))
							{
								AddInfoItems(NoxChannel.Current.des_items);
							}
							else
							{
								ToggleView(true, new List<string>() { setup.error_message }, "ERROR");
							}

						}

					}
					else
					{
						AddInfoItems(NoxChannel.Current.des_items);
					}

				});
				
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
		private void AddMessages(List<string> MsgList, string Title)
		{
			if (this.IsFinishing || this.IsDestroyed)
				return;

			try
			{
				LinearLayout svrt = FindViewById<LinearLayout>(Resource.Id.chan_sessionmsgLayoutSCROLLER_ID);
				LayoutInflater vi = (LayoutInflater)ApplicationContext.GetSystemService(Context.LayoutInflaterService);

				foreach (string line in MsgList)
				{

					View convertView = vi.Inflate(Resource.Layout.sessionLL, null);

					TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.sessionovt1);
					tv1.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
					tv1.Text = "[INFO] " + DateTime.Now.ToString();

					TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.sessionovt2);
					tv2.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
					tv2.Text = line.ToUpper();

					int stackPos = 0;

					svrt.AddView(convertView, stackPos, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
						ViewGroup.LayoutParams.WrapContent));

				}
			}
			catch { }
		}

		public void CreateLayout()
		{
			LoadInpViews();
			ScrollView scrollView = FindViewById<ScrollView>(Resource.Id.chan_sessionmsgSCROLLER_ID);
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
		private void StartPrivMsgs(string action, bool noani = false)
		{
			try
			{

				var nxact = new Intent(this, typeof(SecretActivity));
				nxact.PutExtra("action", action);
				nxact.PutExtra("task", "");
				if (noani)
				{
					nxact.AddFlags(ActivityFlags.NoAnimation);
				}

				StartActivity(nxact);

			}
			catch { }

		}

		private void AddInfoItems(List<WalletLibrary.ActiveApe.SatusListItem> items)
		{

			if (this.IsFinishing || this.IsDestroyed)
				return;

			if (items == null || items.Count == 0)
				return;

			Action uiaction = new Action(() =>
			{
				try
				{
					LinearLayout svrt = FindViewById<LinearLayout>(Resource.Id.chan_sessionmsgLayoutSCROLLER_ID);
					svrt.RemoveAllViews();

					LayoutInflater vi = (LayoutInflater)ApplicationContext.GetSystemService(Context.LayoutInflaterService);

					foreach (var _satusListItems in items)
					{

						View convertView = vi.Inflate(Resource.Layout.status_list, null);
						TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv1);
						TextView tv2 = convertView.FindViewById<TextView>(Resource.Id.status_list_item_tv2);
						tv1.Typeface = TFView.typeface;
						tv2.Typeface = TFView.typeface;
						tv1.Text = _satusListItems.StatusTitle;
						tv2.Text = _satusListItems.StatusValue1;


						svrt.AddView(convertView, 0, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
							ViewGroup.LayoutParams.WrapContent));

					}
				}
				catch { }
			});

			RunOnUiThread(uiaction);

		}
	}
}
