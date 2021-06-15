using System;
using System.Collections.Generic;
using NBitcoin;

namespace NoxKeys
{
  class AltCoinGen
  {
    NoxKeys.NoxKeyGen _noxKeyGen;
    public Network net;
    public bool SegSupport = true;
    public AltCoinGen(Network _net, NoxKeys.NoxKeyGen noxKeyGen)
    {
      net = _net;
      _noxKeyGen = noxKeyGen;

      if (net == NBitcoin.Altcoins.Dogecoin.Instance.Mainnet) SegSupport = false;
      if (net == NBitcoin.Altcoins.Dash.Instance.Mainnet) SegSupport = false;
      if (net == NBitcoin.Altcoins.Zclassic.Instance.Mainnet) SegSupport = false;

    }
    public string GetNewAddress(PubKey pubKey)
    {
      if (net == null) throw new Exception("Invalid currency.");

      if (pubKey.IsCompressed)
      {
        return pubKey.GetSegwitAddress(net).ToString();
      }
      else
      {
        return pubKey.GetAddress(net).ToString();
      }
    }

    public string AltMsgSign(PubKey pubKey, string tprocNoxAddressBTCAddress, string Message)
    {
      if (net == null) throw new Exception("Invalid currency.");

      bool check = false;

      if (pubKey.IsCompressed)
      {
        if (pubKey.GetSegwitAddress(net).ToString() == tprocNoxAddressBTCAddress)
        {
          check = true;
        }

      }
      else
      {
        if (pubKey.GetAddress(net).ToString() == tprocNoxAddressBTCAddress)
        {
          check = true;
        }
      }

      if (!check)
      {
        throw new Exception(Sclear.MSG_ERROR_SIGNMESSAGE);
      }

      Key pkey = new Key(pubKey, _noxKeyGen);

      return pkey.SignMessage(Message);

    }
    public AltTxn AltCoinSign(List<PubKey> PublicKeys, string tprocToAddress, List<NoxKeys.BCUnspent> txnRaw, string tprocAmount, string tprocNoxAddressBTCAddress, string tprocFeeValue)
    {

      if (net == null) throw new Exception("Invalid currency.");

      bool ChangeAChecked = false;
      List<Key> returnKeys = new List<Key>();

      for (int i = 0; i < PublicKeys.Count; i++)
      {
        var pubKey = PublicKeys[i];

        if (pubKey.IsCompressed)
        {
          if (pubKey.GetSegwitAddress(net).ToString() == tprocNoxAddressBTCAddress) ChangeAChecked = true;
        }
        else
        {
          if (pubKey.GetAddress(net).ToString() == tprocNoxAddressBTCAddress) ChangeAChecked = true;
        }

        Key pkey = new Key(PublicKeys[i], _noxKeyGen);
        returnKeys.Add(pkey);
      }

      if (ChangeAChecked == false)
      {
        throw new Exception(Sclear.MSG_ERROR_TRANSACTION);
      }

      return MultipleTransV2(returnKeys.ToArray(), tprocToAddress, txnRaw, tprocAmount, tprocNoxAddressBTCAddress, tprocFeeValue, net);

    }

    private AltTxn MultipleTransV2(Key[] secrects, string ToAddress, List<NoxKeys.BCUnspent> PrevTransList, string Amount, string ChangeAddress, string ExactFeeAmount, Network net)
    {

      AltTxn resp = new AltTxn();

      try
      {
        var txBuilder = net.CreateTransactionBuilder();
        Transaction tx = Transaction.Create(net);
        List<ICoin> m1CoinsV = new List<ICoin>();

        long totalCoins = 0;

        foreach (NoxKeys.BCUnspent trans in PrevTransList)
        {
          var amount = Money.Parse(trans.Amount);
          totalCoins = totalCoins + amount.Satoshi;
          ICoin coin = new Coin(uint256.Parse(trans.TxHash), (uint)trans.OutputN, amount, BitcoinAddress.Create(trans.Address, net).ScriptPubKey);
          m1CoinsV.Add(coin);
        }

        var m2kChange = BitcoinAddress.Create(ChangeAddress, net);
        var m2k = BitcoinAddress.Create(ToAddress, net);

        tx = Transaction.Create(net);
        txBuilder.AddCoins(m1CoinsV.ToArray());
        txBuilder.AddKeys(secrects);

        txBuilder.Send(m2k, Money.Parse(Amount));
        txBuilder.SetChange(m2kChange);
        txBuilder.SendFees(Money.Parse(ExactFeeAmount));
        tx = txBuilder.BuildTransaction(true);
        tx.Version = 2;
        tx = txBuilder.SignTransaction(tx);
        tx.Network = net;

        if (txBuilder.Verify(tx) == true)
        {
          resp.Fee = (totalCoins - tx.TotalOut.Satoshi) * 0.00000001M;
          resp.TxnHex = tx.ToHex();
          return resp;
        }
        else
        {
          resp.IsError = true;
          resp.ErrorMessage = "Error, Not fully signed.";
          return resp;
        }
      }
      catch (Exception ex)
      {
        resp.IsError = true;
        resp.ErrorMessage = ex.Message;
        return resp;
      }
    }

  }
  public class AltTxn
  {
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public decimal Fee { get; set; }
    public string TxnHex { get; set; }
  }


}