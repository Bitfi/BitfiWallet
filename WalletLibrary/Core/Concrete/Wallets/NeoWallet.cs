using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoGasLibrary;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class NeoWallet : CommonWallet, IWallet
  {
    public NeoWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.NEO)
      : base(keySets, product)
    { }

    public string GetLegacyAddress(uint index = 0)
    {
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        var NEOkeypair = new KeyPair(privateKey.Value);
        return NEOkeypair.address;
      }
    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();

      byte[] bts = Convert.FromBase64String(req.MXTxn);
      string jData = System.Text.Encoding.UTF8.GetString(bts);
      NEONeoscanUnspent[] unspent = JsonConvert.DeserializeObject<NEONeoscanUnspent[]>(jData);

      if (unspent == null || unspent.Length < 1)
        throw new Exception("No unspent.");

      List<NeoGasLibrary.NeoAPI.UnspentEntry> entries = new List<NeoGasLibrary.NeoAPI.UnspentEntry>();
      foreach (var un in unspent)
      {
        NeoGasLibrary.NeoAPI.UnspentEntry entry = new NeoGasLibrary.NeoAPI.UnspentEntry();
        entry.index = UInt16.Parse(un.n.ToString());
        entry.txid = un.txid;
        entry.value = un.value;
        entries.Add(entry);
      }

      if (entries == null || entries.Count < 1)
        throw new Exception("No Entries.");
      
      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        var assetId = req.BlkNet.ToLower() == "gas" ? NeoGasLibrary.NeoAPI.gasId : NeoGasLibrary.NeoAPI.neoId;
        NeoGasLibrary.NeoAPI.Transaction tx = NeoGasLibrary.NeoAPI.BuildContractTx(
          entries, 0, req.NoxAddress.BTCAddress, req.ToAddress, req.Amount, assetId);

        NeoGasLibrary.KeyPair keypair = new NeoGasLibrary.KeyPair(privateKey.Value);

        string neoaddress = keypair.address;
        if (neoaddress != req.NoxAddress.BTCAddress)
          throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

        string txHex = NeoGasLibrary.NeoAPI.SignAndSerialize(tx, keypair);
        response.TxnHex = txHex;
        response.Addition = new Addition
        {
          MaxFeeAllowed = tx.gas.ToString()
        };

        return response;
      }
    }

    public PaymentRequestResponse ClaimGas(NoxGasRequests req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();
      byte[] bts = Convert.FromBase64String(req.NXTxn);
      string jData = System.Text.Encoding.UTF8.GetString(bts);
      NEONeoscanUnclaimed unclaimed = JsonConvert.DeserializeObject<NEONeoscanUnclaimed>(jData);

      if (unclaimed == null)
        throw new Exception("Error, no claimable values.");

      if (unclaimed.claimable == null)
        throw new Exception("Error, no claimable values.");

      List<NeoGasLibrary.NeoAPI.ClaimEntry> claims = new List<NeoGasLibrary.NeoAPI.ClaimEntry>();
      foreach (var un in unclaimed.claimable)
      {
        NeoGasLibrary.NeoAPI.ClaimEntry entry = new NeoGasLibrary.NeoAPI.ClaimEntry();
        entry.index = UInt16.Parse(un.n.ToString());
        entry.txid = un.txid;
        entry.value = un.unclaimed;
        claims.Add(entry);
      }

      if (claims.Count < 1)
        throw new Exception("Error, no claimable values.");

      using (var privateKey = KeySets[0].Key.GetPrivateKey())
      {
        var keypair = new NeoGasLibrary.KeyPair(privateKey.Value);

        string neoaddress = keypair.address;
        if (neoaddress.ToUpper() != req.NeoAddress.ToUpper())
          throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

        var tx = NeoGasLibrary.NeoAPI.BuildClamTx(claims, NeoGasLibrary.NeoAPI.Net.Main, keypair.address);
        var txHex = NeoGasLibrary.NeoAPI.SignAndSerialize(tx, keypair);
        
        response.TxnHex = txHex;
        response.Addition = new Addition
        {
          MaxFeeAllowed = tx.gas.ToString()
        };
        return response;
      }
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      throw new NotImplementedException();
    }
  }
}
