using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace BitfiWallet
{
 public enum ProcessorMode
 {
  Single,
  Continuous
 }

 public delegate void WorkThreadDelegate();

 public abstract class WorkProcessor : IWorkProcessor
 {

  private Thread _WorkThread = null;
  private EventWaitHandle _WorkSignal = null;
  private object _SignalLock = new object();

  private object _ProcessData;

  private ProcessorMode _ProcessorMode;

  public string ProcessorName { get; private set; }

  public WorkThreadDelegate ThisWork;

  public WorkProcessor()
  {
   _WorkThread = new Thread(new ThreadStart(ThisWork));
   _WorkThread.SetApartmentState(ApartmentState.MTA);
   _WorkThread.Name = "ROKITS";
   this.ProcessorName = "ROKITS";
   _ProcessorMode = ProcessorMode.Single;
  }

  public virtual void Start()
  {

   _WorkSignal = new EventWaitHandle(false, EventResetMode.AutoReset);
   _WorkThread.Start();
  }

  public void Process()
  {
   this.SignalWork();
  }

  public void Process<T>(T processData)
  {
   _ProcessData = processData;
   this.SignalWork();
  }

  protected void SignalWork()
  {
   lock (_SignalLock)
   {
    if (_WorkSignal == null)
    {
     return;
    }
    _WorkSignal.Set();


   }
  }

  protected void WorkWait()
  {
   _WorkSignal.WaitOne();

  }

  protected void WorkWait<T>(out T processData)
  {
   this.WorkWait();

   processData = (T)_ProcessData;
   _ProcessData = null;
  }

 }

 interface IWorkProcessor
 {

  void Start();

 }
}
