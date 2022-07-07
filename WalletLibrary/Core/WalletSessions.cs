using NoxKeys;
using NoxService.SWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.ActiveApe;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete;
using WalletLibrary.Core.Concrete.ActiveWallets;
using static WalletLibrary.Core.Concrete.ActiveWallets.CommonWallet;

namespace WalletLibrary.Core
{
	class WalletSessions
	{

		const string MSG_ERROR_PROFILE = "INVALID INFO. No profile was found matching a public key derived from information entered.";

		public static void StartWallets(NoxManagedArray _ResizedNoxSecret, NoxManagedArray _ResizedNoxSalt)
		{
			SWS WSS = new SWS();
			var sgamsg = WSS.GetSGAMessage();
			if (string.IsNullOrEmpty(sgamsg))
				throw new OrderStateException("[BITFI_WS] ERROR GETTING REQUEST");

			WalletActiveFactory factory = new WalletActiveFactory(_ResizedNoxSecret, _ResizedNoxSalt);

			var tkmsg = SignBitfiMessage(sgamsg, factory);

			BitfiWallet.NoxChannel.Current.SetActiveWallet(factory, tkmsg);

		}

		static string SignBitfiMessage(string sgaMessage, WalletActiveFactory factory)
		{
			SWS WSS = new SWS();
			var wallet = factory.GetWallet(WalletActiveFactory.Products.BTC, 0);
			string Address = wallet.GetLegacyAddress(0);
			var signer = wallet.GetSigner(NativeSecp256k1ECDSASigner.SignatureType.RECOVERABLE_COMPACT, 0);
			var mesBytes = SGAAuthenticator.FormatMessageForSigning(Encoding.UTF8.GetBytes(sgaMessage));
			var signature = signer.Sign(mesBytes);

			string tkmsg = WSS.GetSGAToken(Address, Convert.ToBase64String(signature));

			try
			{
				Convert.FromBase64String(tkmsg);
			}
			catch (Exception ex)
			{
				factory.Dispose();
				throw new Exception(MSG_ERROR_PROFILE);
			}

			return tkmsg;

		}



	}
}
