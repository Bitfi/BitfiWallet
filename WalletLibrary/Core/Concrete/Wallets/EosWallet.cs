using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NoxKeys;
using NoxService.NWS;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Utils;
using WalletLibrary.EosCore.ActionArgs;
using WalletLibrary.EosCore.ECDSA;
using WalletLibrary.EosCore.Providers;
using WalletLibrary.EosCore.Serialization;
using WalletLibrary.EosCore.Utilities;

namespace WalletLibrary.Core.Concrete.Wallets
{
  class EosWallet : CommonWallet, IWallet
  {
    public const string PUBLIC_WIF_PREFIX = "EOS";
    private NoxKeyGen noxKeyGen;
    public struct Block
    {
      public uint BlockNum { get; set; }
      public uint RefBlockPrefix { get; set; }
      public DateTime TimestampDatetime { get; set; }
    }

    public class Data
    {
      public EosCore.Params.Transaction Tx { get; set; }
      public string ChainIdHex { get; set; }
    }

    public EosWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.EOS)
      : base(keySets, product)
    {
      noxKeyGen = new NoxKeyGen(keySets);
    }

    public string GetLegacyAddress(uint index = 0)
    {
      using (var key = KeySets[0].Key)
      using (var publicKey = key.GetPublicKey())
      {
        return EosCore.Utilities.WifUtility.GetPublicWif(publicKey.Value, PUBLIC_WIF_PREFIX);
      }
    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      throw new NotImplementedException();
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {

      PaymentRequestResponse response = new PaymentRequestResponse();
      using (var key = KeySets[0].Key)
      using (var privateKey = key.GetPrivateKey())
      using (var publicKey = key.GetPublicKey())
      {
        var data = DeserializeMXTxn<Data>(req.MXTxn);

        if (data.Tx.actions.Count() != 1)
          throw new Exception("It should not happen! Only 1 action is supported");

        var actionType = data.Tx.actions[0].name.value.ToLower();
        var contract = data.Tx.actions[0].account.value.ToLower();
        var dataHex = data.Tx.actions[0].data.value.ToLower();
        var abiSerializator = new AbiSerializationProvider();
        var abi = new Abi();



        if (contract == "eosio")
        {
          abi = AbiSerializationProvider.eosAbi;
        }
        else if (contract == "eosio.token")
        {
          abi = AbiSerializationProvider.eosTokenAbi;
        }
        else
          throw new Exception(String.Format("This should not happen! Unknown contract {0}. Please, contact bitfi support!", contract));

        var eosAddition = new EosAddition { };
        switch (actionType)
        {
          case "delegatebw":
            var stakeArgs = abiSerializator.DeserializeActionData<EosStakeArgs>(actionType, dataHex, abi);
            eosAddition.EosStakeArgs = stakeArgs;
            if (eosAddition.EosStakeArgs.transfer) throw new Exception("Validation failed. [transfer must be false]");
            if (eosAddition.EosStakeArgs.receiver != eosAddition.EosStakeArgs.from) throw new Exception("Validation failed. [receiver/from missmatch]");
       
            break;
          case "undelegatebw":
            var unstakeArgs = abiSerializator.DeserializeActionData<EosUnstakeArgs>(actionType, dataHex, abi);
            eosAddition.EosUnstakeArgs = unstakeArgs;
            if (eosAddition.EosUnstakeArgs.transfer) throw new Exception("Validation failed. [transfer must be false]");
            if (eosAddition.EosUnstakeArgs.receiver != eosAddition.EosUnstakeArgs.from) throw new Exception("Validation failed. [receiver/from missmatch]");
          
            break;
          case "sellram":
            var sellRamArgs = abiSerializator.DeserializeActionData<SellRamArgs>(actionType, dataHex, abi);
            eosAddition.SellRamArgs = sellRamArgs;
     
            break;
          case "buyrambytes":
            var buyRamArgs = abiSerializator.DeserializeActionData<BuyRamArgs>(actionType, dataHex, abi);
            eosAddition.BuyRamArgs = buyRamArgs;
            if (eosAddition.BuyRamArgs.receiver != eosAddition.BuyRamArgs.payer) throw new Exception("Validation failed. [receiver/payer missmatch]");
         
            break;
          case "transfer":
            var transferArgs = abiSerializator.DeserializeActionData<TransferArgs>(actionType, dataHex, abi);
            eosAddition.TransferArgs = transferArgs;
            if (NumUtils.Utils.ToSatoshi(req.Amount, 4) != NumUtils.Utils.ToSatoshi(eosAddition.TransferArgs.quantity.Replace(" EOS", ""), 4))
            {
              throw new Exception("Validation failed. [amount]");
            }
          
            break;
          default:
            throw new Exception(String.Format("This should not happen! Unknown action type: {0}. Please, contact bitfi support", actionType));
        }

        string fromadr = EosCore.Utilities.WifUtility.GetPublicWif(publicKey.Value, PUBLIC_WIF_PREFIX);

        if (fromadr.ToUpper() != req.NoxAddress.BTCAddress.ToUpper())
        {
          throw new Exception(Sclear.MSG_ERROR_TRANSACTION);
        }

        var deserializedData = DeserializeMXTxn<Data>(req.MXTxn);

        var packedTransaction = new PackingSerializer().Serialize<EosCore.Params.Transaction>(deserializedData.Tx);
        var chainId = EosCore.ECDSA.Hex.HexToBytes(data.ChainIdHex);
        var message = new byte[chainId.Length + packedTransaction.Length + 32];
        Array.Copy(chainId, message, chainId.Length);
        Array.Copy(packedTransaction, 0, message, chainId.Length, packedTransaction.Length);
        var messageHash = Sha256Manager.GetHash(message);

      

        string[] signatures = new string[1];
        signatures[0] = WifUtility.EncodeSignature(Secp256K1Manager.SignCompressedCompact(messageHash, privateKey.Value));


        var signedTx = new EosCore.Params.PushTransactionParam
        {
          packed_trx = EosCore.ECDSA.Hex.ToString(packedTransaction),
          signatures = signatures,
          packed_context_free_data = string.Empty,
          compression = "none"
        };

        response.Addition = new Addition() { TransactionType = actionType, ContractType = contract };
        response.TransactionType = actionType;
        response.EosAddition = eosAddition;
        response.TxnHex = signedTx.ToBase64();

        return response;
      }
    }
  }
}
