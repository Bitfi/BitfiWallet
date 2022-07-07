using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using WalletLibrary.ActiveApe.ApeShift;
using WalletLibrary.ActiveApe;
using WalletLibrary.Core.Concrete;
using System.Net.Http;
using System.Collections.Concurrent;

namespace BitfiWallet
{
	public class NoxChannel
	{

		public ActiveProcessor _ActiveProcessor { get; private set; }
		public ApeSession _APESession { get; private set; }
		public ConcurrentDictionary<string, decimal> _RateSpot;

		private object _SignalLock = new object();

		public string APEAPI_SECRET;
		public string APEAPI_PUBLIC_ID;
		public string APE_CONNECTION_STRING;
		public string MY_CHANNEL_PUBLIC_KEY;

		public static NoxChannel Current { get; private set; }

		public NoxChannel()
		{
			if (Current != null)
				return;

			Current = this;

			_RateSpot = new ConcurrentDictionary<string, decimal>();
			_ActiveProcessor = new ActiveProcessor();
			_APESession = new ApeSession();
			des_items = new List<SatusListItem>();

		}

		public void SetActiveWallet(WalletActiveFactory walletFactory, string Token)
		{
			_ActiveProcessor.Init(walletFactory, Token);
		}

		public List<SatusListItem> des_items;


		public void StartActivity(MetaActivity context, out CancellationTokenSource cancellation)
		{
			CancellationTokenSource cancellationToken = new CancellationTokenSource();
			cancellation = cancellationToken;


			var work = new RokitWorkDelegate(() =>
			{
				return DoWork(context, cancellationToken).Result;

			});

			Task.Run(async () =>
			{

				var thread = await work.ToRokit(86400, cancellationToken, false);

			});
		}

		async Task<bool> DoWork(MetaActivity context, CancellationTokenSource cancellationToken)
		{
			bool completed = false;

			try
			{
				completed = await Work(context, cancellationToken)
					.SetWaitCancellation(cancellationToken.Token);
			}
			catch (OperationCanceledException)
			{
				_APESession.CancelTasks();
				await _APESession.DisposeEnvoyEvent();
			}
			catch (Exception)
			{

			}
			finally
			{
				_ActiveProcessor.Dispose();
			}

			return completed;
		}

		async Task<bool> Work(MetaActivity context, CancellationTokenSource token)
		{
			Task.Run(async () =>
			{
				try
				{
					await GetCoinbase(token).SetWaitCancellation(token.Token);
				}
				catch (Exception) { }
			});

			await _APESession.StartMQ(token);
			return true;
		}

		private async Task<bool> GetCoinbase(CancellationTokenSource cancellationToken)
		{

			using (var client = new System.Net.Http.HttpClient())
			{

				while (!cancellationToken.IsCancellationRequested)
				{

					await Task.Delay(TimeSpan.FromSeconds(15));

					if (_ActiveProcessor.IsDisposed)
						break;

					try
					{
						var response = await client.GetAsync("https://api.coinbase.com/v2/prices/USD/spot");
						var responseString = await response.Content.ReadAsStringAsync();

						var coinbase = Newtonsoft.Json.JsonConvert.DeserializeObject<CoinbaseData>(responseString);

						foreach (var rate in coinbase.Data)
						{
							_RateSpot[rate.Base] = Convert.ToDecimal(rate.Amount);
						}

					}
					catch (HttpRequestException)
					{

					}
				}
			}

			return true;
		}

	}


}