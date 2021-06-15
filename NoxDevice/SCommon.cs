using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.Graphics;
using WalletLibrary;

namespace BitfiWallet
{

 class Sclear
 {
  public static void Initiate()
  {

   if (!BitfiWallet.Nox.Sclear.KeyDictionaryInitiated())
   {

    BitfiWallet.Nox.Sclear.Initiate();

    Dictionary<byte, BitfiWallet.Nox.Sclear.RCharImages> _KeyDictionary = new Dictionary<byte, BitfiWallet.Nox.Sclear.RCharImages>();

    for (int i = 0; i < BitfiWallet.Nox.Sclear._CharImageList.Count; i++)
    {

     var imgv = BitfiWallet.Nox.Sclear._CharImageList[i];
     byte[] BTS = Convert.FromBase64String(imgv.B64);

     using (Bitmap bitmap = BitmapFactory.DecodeByteArray(BTS, 0, BTS.Length))
     {
      Bitmap bitmap2 = Bitmap.CreateScaledBitmap(bitmap, bitmap.Width * 2, bitmap.Height * 2, true);
      BitfiWallet.Nox.Sclear.RCharImages rChar = new BitfiWallet.Nox.Sclear.RCharImages(imgv.POS, imgv.DEC, bitmap2, Convert.ToByte(imgv.DEC), Convert.ToChar(imgv.DEC));

      _KeyDictionary[Convert.ToByte(imgv.DEC)] = rChar;
     }
    }

    BitfiWallet.Nox.Sclear.KeyDictionaryInitiateRef(_KeyDictionary);
   }
  }

  internal static Bitmap GetKeyDictionary(byte key)
  {
   return (Bitmap)BitfiWallet.Nox.Sclear.GetKeyDictionary(key);
  }
  internal static byte GetConsoleDictionary(char ckey)
  {
   return BitfiWallet.Nox.Sclear.GetConsoleDictionary(ckey);
  }
  internal static byte GetConsoleDictionary(int ikey)
  {
   return BitfiWallet.Nox.Sclear.GetConsoleDictionary(ikey);
  }
  internal static byte GetConsoleDictionaryFromIndex(int index)
  {
   return BitfiWallet.Nox.Sclear.GetConsoleDictionaryFromIndex(index);
  }
  internal static char GetCharFromIndex(int index)
  {
   return BitfiWallet.Nox.Sclear.GetCharFromIndex(index);
  }

 }

}