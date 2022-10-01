using System;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EthereumLibrary.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Math;

namespace DagLibrary
{
	class TransactionV2
	{
		private const UInt64 MIN_SALT = (2 << 53 - 1) - (2 << 48);
		private string hash;
		private string rle;

		public TxV2 tx;

		public Object getPostTransaction()
		{
			return this.tx;
		}



		public TransactionV2(string from, string to, BigInteger amountSat, BigInteger feeSat, LastTxRef lastTxRef)
		{
			this.tx = BuildTx(from, to, amountSat, feeSat, lastTxRef);
			string encodedTx = EncodeTx(this.tx);
			string prefix = "03" + Utils.ByteArray2hex(Utils.Utf8Length((UInt32)(encodedTx.Length + 1))).ToLower();
			byte[] bytes = Encoding.Default.GetBytes(encodedTx);
			string coded = BitConverter.ToString(bytes).Replace("-", "");
			string serialized = (prefix + coded).ToLower();
			byte[] serializedBytes = Utils.Hex2ByteArray(serialized);
			byte[] hashBytes = Hash.Sha256(serializedBytes);
			this.hash = Utils.ByteArray2hex(hashBytes).ToLower();
			this.rle = encodedTx;
		}

		public TransactionV2(string from, string to, string amount, string fee, LastTxRef lastTxRef)
: this(from, to, new BigInteger(amount), new BigInteger(fee), lastTxRef)
		{

		}

		public TransactionV2(string from, string to, decimal amount, decimal fee, LastTxRef lastTxRef)
: this(from, to, new BigInteger(Utils.removeDecimals(amount * 1e8m)), new BigInteger(Utils.removeDecimals(fee * 1e8m)), lastTxRef)
		{

		}


		public Boolean sign(byte[] sk, string uncompressedPk)
		{
			var hash = this.hash;
			var derSignature = DagSigner.DagSign(sk, hash);
			string hexDerSiganture = Utils.ByteArray2hex(derSignature).ToLower();

			var signature = new TxV2.Proof
			{
				signature = hexDerSiganture,
				id = uncompressedPk.Substring(2)
			};

			this.tx.proofs = new TxV2.Proof[] {
								signature
						};

			return DagSigner.DagVerify(
					uncompressedPk.HexToByteArray(),
					hash, hexDerSiganture
			);
		}

		private string EncodeTx(TxV2 tx)
		{
			var parentCount = "2";
			var sourceAddress = tx.value.source;
			var destAddress = tx.value.destination;
			var amount = new BigInteger(tx.value.amount).ToString(16);
			var parentHash = this.tx.value.parent.hash;
			var ordinal = this.tx.value.parent.ordinal.ToString();
			var fee = this.tx.value.fee;
			var salt = new BigInteger(this.tx.value.salt.ToString()).ToString(16);

			return String.Join("", new String[] {
								parentCount,
								sourceAddress.Length.ToString(),
								sourceAddress,
								destAddress.Length.ToString(),
								destAddress,
								amount.Length.ToString(),
								amount,
								parentHash.Length.ToString(),
								parentHash,
								ordinal.Length.ToString(),
								ordinal,
								fee.Length.ToString(),
								fee,
								salt.Length.ToString(),
								salt
						});
		}

		private TxV2 BuildTx(string from, string to, BigInteger amountSat,
				BigInteger feeSat, LastTxRef lastTxRef)
		{

			UInt64 salt = MIN_SALT + Utils.BytesToUint64(Utils.GenRandom(6));
			return new TxV2
			{
				value = new TxV2.Value
				{
					amount = amountSat.ToString(),
					salt = salt,
					source = from,
					fee = feeSat.ToString(),
					destination = to,
					parent = new TxV2.Value.Parent
					{
						hash = lastTxRef.prevHash,
						ordinal = lastTxRef.ordinal
					}
				},
				proofs = new TxV2.Proof[] {

								}
			};
		}
	}

	struct TxV2
	{
		public struct Proof
		{
			public string id;
			public string signature;
		}

		public struct Value
		{
			public class Parent
			{
				public string hash;
				public Int64 ordinal;
			}

			public string source;
			public string destination;
			public string amount;
			public string fee;
			public Parent parent;
			public UInt64 salt;
		}

		public Value value;
		public Proof[] proofs;
	}
}
