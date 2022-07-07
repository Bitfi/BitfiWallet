using EthereumLibrary.Hex.HexConvertors.Extensions;
using EthereumLibrary.RPL;
using System;
using System.Numerics;

namespace EthereumLibrary.Signer
{
	public class Transaction : TransactionBase
	{
		public Transaction(byte[] rawData)
		{
			SimpleRlpSigner = new RLPSigner(rawData, NUMBER_ENCODING_ELEMENTS);
			ValidateValidV(SimpleRlpSigner);
		}

		public Transaction(RLPSigner rlpSigner)
		{
			ValidateValidV(rlpSigner);
			SimpleRlpSigner = rlpSigner;
		}

		private static void ValidateValidV(RLPSigner rlpSigner)
		{
			if (rlpSigner.IsVSignatureForChain())
				throw new Exception("TransactionChainId should be used instead of Transaction");
		}

		public Transaction(byte[] nonce, byte[] gasPrice, byte[] gasLimit, byte[] receiveAddress, byte[] value,
						byte[] data)
		{
			SimpleRlpSigner = new RLPSigner(GetElementsInOrder(nonce, gasPrice, gasLimit, receiveAddress, value, data));
		}

		public Transaction(string to, BigInteger amount, BigInteger nonce, BigInteger gasPrice, BigInteger gasLimit)
						: this(to, amount, nonce, gasPrice, gasLimit, "")
		{
		}

		public Transaction(string to, BigInteger amount, BigInteger nonce, BigInteger gasPrice,
						BigInteger gasLimit, string data) : this(nonce.ToBytesForRLPEncoding(), gasPrice.ToBytesForRLPEncoding(),
						gasLimit.ToBytesForRLPEncoding(), to.HexToByteArray(), amount.ToBytesForRLPEncoding(), data.HexToByteArray()
		)
		{
		}

		public Transaction(string ercAddress, string to, BigInteger amount, BigInteger nonce, BigInteger gasPrice,
						BigInteger gasLimit) : this(ercAddress, BigInteger.Zero, nonce, gasPrice, gasLimit,
						GetTransferCall(to.RemoveHexPrefix(), amount))
		{
		}

		public Transaction(string ercAddress, string to, BigInteger amount, BigInteger nonce, BigInteger gasPrice,
				BigInteger gasLimit, string BlindContract) : this(ercAddress, BigInteger.Zero, nonce, gasPrice, gasLimit,
		BlindContract)
		{

		}
		public string ToJsonHex()
		{
			var s = "['{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}']";
			return string.Format(s, Nonce.ToHex(),
							GasPrice.ToHex(), GasLimit.ToHex(), ReceiveAddress.ToHex(), Value.ToHex(), ToHex(Data),
							Signature.V.ToHex(),
							Signature.R.ToHex(),
							Signature.S.ToHex());
		}

		private byte[][] GetElementsInOrder(byte[] nonce, byte[] gasPrice, byte[] gasLimit, byte[] receiveAddress,
						byte[] value,
						byte[] data)
		{
			if (receiveAddress == null)
				receiveAddress = EMPTY_BYTE_ARRAY;
			//order  nonce, gasPrice, gasLimit, receiveAddress, value, data
			return new[] { nonce, gasPrice, gasLimit, receiveAddress, value, data };
		}

		private static string Fill256Address(string address)
		{
			var str = new String('0', 64) + address;
			return str.Substring(str.Length - 64);
		}

		private static string Fill256Balance(BigInteger amount)
		{
			var str = new String('0', 64) + amount.ToString("x");
			return str.Substring(str.Length - 64);
		}

		private static string GetTransferCall(string to, BigInteger amount)
		{
			var transferMethodId = "a9059cbb";
			return "0x" + transferMethodId + Fill256Address(to) + Fill256Balance(amount);
		}

		public string ToHex()
		{
			return GetRLPEncoded().ToHex();
		}

		public override EthECKey Key => EthECKey.RecoverFromSignature(SimpleRlpSigner.Signature, SimpleRlpSigner.RawHash);
	}
}
