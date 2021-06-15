using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace NoxKeys.MoneroWallet
{
  public class XMRGen
  {

    public string XMRGetRandom(string mixin, string[] amounts)
    {
      using (NoxService.NWS.NOXWS2 serv = new NoxService.NWS.NOXWS2())
      {
        try
        {
          serv.Timeout = 30000;
          return serv.XMRGetRandom("", "", "", mixin, amounts);
        }

        catch (Exception)
        {
          return null;
        }
      }
    }

    private const int kRECONSTRUCT_LIMIT = 10;
    private int constructionAttempt = 0;
    JsonSerializerSettings ser;
    public MoneroWallet.Models.Wallet wallet;
    public XMRGen(MoneroWallet.Models.Wallet _wlt = null)
    {
      ser = new JsonSerializerSettings();
      ser.MissingMemberHandling = MissingMemberHandling.Ignore;
      ser.NullValueHandling = NullValueHandling.Ignore;
      ser.ObjectCreationHandling = ObjectCreationHandling.Auto;
      ser.TypeNameHandling = TypeNameHandling.Auto;

      if (_wlt != null) wallet = _wlt;

    }

    public address_response GetWallet(string seed)
    {
      compute_address adr = new compute_address();
      adr.seed_string = seed;
      adr.nettype_string = "MAINNET";


      string genKeysData = JsonConvert.SerializeObject(adr, ser);
      byte[] genKeysDataArr = Encoding.UTF8.GetBytes(genKeysData);
      var genKeysDataLen = XMRNative.GetKeysAndAddress(genKeysDataArr, genKeysDataArr.Length);
      byte[] genKeysDataRes = new byte[genKeysDataLen];
      XMRNative.GetKeysAndAddressGetData(genKeysDataRes, genKeysDataLen);
      var genKeysDataString = Encoding.UTF8.GetString(genKeysDataRes);

      return JsonConvert.DeserializeObject<address_response>(genKeysDataString, ser);
    }

    public key_images_response GetKeyImage(string ViewKey, string PubKey, string Secret, string Txn)
    {
      compute_key_images img = new compute_key_images();
      img.pub_spendKey_string = PubKey;
      img.sec_spendKey_string = Secret;
      img.sec_viewKey_string = ViewKey;
      img.tx_pub_key = Txn;

      byte[] abcArr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(img, ser));
      var rrr = XMRNative.GenerateKeyImage(abcArr, abcArr.Length);
      byte[] res123 = new byte[rrr];
      XMRNative.GenerateKeyImageGetData(res123, rrr);
      var result123 = Encoding.UTF8.GetString(res123);

      return JsonConvert.DeserializeObject<key_images_response>(result123, ser);

    }

    private decimal NDFormat(string valueparam, int decimalsparam)
    {
      decimal decresult = decimal.Parse(valueparam, System.Globalization.NumberStyles.Any) * (decimal)Math.Pow(10, decimalsparam);
      return decimal.Truncate(decresult);
    }
    private decimal NDReverse(string valueparam, int decimalsparam)
    {
      decimal decresult = decimal.Parse(valueparam, System.Globalization.NumberStyles.Any) / (decimal)Math.Pow(10, decimalsparam);
      return decresult;
    }

    private Step2Response TryReconstructTransaction(string ToAddress, string Amount, string BaseData, string FromAddress, string feeNeeded = null)
    {
      string s1jData = "";
      string s2jData = "";
      var res = Double.Parse(Amount) * 1e12;
      Amount = res.ToString();

      XMRTaskTransferResponse taskTransferResponse = new XMRTaskTransferResponse();
      MoneroWalletInput data = ConvertFromString(BaseData);

      data.PassedInAttemptAtFee = null;
      if (feeNeeded != null) data.PassedInAttemptAtFee = feeNeeded;

      Step1Prepare step1Prepare = ConvertFromWSObject(data, Amount);
      byte[] s1compdata = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(step1Prepare, ser));
      int s1len = XMRNative.s1compute(s1compdata, s1compdata.Length); s1compdata = new byte[s1len]; XMRNative.s1getdata(s1compdata, s1len);
      s1jData = System.Text.Encoding.UTF8.GetString(s1compdata);
      Step1Response step1Response = JsonConvert.DeserializeObject<Step1Response>(s1jData, ser);
      if (!string.IsNullOrEmpty(step1Response.err_msg)) throw new Exception(step1Response.err_msg);

      if (!string.IsNullOrEmpty(step1Response.using_fee) && !string.IsNullOrEmpty(data.MaxAllowedFee))
      {
        BigInteger UsingFee = BigInteger.Parse(step1Response.using_fee);
        BigInteger MaxFee = BigInteger.Parse(data.MaxAllowedFee);
        if (UsingFee > MaxFee) throw new Exception("Max fee exceeded by " + NDReverse((UsingFee - MaxFee).ToString(), 12));
      }

      string[] amounts = new string[step1Response.using_outs.Length];
      for (int i = 0; i < amounts.Length; i++) amounts[i] = "0";
      string mix = XMRGetRandom(step1Response.mixin, amounts);

      if (string.IsNullOrEmpty(mix)) throw new Exception("Error getting mix outs");

      MixOutput[] mixOuts = JsonConvert.DeserializeObject<MixOutput[]>(mix);

      if (mixOuts == null || mixOuts.Length == 0) throw new Exception("Error getting mix outs");

      Step2Prepare step2Prepare = ConcertFromWSObjectAndMergeStep1(step1Response, data, Amount, MoneroWallet.Models.Converters.ByteArrayToHex(wallet.Keys.SpendSecret), MoneroWallet.Models.Converters.ByteArrayToHex(wallet.Keys.ViewSecret), FromAddress, ToAddress, mixOuts);

      var s2 = JsonConvert.SerializeObject(step2Prepare, ser);
      byte[] s2compdata = System.Text.Encoding.UTF8.GetBytes(s2);
      int s2len = XMRNative.s2compute(s2compdata, s2compdata.Length);
      s2compdata = new byte[s2len];
      XMRNative.s2getdata(s2compdata, s2len);
      s2jData = System.Text.Encoding.UTF8.GetString(s2compdata);
      Step2Response step2Response = JsonConvert.DeserializeObject<Step2Response>(s2jData, ser);

      return step2Response;

    }

    public XMRTaskTransferResponse XMR_Sign(string ToAddress, string Amount, string BaseData, string FromAddress)
    {
      XMRTaskTransferResponse taskTransferResponse = new XMRTaskTransferResponse();
      try
      {
        constructionAttempt = 0;

        var step2Response = TryReconstructTransaction(ToAddress, Amount, BaseData, FromAddress);

        constructionAttempt = 1;

        while (bool.Parse(step2Response.tx_must_be_reconstructed) || string.IsNullOrEmpty(step2Response.serialized_signed_tx))
        {

          if (constructionAttempt > kRECONSTRUCT_LIMIT)
          {
            taskTransferResponse.Error = "Unable to construct a transaction with sufficient fee for unknown reason.";
            return taskTransferResponse;
          }

          step2Response = TryReconstructTransaction(ToAddress, Amount, BaseData, FromAddress, step2Response.fee_actually_needed);

          constructionAttempt++;
        }


        if (!string.IsNullOrEmpty(step2Response.serialized_signed_tx) && !string.IsNullOrEmpty(step2Response.tx_key_images))
        {
          taskTransferResponse.TxnHex = step2Response.serialized_signed_tx;
          taskTransferResponse.SpendKeyImages = new string[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(step2Response.tx_key_images)), step2Response.tx_hash, step2Response.tx_key };
          taskTransferResponse.FeeSatUsed = step2Response.fee_actually_needed;
        }
        else
        {
          taskTransferResponse.Error = "Tx construction completed with error. Something went wrong";
        }
      }
      catch (Exception exc)
      {
        taskTransferResponse.Error = exc.Message;
      }

      return taskTransferResponse;
    }
    public MoneroWalletInput ConvertFromString(string BaseData)
    {
      byte[] bts = Convert.FromBase64String(BaseData);
      string jData = System.Text.Encoding.UTF8.GetString(bts);

      var data = JsonConvert.DeserializeObject<MoneroWalletInput>(jData, ser);
      return data;
    }
    public Step1Prepare ConvertFromWSObject(MoneroWalletInput data, string Amount)
    {

      Step1Prepare step1Prepare = new Step1Prepare();
      step1Prepare.fee_per_b = data.FeePerB;
      step1Prepare.is_sweeping = "false";
      step1Prepare.priority = data.Priority;
      step1Prepare.sending_amount = Amount;
      step1Prepare.passedIn_attemptAt_fee = null;
      step1Prepare.payment_id_string = null;
      if (!string.IsNullOrEmpty(data.PassedInAttemptAtFee)) step1Prepare.passedIn_attemptAt_fee = data.PassedInAttemptAtFee;
      if (!string.IsNullOrEmpty(data.PaymentIdString) && data.PaymentIdString.Length > 10) step1Prepare.payment_id_string = data.PaymentIdString;

      if (!string.IsNullOrEmpty(data.fee_mask))
      {
        step1Prepare.fee_mask = data.fee_mask;
      }
      else
      {
        step1Prepare.fee_mask = "10000";
      }

      List<outs> UnspentList = new List<outs>();

      foreach (var usedOutputs in data.UnspentOuts)
      {
        outs Unspent_Out = new outs();
        Unspent_Out.amount = usedOutputs.Amount;
        Unspent_Out.global_index = usedOutputs.GlobalIndex;
        Unspent_Out.index = usedOutputs.Index.ToString();
        Unspent_Out.public_key = usedOutputs.PublicKey;
        Unspent_Out.rct = usedOutputs.Rct;
        Unspent_Out.tx_pub_key = usedOutputs.TxPubKey;
        UnspentList.Add(Unspent_Out);
      }

      step1Prepare.unspent_outs = UnspentList.ToArray();
      return step1Prepare;
    }
    Step2Prepare ConcertFromWSObjectAndMergeStep1(Step1Response step1Response, MoneroWalletInput data, string Amount, string SpendKey, string ViewKey, string FromAddress, string ToAddress, MixOutput[] mixOuts)
    {
      Step2Prepare step2Prepare = new Step2Prepare();
      step2Prepare.change_amount = step1Response.change_amount;
      step2Prepare.fee_amount = step1Response.using_fee;
      step2Prepare.fee_per_b = data.FeePerB;
      step2Prepare.final_total_wo_fee = step1Response.final_total_wo_fee;
      step2Prepare.from_address_string = FromAddress;
      step2Prepare.nettype_string = "MAINNET";
      step2Prepare.priority = data.Priority;
      step2Prepare.sec_spendKey_string = SpendKey;
      step2Prepare.sec_viewKey_string = ViewKey;
      step2Prepare.to_address_string = ToAddress;
      step2Prepare.unlock_time = "0";
      step2Prepare.using_outs = step1Response.using_outs;
      if (!string.IsNullOrEmpty(data.PaymentIdString) && data.PaymentIdString.Length > 10) step2Prepare.payment_id_string = data.PaymentIdString;

      if (!string.IsNullOrEmpty(data.fee_mask))
      {
        step2Prepare.fee_mask = data.fee_mask;
      }
      else
      {
        step2Prepare.fee_mask = "10000";
      }

      List<mixouts> step2PrepareMixOutPrep = new List<mixouts>();

      foreach (var mixA in mixOuts)
      {
        List<outs> MixedOutList = new List<outs>();

        foreach (var usedOutputs in mixA.Outputs)
        {
          outs Mix_Out = new outs();
          Mix_Out.amount = usedOutputs.Amount;
          Mix_Out.global_index = usedOutputs.GlobalIndex;
          Mix_Out.index = usedOutputs.Index.ToString();
          Mix_Out.public_key = usedOutputs.PublicKey;
          Mix_Out.rct = usedOutputs.Rct;
          Mix_Out.tx_pub_key = usedOutputs.TxPubKey;
          MixedOutList.Add(Mix_Out);
        }

        mixouts m = new mixouts()
        {
          amount = mixA.Amount,
          outputs = MixedOutList.ToArray()
        };
        step2PrepareMixOutPrep.Add(m);
      }

      step2Prepare.mix_outs = step2PrepareMixOutPrep.ToArray();

      return step2Prepare;
    }
    public XMRTaskImageResponse XMR_GetImages(NoxService.NWS.ImageRequestTable[] requestTable)
    {

      XMRTaskImageResponse taskTransferResponse = new XMRTaskImageResponse();

      try
      {

        List<string> imgList = new List<string>();
        foreach (var tx in requestTable)
        {
          compute_key_images img = new compute_key_images();
          img.pub_spendKey_string = MoneroWallet.Models.Converters.ByteArrayToHex(wallet.Keys.SpendPublic);
          img.sec_spendKey_string = MoneroWallet.Models.Converters.ByteArrayToHex(wallet.Keys.SpendSecret);
          img.sec_viewKey_string = MoneroWallet.Models.Converters.ByteArrayToHex(wallet.Keys.ViewSecret);
          img.tx_pub_key = tx.TxPubKey;
          img.out_index = tx.OutIndex;

          byte[] abcArr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(img, ser));
          var rrr = XMRNative.GenerateKeyImage(abcArr, abcArr.Length);
          byte[] res123 = new byte[rrr];
          XMRNative.GenerateKeyImageGetData(res123, rrr);
          var result123 = Encoding.UTF8.GetString(res123);

          var resp = JsonConvert.DeserializeObject<key_images_response>(result123, ser);
          if (resp != null && !string.IsNullOrEmpty(resp.retVal))
          {
            if (string.IsNullOrEmpty(resp.err_msg))
            {
              imgList.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes((resp.retVal + "|" + tx.TxPubKey + "|" + tx.OutIndex))));
            }
          }
        }


        taskTransferResponse.SpendKeyImages = imgList.ToArray();
        taskTransferResponse.WalletAddress = wallet.Address;
        taskTransferResponse.Error = null;

        if (imgList.Count() < 1)
        {
          taskTransferResponse.Error = "Invalid info or request for key images.";
        }

        return taskTransferResponse;

      }
      catch (Exception ex)
      {

        taskTransferResponse.Error = ex.Message;
        return taskTransferResponse;
      }
    }


  }

  public class XMRNative
  {


    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_send_step1__prepare_params_for_get_decoys_compute")] //, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    public static extern int s1compute(byte[] data, int length);


    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_send_step1__prepare_params_for_get_decoys_get_data")] //, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    public static extern void s1getdata(byte[] response, int lenght);


    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_send_step2__try_create_transaction_compute")]
    public static extern int s2compute(byte[] data, int length);


    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_send_step2__try_create_transaction_get_data")]
    public static extern void s2getdata(byte[] response, int lenght);




    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_address_and_keys_from_seed_compute")]
    public static extern int GetKeysAndAddress(byte[] data, int len);

    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_address_and_keys_from_seed_get_data")]
    public static extern void GetKeysAndAddressGetData(byte[] data, int len);




    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_generate_key_image_compute")]
    public static extern int GenerateKeyImage(byte[] data, int len);

    [DllImport("libbitfi_entry_armeabi-v7a_Release.so", EntryPoint = "Java_com_bitfi_xmrlibnox_NativeJC_generate_key_image_get_data")]
    public static extern void GenerateKeyImageGetData(byte[] data, int len);

  }

  public class MoneroWalletInput
  {
    public string SendingAmount { get; set; }
    public string IsSweeping { get; set; }
    public string Priority { get; set; }
    public string FeePerB { get; set; }
    public Output[] UnspentOuts { get; set; }
    public string PaymentIdString { get; set; }
    public string PassedInAttemptAtFee { get; set; }

    public string MaxAllowedFee { get; set; }

    public string fee_mask { get; set; }

  }

  public struct Output
  {
    public string Key;
    public string Amount;
    public string GlobalIndex;
    public UInt64 Height;
    public UInt64 Index;
    public string PublicKey;
    public string Rct;
    public string Commit;
    public string[] SpendKeyImages;
    public string KeyImage;
    public Int32 outIndex;
    public Int32 mixin;
    public string Timestamp;
    public string TxHash;
    public UInt64 TxId;
    public string TxPrefixHash;
    public string TxPubKey;
  }

  public class MixOutput
  {
    public string Amount { get; set; }
    public Output[] Outputs { get; set; }
  }

  public class compute_key_images
  {
    public string sec_viewKey_string { get; set; }

    public string sec_spendKey_string { get; set; }

    public string pub_spendKey_string { get; set; }


    public string tx_pub_key { get; set; }

    public string out_index { get; set; }
  }

  public class key_images_response
  {
    public string retVal { get; set; }

    public string err_msg { get; set; }
  }

  public class compute_address
  {
    public string seed_string { get; set; }

    public string nettype_string { get; set; }// = "MAINNET";
  }

  public class address_response
  {
    public string address_string { get; set; }

    public string pub_viewKey_string { get; set; }

    public string sec_viewKey_string { get; set; }

    public string pub_spendKey_string { get; set; }

    public string sec_spendKey_string { get; set; }


  }
  public class outs
  {
    public string amount { get; set; } //100000000
    public string global_index { get; set; } //4892334
    public string index { get; set; } //0
    public string public_key { get; set; } //
    public string tx_pub_key { get; set; } //
    public string rct { get; set; } //

  }

  public class Step1Prepare
  {
    public string sending_amount { get; set; } //100000000
    public string is_sweeping { get; set; } //false
    public string priority { get; set; } //1
    public string fee_per_b { get; set; } //24658
    public outs[] unspent_outs { get; set; }

    public string payment_id_string { get; set; }
    public string passedIn_attemptAt_fee { get; set; }

    public string fee_mask { get; set; }// = "10000";
  }

  public class Step1Response
  {
    public string mixin { get; set; } //10
    public string using_fee { get; set; } //66009466
    public string final_total_wo_fee { get; set; } //100000000
    public string change_amount { get; set; } //8810831068
    public outs[] using_outs { get; set; }

    public string err_msg { get; set; }

  }

  public class Step2Prepare
  {
    public string from_address_string { get; set; } //
    public string sec_viewKey_string { get; set; } //
    public string sec_spendKey_string { get; set; } //
    public string to_address_string { get; set; } //
    public string final_total_wo_fee { get; set; } //100000000
    public string change_amount { get; set; } //8810831068
    public string fee_amount { get; set; } //66009466
    public string priority { get; set; } //1
    public string fee_per_b { get; set; } //24658
    public outs[] using_outs { get; set; }
    public mixouts[] mix_outs { get; set; }
    public string unlock_time { get; set; }//0
    public string nettype_string { get; set; }//MAINNET
    public string payment_id_string { get; set; }

    public string fee_mask { get; set; } // = "10000";
  }
  public class mixouts
  {
    public string amount { get; set; }
    public outs[] outputs { get; set; }
  }
  public class Step2Response
  {
    public string tx_must_be_reconstructed { get; set; }
    public string serialized_signed_tx { get; set; }
    public string tx_hash { get; set; }
    public string tx_key { get; set; }
    public string fee_actually_needed { get; set; }
    public string tx_key_images { get; set; }
  }
  public class XMRTaskTransferResponse
  {
    public string Error { get; set; }
    public string TxnHex { get; set; }
    public string[] SpendKeyImages { get; set; }

    public string FeeSatUsed { get; set; }
  }
  public class XMRTaskImageResponse
  {
    public string Error { get; set; }
    public string WalletAddress { get; set; }
    public string[] SpendKeyImages { get; set; }
  }
}