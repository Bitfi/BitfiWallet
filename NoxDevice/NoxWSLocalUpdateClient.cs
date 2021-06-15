using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using WalletLibrary.NoxShared.WebSockets;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using WalletLibrary.NoxShared;
using System.IO;
using Android.OS;
using Android.Content.PM;
using Android.App;

namespace BitfiWallet
{

 public class NoxWSLocalUpdateClient
 {

  WebSocket webSocket;
  WebSocketClientFactory factory;

  CancellationToken _token;

  public event DataMSGCompletedEventHandler OnStatusChanged;

  public NoxWSLocalUpdateClient()
  {
   factory = new WebSocketClientFactory();
  }

  public async Task ProcessWSUpdate(string hostAddress, string hostPublicKey, CancellationToken token, PendingIntent pendingIntent)
  {
   _token = token;

   try
   {
    Notification("connecting...", false);

    webSocket = await factory.ConnectAsync(new Uri(hostAddress), token, hostPublicKey);

    Notification("requesting signature", false);
    await Send("SIGNATURE");

    string _signature = await GetMessageResponse();

    if (string.IsNullOrEmpty(_signature))
    {
     Notification("no signature", true);
     return;
    }

    int size = 0;

    try
    {
     Notification("requesting package size", false);
     await Send("SIZE");

     string size_string = await GetMessageResponse();
     size = Convert.ToInt32(size_string);

     if (size > 52428800)
     {
      Notification("Max size is 50mb", true);
      return;
     }
    }
    catch
    {
     Notification("missing or invalid size", true);
     return;
    }

    Notification("getting package...", false);
    await Send("PACKAGE");

    byte[] buff = await GetPackageResponse(size);

    Notification("validating...", false);

    string hash = GetHashString(buff);
    PackageInstaller.Session session = null;

    if (NoxDPM.NoxT.noxDevice.ValidUpdateSignature(hash, _signature))
    {
     Notification("ok, now installing...", false);

     WSClose();

     MemoryStream data = new MemoryStream(buff);
     NoxDPM.dpm.ClearUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);

     try
     {
      int SessionID;
      session = UpdateSession.CreateSession(NoxDPM.GetContext(), out SessionID);
      UpdateSession.WriteSession(session, data);

     }
     catch (Exception ex)
     {

      Notification("Install Failed: " + ex.Message, true);
      return;
     }
     finally
     {
      NoxDPM.dpm.AddUserRestriction(NoxDPM.deviceAdmin, UserManager.DisallowInstallApps);
     }

    }
    else
    {
     Notification("sorry, invalid signature", true);
     return;
    }

    try
    {
     UpdateSession.CommitSession(session, pendingIntent, NoxDPM.GetContext(), false);

    }
    catch (Exception ex)
    {
     Notification("Install Failed: " + ex.Message, true);
     return;
    }


   }
   catch (Exception ex)
   {
    Notification(ex.Message, true);

   }
  }

  private async Task<byte[]> GetPackageResponse(int size)
  {

   byte[] package = new byte[size];

   bool _eof = false;
   int _totalRead = 0;

   while (!_eof)
   {
    ArraySegment<byte> in_buffer = new ArraySegment<byte>(new byte[1024 * 400]);
    var resp = await webSocket.ReceiveAsync(in_buffer, _token);
    _eof = resp.EndOfMessage;

    if (resp.Count > 0)
    {
     Buffer.BlockCopy(in_buffer.Array, 0, package, _totalRead, resp.Count);

     _totalRead = _totalRead + resp.Count;

     if (_totalRead >= size)
     {
      break;
     }
     else
     {
      int progress = (_totalRead * 100 / size);
      if (progress > 0)
      {
       Notification(progress.ToString() + "% complete", false);
      }
     }
    }
   }

   if (package.Length != size)
    throw new Exception("Error receiving package");

   Notification("100% complete", false);

   return package;

  }

  private async Task<string> GetMessageResponse()
  {
   ArraySegment<byte> in_buffer = new ArraySegment<byte>(new byte[1024 * 500]);
   WebSocketReceiveResult result = await webSocket.ReceiveAsync(in_buffer, _token);
   return Encoding.UTF8.GetString(in_buffer.Array, 0, result.Count);
  }

  static string GetHashString(byte[] input)
  {
   using (System.Security.Cryptography.SHA256Managed sha2 = new System.Security.Cryptography.SHA256Managed())
   {
    return sha2.ComputeHash(input).ToHex();
   }
  }

  private void Notification(string msg, bool error, bool taskSuccess = false)
  {
   try
   {

    if (error)
    {
     WSClose();
    }

    LocallyHandleMessageArrived(msg, taskSuccess, error);
   }
   catch { }
  }

  public void WSClose()
  {
   try
   {
    if (webSocket == null)
     return;


    if (webSocket.State != WebSocketState.Open)
    {
     return;
    }

    Task.Run(async () =>
    {
     try
     {
      await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "user", CancellationToken.None);

     }
     catch { }
    });
   }
   catch { }

  }

  public async Task Send(string Msg)
  {
   var array = Encoding.UTF8.GetBytes(Msg);
   var buffer = new ArraySegment<byte>(array);
   await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

  }
  public async void Dispose()
  {
   try
   {
    webSocket.Dispose();
   }
   catch { }
  }

  void LocallyHandleMessageArrived(string msg, bool data, bool error)
  {
   if (OnStatusChanged == null) return;

   try
   {
    Interlocked.CompareExchange(ref OnStatusChanged, null, null)?.Invoke(new DataMSGCompletedEventArgs(msg, data, error));
   }
   catch { }
  }
 }


 public delegate void DataMSGCompletedEventHandler(DataMSGCompletedEventArgs e);

 public class DataMessageProxy
 {
  public event DataMSGCompletedEventHandler MsgCompleted;
  public void LocallyHandleMessageArrived(string msg, bool data, bool error)
  {
   try
   {
    Interlocked.CompareExchange(ref MsgCompleted, null, null)?.Invoke(new DataMSGCompletedEventArgs(msg, data, error));
   }
   catch { }
  }
 }

 public partial class DataMSGCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
 {
  public DataMSGCompletedEventArgs(object result, bool value, bool error) : base(null, false, null) { Message = (string)result; TaskSuccess = value; IsError = error; }

  public string Message { get; }

  public bool TaskSuccess { get; }

  public bool IsError { get; }
 }
}

