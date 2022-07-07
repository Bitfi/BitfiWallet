using NoxKeys;
using NoxService.NWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.ActiveApe;
using WalletLibrary.Core.Concrete;

namespace WalletLibrary.Core.Abstract
{
	public interface IActiveWallet : IDisposable
	{
		string Symbol { get; }

		string GetLegacyAddress(uint index);
		string GetSegwitAddress(uint index);
		string GetPubKey(uint index);

		TransferInfoRespose SignPaymentRequest(TransferInfo info, List<SatusListItem> PromptInfo, uint index);
		MsgTaskTransferResponse SignMessage(NoxService.NWS.NoxMsgProcess req, uint index);

		ISigner GetSigner(NativeSecp256k1ECDSASigner.SignatureType type, UInt32 index);

		void AddKey(IKey keySet, UInt32 index);

		bool HasIndex(UInt32 index);

	}
}
