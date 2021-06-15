using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using NoxKeys;
using System.Threading.Tasks;
using System.Threading;
using BitfiWallet;

namespace NoxService.NWS
{
  public struct HWSResponse
  {
    public bool WebError { get; set; }
    public bool Notfify { get; set; }
    public FormUserInfo formUserInfo { get; set; }
  }

  public class HWS
  {

    NOXWS2 serv
    {
      get
      {
        return NoxDPM.NoxT.serv;
      }
    }

    public async Task<HWSResponse> GetCurrentSMSToken()
    {

      using (CancellationTokenSource cancellation = new CancellationTokenSource())
      {

        cancellation.CancelAfter(TimeSpan.FromSeconds(6));

        try
        {
         
          string msg = NoxDPM.NoxT.noxDevice.SHashSHA1(Guid.NewGuid().ToString());
          string signature = NoxDPM.NoxT.noxDevice.SignMsg(msg + ConfigValues.SMSTokenMsg);

          serv.Timeout = 5000;
          var resp = await GetCurrentSMSTokenTask(serv, signature, msg, cancellation.Token);

          HWSResponse hWSResponse = new HWSResponse();

          if (resp.Error != null)
          {
            hWSResponse.WebError = true;
          }
          else
          {
            if (!resp.Cancelled)
            {
              if (resp.Result != null && resp.Result.Response != null)
              {
                hWSResponse.formUserInfo = resp.Result.Response;

                if (NoxDPM.NoxT.noxDevice.ValidMsg(hWSResponse.formUserInfo.SMSToken + hWSResponse.formUserInfo.ReqType, resp.Result.Signature))
                {
                  hWSResponse.Notfify = true;
                }
              }
            }
          }

          return hWSResponse;

        }
        catch (WebException)
        {
          return new HWSResponse() { WebError = true };
        }
        catch (Exception)
				{
          return new HWSResponse() { WebError = true };
        }

      }

    }

    private TaskCompletionSource<GetCurrentSMSTokenCompletedEventArgs> ntask;

    void NOXWSCompleted<T>(TaskCompletionSource<T> tcs, System.ComponentModel.AsyncCompletedEventArgs e, Func<T> getResult)
    {
      try
      {

        if (e.Error != null)
        {
          tcs.TrySetException(e.Error);
        }
        else if (e.Cancelled)
        {
          tcs.TrySetCanceled();
        }
        else
        {

          tcs.TrySetResult(getResult());
        }
      }
      catch (WebException)
      {

        tcs.TrySetCanceled();
      }
      catch
      {
        tcs.TrySetCanceled();
      }
    }

    Task<GetCurrentSMSTokenCompletedEventArgs> GetCurrentSMSTokenTask(NOXWS2 serv, string signature, string msg, CancellationToken cancellationToken)
    {
      ntask = new TaskCompletionSource<GetCurrentSMSTokenCompletedEventArgs>();
      serv.GetCurrentSMSTokenCompleted += (x, e) => NOXWSCompleted(ntask, e, () => e);

      cancellationToken.Register(() =>
      {
        ntask.TrySetCanceled();
      });

      serv.GetCurrentSMSTokenAsync(signature, msg, NoxDPM.NoxT.noxDevice.DevicePubHash(), NoxDPM.NoxT.noxDevice.GetDeviceID(),
        NoxDPM.NoxT.noxDevice.GetWalletID());
      return ntask.Task;
    }

  }
}