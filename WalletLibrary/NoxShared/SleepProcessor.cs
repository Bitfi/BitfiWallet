using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WalletLibrary.NoxShared
{
  public delegate void MSGCollectionCompletedEventHandler(MSGCollectionCompletedEventArgs e);
  public class AlertProxy
  {
    public event MSGCollectionCompletedEventHandler MsgCompleted;
    public void LocallyHandleMessageArrived(SleepResponse msg)
    {
      try
      {
        //  Interlocked.CompareExchange(ref MsgCompleted, null, null)?.Invoke(new MSGCollectionCompletedEventArgs(msg));
        if (MsgCompleted == null) return;
        MsgCompleted.Invoke(new MSGCollectionCompletedEventArgs(msg));
      }
      catch { }
    }
  }
  public partial class MSGCollectionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
  {
    public MSGCollectionCompletedEventArgs(SleepResponse result) : base(null, false, null) { Messages = result; }
    public SleepResponse Messages { get; }
  }

  public class SleepResponse
  {
    public string[] PeerList { get; set; }
    public string[] PeerDisplayList { get; set; }
    public bool CloseActivity { get; set; }
  }

  public class SleepProcessor
  {
    public SleepProcessor(SharedSession sharedSession, AlertProxy alertProxy, TimeSpan SessionTimeout, TimeSpan RequestPause)
    {
      _sharedSession = sharedSession;
      _alertProxy = alertProxy;
      _SessionTimout = SessionTimeout;
      _RequestPause = RequestPause;
    }

    Task _WorkThread;
    CancellationTokenSource cancellation;

    SharedSession _sharedSession;
    AlertProxy _alertProxy;
    TimeSpan _SessionTimout;
    TimeSpan _RequestPause;

    public bool StopRequested;

    public async void Start()
    {
      if (_WorkThread != null && !_WorkThread.IsCompleted)
      {
        Stop();
        await Task.WhenAll(_WorkThread);
      }

      StopRequested = false;

      _WorkThread = Task.Run(async () =>
      {
        try
        {
          cancellation = new CancellationTokenSource(_SessionTimout);
          cancellation.Token.ThrowIfCancellationRequested();

          await _WSTicks();

        }
        catch { }

        if (!StopRequested)
        {
          Notification(new string[] { "CLOSE" });
        }
      });

    }
    public async void Stop()
    {
      if (cancellation != null && !cancellation.IsCancellationRequested)
      {
        StopRequested = true;
        cancellation.Cancel();
      }
    }
    async Task _WSTicks()
    {
      if (_sharedSession == null) return;

      while (!cancellation.IsCancellationRequested)
      {
        var resp = await _sharedSession.GetOfflineAlert();

        if (resp != null && resp.Length > 0)
        {
          Notification(resp);
        }

        await Task.Delay(_RequestPause);
      }
    }


    public string[] peerAlerts = new string[0];
    private void Notification(string[] msg)
    {
      try
      {
        if (msg == null) return;
        if (msg.Length == 0) return;

        SleepResponse sleepResponse = new SleepResponse();
       
        if (msg[0] == "CLOSE")
        {
            sleepResponse.CloseActivity = true; 
        }
        else
        {
          foreach(var item in msg)
          {
            peerAlerts = peerAlerts.ListAddString(item);
          }

          string[] displayNames = new string[0];
          string[] uniqueNames = peerAlerts.ListDistinct();

          foreach(var un in uniqueNames)
          {
            int count = peerAlerts.ListContainsCount(un);
            displayNames =  displayNames.ListAddString(" ( " + count + " ) " + un);
          }

          sleepResponse.PeerDisplayList = displayNames;
          sleepResponse.PeerList = uniqueNames;

        }

        _alertProxy.LocallyHandleMessageArrived(sleepResponse);
      }
      catch { }
    }

  }
}