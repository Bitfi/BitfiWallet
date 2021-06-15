using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoxKeys;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Org.BouncyCastle.Crypto.Parameters;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.NoxShared
{
  public class SharedSession : IDisposable
  {
    NoxManagedArray User; string Token;
    ECPrivateKeyParameters _Private;

    BitfiWallet.Device noxDevice
		{
      get
			{
        return BitfiWallet.NoxDPM.NoxT.noxDevice;
			}

		}

    public SharedSession(NoxManagedArray UserPrivate, string SGAMSGTOKEN)
    {
      User = UserPrivate; Token = SGAMSGTOKEN;

      _Private = ECKEY.GetECPrivate(User);

     
    }
    public byte[] MyPublicKey()
    {
      return ECKEY.GetPubKey(_Private, true);
    }
    public bool Enabled { get; set; }
    private byte[] SharedSecret(byte[] PeerPublicKey)
    {
      using (SHA256Managed sha = new SHA256Managed())
      {
        return sha.ComputeHash(ECKEY.GetSharedPubkey(_Private, ECKEY.GetECPublic(PeerPublicKey)));
      }
    }
    public string GetAuthRequest(byte[] PeerPublicKey)
    {
      SharedRequest sharedRequest = new SharedRequest();
      SharedAuthRequest sharedAuthRequest = new SharedAuthRequest();
      sharedAuthRequest.authorization = BuildAuthorization();
      sharedAuthRequest.UserSession = MyPublicKey().ToHex();
      sharedAuthRequest.PeerSession = PeerPublicKey.ToHex();
      sharedRequest.sharedRequestType = SharedRequestType.Auth;
      sharedRequest.authRequest = sharedAuthRequest;
      return SerializeRequest(sharedRequest);
    }
    public string CreateMoreRequest()
    {
      SharedRequest sharedRequest = new SharedRequest();
      sharedRequest.sharedRequestType = SharedRequestType.More;
      return SerializeRequest(sharedRequest);
    }
    public string CreateMessage(NoxManagedArray message, byte[] PeerPublicKey)
    {
      SharedRequest sharedRequest = new SharedRequest();
      byte[] encrypted = AesEncrypt(message.Value, SharedSecret(PeerPublicKey));
      sharedRequest.encryptedMessage = encrypted.ToHex();
      sharedRequest.sharedRequestType = SharedRequestType.Message;
      message.Dispose();
      return SerializeRequest(sharedRequest);
    }
    public async Task<List<MessageResponse>> GetResponse(string response, byte[] PeerPublicKey, NoxWSClient noxWSClient)
    {
      try
      {
        var resplist = DeserializeResponse(response);

        List<MessageResponse> responses = new List<MessageResponse>();
        if (resplist == null) return null;

        foreach (var resp in resplist)
        {
          var utresp = ParseResponse(resp, PeerPublicKey, noxWSClient);
          responses.Add(utresp);
        }

        return responses;
      }
      catch
      {
        return null;
      }
    }
    private MessageResponse ParseResponse(SharedResponse resp, byte[] PeerPublicKey, NoxWSClient noxWSClient)
    {
      MessageResponse messageResponse = new MessageResponse();

      switch (resp.responseType)
      {
        case SharedResponseType.Peer:
         using (ParagraphBuilder paragraphBuilder = new ParagraphBuilder(
            AesDecrypt(resp.message.HexToByteArray(), SharedSecret(PeerPublicKey))))
          {
            messageResponse.peerMessage = paragraphBuilder.GetParagraphArray();
          }

          if (!resp.offLine)
          {
            try
            {
              Task.Run(async () =>
              {
                SharedRequest sharedRequest = new SharedRequest();
                sharedRequest.sharedRequestType = SharedRequestType.Receipt;
                sharedRequest.MessageID = resp.MessageID;
                await noxWSClient.Send(SerializeRequest(sharedRequest));

              });
            }
            catch { }
          }
          else
          {
            messageResponse.isHistory = true;
          }

          break;
        case SharedResponseType.User:
          using (ParagraphBuilder paragraphBuilder = new ParagraphBuilder(
            AesDecrypt(resp.message.HexToByteArray(), SharedSecret(PeerPublicKey))))
          {
            messageResponse.peerMessage = paragraphBuilder.GetParagraphArray();
          }
          break;
        case SharedResponseType.Service:
          messageResponse.serviceMessage = resp.message;
          break;
      }

      try
      {
        if (!string.IsNullOrEmpty(resp.MessageID))
        {
          messageResponse.MessageID = new Guid(resp.MessageID);
        }
      }
      catch { }

      messageResponse.responseType = resp.responseType;
      messageResponse.dateTime = resp.dateTime;
      messageResponse.offLine = resp.offLine;
      messageResponse.BottomStack = resp.BottomStack;

      return messageResponse;
    }
    private string SerializeRequest(SharedRequest response)
    {
      return JsonConvert.SerializeObject(response);
    }
    private SharedResponse[] DeserializeResponse(string Json)
    {
      return JsonConvert.DeserializeObject<SharedResponse[]>(Json);
    }
    static byte[] AesEncrypt(byte[] inputByteArray, byte[] _SharedSecret)
    {

      var iv = _SharedSecret.SafeSubarray(0, 16);
      var encryptionKey = _SharedSecret.SafeSubarray(16, 16);

      var aes = new AesBuilder().SetKey(encryptionKey).SetIv(iv).IsUsedForEncryption(true).Build();
      return aes.Process(inputByteArray, 0, inputByteArray.Length);
    }
    static NoxManagedArray AesDecrypt(byte[] encrypted, byte[] _SharedSecret)
    {

      var iv = _SharedSecret.SafeSubarray(0, 16);
      var encryptionKey = _SharedSecret.SafeSubarray(16, 16);

      var aes = new AesBuilder().SetKey(encryptionKey).SetIv(iv).IsUsedForEncryption(false).Build();
      return new NoxManagedArray(aes.Process(encrypted, 0, encrypted.Length));
    }
    public string GetEncryptedPref(string json)
    {
      if (User == null) return null;
      if (string.IsNullOrEmpty(noxDevice.DevicePubHash())) return null;

      try
      {
        var encrypted = AesEncrypt(System.Text.Encoding.UTF8.GetBytes(json),
          SHA256Managed.Create().ComputeHash(User.Value.Concat(noxDevice.DevicePubHash().HexToByteArray())));
        return Convert.ToBase64String(encrypted);
      }
      catch { return null; }
    }
    public string GetDecryptedPref(string b64)
    {
      if (User == null) return null;
      if (string.IsNullOrEmpty(noxDevice.DevicePubHash())) return null;

      try
      {
        var decrypted = AesDecrypt(Convert.FromBase64String(b64),
          SHA256Managed.Create().ComputeHash(User.Value.Concat(noxDevice.DevicePubHash().HexToByteArray())));
        var resp = System.Text.Encoding.UTF8.GetString(decrypted.Value);
        decrypted.Dispose();
        return resp;
      }
      catch { return null; }
    }
    public void Dispose()
    {
      User.Dispose();
      _Private = null;
      User = null;
    }
    public async Task<string[]> GetOfflineAlert()
    {
      ManagerRequest managerRequest = new ManagerRequest();
      MessageStatusRequest messageStatusRequest = new MessageStatusRequest();
      messageStatusRequest.user_pubKey = MyPublicKey().ToHex();

      managerRequest.messageStatusRequest = messageStatusRequest;
      managerRequest.requestType = ManagerRequestType.MessageStatus;

      var resp = await PostAsync(managerRequest);

      if (resp == null) return null;
      if (!resp.success) return null;
      if (resp.messageStatusResponse == null) return null;
      if (resp.messageStatusResponse.peer_name == null) return null;
      if (resp.messageStatusResponse.peer_name.Length == 0) return null;
      return resp.messageStatusResponse.peer_name;
    }
    public async Task<ManagerResponse> PostAsync(ManagerRequest obj)
    {
      try
      {
        obj.authorization = BuildAuthorization();
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(obj));
        using (var client = new HttpClient())
        {
          client.Timeout = TimeSpan.FromMilliseconds(10000);

          var content = new ByteArrayContent(byteArray);
          using (var response = await client.PostAsync("https://bitfi.com/MSGX/Manager1.aspx?TxnType=1", content))
          {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ManagerResponse>(responseString);
          }
        }
      }
      catch (HttpRequestException)
      {
        return new ManagerResponse() { success = false, error_message = "Error starting request." };
      }
      catch (TaskCanceledException)
      {
        return new ManagerResponse() { success = false, error_message = "Error starting request." };
      }
      catch (Exception)
      {
        return new ManagerResponse() { success = false, error_message = "Error starting request." };
      }
    }
    private ManagerAuthorization BuildAuthorization()
    {
      ManagerAuthorization authorization = new ManagerAuthorization();
      noxDevice.MsgCount++;
      authorization.Message = noxDevice.MsgCount.ToString();
      authorization.Signature = noxDevice.SignMsg(authorization.Message);
      authorization.DevicePubHash = noxDevice.DevicePubHash();
      authorization.DeviceID = noxDevice.GetDeviceID();
      authorization.SGAMSGTOKEN = Token;
      authorization.HdrType = "na";

      return authorization;
    }

  }
}