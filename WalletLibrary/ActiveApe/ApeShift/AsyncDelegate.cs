using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WalletLibrary.ActiveApe.ApeShift
{
	internal class AsyncDelegate
	{

		private MQDiffieResult _SendToApePubKeyQ;

		private ApeSession _Session;

		private WalletRequest<object> _BroadcastRequest;
		private wallet_methods _method;
		private string _ContextToken { get; set; }
		private event BroadcastResponseCompletedEventHandler TaskCompleted;

		public AsyncDelegate(ApeSession ApeSession, WalletRequest<object> BroadcastRequest,
			MQDiffieResult sender, wallet_methods method)
		{
			_method = method;
			_ContextToken = sender.MQContext;
			_Session = ApeSession;
			_SendToApePubKeyQ = sender;
			_BroadcastRequest = BroadcastRequest;
		}

		void Processor_Exp(BroadcastResponseCompletedEventArgs Resp)
		{
			if (Resp.Sender.MQContext == _ContextToken)
				TaskCompleted.Invoke(Resp);
		}

		public async Task<BroadcastResponseCompletedEventArgs> RequestAsync(CancellationToken token)
		{
				var tcs = new TaskCompletionSource<BroadcastResponseCompletedEventArgs>();
				TaskCompleted += (e) => CompletedEvent(tcs, e, () => e);

				token.Register(() =>
				{
					tcs.TrySetCanceled();

				});

				_Session._OnResponse += Processor_Exp;

				_Session.RequestArrived(_BroadcastRequest, _SendToApePubKeyQ, _method);

				var task = await tcs.Task;

				_Session._OnResponse -= Processor_Exp;

				return task;
			
		}

		void CompletedEvent<T>(TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult)
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

	}
}
