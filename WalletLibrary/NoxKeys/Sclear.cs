using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using NBitcoin;


namespace NoxKeys
{
	public class Sclear
	{

		internal static int GetCurrencyIndex(string currencySymbol)
		{
			currencySymbol = currencySymbol.ToLower();
			string currencyIndexString = "";
			foreach (char c in currencySymbol)
			{
				currencyIndexString = currencyIndexString + (char.ToUpper(c) - 64).ToString();
			}
			return Convert.ToInt32(currencyIndexString);
		}
		public static Network GetBLKNetworkAlt(string blk)
		{
			Network net = null;

			switch (blk)
			{
				case "btc":
					net = Network.Main;
					break;

				case "ltc":
					net = NBitcoin.Altcoins.Litecoin.Instance.Mainnet;
					break;

				case "grs":
					net = NBitcoin.Altcoins.Groestlcoin.Instance.Mainnet;
					break;

				case "ftc":
					net = NBitcoin.Altcoins.Feathercoin.Instance.Mainnet;
					break;

				case "via":
					net = NBitcoin.Altcoins.Viacoin.Instance.Mainnet;
					break;

				case "doge":
					net = NBitcoin.Altcoins.Dogecoin.Instance.Mainnet;
					break;

				case "btg":
					net = NBitcoin.Altcoins.BGold.Instance.Mainnet;
					break;

				case "mona":
					net = NBitcoin.Altcoins.Monacoin.Instance.Mainnet;
					break;

				case "dash":
					net = NBitcoin.Altcoins.Dash.Instance.Mainnet;
					break;

				case "zcl":
					net = NBitcoin.Altcoins.Zclassic.Instance.Mainnet;
					break;

				case "strat":
					net = NBitcoin.Altcoins.Stratis.Instance.Mainnet;
					break;

				case "dgb":
					net = NBitcoin.Altcoins.DigiByte.Instance.Mainnet;
					break;

				case "qtum":
					net = NBitcoin.Altcoins.Qtum.Instance.Mainnet;
					break;

				case "bch":
					net = NBitcoin.Altcoins.BCash.Instance.Mainnet;
					break;
			}


			return net;
		}

		public const string MSG_ERROR_PROFILE = "INVALID INFO. No profile was found matching a public key derived from information entered.";
		public const string MSG_ERROR_ADDRESS = "INVALID INFO. Please provide the same information used when creating initial profile addresses.";
		public const string MSG_ERROR_TRANSACTION = "INVALID INFO. Keys required to spend this asset cannot be derived from information entered.";
		public const string MSG_ERROR_SIGNMESSAGE = "INVALID INFO. The corresponding private key cannot be derived from information entered.";

	}
}
