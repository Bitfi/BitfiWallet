using NBitcoin;
using NBitcoin.Crypto;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.SWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;
using static WalletLibrary.Core.Concrete.Wallets.CommonWallet;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace WalletLibrary.Core.Concrete
{
  public class SGAAuthenticator
  {
    
    private WalletFactory Factory;
    private NoxKeyGen NoxKeyGen;
    private bool Initialized = false;

    WalletEventHandler _NoxEvent;

    WalletEventProxy _eventProxy;

    WSTask statusTask = new WSTask();

    public SGAAuthenticator(WalletFactory factory, WalletEventProxy eventProxy)
    {
      Factory = factory;
      Initialized = true;
      _eventProxy = eventProxy;

      statusTask.Transition = UITransition.DlgModel;
   
    }

    public string[] SignTask(Guid Message)
    {
      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC, new UInt32[] { 0 }, true))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {
        string address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Message.ToByteArray());
        string signature = Convert.ToBase64String(signer.Sign(mesBytes));

        return new string[] { signature, address };
      }
    }

    public string SignSGAMessage(string sgaMessage)
    {
      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC, new UInt32[] { 0 }, true))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {
        var address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
        var signature = signer.Sign(mesBytes);
        
        SWS WS = new SWS();
        var tkn = WS.GetSGAToken(address, Convert.ToBase64String(signature));

        if (string.IsNullOrEmpty(tkn))
          throw new Exception(Sclear.MSG_ERROR_PROFILE);

        return tkn;
      }
    }

    public string SignSGAMessageWithCode(string sgaMessage, string DisplayCode)
    {
      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC, new UInt32[] { 0 }, true))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {
        var address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
        var signature = signer.Sign(mesBytes);
        
        if (!signer.Verify(mesBytes, signature))
          throw new Exception("Unable to verfy the signature");

        SWS WS = new SWS();
        var tkn = WS.GetSGATokenForSignin(address, Convert.ToBase64String(signature), DisplayCode);

        if (string.IsNullOrEmpty(tkn))
          throw new Exception(Sclear.MSG_ERROR_PROFILE);

        return tkn;
      }
    }
    public DerResponse SignSGAMessageV2(string sgaMessageHex, WalletFactory.Products Type, uint index = 0)
    {
      uint[] indexes = new uint[] { index };

      bool Authenticating = false;

      if (Type == WalletFactory.Products.BTC)
        Authenticating = true;

      using (var Wallet = Factory.ConstructWallet(Type, indexes, Authenticating))
      using (var signer = Wallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.DER))
      {
        return signer.GetDerResponse(sgaMessageHex.HexToByteArray());
      }
    }
    public string Sign2FAMessage(string sgaMessage, string message_2FA, string DisplayCode)
    {
      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {
        var address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
        var signature = signer.Sign(mesBytes);

        if (!signer.Verify(mesBytes, signature))
          throw new Exception("Unable to verfy the signature");

        using (var b2faWallet = Factory.ConstructWallet(WalletFactory.Products.BFA))
        {
          using (var b2fasigner = b2faWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.DER))
          {
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
              byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message_2FA));

              var b2fapubKey = b2faWallet.GetLegacyAddress();
              var b2fasignature = b2fasigner.Sign(hash);

              SWS WS = new SWS();
              var tkn = WS.GetSGATokenForSignin(address, Convert.ToBase64String(signature), DisplayCode + "|" + b2fapubKey + "|" + b2fasignature.ByteToHex());

              if (string.IsNullOrEmpty(tkn))
                throw new Exception(Sclear.MSG_ERROR_PROFILE);

              return tkn;
            }

          }
        }

      }
    }

    public Guid SignSGASessionShared(string sgaMessage)
    {
      string tkmsg;

      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {
        var address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
        var signature = signer.Sign(mesBytes);

        SWS WS = new SWS();
        tkmsg = WS.GetSGAToken(address, Convert.ToBase64String(signature));

      }
        if (tkmsg.Length < 10)
          throw new Exception(Sclear.MSG_ERROR_PROFILE);

        using (var bfaWallet = Factory.ConstructWallet(WalletFactory.Products.BFA))
        using (var ethsigner = bfaWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.DER))
        {
          NoxShared.SharedSession sharedSession = new NoxShared.SharedSession(new NoxManagedArray(ethsigner.GetBFASecret()), tkmsg);
          Guid id = Guid.NewGuid();
        BitfiWallet.NoxDPM.NoxT.noxDevice.sharedSessions[id] = sharedSession;
      
          return id;
        }
      
    }

    public string[] SignSGAMessageOut(string sgaMessage)
    {
      using (var btcWallet = Factory.ConstructWallet(WalletFactory.Products.BTC, null, true))
      using (var signer = btcWallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT))
      {


        var address = btcWallet.GetLegacyAddress();
        var mesBytes = FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
        var signature = signer.Sign(mesBytes);
        
        SWS WS = new SWS();
        List<NoxService.SWS.NoxAddresses> lnoxAddresses = new List<NoxService.SWS.NoxAddresses>();
        string adrlist = JsonConvert.SerializeObject(lnoxAddresses);

        string tkmsg = WS.GetSGAToken(address, Convert.ToBase64String(signature));

        if (tkmsg.Length < 10)
          throw new Exception(Sclear.MSG_ERROR_PROFILE);

        statusTask.StatusMsg = "Getting address index list";
        _eventProxy.HandleStatusEvent(statusTask);

        var objaddrListindexes = WS.GetAddressIndexes(tkmsg);
        if (objaddrListindexes != null)
        {
          List<NoxAddresses> invalidAddresses;
          List<NoxAddresses> noxAddresses;

          lnoxAddresses = GetReviewIndexes(objaddrListindexes, out invalidAddresses);
          if (invalidAddresses.Count != 0)
          {
            //we have found not legal address
          }
          adrlist = JsonConvert.SerializeObject(lnoxAddresses);
        }
        else
          throw new Exception("No addresses.");

        return new string[2] { tkmsg, adrlist };
      }
    }
    
    public List<NoxAddresses> GetReviewIndexes(NoxAddressReviewV3 noxAddressReviews,
      out List<NoxAddresses> invalidAddresses)
    {
      var noxAddresses = new List<NoxService.SWS.NoxAddresses>();
      invalidAddresses = new List<NoxService.SWS.NoxAddresses>();

    

      foreach (var adrReview in noxAddressReviews.AdrReview)
      {
        string currencySymbol = adrReview.Blk;


        uint[] indexes = new uint[adrReview.IndexCount];
        for (int i = 0; i < adrReview.IndexCount; ++i)
          indexes[i] = (uint)i;

        using (var wallet = Factory.ConstructWallet(WalletFactory.SymbolToWalletProduct(currencySymbol), indexes))
        {
          var invalidAddressesTmp = new List<NoxAddresses>();
          var noxAddressesPortion = noxAddressReviews.Addresses
            .Where(addr => addr.BlkNet.ToLower() == currencySymbol.ToLower())
            .Select(addr =>
            {
              var btcAddress = addr.DoSegwit ?
                wallet.GetSegwitAddress((uint)addr.HDIndex) : wallet.GetLegacyAddress((uint)addr.HDIndex);


                if (btcAddress.ToLower() != addr.BTCAddress.ToLower())
                {
                  invalidAddressesTmp.Add(addr);
                  btcAddress = null;
                }
              

              return new NoxService.SWS.NoxAddresses
              {
                BlkNet = currencySymbol,
                BTCAddress = btcAddress
              };
            }).ToList();

          invalidAddresses = invalidAddressesTmp;
          noxAddresses.AddRange(noxAddressesPortion);
        }
      }

      return noxAddresses;
    }

    private byte[] FormatMessageForSigning(byte[] messageBytes)
    {
      byte[] data = NBitcoin.Utils.FormatMessageForSigning(messageBytes);
      var hash = Hashes.Hash256(data);
      return hash.ToBytes();
    }

    private byte[] FormatBecauseOfBug(byte[] signature)
    {
      var incorrectLength = 72;
      var formatted = new byte[incorrectLength];
      if (signature.Length <incorrectLength)
      {
        for (int i = 0; i < formatted.Length; ++i)
        {
          formatted[i] = (i < signature.Length) ? signature[i] : (byte)(0);
        }
      }
      else
      {
        Array.Copy(signature, formatted, incorrectLength);
      }
      return formatted;
    }
  }
}
