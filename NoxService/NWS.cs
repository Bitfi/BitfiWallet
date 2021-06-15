using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using BitfiWallet;

namespace NoxService.NWS
{
	public class NWS
	{
		Device noxDevice
		{
			get
			{
				return NoxDPM.NoxT.noxDevice;
			}
		}

		NOXWS2 serv
		{
			get
			{
				return NoxDPM.NoxT.serv;
			}
		}

		public string RespondFirstAddressRequest(string TXNLineID, FirstAdrCollection Addresses)
		{
			try
			{
				serv.Timeout = 60000;
				string msg = Addresses.BTC + Addresses.ETH + Addresses.LTC + Addresses.Monero + Addresses.NEO;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.RespondFirstAddressRequest(signature, msg, noxDevice.DevicePubHash(), TXNLineID, Addresses);

			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string RespondAddressRequest(string TXNLineID, string adr)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = adr;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.RespondAddressRequestV3(signature, msg, noxDevice.DevicePubHash(), TXNLineID, adr);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string[] GetFirstAddress(string TXNLineID)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.GetFirstAddress(signature, msg, noxDevice.DevicePubHash(), TXNLineID);

			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string XMRGetRandom(string mixin, string[] amounts)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.XMRGetRandom(signature, msg, noxDevice.DevicePubHash(), mixin, amounts);


				return resp;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public NoxAddressRequests GetRequest(string actiontask)
		{

			try
			{
				serv.Timeout = 10000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetRequest(signature, msg, noxDevice.DevicePubHash(), actiontask);

				if (resp == null) return null;

				if (noxDevice.ValidMsg(resp.Response.SMSToken + resp.Response.BlkNet + resp.Response.HDIndex.ToString(), resp.Signature))
				{
					return resp.Response;
				}

				return null;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string SubmitTxnResponseMX(string TXNLineID, string TxnHex, string[] SpendKeyImages)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = TxnHex;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.SubmitTxnResponseMX(signature, msg, noxDevice.DevicePubHash(), TXNLineID, TxnHex, SpendKeyImages);

				return resp;
			}
			catch (WebException)
			{
				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string SubmitTxnResponse(string TXNLineID, string TxnHex)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = TxnHex;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.SubmitTxnResponse(signature, msg, noxDevice.DevicePubHash(), TXNLineID, TxnHex);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string SubmitMsgResponse(string TXNLineID, string MsgSig)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = MsgSig;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.SubmitMsgResponse(signature, msg, noxDevice.DevicePubHash(), TXNLineID, MsgSig);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public NoxMsgProcess GetMsgRequest(string actiontask)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetMsgRequest(signature, msg, noxDevice.DevicePubHash(), actiontask);
				if (noxDevice.ValidMsg(resp.Response.Msg, resp.Signature))
				{
					return resp.Response;
				}

				return null;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}

		public NoxSwapRequests GetSwapRequest(string SMSToken, string[] Signature)
		{

			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.GetSwapRequest(signature, msg, noxDevice.DevicePubHash(), SMSToken + "|" + Signature[0] + "|" + Signature[1]);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}

		public SignedNoxTxnProcess GetSwapTransaction(string SMSToken, string ResponseObject)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetSwapTransaction(signature, msg, noxDevice.DevicePubHash(), SMSToken, ResponseObject);


				return resp;

			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public NoxTxnProcess GetTxnRequest(string actiontask)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetTxnRequestV4(signature, msg, noxDevice.DevicePubHash(), actiontask);
				if (noxDevice.ValidMsg(resp.Response.Amount + resp.Response.BlkNet + resp.Response.ToAddress + resp.Response.FeeValue + resp.Response.FeeTotal + resp.Response.MXTxn + resp.Response.ETCGasUsed + resp.Response.ETCNonce + resp.Response.ETCToken + resp.Response.USDRate.ToString(), resp.Signature))
				{
					return resp.Response;
				}

				return null;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}
		public string SubmitGasResponse(string TXNLineID, string txHex)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = txHex;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.SubmitGasResponse(signature, msg, noxDevice.DevicePubHash(), TXNLineID, txHex);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public NoxGasRequests GetGasRequest(string actiontask)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetGasRequest(signature, msg, noxDevice.DevicePubHash(), actiontask);
				if (resp == null) return null;

				if (noxDevice.ValidMsg(resp.Response.SMSToken + resp.Response.TXNLineID, resp.Signature))
				{
					return resp.Response;
				}

				return null;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public string RespondImageRequest(string TXNLineID, string[] images, string Address)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = Address;
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				return serv.RespondImageRequestV2(signature, msg, noxDevice.DevicePubHash(), TXNLineID, images, Address);
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}
		public NoxImageRequests GetImageRequest(string actiontask)
		{
			try
			{
				serv.Timeout = 30000;
				string msg = Guid.NewGuid().ToString();
				msg = noxDevice.SHashSHA1(msg);
				string signature = noxDevice.SignMsg(msg);
				var resp = serv.GetImageRequestV2(signature, msg, noxDevice.DevicePubHash(), actiontask);

				if (resp == null) return null;

				if (noxDevice.ValidMsg(resp.Response.SMSToken + resp.Response.TXNLineID, resp.Signature))
				{
					return resp.Response;
				}

				return null;
			}
			catch (WebException)
			{

				return null;
			}
			catch (Exception)
			{
				return null;
			}

		}

	}
}