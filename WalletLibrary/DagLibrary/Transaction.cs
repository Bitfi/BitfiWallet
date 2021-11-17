using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace DagLibrary
{
  class Transaction
  {

    private const UInt64 MIN_SALT = (2 << 53 - 1) - (2 << 48);
    private string from;
    private string to;
    private decimal amount;
    public Tx tx;

    public Transaction(string from, string to, decimal amount, decimal fee, LastTxRef lastTxRef)
    {
      this.from = from;
      this.to = to;
      this.amount = amount;

      var amountSat = new BigInteger(Utils.removeDecimals(amount * 1e8m));
      var feeSat = new BigInteger(Utils.removeDecimals(fee * 1e8m));

      this.tx = BuildTx(from, to, amountSat, feeSat, lastTxRef);
      string hashReference = EncodeTx(this.tx, true);
      this.tx.edge.observationEdge.data.hashReference = hashReference;
      string encodedTx = EncodeTx(this.tx, false);

      string prefix = "0301" + Utils.ByteArray2hex(Utils.Utf8Length((UInt32)(encodedTx.Length + 1))).ToLower();

      byte[] bytes = Encoding.Default.GetBytes(encodedTx);
      string coded = BitConverter.ToString(bytes).Replace("-", "");
      string serialized = (prefix + coded).ToLower();

      byte[] serializedBytes = Utils.Hex2ByteArray(serialized);
      byte[] hashBytes = Hash.Sha256(serializedBytes);
      string hash = Utils.ByteArray2hex(hashBytes).ToLower();
      this.tx.edge.signedObservationEdge.signatureBatch.hash = hash;
    }

    public Boolean sign(byte[] sk)
    {
      var hash = this.tx.edge.signedObservationEdge.signatureBatch.hash;
      var signer = new ECDSASecp256k1Signer();
      var derSignature = signer.Sign(sk, hash);
      string hexDerSiganture = Utils.ByteArray2hex(derSignature).ToLower();
      string uncompressedPk = Account.GetPublicKeyFromPrivatekey(sk, false);


      var signature = new Tx.SignedObservationEdge.SignatureBatch.Signature
      {
        signature = hexDerSiganture,
        id = new Tx.SignedObservationEdge.SignatureBatch.Signature.ID
        {
          hex = uncompressedPk.Substring(2) //remove 04 prefix
        }
      };

      var list = this.tx.edge.signedObservationEdge.signatureBatch.signatures.ToList();
      list.Add(signature);
      this.tx.edge.signedObservationEdge.signatureBatch.signatures = list.ToArray();

      return true;
    }

    private string EncodeTx(Tx tx, Boolean hashReference)
    {
      string parentsTx = "";

      if (!hashReference)
      {
        parentsTx += tx.edge.observationEdge.parents.Length.ToString();
        parentsTx += tx.edge
          .observationEdge
          .parents
          .Aggregate("", (acc, p) => $"{acc}{p.hashReference.Length}{p.hashReference}");
      }

      string encodedTx = "";
      string amount = new BigInteger(tx.edge.data.amount).ToString(16);
      encodedTx += amount.Length;
      encodedTx += amount;

      encodedTx += tx.lastTxRef.prevHash.Length;
      encodedTx += tx.lastTxRef.prevHash;

      string ordinal = tx.lastTxRef.ordinal.ToString();
      encodedTx += ordinal.Length;
      encodedTx += ordinal;

      string fee = tx.edge.data.fee;
      encodedTx += fee.Length;
      encodedTx += fee;

      string salt = tx.edge.data.salt.ToString("x");
      encodedTx += salt.Length;
      encodedTx += salt;

      return parentsTx + (encodedTx).ToLower();
    }

    private Tx BuildTx(string from, string to, BigInteger amountSat,
      BigInteger feeSat, LastTxRef lastTxRef)
    {

      UInt64 salt = MIN_SALT + Utils.BytesToUint64(Utils.GenRandom(6));
      return new Tx
      {
        edge = new Tx.Edge
        {
          observationEdge = new Tx.ObservationEdge
          {
            parents = new Tx.ObservationEdge.ObservationEdgeData[] {
              new Tx.ObservationEdge.ObservationEdgeData {
                hashReference = from,
                hashType = Tx.ObservationEdge.HashType.AddressHash
              },
              new Tx.ObservationEdge.ObservationEdgeData {
                hashReference = to,
                hashType = Tx.ObservationEdge.HashType.AddressHash
              }
            },
            data = new Tx.ObservationEdge.ObservationEdgeData
            {
              hashReference = "",
              hashType = Tx.ObservationEdge.HashType.TransactionDataHash
            }
          },

          signedObservationEdge = new Tx.SignedObservationEdge
          {
            signatureBatch = new Tx.SignedObservationEdge.SignatureBatch
            {
              hash = "",
              signatures = new Tx.SignedObservationEdge.SignatureBatch.Signature[] { }
            }
          },

          data = new Tx.Data
          {
            lastTxRef = new LastTxRef
            {
              prevHash = lastTxRef.prevHash,
              ordinal = lastTxRef.ordinal
            },
            fee = feeSat.ToString(),
            amount = amountSat.ToString(),
            salt = salt
          }
        },

        isDummy = false,
        isTest = false,
        lastTxRef = new LastTxRef
        {
          prevHash = lastTxRef.prevHash,
          ordinal = lastTxRef.ordinal
        }
      };
    }
  }

  public struct LastTxRef
  {
    public string prevHash;
    public Int64 ordinal;
  }

  struct Tx
  {
    public struct ObservationEdge
    {
      public class HashType
      {
        public const string AddressHash = "AddressHash";
        public const string TransactionDataHash = "TransactionDataHash";
        public const string None = "";
      }

      public struct ObservationEdgeData
      {
        public string hashReference;
        public string hashType;
      }

      public ObservationEdgeData[] parents;
      public ObservationEdgeData data;
    }

    public struct SignedObservationEdge
    {
      public struct SignatureBatch
      {
        public struct Signature
        {
          public struct ID
          {
            public string hex;
          }

          public string signature;
          public ID id;
        }
        public string hash;
        public Signature[] signatures;
      }
      public SignatureBatch signatureBatch;
    }

    public struct Data
    {
      public string amount;
      public LastTxRef lastTxRef;
      public UInt64 salt;
      public string fee;
    }

    public struct Edge
    {
      public ObservationEdge observationEdge;
      public SignedObservationEdge signedObservationEdge;
      public Data data;
    }

    public Edge edge;
    public Boolean isDummy;
    public Boolean isTest;
    public LastTxRef lastTxRef;
  }
}

