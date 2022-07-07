using NoxKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletLibrary.Core.Abstract;
using WalletLibrary.Core.Concrete.ActiveWallets;
using static WalletLibrary.Core.Concrete.ActiveWallets.CommonWallet;

namespace WalletLibrary.Core.Concrete
{

	public class WalletActiveFactory
	{
		public enum Products { BTC = 0, ETH, XDC, DAG, LTC, GRS, DGB, DOGE, DASH, BCH }
		public static Int64 Counter = 0;

		class WalletSets
		{
			public IActiveWallet Wallet { get; set; }
			public IKey Parent { get; set; }

		}

		private Dictionary<Products, WalletSets> wallets = new Dictionary<Products, WalletSets>();

		public IActiveWallet GetWallet(Products product, uint[] indexes)
		{
			if (wallets.ContainsKey(product))
			{
				var common = wallets[product].Wallet;

				foreach (var index in indexes)
				{
					if (!common.HasIndex(index))
					{
						var key = GenerateKeyForParent(wallets[product].Parent, index);
						common.AddKey(key, index);
					}
				}
				return common;
			}

			var productStr = product.ToString().ToLower();
			var parent = GetParent(productStr);
			var wallet = ConstructWallet(product);

			foreach (var index in indexes)
			{
				var keyset = GenerateKeyForParent(parent, index);
				wallet.AddKey(keyset, index);
			}

			var save = new WalletSets()
			{
				Parent = parent,
				Wallet = wallet
			};

			if (wallets.TryAdd(product, save))
				return wallet;

			throw new Exception("Unable to add wallet.");
		}

		public IActiveWallet GetWallet(Products product, uint index)
		{

			if (wallets.ContainsKey(product))
			{
				var common = wallets[product].Wallet;

				if (!common.HasIndex(index))
				{
					var key = GenerateKeyForParent(wallets[product].Parent, index);
					common.AddKey(key, index);
				}

				return common;
			}

			var productStr = product.ToString().ToLower();
			var parent = GetParent(productStr);
			var wallet = ConstructWallet(product);
			var keyset = GenerateKeyForParent(parent, index);

			wallet.AddKey(keyset, index);

			var save = new WalletSets()
			{
				Parent = parent,
				Wallet = wallet
			};

			if (wallets.TryAdd(product, save))
				return wallet;

			throw new Exception("Unable to add wallet.");

		}


		private IKey MasterKey;
		private bool Initialized = false;
		private static int SALT_MIN_LENGTH = 5;
		private static int SECRET_MIN_LENGTH = 29;

		private void Init()
		{
			Initialized = true;
			Counter++;
		}

		private void Deinit()
		{
			Initialized = false;
			Counter--;
		}

		public static WalletActiveFactory.Products SymbolToWalletProduct(string symbol)
		{
			return (Products)Enum.Parse(typeof(Products), symbol.ToUpper());
		}

		public WalletActiveFactory(NoxManagedArray secret, NoxManagedArray salt)
		{

			if (salt.Value.Length <= SALT_MIN_LENGTH)
			{
				Thread.Sleep(500);
				throw new Exception("Invalid length. Salt should be at least 6 characters.");
			}

			if (secret.Value.Length <= SECRET_MIN_LENGTH)
			{
				Thread.Sleep(500);
				throw new Exception("Invalid length. Secret phrase should be at least 30 characters.");
			}

			MasterKey = new NativeSecp256k1Key(secret, salt);
			Init();
		}

		public string GetTestHash()
		{
			var index = (uint)Sclear.GetCurrencyIndex("test");

			using (var hmac = new System.Security.Cryptography.SHA256Managed())
			using (var derivedKey = MasterKey.DerivePrivate(index))
			using (var privateKey = derivedKey.GetPrivateKey())
			{
				return hmac.ComputeHash(privateKey.Value).ByteToHex();
			}
		}

		public IKey GetParent(string currencySymbol)
		{
			return MasterKey.DeriveCurrency(currencySymbol);
		}

		public IKey GenerateKeyForParent(IKey parent, uint index)
		{
			return parent.DerivePrivate(index);
		}

		public IActiveWallet ConstructWallet(WalletActiveFactory.Products product)
		{

			var productStr = product.ToString().ToLower();
			switch (product)
			{
				case Products.BTC:
				case Products.DGB:
				case Products.DOGE:
				case Products.GRS:
				case Products.LTC:
				case Products.DASH:
				case Products.BCH:
					return new BtcWallet(product);
				case Products.ETH:
					return new EthWallet();
				case Products.XDC:
					return new XDCWallet();
				case Products.DAG:
					return new DagWallet();

				default:
					throw new Exception(String.Format("Invalid currencySymbol: {0}", productStr));
			}
		}

		public void Dispose()
		{
			if (Initialized)
			{
				foreach (var wallet in wallets.Values)
				{
					wallet.Parent.Dispose();
					wallet.Wallet.Dispose();
				}

				MasterKey.Dispose();
				wallets.Clear();
				Deinit();
			}
		}
	}
}