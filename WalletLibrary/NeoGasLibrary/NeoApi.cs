using NeoGasLibrary.Cryptography;
using NeoGasLibrary.Neoscan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NeoGasLibrary.NeoAPI.Transaction;

namespace NeoGasLibrary
{
    public static class NeoAPI
    {
        public enum Net
        {
            Main,
            Test
        }

        // hard-code asset ids for NEO and GAS
        public const string neoId = "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        public const string gasId = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public struct InvokeResult
        {
            public string state;
            public decimal gasSpent;
            public object result;
        }

        private static Transaction Sign(this Transaction transaction, KeyPair key)
        {
            var txdata = SerializeTransaction(transaction, false);
            var txstr = txdata.HexToBytes();
            /*
             * var de = DeserializeTransaction(txdata, false);
            var sad = DeserializeTransaction("80000001ebf44735d11529c85f2c5c1fbd295eca38064621c77c67e05a1072d24dfea3430200" +
              "019b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc50065cd1d00000000c35befff207c56bc1648efcfe" +
              "70824186609e63f0141405dae3d266348364042b7d432c863377b2efa1d4c178250341fb4a15bb46a695dfbc07d569d2e97cd75319f" +
              "b309ab9952480efc443b51a1da8635a82b4016a1d0232102b38857ac93546595d69115cce83c46f7e3e0e9e1f71bc12cb11fd475fc352c89ac", true);
            */
            var privkey = key.PrivateKey;
            var pubkey = key.PublicKey;
            var signature = Crypto.Default.Sign(txstr, privkey, pubkey);
            var isVerified = Crypto.Default.VerifySignature(txstr, signature, pubkey);
            if (!isVerified)
            {
                throw new Exception("Transaction signing is not correct");
            }

            var invocationScript = "40" + signature.ByteToHex();
            var verificationScript = key.signatureScript;
            transaction.witnesses = new Transaction.Witness[] { new Transaction.Witness() { invocationScript = invocationScript, verificationScript = verificationScript } };

            return transaction;
        }

        #region NEEDS CLEANUP
        public struct Transaction
        {
            public struct Witness
            {
                public string invocationScript;
                public string verificationScript;
            }

            public struct Input
            {
                public string prevHash;
                public uint prevIndex;
            }

            public struct Output
            {
                public string scriptHash;
                public string assetID;
                public decimal value;
            }

            public struct Claim
            {
                public string prevHash;
                public uint prevIndex;
            }

            public byte type;
            public byte version;
            public byte[] script;
            public decimal gas;

            public Input[] inputs;
            public Output[] outputs;
            public Witness[] witnesses;
            public Claim[] claims;
        }
        #endregion

        private static string num2hexstring(long num, int size = 2)
        {
            return num.ToString("X" + size);
        }

        private static long varIntToNum(string hex, out int len)
        {
            var prefix = hex.Substring(0, 2);
            var value = prefix;
            len = 2;
            if (prefix == "ff")
            {
                value = hex.Substring(2, 16 + 2);
                len = 16;
            }
            if (prefix == "fe")
            {
                value = hex.Substring(2, 8 + 2);
                len = 8;
            }
            if (prefix == "fd")
            {
                value = hex.Substring(2, 4 + 2);
                len = 4;
            }

            return long.Parse(value, System.Globalization.NumberStyles.HexNumber);
        }

        private static string num2VarInt(long num)
        {
            if (num < 0xfd)
            {
                return num2hexstring(num);
            }

            if (num <= 0xffff)
            {
                return "fd" + num2hexstring(num, 4);
            }

            if (num <= 0xffffffff)
            {
                return "fe" + num2hexstring(num, 8);
            }

            return "ff" + num2hexstring(num, 8) + num2hexstring(num / (int)Math.Pow(2, 32), 8);
        }

        private static string reverseHex(string hex)
        {
            string result = "";
            for (var i = hex.Length - 2; i >= 0; i -= 2)
            {
                result += hex.Substring(i, 2);
            }
            return result;
        }

        private static long hexToLong(string hex)
        {
            return long.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        private static string num2fixed8(decimal num)
        {
            long val = (long)Math.Round(num * 100000000);
            var hexValue = val.ToString("X16");
            return reverseHex(("0000000000000000" + hexValue).Substring(hexValue.Length));
        }

        private static string SerializeWitness(Transaction.Witness witness)
        {
            var invoLength = num2hexstring((witness.invocationScript.Length / 2));
            var veriLength = num2hexstring(witness.verificationScript.Length / 2);
            return invoLength + witness.invocationScript + veriLength + witness.verificationScript;
        }

        private static Witness DeserializeWitness(string hex, out int len)
        {
            var witness = new Witness();
            var invoLen = (int)hexToLong(hex.Substring(0, 2)) << 1;

            witness.invocationScript = hex.Substring(4, invoLen);

            var veriLen = (int)hexToLong(hex.Substring(invoLen + 2, 2)) << 1;
            witness.verificationScript = hex.Substring(4 + invoLen, veriLen);
            len = invoLen + veriLen + 4;
            return witness;
        }

        private static string SerializeTransactionInput(Transaction.Input input)
        {
            return reverseHex(input.prevHash) + reverseHex(num2hexstring(input.prevIndex, 4));
        }

        private static Input DeserializeTransactionInput(string hex)
        {
            var input = new Input();
            input.prevHash = reverseHex(hex.Substring(0, 64));
            input.prevIndex = (uint)hexToLong(reverseHex(hex.Substring(64, 4)));
            return input;
        }

        private static string SerializeTransactionClaim(Transaction.Claim claim)
        {
            return reverseHex(claim.prevHash) + reverseHex(num2hexstring(claim.prevIndex, 4));
        }

        private static Claim DeserializeTransactionClaim(string hex)
        {
            var claim = new Claim();
            claim.prevHash = reverseHex(hex.Substring(0, 64));
            claim.prevIndex = (uint)hexToLong(reverseHex(hex.Substring(64, 4)));
            return claim;
        }

        private static string SerializeTransactionOutput(Transaction.Output output)
        {
            var value = num2fixed8(output.value);
            return reverseHex(output.assetID) + value + reverseHex(output.scriptHash);
        }

        private static Output DeserializeTransactionOutput(string hex)
        {
            var output = new Output();
            output.assetID = reverseHex(hex.Substring(0, 64));
            output.value = hexToLong(reverseHex(hex.Substring(64, 16))) * 1e-8m;
            output.scriptHash = reverseHex(hex.Substring(64 + 16, 40));
            return output;
        }

        public static Transaction DeserializeTransaction(string hex, bool signed = true)
        {
            var tx = new Transaction();
            var toParse = hex.Substring(0, 2);
            tx.type = (byte)hexToLong(toParse);
            hex = hex.Remove(0, 2);

            toParse = hex.Substring(0, 2);
            tx.version = (byte)hexToLong(toParse);
            hex = hex.Remove(0, 2);

            if (tx.type == 0x02) //claim tx
            {
                var claimsAmount = varIntToNum(hex, out int clen);
                hex = hex.Remove(0, clen);

                var claimSize = 64 + 4;
                tx.claims = new Claim[claimsAmount];
                for (int i = 0; i < claimsAmount; ++i)
                {
                    toParse = hex.Substring(0, claimSize);
                    tx.claims[i] = DeserializeTransactionClaim(toParse);
                    hex = hex.Remove(0, claimSize);
                }
            }

            //result.Append(num2VarInt(tx.script.Length));
            //result.Append(tx.script.ToHexString());
            if (tx.version >= 1)
            {
                toParse = hex.Substring(0, 16);
                tx.gas = hexToLong(toParse);
                hex = hex.Remove(0, 16);
            }

            // Don't need any attributes
            hex = hex.Remove(0, 2);

            if (tx.type == 0x02)
            {
                //claim tx doesn't have inputs
                hex = hex.Remove(0, 2);
            }
            else
            {
                var inputsAmount = varIntToNum(hex, out int ilen);
                hex = hex.Remove(0, ilen);

                var inputSize = 64 + 4;
                tx.inputs = new Input[inputsAmount];
                for (int i = 0; i < inputsAmount; ++i)
                {
                    toParse = hex.Substring(0, inputSize);
                    tx.inputs[i] = DeserializeTransactionInput(toParse);
                    hex = hex.Remove(0, inputSize);
                }
            }

            var outputsAmount = varIntToNum(hex, out int olen);
            hex = hex.Remove(0, olen);

            var outputSize = 64 + 16 + 40;
            tx.outputs = new Output[outputsAmount];
            for (int i = 0; i < outputsAmount; ++i)
            {
                toParse = hex.Substring(0, outputSize);
                tx.outputs[i] = DeserializeTransactionOutput(toParse);
                hex = hex.Remove(0, outputSize);
            }

            if (signed && hex.Length != 0)
            {
                var witnessesAmount = varIntToNum(hex, out int wlen);
                hex = hex.Remove(0, wlen);

                tx.witnesses = new Witness[witnessesAmount];
                for (int i = 0; i < outputsAmount; ++i)
                {
                    toParse = hex;
                    tx.witnesses[i] = DeserializeWitness(toParse, out int size);
                    hex = hex.Remove(0, size);
                }
            }

            return tx;
            //return result.ToString().ToLower();
        }

        private static string SerializeTransaction(Transaction tx, bool signed = true)
        {
            var result = new StringBuilder();
            result.Append(num2hexstring(tx.type));
            result.Append(num2hexstring(tx.version));

            // excluusive data
            if (tx.type == 0x02) //claim tx
            {
                result.Append(num2VarInt(tx.claims.Length));
                foreach (var claim in tx.claims)
                {
                    result.Append(SerializeTransactionClaim(claim));
                }
            }

            //result.Append(num2VarInt(tx.script.Length));
            //result.Append(tx.script.ToHexString());
            if (tx.version >= 1)
            {
                result.Append(num2fixed8(tx.gas));
            }

            // Don't need any attributes
            result.Append("00");

            if (tx.type == 0x02)
            {
                //claim tx doesn't have inputs
                result.Append("00");
            }
            else
            {
                result.Append(num2VarInt(tx.inputs.Length));
                foreach (var input in tx.inputs)
                {
                    result.Append(SerializeTransactionInput(input));
                }
            }

            result.Append(num2VarInt(tx.outputs.Length));
            foreach (var output in tx.outputs)
            {
                result.Append(SerializeTransactionOutput(output));
            }

            if (signed && tx.witnesses != null && tx.witnesses.Length > 0)
            {
                result.Append(num2VarInt(tx.witnesses.Length));
                foreach (var script in tx.witnesses)
                {
                    result.Append(SerializeWitness(script));
                }
            }

            return result.ToString().ToLower();
        }

        public static Transaction BuildClamTx(List<ClaimEntry> unclaimed, Net net, string address)
        {
            var tx = new Transaction
            {
                type = 0x02,
                version = 0x0
            };

            tx.claims = unclaimed.Select(c => new Claim
            {
                prevHash = c.txid,
                prevIndex = c.index
            }).ToArray();

            var unclaimedTotal = unclaimed.Sum(c => c.value);
            tx.outputs = new Output[] {
        new Output
        {
          assetID = gasId,
          scriptHash = address.ToScriptHash().ToString().Substring(2),
          value = unclaimedTotal
        }
      };
            return tx;
        }

        public static string SignAndSerialize(Transaction tx, KeyPair keyPair)
        {
            tx = tx.Sign(keyPair);
            return SerializeTransaction(tx, true);
        }

        public static bool ClaimGas(Net net, byte[] privateKey)
        {
            var keyPair = new KeyPair(privateKey);
            {
                var address = keyPair.address;
                var unclaimed = GetUnclaimed(net, address);
                var tx = BuildClamTx(unclaimed, net, address);
                var signedHexTx = SignAndSerialize(tx, keyPair);
                var rpc = new NeoRpc.Rpc(net);
                var status = rpc.SendRawTransaction(signedHexTx);
                return status;
            }
        }

        public static Transaction BuildContractTx(List<UnspentEntry> unspent, Net net,
          string from, string to, string amount, string assetId)
        {
            var totalBalance = unspent.Sum(v => v.value);
            var tx = new Transaction();

            if (amount.ToUpper() == "ALL")
            {
                tx.inputs = unspent.Select(v => new Input
                {
                    prevIndex = v.index,
                    prevHash = v.txid
                }).ToArray();

                tx.outputs = new Output[] {
          new Output
          {
            assetID = assetId,
            scriptHash = to.ToScriptHash().ToString().Substring(2),
            value = totalBalance
          }
        };
            }
            else
            {
                var value = decimal.Parse(amount);
                if (totalBalance < value)
                {
                    throw new Exception(String.Format("You try to send : {0}, though available balance is {1}", value, totalBalance));
                }

                var output = new Output
                {
                    assetID = assetId,
                    scriptHash = to.ToScriptHash().ToString().Substring(2),
                    value = value
                };

                //get inputs
                var usedInputs = new List<Input>();
                int i = 0;
                var left = value;
                decimal totalUnspent = 0;
                while (left > 0)
                {
                    var v = unspent[i].value;
                    left -= v;
                    totalUnspent += v;

                    usedInputs.Add(new Input
                    {
                        prevIndex = unspent[i].index,
                        prevHash = unspent[i].txid
                    });
                    ++i;
                }

                var hasChange = totalUnspent - value != 0;
                if (hasChange)
                {
                    var changeOutput = new Output
                    {
                        assetID = assetId,
                        scriptHash = from.ToScriptHash().ToString().Substring(2),
                        value = totalUnspent - value
                    };

                    tx.outputs = new Output[] { output, changeOutput };
                }
                else
                {
                    tx.outputs = new Output[] { output };
                }

                tx.inputs = usedInputs.ToArray();
            }

            tx.type = 0x80;
            tx.version = 0x0;
            return tx;
        }


        public static bool SendAsset(Net net, string toAddress, byte[] privateKey,
          string assetId, string amount)
        {
            if (assetId.ToLower() != neoId.ToLower() && assetId.ToLower() != gasId.ToLower())
            {
                throw new Exception(String.Format("This assetId ({0}) isn't supported!", assetId));
            }

            var keyPair = new KeyPair(privateKey);
            {
                var address = keyPair.address;

                var unspent = GetUnspent(net, address)[assetId.GetNameFromAssetId().ToUpper()];
                var tx = BuildContractTx(unspent, net, address, toAddress, amount, assetId);
                var signedHexTx = SignAndSerialize(tx, keyPair);
                var rpc = new NeoRpc.Rpc(net);
                var status = rpc.SendRawTransaction(signedHexTx);
                return status;
            }
        }

        /**
         * API Switch for MainNet and TestNet
         * @param {string} net - 'MainNet' or 'TestNet'.
         * @return {string} URL of API endpoint.
         */


        public struct UnspentEntry
        {
            public string txid;
            public uint index;
            public decimal value;
        }


        public struct ClaimEntry
        {
            public string txid;
            public uint index;
            public decimal value;
        }

        public static List<ClaimEntry> GetUnclaimed(Net net, string address)
        {
            var neoscan = new NeoscanApi(net);
            var response = neoscan.GetUnclaimed(address);

            var result = response.Claimable.Select(c => new ClaimEntry
            {
                index = c.N,
                txid = c.Txid,
                value = c.Unclaimed
            });

            return result.ToList();
        }

        public static Dictionary<string, List<UnspentEntry>> GetUnspent(Net net, string address)
        {
            var result = new Dictionary<string, List<UnspentEntry>>();
            var neoscan = new NeoscanApi(net);
            var response = neoscan.GetBalance(address);

            foreach (var b in response.Balance)
            {
                var entry = b.Unspent.Select(u =>
                  new UnspentEntry
                  {
                      index = u.N,
                      txid = u.Txid,
                      value = u.Value
                  }
                );

                result.Add(b.Asset.ToUpper(), entry.ToList());
            }

            return result;
        }
    }
}
