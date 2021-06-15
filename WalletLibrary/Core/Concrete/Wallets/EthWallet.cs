using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.RPL;
using EthereumLibrary.Signer;
using NBitcoin;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;

namespace WalletLibrary.Core.Concrete.Wallets
{
  public class EthWallet : CommonWallet, IWallet
  {
    public string Symbol { get; private set; } = "eth";
    //to do: rewrite, i had to implement it because of previous design
    private NoxKeyGen NoxKeyGen;

    public EthWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.ETH) 
      : base(keySets, product)
    {
      NoxKeyGen = new NoxKeyGen(keySets);
    }

    public string GetLegacyAddress(uint index = 0)
    {
      if (!HasIndex(index))
        throw new Exception(String.Format("Unable to find such index: {0} in the wallet", index));

      var keySet = KeySets.FirstOrDefault(k => k.Index == index);
      using (var pubKeyUncompressed = keySet.Key.GetPublicKey(false))
      {
        //todo: rewrite using native methods
        var pubKey = new PubKey(pubKeyUncompressed.Value, (int)index, Symbol);

        EthECKey ekey = new EthECKey(pubKey, NoxKeyGen);
        return ekey.GetPublicAddress();
      }
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {
      PaymentRequestResponse response = new PaymentRequestResponse();
      
      string ethadr = GetLegacyAddress();
      if (ethadr.ToLower() != req.NoxAddress.BTCAddress.ToLower())
        throw new Exception(Sclear.MSG_ERROR_TRANSACTION);

      using (var publicKey = KeySets[0].Key.GetPublicKey(false))
      {
        PubKey pubKey = new PubKey(publicKey.Value, 0, Symbol);
        EthECKey key = new EthECKey(pubKey, NoxKeyGen);

        if (!string.IsNullOrEmpty(req.ETCToken))
        {
          var contractAddress = req.ETCToken.EnsureHexPrefix();
          var to = req.ToAddress.EnsureHexPrefix();
          var amount = NumUtils.Utils.ToSatoshi(req.Amount, 18);
          var nonce = System.Numerics.BigInteger.Parse(req.ETCNonce);
          var gasPrice = NumUtils.Utils.ToSatoshi(req.FeeValue, 18);
          var gasLimit = new System.Numerics.BigInteger(Int64.Parse(req.ETCGasUsed));
          var tx = new EthereumLibrary.Signer.Transaction(contractAddress, to, amount, nonce, gasPrice, gasLimit);
          tx.Sign(key);
          var signedHex = tx.ToHex();

          response.TxnHex = signedHex;
          var ethFeeLimitSat = tx.GasLimit.ToBigIntegerFromRLPDecoded() * tx.GasPrice.ToBigIntegerFromRLPDecoded();
          var ethFeeLimitBtc = NumUtils.Utils.FromSatoshi(ethFeeLimitSat, 18);
          response.Addition = new Addition
          {
            MaxFeeAllowed = ethFeeLimitBtc,
            ContractAddress = contractAddress
          };
        }
        else
        {
          var to = req.ToAddress.EnsureHexPrefix();
          var amount = NumUtils.Utils.ToSatoshi(req.Amount, 18);
          var nonce = System.Numerics.BigInteger.Parse(req.ETCNonce);
          var gasPrice = NumUtils.Utils.ToSatoshi(req.FeeValue, 18);
          var gasLimit = new System.Numerics.BigInteger(Int64.Parse(req.ETCGasUsed));

          var tx = new EthereumLibrary.Signer.Transaction(to, amount, nonce, gasPrice, gasLimit);
          tx.Sign(key);
          var signedHex = tx.ToHex();

          response.TxnHex = signedHex;

          var ethFeeLimitSat = tx.GasLimit.ToBigIntegerFromRLPDecoded() * tx.GasPrice.ToBigIntegerFromRLPDecoded();
          var ethFeeLimitBtc = NumUtils.Utils.FromSatoshi(ethFeeLimitSat, 18);
          response.Addition = new Addition
          {
            MaxFeeAllowed = ethFeeLimitBtc
          };
        }

        return response;
      }
    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      string ethadr = GetLegacyAddress();
      if (ethadr.ToLower() != req.NoxAddress.BTCAddress.ToLower())
        throw new Exception(Sclear.MSG_ERROR_SIGNMESSAGE);

      MsgTaskTransferResponse response = new MsgTaskTransferResponse();
      byte[] Msg = Convert.FromBase64String(req.Msg);

      using (var publicKeyUncompressed = KeySets[0].Key.GetPublicKey(false))
      {
        PubKey pubKeyUncompressed = new PubKey(publicKeyUncompressed.Value, req.NoxAddress.HDIndex, req.BlkNet);
        EthECKey ekey = new EthECKey(pubKeyUncompressed, NoxKeyGen);
        EthereumLibrary.MsgSigning elib = new EthereumLibrary.MsgSigning();
        response.MsgSig = elib.ETHMsgSign(Msg, ekey);
      }

      return response;
    }
  }
}
