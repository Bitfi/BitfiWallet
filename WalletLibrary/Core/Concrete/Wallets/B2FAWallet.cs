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
  public class B2FAWallet : CommonWallet, IWallet
  {
    public string Symbol { get; private set; } = "bfa";

    private NoxKeyGen NoxKeyGen;

    public B2FAWallet(KeySet[] keySets, WalletFactory.Products product = WalletFactory.Products.BFA)
      : base(keySets, product)
    {
      NoxKeyGen = new NoxKeyGen(keySets);
    }

    public string GetLegacyAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public PaymentRequestResponse SignPaymentRequest(NoxTxnProcess req)
    {

      throw new NotImplementedException();

    }

    public string GetSegwitAddress(uint index = 0)
    {
      throw new NotImplementedException();
    }

    public MsgTaskTransferResponse SignMessage(NoxMsgProcess req)
    {
      throw new NotImplementedException();
    }

  }
}

