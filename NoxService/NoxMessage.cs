using EthereumLibrary.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using NBitcoin;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using WalletLibrary;
using System.Security.Cryptography;

namespace NoxService
{
  public class NoxMessage
  {
    public TxnResponse SendMessage(string uri, string message, BitfiWallet.Device noxDevice)
    {
      try
      {
        string Signature = noxDevice.SignMsg(message);

        var sha = SHA256Managed.Create();
        RIPEMD160Managed rIPEMD160Managed = new RIPEMD160Managed();
        var ripemd = rIPEMD160Managed.ComputeHash(sha.ComputeHash(noxDevice.PubTEE().ToBytes()));
        byte[] msgbts = System.Text.Encoding.UTF8.GetBytes(message);
        string rawtxn = BuildMsgTxn(ripemd.ToHex(), msgbts.ToHex(), Signature, "");

        return POST(uri, rawtxn);
      }
      catch
      {
        TxnResponse txnResponse = new TxnResponse();
        txnResponse.success = false;
        txnResponse.error_message = "Unexpected result, please check connection.";
        return txnResponse;
      }
    }

    private static void Write(MemoryStream ms, byte[] bytes)
    {
      ms.Write(bytes, 0, bytes.Length);
    }

    private static byte[] WriteBts(byte[] bytes, int start, int lenght)
    {
      MemoryStream ms = new MemoryStream();
      ms.Write(bytes, start, lenght);
      return ms.ToArray();
    }
    private string BuildMsgTxn(string RIPEMD_FromAddress, string MessageHex, string Signature, string LinkedTxnHex)
    {

      if (string.IsNullOrEmpty(LinkedTxnHex)) LinkedTxnHex = new byte[16].ToHex();

      Signature = Convert.FromBase64String(Signature).ToHex();
      string MsgData = RIPEMD_FromAddress + Signature + MessageHex;
      System.Security.Cryptography.MD5 sha1 = System.Security.Cryptography.MD5.Create();

      string TxnHex = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(MsgData.ToLower())).ToHex();
      MsgData = TxnHex + LinkedTxnHex + MsgData;

      return MsgData.ToLower();

    }
    public TxnResponse POST(string url, string obj)
    {
      try
      {
        WebRequest request = WebRequest.Create(url);
        request.Method = "POST";
        request.Timeout = 5000;
        request.ContentType = "application/text";
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(obj);
        request.ContentLength = byteArray.Length;

        using (System.IO.Stream dataStream = request.GetRequestStream())
        {
          dataStream.Write(byteArray, 0, byteArray.Length);
          dataStream.Close();
        }

        using (WebResponse response = request.GetResponse())
        using (System.IO.Stream stream = response.GetResponseStream())
        {
          System.IO.StreamReader sr = new System.IO.StreamReader(stream);
          var result = sr.ReadToEnd();
          return JsonConvert.DeserializeObject<TxnResponse>(result);
        }

      }
      catch (WebException)
      {
        TxnResponse txnResponse = new TxnResponse();
        txnResponse.success = false;
        txnResponse.error_message = "Please check connection.";
        return txnResponse;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        TxnResponse txnResponse = new TxnResponse();
        txnResponse.success = false;
        txnResponse.error_message = "Please check connection.";
        return txnResponse;
      }

    }

  }
  public class TxnResponse
  {
    public bool success { get; set; }
    public string error_message { get; set; }
  }
}