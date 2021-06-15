using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BitfiWallet
{
 public static class SPValidator
 {
  public static bool Validator(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
  {
   if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
   {
    return true;
   }

   return false;
  }
  public static void Initialize()
  {
   ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
   ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(Validator);
  }
 }
}