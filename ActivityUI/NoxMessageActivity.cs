using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoxKeys;
using Newtonsoft.Json;
using Android.Graphics;
using WalletLibrary.NoxShared;
using System.Globalization;
using Android.Preferences;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using WalletLibrary;

namespace BitfiWallet
{
	class NoxMessageActivity : WalletLibrary.Wallet
	{

		Activity _activity;

		Dictionary<Guid, TextView> pendingViews = new Dictionary<Guid, TextView>();
		ActiveView activeView = ActiveView.Main;

		NoxWSClient wSClient;
		MessageProxy eventProxy;
		AlertProxy alertProxy;
		MSGCompletedEventHandler localRegistration;
		MSGCollectionCompletedEventHandler alertRegistration;
		SleepProcessor sleepProcessor;
		NoxDlgModel noxDlgModel;

		delegate void ScrollReachedBottomEventHandler(object sender, EventArgs e);
		event ScrollReachedBottomEventHandler ScrollReachedBottom;
		SharedSession sharedSession;

		VibrationEffect vibeEffect;
		Task EnterTask;

		string DefaultNode = EthNode.DEFAULT_HOST;
		string Node = "";
		string[] Recents = new string[0];
		bool Closing = false;
		int ChildCount = 0;

		public NoxMessageActivity() { }

		public void Init(Activity activity)
		{
			this._activity = activity;

			string task = _activity.Intent.GetStringExtra("task");

			JavaClassInit(_activity.Intent.GetStringExtra("action"), task, "");

			CreateLayout();

			long[] mVibratePattern = new long[] { 0, 400, 800, 600, 800, 800, 800 };
			int[] mAmplitudes = new int[] { 0, 255, 0, 255, 0, 255, 0 };
			vibeEffect = VibrationEffect.CreateWaveform(mVibratePattern, mAmplitudes, -1);
			noxDlgModel = new NoxDlgModel(_activity);

			sharedSession = NoxDPM.NoxT.noxDevice.sharedSessions[new Guid(task)];

		}


		protected override void NoxEvent(WalletEventArgs e)
		{

		}

		private void StartActivity()
		{

			ToggleView(ActiveView.Validation, false, "requesting contract status...");

			Task.Run(async () =>
			{

				try
				{

					LoadPrefNode();
					Recents = LoadPrefRecents();

					ValidationRequest vrequest = new ValidationRequest();
					vrequest.pubKey = sharedSession.MyPublicKey().ToHex();
					ManagerRequest managerRequest = new ManagerRequest();
					managerRequest.validationRequest = vrequest;
					managerRequest.requestType = ManagerRequestType.Validation;
					var status = await sharedSession.PostAsync(managerRequest);

					if (status.subscriptionError)
					{
						Action subsction = new Action(() =>
						{
							ShowSubscriptionMsg(status.subscriptionInfo);
						});

						ToggleView(ActiveView.Error, true, "[pending registration]");
						Closing = true;

						_activity.RunOnUiThread(subsction);
					}
					else
					{
						if (!status.success)
						{
							ToggleView(ActiveView.Error, true, status.error_message);
							Closing = true;
						}
						else
						{
							if (status.validationResponse.IsRegistered == false)
							{
								ToggleView(ActiveView.RegistrationManager, true, "your public key isn't registered, please enter a friendly name for registration:");
							}
							else
							{
								ToggleView(ActiveView.Main, true);

							}

							alertProxy = new AlertProxy();
							alertRegistration = new MSGCollectionCompletedEventHandler(Processor_AlertEvent);
							alertProxy.MsgCompleted += alertRegistration;
							sleepProcessor = new SleepProcessor(sharedSession, alertProxy, TimeSpan.FromMinutes(45), TimeSpan.FromSeconds(5));
						}
					}

				}
				catch (Exception ex)
				{
					ToggleView(ActiveView.Error, true, ex.Message);
					Closing = true;
				}
			});

		}

		private void ShowSubscriptionMsg(SubscriptionInfo subscriptionInfo)
		{

			AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity).SetMessage(subscriptionInfo.message_prompt).SetTitle(subscriptionInfo.title_prompt).SetCancelable(false)
					.SetNegativeButton("CLOSE", (EventHandler<DialogClickEventArgs>)null)
					.SetPositiveButton("CONTINUE", (EventHandler<DialogClickEventArgs>)null);
			var wfalert = wfbuilder.Create();

			try
			{
				wfalert.Show();

				var wfno = wfalert.GetButton((int)DialogButtonType.Negative);
				wfno.Click += (asender, args) =>
				{
					wfalert.Dismiss();
				};

				var wfokBtn = wfalert.GetButton((int)DialogButtonType.Positive);
				wfokBtn.Click += (asender, args) =>
				{
					ShowNoxNotice(subscriptionInfo);
					wfalert.Dismiss();
				};
			}
			catch { }

		}

		private void ShowNoxNotice(SubscriptionInfo subscriptionInfo)
		{

			Action action1 = new Action(() =>
			{
				ShowSubscriptionDlg(subscriptionInfo, 1);
			});

			Action action2 = new Action(() =>
			{
				ShowSubscriptionDlg(subscriptionInfo, 2);
			});

			noxDlgModel.ShowNoxNotice(subscriptionInfo.title_notice, subscriptionInfo.message_notice,
					subscriptionInfo.btn_a_notice, subscriptionInfo.btn_b_notice, action1, action2);
		}

		private void ShowSubscriptionDlg(SubscriptionInfo subscriptionInfo, int _option)
		{

			NoxDlgModel.SheetData data = new NoxDlgModel.SheetData();

			Action n_action1 = new Action(() =>
			{
				noxDlgModel.alertDialog1.Dismiss();
			});

			Action p_action1 = new Action(() =>
			{
				noxDlgModel.ShowWaitMsg("preparing method, please wait...", data.Items[data.Selected] + " Payment");

				Task.Run(async () =>
				{

					SubsriptionRequest subsriptionRequest = new SubsriptionRequest();
					subsriptionRequest.blkNet = data.Items[data.Selected];
					subsriptionRequest.option = _option;
					ManagerRequest managerRequest = new ManagerRequest();
					managerRequest.subsriptionRequest = subsriptionRequest;
					managerRequest.requestType = ManagerRequestType.Subscription;
					var status = await sharedSession.PostAsync(managerRequest);

					if (status == null || !status.success)
					{
						Action actionError = new Action(() =>
						{
							noxDlgModel.ShowErrorMsg("Error processing request, please try again.");
						});

						_activity.RunOnUiThread(actionError);
					}
					else
					{
						if (!string.IsNullOrEmpty(status.subsriptionResponse.subscription_message))
						{
							Action actionError = new Action(() =>
							{
								noxDlgModel.ShowErrorMsg(status.subsriptionResponse.subscription_message);
							});

							_activity.RunOnUiThread(actionError);
						}
						else
						{
							Action actionSuccess = new Action(() =>
							{
								var nxact = new Intent(_activity, typeof(SecretActivity));
								nxact.PutExtra("action", "txn");
								nxact.PutExtra("task", status.subsriptionResponse.smsToken);
								_activity.StartActivity(nxact);
							});

							Action actionDismiss = new Action(() =>
							{
								noxDlgModel.alertDialog2.Dismiss();
								noxDlgModel.alertDialog1.Dismiss();
								sharedSession.Enabled = false;
							});

							_activity.RunOnUiThread(actionSuccess);

							await Task.Delay(500);

							_activity.RunOnUiThread(actionDismiss);
						}

					}
				});

			});

			noxDlgModel.ShowNoxDlg(subscriptionInfo.subscriptionMethods.methods, 0,
					subscriptionInfo.mthds_title, "CONTINUE", "CANCEL", p_action1, n_action1, out data);


			Task.Run(async () =>
			{
				Action actiondlgNoxNoticeDismiss = new Action(() =>
				{

					try
					{
						noxDlgModel.dlgNoxNotice.Dismiss();
					}
					catch { }
				});

				await Task.Delay(200);

				_activity.RunOnUiThread(actiondlgNoxNoticeDismiss);

			});

		}



		public void Processor_MSGEvent(MSGCompletedEventArgs processor)
		{
			try
			{
				Action uiaction = new Action(() =>
				{
					CloseMsg();
				});

				if (processor.Message == "close")
				{

					_activity.RunOnUiThread(uiaction);
				}
				else
				{
					ProcessResponse(processor.Message);
				}
			}
			catch { }
		}

		private void ScrollViewScrolled(object sender, View.ScrollChangeEventArgs e)
		{
			if (ScrollReachedBottom != null && GettingMore == false)
			{
				ScrollView scrollView = sender as ScrollView;
				int pos = e.ScrollY;
				var view = scrollView.GetChildAt(scrollView.ChildCount - 1);

				var diff = (view.Bottom - (scrollView.Height + e.ScrollY));

				if (diff == 0)
				{
					GettingMore = true;
					ScrollReachedBottom.Invoke(this, new EventArgs());
				}
			}

		}

		private void Processor_AlertEvent(MSGCollectionCompletedEventArgs processor)
		{
			try
			{
				if (processor.Messages == null) return;
				if (processor.Messages.CloseActivity)
				{
					Close();
				}
				else
				{
					Action uiaction = new Action(() =>
					{
						ShowOfflineMsg(processor.Messages);
					});

					_activity.RunOnUiThread(uiaction);
				}
			}
			catch { }
		}

		private void ShowOfflineMsg(SleepResponse sleepResponse)
		{
			NoxDlgModel.SheetData data = null;

			Action p_action = new Action(() =>
			{
				if (data != null)
				{
					if (data.Selected == -1) data.Selected = 0;
					EnterEvent(data.Items[data.Selected]);
				}
			});

			Action n_action = new Action(() =>
			{
				sleepProcessor.peerAlerts = new string[0];
			});

			noxDlgModel.ShowMdlDlg(sleepResponse.PeerList, sleepResponse.PeerDisplayList, 0,
					"Messages detected while on standby, connect?", "YES", "NO", vibeEffect, v, n_action, p_action, out data);

		}


		void StartSleep()
		{
			if (Closing) return;
			if (sharedSession == null) return;
			if (wSClient != null) return;
			if (sleepProcessor == null) return;

			sleepProcessor.Start();

		}
		void StopSleep()
		{
			try
			{
				if (sleepProcessor == null) return;

				sleepProcessor.Stop();
			}
			catch { }
		}

		public void HandleDstry()
		{
			try
			{
				NoxResetArrays();
			}
			catch { }

			try
			{
				if (sleepProcessor != null)
				{
					sleepProcessor.Stop();
					sleepProcessor = null;
				}
			}
			catch { }

			sharedSession.Dispose();
			sharedSession = null;

			NoxDPM.NoxT.noxDevice.sharedSessions.Clear();
		}
		public void HandleStop()
		{
			CloseSession();
			StartSleep();
		}
		public void HandleStart()
		{
			if (!sharedSession.Enabled)
			{
				sharedSession.Enabled = true;
				Closing = false;
				StartActivity();
			}
			else
			{
				StopSleep();
			}
		}
		public void HandleBP()
		{
			if (wSClient == null)
			{
				PromptMsg();
			}
			else
			{
				CloseSession();
			}
		}

		private void Close()
		{

			try
			{
				if (sleepProcessor != null)
				{
					sleepProcessor.Stop();
					sleepProcessor = null;
				}
			}
			catch { }

			_activity.Finish();
		}
		public void CloseSession()
		{
			if (wSClient == null) return;
			if (GettingMore) return;

			ToggleView(ActiveView.Main, false);

			wSClient.WSClose();
			eventProxy.MsgCompleted -= localRegistration;
			localRegistration = null;
			wSClient = null;

			if (!KbLyt)
			{
				TglClick();
			}

		}
		private void Reset(bool resetList = false)
		{
			try
			{
				NoxResetArrays();
				LinearLayout box = (LinearLayout)_activity.FindViewById(Resource.Id.session_message);
				box.RemoveAllViews();
				ResetTags();
				if (resetList)
				{
					var sv = _activity.FindViewById<LinearLayout>(Resource.Id.sessionmsgLayoutSCROLLER_ID);
					sv.RemoveAllViews();
					var tvs = _activity.FindViewById<TextView>(Resource.Id.sessiontglStatus);
					tvs.Text = "";
				}
			}
			catch { }
		}

		private void ProcessUIResp(MessageResponse resp)
		{

			if (resp.peerMessage != null)
			{

				AddParagraph(resp);
			}
			else
			{
				if (resp.serviceMessage == "peer connected")
				{

					var tvs = _activity.FindViewById<TextView>(Resource.Id.sessiontglStatus);
					tvs.Text = "PEER ONLINE";
					return;
				}
				if (resp.serviceMessage == "peer disconnected")
				{
					var tvs = _activity.FindViewById<TextView>(Resource.Id.sessiontglStatus);
					tvs.Text = "PEER OFFLINE";
					return;
				}

				if (resp.responseType == SharedResponseType.Receipt)
				{

					if (pendingViews.ContainsKey(resp.MessageID))
					{
						var tv1 = pendingViews[resp.MessageID];
						tv1.Text = tv1.Text.Replace("[SENT]", "DLVRD");
						tv1.SetTypeface(tv1.Typeface, TypefaceStyle.Normal);
						tv1.SetTextColor(new Color(LinearColor));
						pendingViews.Remove(resp.MessageID);
					}

					return;
				}

				AddMsgView(resp.serviceMessage);
			}

		}
		public void ProcessResponse(string msg)
		{
			if (sharedSession == null) return;

			try
			{
				Task.Run(async () =>
				{
					var respList = await sharedSession.GetResponse(msg, wSClient.Peer(), wSClient);

					if (respList != null)
					{
						Action uiaction = new Action(() =>
						{

							foreach (var resp in respList)
							{
								ProcessUIResp(resp);
							}

							GettingMore = false;

						});

						_activity.RunOnUiThread(uiaction);

						if (ScrollReachedBottom == null)
						{
							ScrollReachedBottom += delegate
							{
								GetMoreRequest();

							};
						}
					}
					else
					{
						GettingMore = false;
					}

				});

			}
			catch { }
		}

		bool GettingMore = false;
		private async void GetMoreRequest()
		{
			string sessionmsg = sharedSession.CreateMoreRequest();
			await wSClient.Send(sessionmsg);
		}
		private void ToggleView(ActiveView toggleView, bool UiThread, string Msg = null)
		{
			if (UiThread)
			{
				Action uiaction = new Action(() =>
				{
					ActionToggleView(toggleView, Msg);
				});

				_activity.RunOnUiThread(uiaction);
			}
			else
			{
				ActionToggleView(toggleView, Msg);
			}
		}
		private async Task NodeEnterEvent()
		{
			string node = GetInstrctionText();

			ToggleView(ActiveView.UpdateNodeChecking, true);

			string peer = await EthNode.GetPeer("noxdemo", node);
			if (string.IsNullOrEmpty(peer))
			{
				ToggleView(ActiveView.UpdateNodeError, true);
			}
			else
			{

				SavePrefNode(node);
				LoadPrefNode();
				ToggleView(ActiveView.Main, true);

			}
		}


		private async Task RegisterEnterEvent()
		{

			string name = GetInstrctionText();

			ToggleView(ActiveView.RegistrationManager, true, "validating, please wait...");

			RegistrationRequest request = new RegistrationRequest();
			request.pubKey = sharedSession.MyPublicKey().ToHex();
			request.registerName = name;
			ManagerRequest managerRequest = new ManagerRequest();
			managerRequest.registrationRequest = request;
			managerRequest.requestType = ManagerRequestType.Registration;

			var status = await sharedSession.PostAsync(managerRequest);

			if (!status.success)
			{
				ToggleView(ActiveView.Error, true, status.error_message);
			}
			else
			{
				if (status.registrationResponse.IsRegistered == false)
				{
					ToggleView(ActiveView.RegistrationManager, true, status.registrationResponse.registration_message);
				}
				else
				{
					ToggleView(ActiveView.RegistrationManager, true, "Registration successful!");
					await Task.Delay(1500);
					ToggleView(ActiveView.Main, true);
				}
			}

		}
		private async Task MessageEnterEvent(string selectedRecent = null)
		{

			if (wSClient == null)
			{
				string name = "";
				if (!string.IsNullOrEmpty(selectedRecent))
				{
					name = selectedRecent;
					selectedRecent = null;
				}
				else
				{
					name = GetInstrctionText();
				}

				if (string.IsNullOrEmpty(name)) return;


				ToggleView(ActiveView.MessageCheckingName, true);

				string peer = await EthNode.GetPeer(name, Node);

				if (string.IsNullOrEmpty(peer))
				{
					ToggleView(ActiveView.MessageNameError, true);
				}
				else
				{
					if (peer.ToUpper() == sharedSession.MyPublicKey().ToHex().ToUpper())
					{
						ToggleView(ActiveView.MessageNameError, true, "Invalid peer...can't message with yourself.");

					}
					else
					{
						sleepProcessor.peerAlerts = new string[0];

						ToggleView(ActiveView.MessageStarting, true);

						if (!Recents.ListContains(name))
						{
							Recents = Recents.ListAddString(name);
							SavePrefRecents(Recents);
						}

						try
						{
							pendingViews = new Dictionary<Guid, TextView>();
							eventProxy = new MessageProxy();
							localRegistration = new MSGCompletedEventHandler(Processor_MSGEvent);
							eventProxy.MsgCompleted += localRegistration;

							ChildCount = 0;
							wSClient = new NoxWSClient(eventProxy, peer);
							string authReq = sharedSession.GetAuthRequest(wSClient.Peer());
							await wSClient.StartWS(authReq);
						}
						catch (Exception)
						{
							wSClient = null;

							ToggleView(ActiveView.Error, true, "error starting message service");
						}
					}
				}

			}
			else
			{
				if (GettingMore) return;

				var usermsg = GetUserMessage();

				if (usermsg.Value.Length < 1) return;

				_activity.RunOnUiThread(
				new Action(() =>
				{
					Reset(false);
				}));

				{
					string sessionmsg = sharedSession.CreateMessage(usermsg, wSClient.Peer());
					await wSClient.Send(sessionmsg);
				}
			}

		}

		private void EnterEvent(string name = null)
		{

			if (EnterTask != null && !EnterTask.IsCompleted) return;

			EnterTask = Task.Run(async () =>
			{
				switch (activeView)
				{
					case ActiveView.UpdateNode:
						await NodeEnterEvent();
						break;

					case ActiveView.RegistrationManager:
						await RegisterEnterEvent();
						break;

					case ActiveView.Main:
					case ActiveView.Message:
						await MessageEnterEvent(name);
						break;
				}

			});
		}
		enum ActiveView
		{
			Main = 0,
			UpdateNode = 1,
			UpdateNodeChecking = 2,
			UpdateNodeError = 3,
			Message = 4,
			MessageCheckingName = 5,
			MessageNameError = 6,
			MessageStarting = 7,
			RegistrationManager = 8,
			Validation = 9,
			Error = 10
		}
		private void ActionToggleView(ActiveView toggleView, string Msg)
		{

			ScrollReachedBottom = null;
			GettingMore = false;

			switch (toggleView)
			{

				case ActiveView.Main:
					Reset(true);
					AddInstructionView("enter registered name of peer:");
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.Main;
					break;

				case ActiveView.MessageCheckingName:
					Reset(true);
					AddInstructionView("requesting contract, please wait...");
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.Message;
					break;

				case ActiveView.MessageNameError:
					Reset(true);
					if (!string.IsNullOrEmpty(Msg))
					{
						AddInstructionView(Msg);
					}
					else
					{
						AddInstructionView("name not found, please try again:");
					}
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.Message;
					break;

				case ActiveView.MessageStarting:
					Reset(true);
					var tvs = _activity.FindViewById<TextView>(Resource.Id.sessiontglStatus);
					tvs.Text = "STARTING CHANNEL";
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.Message;
					break;

				case ActiveView.UpdateNode:
					Reset(true);
					AddInstructionView("enter new host, currently using: " + Node);
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.UpdateNode;
					break;

				case ActiveView.UpdateNodeChecking:
					Reset(true);
					AddInstructionView("validating node, please wait...");
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.UpdateNode;
					break;

				case ActiveView.UpdateNodeError:
					Reset(true);
					AddInstructionView("invalid or unresponsive node, please try again:");
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.UpdateNode;
					break;

				case ActiveView.RegistrationManager:
					Reset(true);
					AddInstructionView(Msg);
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Visible;
					activeView = ActiveView.RegistrationManager;
					break;

				case ActiveView.Validation:
					Reset(true);
					AddInstructionView(Msg);
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Gone;
					activeView = ActiveView.Validation;
					break;

				case ActiveView.Error:
					Reset(true);
					AddInstructionView(Msg);
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtn).Visibility = ViewStates.Gone;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose).Visibility = ViewStates.Visible;
					_activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo).Visibility = ViewStates.Visible;
					activeView = ActiveView.Error;
					break;
			}

		}

		private void BtnInfo()
		{
			Task.Run(async () =>
			{

				try
				{
					ManagerRequest managerRequest = new ManagerRequest();
					managerRequest.requestType = ManagerRequestType.Info;
					var status = await sharedSession.PostAsync(managerRequest);

					if (!status.success)
					{
						return;
					}
					else
					{
						var nxact = new Intent(_activity.ApplicationContext, typeof(PageActivity));
						nxact.PutExtra("pmtask", status.infoResponse.Article);
						nxact.PutExtra("title", status.infoResponse.Title);

						nxact.AddFlags(ActivityFlags.NoAnimation);
						_activity.ApplicationContext.StartActivity(nxact);
						sharedSession.Enabled = false;

					}
				}
				catch { }
			});

		}

		public void LoadPrefNode()
		{
			try
			{
				string key = "node_" + sharedSession.MyPublicKey().ToHex();
				string val = GetPref(key, "");
				if (string.IsNullOrEmpty(val))
				{
					Node = DefaultNode;
				}
				else
				{
					Node = val;
				}
			}
			catch
			{
				Node = DefaultNode;
			}
		}
		public void SavePrefNode(string NewNode)
		{
			try
			{
				string key = "node_" + sharedSession.MyPublicKey().ToHex();
				UpdatePref(key, NewNode);
			}
			catch { }
		}
		public string[] LoadPrefRecents()
		{
			try
			{
				string key = "recents_" + sharedSession.MyPublicKey().ToHex();
				string val = GetPref(key, "");
				if (!string.IsNullOrEmpty(val))
				{
					val = sharedSession.GetDecryptedPref(val);
					if (string.IsNullOrEmpty(val)) return null;
					string[] vals = JsonConvert.DeserializeObject<string[]>(val);
					if (vals == null) vals = new string[0];
					return vals;
				}
			}
			catch { }

			return new string[0];
		}
		public void SavePrefRecents(string[] _recents)
		{
			try
			{
				string key = "recents_" + sharedSession.MyPublicKey().ToHex();
				string val = JsonConvert.SerializeObject(_recents);
				val = sharedSession.GetEncryptedPref(val);

				if (!string.IsNullOrEmpty(val))
				{
					UpdatePref(key, val);
				}
			}
			catch { }
		}
		public string GetPref(string Key, string DefaultVal)
		{

			ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(_activity.ApplicationContext);
			return sharedPref.GetString(Key, DefaultVal);
		}
		public void UpdatePref(string Key, string Value)
		{
			ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(_activity.ApplicationContext);

			var existing = sharedPref.GetString(Key, "");

			var esharedPrefe = sharedPref.Edit();

			if (!string.IsNullOrEmpty(existing))
			{
				esharedPrefe.Remove(Key);
			}

			esharedPrefe.PutString(Key, Value);
			esharedPrefe.Commit();

		}

		private void BtnRecents()
		{
			if (!_activity.HasWindowFocus) return;
			if (Recents.Length == 0)
			{
				NoRecents();
				return;
			}

			Recents = Recents.ListDistinct();

			NoxDlgModel.SheetData data = null;

			Action p_action1 = new Action(() =>
			{
				if (data != null && data.Selected > -1)
				{
					EnterEvent(data.Items[data.Selected]);
				}
			});

			noxDlgModel.ShowSimplesDlg(Recents, -1, "recent peers", "CONNECT", "BACK", p_action1, out data);

		}

		private void NoRecents()
		{
			AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity).SetMessage("There are no recents to display. Peers with whom you connect will be added to this list.").SetCancelable(true)
		.SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null);
			var wfalert = wfbuilder.Create();

			try
			{
				wfalert.Show();

				var wfokBtn = wfalert.GetButton((int)DialogButtonType.Positive);
				wfokBtn.Click += (asender, args) =>
				{
					wfalert.Dismiss();
				};
			}
			catch { }
		}


		//INFLATE VIEW
		private void AddInstructionView(string msg)
		{
			LayoutInflater vi = (LayoutInflater)_activity.ApplicationContext.GetSystemService(Context.LayoutInflaterService);
			View convertView = vi.Inflate(Resource.Layout.overview, null);

			TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
			tv1.Text = msg;
			var sv = _activity.FindViewById<LinearLayout>(Resource.Id.sessionmsgLayoutSCROLLER_ID);
			sv.AddView(convertView, 0, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent));
		}

		private void AddMsgViewBtm(string msg, int start)
		{
			LayoutInflater vi = (LayoutInflater)_activity.ApplicationContext.GetSystemService(Context.LayoutInflaterService);
			View convertView = vi.Inflate(Resource.Layout.overview, null);

			msg = msg.ToLower().Replace(".", "");
			TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
			tv1.Text = DateTime.Now.ToShortTimeString();
			tv1.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
			if (!string.IsNullOrEmpty(msg))
			{
				TextView tv3 = convertView.FindViewById<TextView>(Resource.Id.ovt3);
				tv3.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);
				tv3.Text = "|> " + msg;
			}

			var sv = _activity.FindViewById<LinearLayout>(Resource.Id.sessionmsgLayoutSCROLLER_ID);
			sv.AddView(convertView, start, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent));
		}
		private void AddMsgView(string msg)
		{
			LayoutInflater vi = (LayoutInflater)_activity.ApplicationContext.GetSystemService(Context.LayoutInflaterService);
			View convertView = vi.Inflate(Resource.Layout.overview, null);

			msg = msg.ToLower().Replace(".", "");
			TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.ovt1);
			tv1.Text = DateTime.Now.ToShortTimeString();
			tv1.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
			if (!string.IsNullOrEmpty(msg))
			{
				TextView tv3 = convertView.FindViewById<TextView>(Resource.Id.ovt3);
				tv3.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);
				tv3.Text = "|> " + msg;
			}
			var sv = _activity.FindViewById<LinearLayout>(Resource.Id.sessionmsgLayoutSCROLLER_ID);
			sv.AddView(convertView, 0, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent));
		}
		public override void NoxUpdateView(byte[] inputs)
		{
			LinearLayout box = (LinearLayout)_activity.FindViewById(Resource.Id.session_message);
			box.RemoveAllViews();
			int cnt = 0; int pos = 1;
			int gg = GetActualLength(inputs) - 1;

			for (int i = gg; i >= 0; i--)
			{
				if (cnt < 30)
				{
					ImageView imageView = new ImageView(_activity.ApplicationContext);
					imageView.SetImageBitmap(Sclear.GetKeyDictionary(inputs[i]));
					box.AddView(imageView, 0);
					cnt = cnt + 1;
				}
				else
				{
					break;
				}
			}
		}

		int LinearColor = 0;
		LinearLayout svrt;
		private void AddParagraph(MessageResponse messageResponse)
		{
			LinearLayout outer_layout = new LinearLayout(_activity.ApplicationContext);
			outer_layout.Orientation = Orientation.Vertical;
			outer_layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

			List<LinearLayout> il = new List<LinearLayout>();

			foreach (NoxLines lines in messageResponse.peerMessage)
			{
				int lc = lines.GetLineCount();
				LinearLayout inner_layout = new LinearLayout(_activity.ApplicationContext);
				inner_layout.Orientation = Orientation.Horizontal;
				inner_layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

				for (int i = 0; i < lc; i++)
				{
					int ocnt = 0;

					var gg = lines.GetLine(i);

					for (int v = 0; v < gg.Length; v++)
					{
						ImageView imageView = new ImageView(_activity.ApplicationContext);
						var bmp = gg[v];//Sclear.GetKeyDictionary(gg[v]);
						imageView.SetImageBitmap((Bitmap)bmp);
						inner_layout.AddView(imageView, 0);
					}
				}

				il.Add(inner_layout);

			}

			foreach (var lyt in il)
			{
				outer_layout.AddView(lyt, 0);
			}

			LayoutInflater vi = (LayoutInflater)_activity.ApplicationContext.GetSystemService(Context.LayoutInflaterService);
			View convertView = vi.Inflate(Resource.Layout.sessionLP, null);

			TextView tv1 = convertView.FindViewById<TextView>(Resource.Id.sessionovt1lp);
			tv1.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);

			string dt = messageResponse.dateTime.ToLocalTime().ToString("g", DateTimeFormatInfo.InvariantInfo);

			if (messageResponse.responseType == SharedResponseType.Peer)
			{
				tv1.Text = dt + " [PEER]";

				if (messageResponse.isHistory)
				{
					tv1.SetTypeface(tv1.Typeface, TypefaceStyle.Normal);
				}
				else
				{
					tv1.SetTypeface(tv1.Typeface, TypefaceStyle.Bold);
					tv1.SetTextColor(Color.DarkCyan);
				}
			}

			if (messageResponse.responseType == SharedResponseType.User)
			{
				tv1.Text = dt + " DLVRD";

				if (LinearColor == 0) LinearColor = tv1.CurrentTextColor;

				if (!messageResponse.offLine)
				{
					tv1.Text = dt + " [SENT]";
					pendingViews[messageResponse.MessageID] = tv1;
					tv1.SetTypeface(tv1.Typeface, TypefaceStyle.Bold);
					tv1.SetTextColor(Color.SlateBlue); //TEAL
				}
			}

			LinearLayout tv3 = convertView.FindViewById<LinearLayout>(Resource.Id.sessionovt3lp);
			tv3.AddView(outer_layout, 0, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent));

			if (svrt == null)
			{
				svrt = _activity.FindViewById<LinearLayout>(Resource.Id.sessionmsgLayoutSCROLLER_ID);
			}

			int stackPos = 0;
			if (messageResponse.BottomStack)
			{
				stackPos = ChildCount; ////svrt.ChildCount;
			}

			svrt.AddView(convertView, stackPos, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent));
			ChildCount++;
		}


		public void CreateLayout()
		{
			LoadBtns();
			LoadInpViews();
			v = (Vibrator)_activity.GetSystemService("vibrator");

			ltheight = _activity.FindViewById<LinearLayout>(Resource.Id.sessionllMessages).LayoutParameters.Height;
			Button btnHide = _activity.FindViewById<Button>(Resource.Id.sessiontglBtn);
			btnHide.Click += delegate { TglClick(); };

			Button bvRecents = _activity.FindViewById<Button>(Resource.Id.sessiontglBtnRecents);
			bvRecents.Click += delegate { BtnRecents(); };

			Button bvBack = _activity.FindViewById<Button>(Resource.Id.sessiontglBtnBack);
			bvBack.Click += delegate { ToggleView(ActiveView.Main, false); };

			Button bvNode = _activity.FindViewById<Button>(Resource.Id.sessiontglBtnNode);
			bvNode.Click += delegate { ToggleView(ActiveView.UpdateNode, false); };

			Button bvClose = _activity.FindViewById<Button>(Resource.Id.sessiontglBtnClose);
			//	bvClose.Click += delegate { Close(); };
			bvClose.Click += delegate { HandleBP(); };
			bvClose.Text = "BACK";

			Button bvInfo = _activity.FindViewById<Button>(Resource.Id.sessiontglBtnInfo);
			bvInfo.Click += delegate { BtnInfo(); };

			ScrollView scrollView = _activity.FindViewById<ScrollView>(Resource.Id.sessionmsgSCROLLER_ID);
			scrollView.ScrollChange += ScrollViewScrolled;
		}

		bool KbLyt = true;
		int ltheight;
		private void TglClick()
		{
			LinearLayout lyt = _activity.FindViewById<LinearLayout>(Resource.Id.sessionKB);
			Button btnHide = _activity.FindViewById<Button>(Resource.Id.sessiontglBtn);

			LinearLayout lytmn = _activity.FindViewById<LinearLayout>(Resource.Id.sessionllMessages);

			if (KbLyt)
			{
				lyt.Visibility = ViewStates.Gone;
				btnHide.Text = "SHOW";
				KbLyt = false;
				lytmn.LayoutParameters.Height = ltheight * 2;
				return;
			}

			lyt.Visibility = ViewStates.Visible;
			btnHide.Text = "HIDE";
			lytmn.LayoutParameters.Height = ltheight;
			KbLyt = true;
		}

		private void CloseMsg()
		{

			if (!_activity.HasWindowFocus) return;
			AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity).SetTitle("").SetMessage("This session has ended.").SetCancelable(false).SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null);
			var wfalert = wfbuilder.Create();

			try
			{
				wfalert.Show();
				var wfokBtn = wfalert.GetButton((int)DialogButtonType.Positive);
				wfokBtn.Click += (asender, args) =>
				{
					wfalert.Dismiss();
					CloseSession();
				};
			}
			catch { }
		}

		public void PromptMsg()
		{

			if (!_activity.HasWindowFocus) return;

			string msg = "Close Bitfi messaging now?"; 

			AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity).SetTitle("").SetMessage(msg).SetCancelable(false).SetPositiveButton("Yes", (EventHandler<DialogClickEventArgs>)null).SetNegativeButton("No", (EventHandler<DialogClickEventArgs>)null);
			var wfalert = wfbuilder.Create();

			try
			{
				wfalert.Show();
				var wfokBtn = wfalert.GetButton((int)DialogButtonType.Positive);

				wfokBtn.Click += (asender, args) =>
				{
					wfalert.Dismiss();
					Closing = true;
					Close();

				};

				var wfnoBtn = wfalert.GetButton((int)DialogButtonType.Negative);

				wfnoBtn.Click += (asender, args) =>
				{
					wfalert.Dismiss();

				};
			}
			catch { }
		}


		private void ResetTags()
		{
			mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25; mButton0.Tag = 52; mButton1.Tag = 53; mButton2.Tag = 54; mButton3.Tag = 55; mButton4.Tag = 56; mButton5.Tag = 57; mButton6.Tag = 58; mButton7.Tag = 59; mButton8.Tag = 60; mButton9.Tag = 61;
			mButtonShift.Text = "^";
			mButtonCaps.Text = "CAPS";
			capson = false;
			foreach (Button b in buttonLIst)
			{
				b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
			}
		}

		List<Button> buttonLIst; bool capson;

		public AlertDialog bwalert; Vibrator v;

		Button mButton1; Button mButton2; Button mButton3; Button mButton4; Button mButton5; Button mButton6; Button mButton7; Button mButton8; Button mButton9; Button mButton0; Button mButtonA; Button mButtonB; Button mButtonC; Button mButtonD; Button mButtonE; Button mButtonF; Button mButtonG; Button mButtonH; Button mButtonI; Button mButtonJ; Button mButtonK; Button mButtonL; Button mButtonM; Button mButtonN; Button mButtonO; Button mButtonP; Button mButtonQ; Button mButtonR; Button mButtonS; Button mButtonT; Button mButtonU; Button mButtonV; Button mButtonW; Button mButtonX; Button mButtonY; Button mButtonZ; Button mButtonDelete;

		Button mButtonEnter; Button mButtonSpace; Button mButtonShift; Button mButtonCaps;
		private void LoadBtns()
		{
			buttonLIst = new List<Button>();

			mButton1 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_1); mButton2 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_2); mButton3 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_3); mButton4 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_4); mButton5 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_5); mButton6 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_6); mButton7 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_7); mButton8 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_8); mButton9 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_9); mButton0 = (Button)_activity.FindViewById(Resource.Id.sessionbutton_0); mButtonA = (Button)_activity.FindViewById(Resource.Id.sessionbutton_A); mButtonB = (Button)_activity.FindViewById(Resource.Id.sessionbutton_B); mButtonC = (Button)_activity.FindViewById(Resource.Id.sessionbutton_C); mButtonD = (Button)_activity.FindViewById(Resource.Id.sessionbutton_D); mButtonE = (Button)_activity.FindViewById(Resource.Id.sessionbutton_E); mButtonF = (Button)_activity.FindViewById(Resource.Id.sessionbutton_F); mButtonG = (Button)_activity.FindViewById(Resource.Id.sessionbutton_G); mButtonH = (Button)_activity.FindViewById(Resource.Id.sessionbutton_H); mButtonI = (Button)_activity.FindViewById(Resource.Id.sessionbutton_I); mButtonJ = (Button)_activity.FindViewById(Resource.Id.sessionbutton_J); mButtonK = (Button)_activity.FindViewById(Resource.Id.sessionbutton_K); mButtonL = (Button)_activity.FindViewById(Resource.Id.sessionbutton_L); mButtonM = (Button)_activity.FindViewById(Resource.Id.sessionbutton_M); mButtonN = (Button)_activity.FindViewById(Resource.Id.sessionbutton_N); mButtonO = (Button)_activity.FindViewById(Resource.Id.sessionbutton_O); mButtonP = (Button)_activity.FindViewById(Resource.Id.sessionbutton_P); mButtonQ = (Button)_activity.FindViewById(Resource.Id.sessionbutton_Q); mButtonR = (Button)_activity.FindViewById(Resource.Id.sessionbutton_R); mButtonS = (Button)_activity.FindViewById(Resource.Id.sessionbutton_S); mButtonT = (Button)_activity.FindViewById(Resource.Id.sessionbutton_T); mButtonU = (Button)_activity.FindViewById(Resource.Id.sessionbutton_U); mButtonV = (Button)_activity.FindViewById(Resource.Id.sessionbutton_V); mButtonW = (Button)_activity.FindViewById(Resource.Id.sessionbutton_W); mButtonX = (Button)_activity.FindViewById(Resource.Id.sessionbutton_X); mButtonY = (Button)_activity.FindViewById(Resource.Id.sessionbutton_Y); mButtonZ = (Button)_activity.FindViewById(Resource.Id.sessionbutton_Z);
			buttonLIst.Add(mButton0); buttonLIst.Add(mButton1); buttonLIst.Add(mButton2); buttonLIst.Add(mButton3); buttonLIst.Add(mButton4); buttonLIst.Add(mButton5); buttonLIst.Add(mButton6); buttonLIst.Add(mButton7); buttonLIst.Add(mButton8); buttonLIst.Add(mButton9); buttonLIst.Add(mButtonA); buttonLIst.Add(mButtonB); buttonLIst.Add(mButtonC); buttonLIst.Add(mButtonD); buttonLIst.Add(mButtonE); buttonLIst.Add(mButtonF); buttonLIst.Add(mButtonG); buttonLIst.Add(mButtonH); buttonLIst.Add(mButtonI); buttonLIst.Add(mButtonJ); buttonLIst.Add(mButtonK); buttonLIst.Add(mButtonL); buttonLIst.Add(mButtonM); buttonLIst.Add(mButtonN); buttonLIst.Add(mButtonO); buttonLIst.Add(mButtonP); buttonLIst.Add(mButtonQ); buttonLIst.Add(mButtonR); buttonLIst.Add(mButtonS); buttonLIst.Add(mButtonT); buttonLIst.Add(mButtonU); buttonLIst.Add(mButtonV); buttonLIst.Add(mButtonW); buttonLIst.Add(mButtonX); buttonLIst.Add(mButtonY); buttonLIst.Add(mButtonZ);
			mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25; mButton0.Tag = 52; mButton1.Tag = 53; mButton2.Tag = 54; mButton3.Tag = 55; mButton4.Tag = 56; mButton5.Tag = 57; mButton6.Tag = 58; mButton7.Tag = 59; mButton8.Tag = 60; mButton9.Tag = 61;

			mButtonEnter = (Button)_activity.FindViewById(Resource.Id.sessionbutton_enter);
			mButtonEnter.Tag = 1005;
			mButtonEnter.Click += BtnEnterClick;

			mButtonSpace = (Button)_activity.FindViewById(Resource.Id.sessionbutton_SPACE);
			mButtonSpace.Tag = 88;
			mButtonSpace.Click += SpaceAddClick;

			mButtonDelete = (Button)_activity.FindViewById(Resource.Id.sessionbutton_delete);
			mButtonDelete.Tag = 1002;
			mButtonDelete.Click += DelClick;

			mButtonShift = (Button)_activity.FindViewById(Resource.Id.sessionbutton_SHIFT);
			mButtonShift.Tag = 1001;
			mButtonShift.Click += ShiftModClick;

			mButtonCaps = (Button)_activity.FindViewById(Resource.Id.sessionbutton_CAPS);
			mButtonCaps.Tag = 1000;
			mButtonCaps.Click += CapsModClick;


			foreach (Button b in buttonLIst)
			{
				b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
				b.Click += BtnClick;
			}

		}
		private void LoadInpViews()
		{
			LayoutInflater inflater = (LayoutInflater)_activity.GetSystemService(Context.LayoutInflaterService);
			View dialoglayout = inflater.Inflate(Resource.Layout.splashmodel, null);

			AlertDialog.Builder builder = new AlertDialog.Builder(_activity, Resource.Style.FullscreenTheme);
			builder.SetCancelable(false);
			builder.SetView(dialoglayout);
			bwalert = builder.Create();

		}
		private void SpaceAddClick(object sender, System.EventArgs e)
		{
			var btn = (Button)sender;
			NoxAdd(Sclear.GetConsoleDictionaryFromIndex((int)btn.Tag));
			Vibe();

		}
		void DelClick(object sender, System.EventArgs e)
		{
			NoxRemove();
		}
		private void CapsModClick(object sender, System.EventArgs e)
		{

			mButtonShift.Text = "^";

			if (mButtonCaps.Text == "CAPS")
			{
				mButtonCaps.Text = "SCAP";
				capson = true;

				mButtonA.Tag = 26; mButtonB.Tag = 27; mButtonC.Tag = 28; mButtonD.Tag = 29; mButtonE.Tag = 30; mButtonF.Tag = 31; mButtonG.Tag = 32; mButtonH.Tag = 33; mButtonI.Tag = 34; mButtonJ.Tag = 35; mButtonK.Tag = 36; mButtonL.Tag = 37; mButtonM.Tag = 38; mButtonN.Tag = 39; mButtonO.Tag = 40; mButtonP.Tag = 41; mButtonQ.Tag = 42; mButtonR.Tag = 43; mButtonS.Tag = 44; mButtonT.Tag = 45; mButtonU.Tag = 46; mButtonV.Tag = 47; mButtonW.Tag = 48; mButtonX.Tag = 49; mButtonY.Tag = 50; mButtonZ.Tag = 51;
			}
			else
			{

				mButtonCaps.Text = "CAPS";
				capson = false;

				mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25;
			}

			foreach (Button b in buttonLIst)
			{
				b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);
			}
		}

		private void ShiftModClick(object sender, System.EventArgs e)
		{
			if (mButtonShift.Text == "^")
			{
				mButtonShift.Text = "<";

				mButtonA.Tag = 62; mButtonB.Tag = 63; mButtonC.Tag = 64; mButtonD.Tag = 65; mButtonE.Tag = 66; mButtonF.Tag = 67; mButtonG.Tag = 68; mButtonH.Tag = 69; mButtonI.Tag = 70; mButtonJ.Tag = 71; mButtonK.Tag = 72; mButtonL.Tag = 73; mButtonM.Tag = 74; mButtonN.Tag = 75; mButtonO.Tag = 76; mButtonP.Tag = 77; mButtonQ.Tag = 78; mButtonR.Tag = 79; mButtonS.Tag = 80; mButtonT.Tag = 81; mButtonU.Tag = 82; mButtonV.Tag = 83; mButtonW.Tag = 84; mButtonX.Tag = 85; mButtonY.Tag = 86; mButtonZ.Tag = 87;

			}
			else
			{
				mButtonShift.Text = "^";

				if (capson)
				{
					mButtonA.Tag = 26; mButtonB.Tag = 27; mButtonC.Tag = 28; mButtonD.Tag = 29; mButtonE.Tag = 30; mButtonF.Tag = 31; mButtonG.Tag = 32; mButtonH.Tag = 33; mButtonI.Tag = 34; mButtonJ.Tag = 35; mButtonK.Tag = 36; mButtonL.Tag = 37; mButtonM.Tag = 38; mButtonN.Tag = 39; mButtonO.Tag = 40; mButtonP.Tag = 41; mButtonQ.Tag = 42; mButtonR.Tag = 43; mButtonS.Tag = 44; mButtonT.Tag = 45; mButtonU.Tag = 46; mButtonV.Tag = 47; mButtonW.Tag = 48; mButtonX.Tag = 49; mButtonY.Tag = 50; mButtonZ.Tag = 51;
				}
				else
				{
					mButtonA.Tag = 0; mButtonB.Tag = 1; mButtonC.Tag = 2; mButtonD.Tag = 3; mButtonE.Tag = 4; mButtonF.Tag = 5; mButtonG.Tag = 6; mButtonH.Tag = 7; mButtonI.Tag = 8; mButtonJ.Tag = 9; mButtonK.Tag = 10; mButtonL.Tag = 11; mButtonM.Tag = 12; mButtonN.Tag = 13; mButtonO.Tag = 14; mButtonP.Tag = 15; mButtonQ.Tag = 16; mButtonR.Tag = 17; mButtonS.Tag = 18; mButtonT.Tag = 19; mButtonU.Tag = 20; mButtonV.Tag = 21; mButtonW.Tag = 22; mButtonX.Tag = 23; mButtonY.Tag = 24; mButtonZ.Tag = 25;
				}
			}


			foreach (Button b in buttonLIst)
			{
				b.SetText(new char[] { Sclear.GetCharFromIndex((int)b.Tag) }, 0, 1);

			}
		}

		private void BtnClick(object sender, System.EventArgs e)
		{
			var b = (Button)sender;

			if ((int)b.Tag < 52)
			{
				if (capson)
				{
					if ((int)b.Tag < 26) b.Tag = (int)b.Tag + 26;

				}
				else
				{
					if ((int)b.Tag > 25) b.Tag = (int)b.Tag - 26;

				}
			}

			NoxAdd(Sclear.GetConsoleDictionaryFromIndex((int)b.Tag));
			Vibe();
		}
		private void BtnEnterClick(object sender, System.EventArgs e)
		{
			EnterEvent();

		}

		private void Vibe()
		{
			v.Vibrate(VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude));
		}

	}



}