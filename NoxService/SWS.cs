using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using BitfiWallet;

namespace NoxService.SWS
{
  public class SWS
  {
    Device noxDevice
		{
      get
			{
        return NoxDPM.NoxT.noxDevice;
			}
		}

    SGADWS walletServ
		{
      get
      {
        return NoxDPM.NoxT.walletServ;
      }

    }

    public string GetSGAMessageV2(string SMSToken)
    {
      try
      {

        walletServ.Timeout = 30000;

        string msg = Guid.NewGuid().ToString();
        msg = noxDevice.SHashSHA1(msg);
        string signature = noxDevice.SignMsg(msg);
        string sgamessage = walletServ.GetSGAMessageV2(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), SMSToken);

        return sgamessage;

      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public string GetSGAMessage()
    {
      try
      {

        walletServ.Timeout = 30000;

        string msg = Guid.NewGuid().ToString();
        msg = noxDevice.SHashSHA1(msg);
        string signature = noxDevice.SignMsg(msg);
        string sgamessage = walletServ.GetSGAMessage(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID());

        return sgamessage;

      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }

    }
    public string GetSGAToken(string userPubKey, string Signature)
    {

      try
      {

        walletServ.Timeout = 30000;
        string msg = Guid.NewGuid().ToString();
        msg = noxDevice.SHashSHA1(msg);
        string signature = noxDevice.SignMsg(msg);
        string MessageToken = walletServ.GetSGAToken(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), userPubKey, Signature);

        return MessageToken;
      }
      catch (WebException)
      {
        return null;
      }
      catch (Exception)
      {
        return null;
      }

    }

    public string GetSGATokenForSigninV2(string JsonResponseObject)
    {
      try
      {

        walletServ.Timeout = 30000;
        string msg = Guid.NewGuid().ToString();
        msg = noxDevice.SHashSHA1(msg);
        string signature = noxDevice.SignMsg(msg);
        string MessageToken = walletServ.GetSGATokenForSignInV2(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), JsonResponseObject);

        return MessageToken;
      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }
    public string GetSGATokenForSignin(string userPubKey, string Signature, string DisplayCode)
    {
      try
      {
        walletServ.Timeout = 30000;
        string msg = Guid.NewGuid().ToString();
        msg = noxDevice.SHashSHA1(msg);
        string signature = noxDevice.SignMsg(msg);
        string MessageToken = walletServ.GetSGATokenForSignIn(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), userPubKey, Signature, DisplayCode);

        return MessageToken;
      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }
    public NoxMessagesConfig GetMessagesConfig()
    {
      try
      {
        walletServ.Timeout = 30000;

        var config = walletServ.GetMessageConfigV3();

        return config;
      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }

    }
    public NoxAddressReviewV3 GetAddressIndexes(string SGAMSGTOKEN)
    {
      try
      {

        walletServ.Timeout = 30000;
        if (string.IsNullOrEmpty(SGAMSGTOKEN)) return null;

        noxDevice.MsgCount++;

        string msg = noxDevice.MsgCount.ToString();
        string signature = noxDevice.SignMsg(msg);
        return walletServ.GetAddressIndexesV3(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), SGAMSGTOKEN);

      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }


    }

    public OverviewViewModel GetOverviewModel(string SGAMSGTOKEN)
    {
      try
      {
        walletServ.Timeout = 30000;
        if (string.IsNullOrEmpty(SGAMSGTOKEN)) return null;

        noxDevice.MsgCount++;

        string msg = noxDevice.MsgCount.ToString();

        string signature = noxDevice.SignMsg(msg);
        var model = walletServ.GetOverviewModelV3(signature, msg, noxDevice.DevicePubHash(), noxDevice.GetDeviceID(), SGAMSGTOKEN);

        return model;

      }
      catch (WebException)
      {

        return null;
      }
      catch (Exception)
      {
        return null;
      }

    }

  }
}