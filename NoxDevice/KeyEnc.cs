using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Security.Keystore;
using Java.Security;
using System;
using Java.Util;
using Javax.Security.Auth.X500;
using Java.Math;
using Javax.Crypto;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BitfiWallet
{
 class KeyEnc
 {

  public static string dummyKey = "5Jcs7yFWtMBaKesMhQMaKVXmbnLuS6LMaWxrrjuzW8b7BT1B8wi";

  static void MigrateToken(Context c)
  {
   try
   {
    if (KeyEnc.GetDecryptTokenStrRe(c).Equals(KeyEnc.dummyKey))
    {
     string token = KeyEnc.GetDecryptTokenDMA2Str(c);

     if (token.Equals(KeyEnc.dummyKey))
     {
      return;
     }

     if (!string.IsNullOrEmpty(token))
     {

      ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(c);
      string user = sharedPref.GetString("noxtoken", "");
      if (!string.IsNullOrEmpty(user))
      {
       var esharedPrefe = sharedPref.Edit();
       esharedPrefe.Remove(token);
       var apply = esharedPrefe.Commit();
      }

      EncryptToken(token, c);
     }
     return;
    }
   }
   catch { }

  }

  public static string[] GetKeyStore(Context c)
  {

   MigrateToken(c);


   try
   {
    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(c);
    string user = sharedPref.GetString("pref_USER", "");
    string token = GetDecryptTokenStrRe(c);
    return new string[] { user, token };
   }
   catch
   {
    return new string[] { Guid.NewGuid().ToString(), dummyKey };

   }
  }

  public static void SetTestUser(Context c, string apc, string token)
  {
   try
   {

    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(c);
    string user = sharedPref.GetString("pref_USER", "");
    if (string.IsNullOrEmpty(user))
    {
     EncryptToken(token, c);

     var esharedPrefe = sharedPref.Edit();
     esharedPrefe.PutString("pref_USER", apc);
     var apply = esharedPrefe.Commit();

    }
   }
   catch (Exception ex) { }
  }

  static string GetDecryptTokenStrRe(Context c)
  {
   try
   {

    byte[] dtokenre = DecryptToken(c, "noxtoken");
    if (dtokenre == null) return dummyKey;

    return Encoding.UTF8.GetString(dtokenre);
   }
   catch (Exception e)
   {
    return dummyKey;
   }
  }
  static string GetDecryptTokenDMA2Str(Context c)
  {
   try
   {
    byte[] dtokenre = DecryptToken(c, "token");
    if (dtokenre == null) return dummyKey;

    return Encoding.UTF8.GetString(dtokenre);
   }
   catch (Exception e)
   {
    return dummyKey;
   }
  }
  private static byte[] DecryptToken(Context c, string alias)
  {
   try
   {

    KeyStore ks = KeyStore.GetInstance("AndroidKeyStore");
    ks.Load(null);

    if (!ks.ContainsAlias(alias)) return null;

    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(c);
    string token = sharedPref.GetString(alias, "");

    if (string.IsNullOrEmpty(token)) return null;

    KeyStore.PrivateKeyEntry privateKeyEntry = (KeyStore.PrivateKeyEntry)ks.GetEntry(alias, null);

    Cipher output = Cipher.GetInstance("RSA/ECB/PKCS1Padding");
    output.Init(CipherMode.DecryptMode, privateKeyEntry.PrivateKey);

    byte[] vals = Convert.FromBase64String(token);
    Stream stream = new MemoryStream(vals);
    CipherInputStream cipherInputStream = new CipherInputStream(stream, output);

    List<byte> values = new List<byte>();
    int nextByte;
    while ((nextByte = cipherInputStream.Read()) != -1)
    {
     values.Add((byte)nextByte);
    }

    byte[] bytes = new byte[values.Count];
    for (int i = 0; i < bytes.Length; i++)
    {
     bytes[i] = values[i];
    }

    return bytes;

   }
   catch (Exception e)
   {

    return null;
   }
  }

  static void EncryptToken(string token, Context c)
  {

   try
   {

    string alias = "noxtoken";

    KeyStore ks = KeyStore.GetInstance("AndroidKeyStore");
    ks.Load(null);

    Date notBefore = new Date(1262131200000);
    Date notAfter = new Date(1893283200000);

    KeyPairGeneratorSpi generator = KeyPairGenerator.GetInstance("RSA", "AndroidKeyStore");

    KeyGenParameterSpec para = new KeyGenParameterSpec.Builder(alias, KeyStorePurpose.Decrypt | KeyStorePurpose.Encrypt)
        .SetKeySize(2048)
        .SetCertificateSubject(new X500Principal("CN=btnox"))
        .SetCertificateSerialNumber(BigInteger.One)
        .SetKeyValidityStart(notBefore)
        .SetKeyValidityEnd(notAfter)
        .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
        .Build();

    generator.Initialize(para, new SecureRandom());
    KeyPair keyPair = generator.GenerateKeyPair();

    Cipher input = Cipher.GetInstance("RSA/ECB/PKCS1Padding");
    input.Init(CipherMode.EncryptMode, keyPair.Public);

    MemoryStream outputStream = new MemoryStream();
    CipherOutputStream cipherOutputStream = new CipherOutputStream(outputStream, input);
    cipherOutputStream.Write(System.Text.Encoding.UTF8.GetBytes(token));
    cipherOutputStream.Close();

    byte[] vals = outputStream.ToArray();
    string encval = Convert.ToBase64String(vals);

    if (string.IsNullOrEmpty(encval)) return;

    ISharedPreferences sharedPref = PreferenceManager.GetDefaultSharedPreferences(c);
    var esharedPrefe = sharedPref.Edit();
    esharedPrefe.PutString(alias, encval);
    var apply = esharedPrefe.Commit();

   }
   catch (Exception e)
   {

   }


  }
 }
}