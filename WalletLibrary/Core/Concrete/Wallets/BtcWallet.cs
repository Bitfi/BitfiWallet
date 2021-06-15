using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class BtcWallet : CommonWallet, IWallet
  {
    private Network Network;
    //to do: rewrite, i had to implement it because of previous design
    private NoxKeyGen NoxKeyGen;

    public BtcWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.BTC) 
      : base(keySets, product)
    {
      Network = Sclear.GetBLKNetworkAlt(Symbol);
      NoxKeyGen = new NoxKeyGen(keySets);
    }

    public string GetLegacyAddress(uint index = 0)
    {
      return GetAddress(index, false);
    }

    public string GetSegwitAddress(uint index = 0)
    {
      return GetAddress(index, true);
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      if (!HasIndex((uint)req.NoxAddress.HDIndex))
        throw new Exception(String.Format("Unable to find such index: {0} in the wallet", req.NoxAddress.HDIndex));

      var addr = GetAddress((uint)req.NoxAddress.HDIndex, req.NoxAddress.DoSegwit);
      if (addr != req.NoxAddress.BTCAddress)
        throw new Exception(String.Format(Sclear.MSG_ERROR_SIGNMESSAGE, addr, req.NoxAddress.BTCAddress));

      MsgTaskTransferResponse response = new MsgTaskTransferResponse();
      byte[] msg = Convert.FromBase64String(req.Msg);

      var keySet = KeySets.FirstOrDefault(k => k.Index == req.NoxAddress.HDIndex);
      
      var isCompressed = req.NoxAddress.DoSegwit;
      using (var publicKey = keySet.Key.GetPublicKey(isCompressed))
      {
        PubKey pubKey = new PubKey(publicKey.Value, req.NoxAddress.HDIndex, req.BlkNet);
        AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);
        response.MsgSig = altCoinGen.AltMsgSign(pubKey, req.NoxAddress.BTCAddress, System.Text.Encoding.UTF8.GetString(msg));
      }
      
      return response;
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();
      var addition = new Addition { };

      var HDIndexes = req.HDIndexList;
      List<PubKey> KBList = new List<PubKey>();

      for (int i = 0; i < HDIndexes.Length; i++)
      {
        int hdIndex = Convert.ToInt32(HDIndexes[i]);
        var keySet = KeySets.FirstOrDefault(k => k.Index == hdIndex);

        var pubKeyCompressed = new PubKey(keySet.Key.GetPublicKey(true).Value, hdIndex, req.BlkNet);
        var pubKeyUncompressed = new PubKey(keySet.Key.GetPublicKey(false).Value, hdIndex, req.BlkNet);

        KBList.Add(pubKeyCompressed);
        KBList.Add(pubKeyUncompressed);
      }

      AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);

      List<NoxKeys.BCUnspent> txnRaw = new List<NoxKeys.BCUnspent>();
      foreach (var txnin in req.UnspentList)
      {
        NoxKeys.BCUnspent bCUnspent = new NoxKeys.BCUnspent();
        bCUnspent.Address = txnin.Address;
        bCUnspent.OutputN = txnin.OutputN;
        bCUnspent.TxHash = txnin.TxHash;
        bCUnspent.Amount = txnin.Amount;
        txnRaw.Add(bCUnspent);
      }

      var txn = altCoinGen.AltCoinSign(KBList, req.ToAddress, txnRaw, req.Amount, req.NoxAddress.BTCAddress, req.FeeTotal);

      if (txn.IsError) throw new Exception(txn.ErrorMessage);

      response.TxnHex = txn.TxnHex;
      addition.FeeAmount = txn.Fee.ToString(); 

      response.Addition = addition;
      return response;

    }

    private string GetAddress(uint index, bool isSegwit)
    {
      if (!HasIndex(index))
        throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

      var keySet = KeySets.FirstOrDefault(k => k.Index == index);
      using (var pubKeyUncompressed = keySet.Key.GetPublicKey(isSegwit))
      {
        //todo: rewrite using native methods
        var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);
        AltCoinGen altCoinGen = new AltCoinGen(Network, NoxKeyGen);
        return altCoinGen.GetNewAddress(pubKey);
      }
    }

    private List<PubKey> HDDerriveMultipleKey(int[] hdIndexes, string currencySymbol)
    {
      List<PubKey> KBList = new List<PubKey>();
      
      foreach (int hdIndex in hdIndexes.Distinct())
      {
        var keySet = KeySets.FirstOrDefault(k => k.Index == hdIndex);
        
        var pubKeyCompressed = new PubKey(keySet.Key.GetPublicKey(true).Value, hdIndex, Symbol);
        var pubKeyUncompressed = new PubKey(keySet.Key.GetPublicKey(false).Value, hdIndex, Symbol);

        KBList.Add(pubKeyCompressed);
        KBList.Add(pubKeyUncompressed);
      }

      return KBList;
    }
  }
}
