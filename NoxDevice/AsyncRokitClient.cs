using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BitfiWallet
{
 public static class RokitTaskExtension
 {
  public async static Task<Response> ToRokit(this RokitWorkDelegate task, int Timeout,
   CancellationTokenSource cancellationTokenMA, bool CanAbort = true)
  {
   AsyncRokitClient rokitsClient = new AsyncRokitClient(new Request { Timeout = Timeout, Work = task }, CanAbort);
   return await rokitsClient.GetTask(cancellationTokenMA.Token);
  }

  public async static Task<T> SetWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
  {

   var tcs = new TaskCompletionSource<bool>();
   using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))

    if (task != await Task.WhenAny(task, tcs.Task))
     throw new OperationCanceledException(cancellationToken);

   return await task;
  }
 }

 public delegate object RokitWorkDelegate();

 class AsyncRokitClient
 {

  private delegate void RokitThreadDelegate();

  private Thread _WorkThread;

  RokitProxy eventProxy;
  TimeSpan Timeout;

  bool _CanAbort;

  public AsyncRokitClient(Request workRequest, bool CanAbort)
  {
   eventProxy = new RokitProxy();
   Timeout = TimeSpan.FromSeconds(workRequest.Timeout);
   _CanAbort = CanAbort;

   var work = new RokitThreadDelegate(() =>
   {
    var resp = workRequest.Work.Invoke();
    eventProxy.LocallyHandledObjectArrived(new Response() { Content = resp });

   });

   _WorkThread = new Thread(new ThreadStart(work));
   _WorkThread.SetApartmentState(ApartmentState.MTA);

  }

  private void Process()
  {
   _WorkThread.Start();
  }

  private void Dispose_EnsureDead()
  {

   if (!_CanAbort) return;

   try { if (_WorkThread.IsAlive) _WorkThread.Abort(); } catch { }
  }

  public async Task<Response> GetTask(CancellationToken cancellationTokenMA)
  {
   using (var cancellationTokenSource = new CancellationTokenSource(Timeout))
   {
    try
    {
     var result = await GetRokitsTask(cancellationTokenSource.Token, cancellationTokenMA);
     return result.RokitResult;
    }
    catch (TaskCanceledException)
    {

     Dispose_EnsureDead();

     if (cancellationTokenSource.IsCancellationRequested)
     {
      return new Response() { Error = new Error() { IsTimeout = true } };
     }

     return null;
    }
    catch (ThreadStartException)
    {
     return new Response() { Error = new Error() { IsThreadError = true } };
    }

   }
  }

  Task<RokitsCompletedEventArgs> GetRokitsTask(CancellationToken cancellationToken, CancellationToken cancellationTokenMA)
  {

   TaskCompletionSource<RokitsCompletedEventArgs> ntask = new TaskCompletionSource<RokitsCompletedEventArgs>();
   eventProxy.TaskCompleted += (e) => CompletedEvent(ntask, e, () => e);

   cancellationToken.Register(() =>
   {
    ntask.TrySetCanceled();
   });

   cancellationTokenMA.Register(() =>
   {
    ntask.TrySetCanceled();
   });

   Process();
   return ntask.Task;
  }

  void CompletedEvent<T>(TaskCompletionSource<T> tcs,
   System.ComponentModel.AsyncCompletedEventArgs e, Func<T> getResult)
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

  class RokitProxy
  {
   public event RokitsCompletedEventHandler TaskCompleted;
   public void LocallyHandledObjectArrived(Response result)
   {
    if (TaskCompleted != null)
    {
     TaskCompleted.Invoke(new RokitsCompletedEventArgs(result, null));
    }
   }
  }

  delegate void RokitsCompletedEventHandler(RokitsCompletedEventArgs e);
  partial class RokitsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
  {
   public RokitsCompletedEventArgs(Response result, object error) : base(null, false, null)
   {
    RokitResult = result;
   }
   public Response RokitResult { get; }

  }
 }

 public class Error
 {
  public bool IsTimeout { get; set; }
  public bool IsThreadError { get; set; }
  public string Message { get; set; }
 }

 public class Request
 {
  public RokitWorkDelegate Work { get; set; }
  public int Timeout { get; set; }
 }
 public class Response
 {
  public object Content { get; set; }
  public Error Error { get; set; }
 }

}
