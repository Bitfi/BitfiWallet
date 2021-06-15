using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using NBitcoin;
using Org.BouncyCastle.Math;

namespace BitfiWallet.Nox
{
 public class Sclear
 {

  static Dictionary<byte, RCharImages> _KeyDictionary;

  public static void KeyDictionaryInitiateRef(Dictionary<byte, RCharImages> Value)
  {
   if (KeyDictionaryInitiated()) return;
   _KeyDictionary = Value;
  }

  public static bool KeyDictionaryInitiated()
  {
   if (_KeyDictionary == null || _KeyDictionary.Count == 0) return false;

   return true;
  }
  public static byte[] ToArrayEfficient(MemoryStream ms)
  {
   var bytes = ms.GetBuffer();
   Array.Resize(ref bytes, (int)ms.Length);
   return bytes;
  }

  public static object GetKeyDictionary(byte key)
  {

   if (_KeyDictionary.ContainsKey(key))
   {
    return _KeyDictionary[key].BITMAP;
   }

   return null;

  }
  public static byte GetConsoleDictionary(char ckey)
  {
   byte key = Convert.ToByte(ckey);
   if (_KeyDictionary.ContainsKey(key))
   {

    return _KeyDictionary[key].SBYTE;
   }

   return 0;
  }

  public static byte GetConsoleDictionary(int ikey)
  {
   byte key = Convert.ToByte(ikey);
   if (_KeyDictionary.ContainsKey(key))
   {

    return _KeyDictionary[key].SBYTE;
   }

   return 0;
  }


  public static byte GetConsoleDictionaryFromIndex(int index)
  {
   if (index > 88) return 0;

   var item = _KeyDictionary.ElementAt(index).Value;

   if (item.POS == index)
   {
    return item.SBYTE;
   }

   return 0;
  }

  public static char GetCharFromIndex(int index)
  {
   if (index > 88) return '_';
   var item = _KeyDictionary.ElementAt(index).Value;
   if (item.POS == index)
   {
    return item.SCHAR;
   }


   return '_';
  }

  public struct RCharImages
  {
   internal readonly int POS;
   readonly int DEC;
   internal readonly object BITMAP;
   internal readonly byte SBYTE;
   internal readonly char SCHAR;
   public RCharImages(int _pos, int _dec, object _bitmap, byte _sbyte, char _schar)
   {
    POS = _pos;
    DEC = _dec;
    BITMAP = _bitmap;
    SBYTE = _sbyte;
    SCHAR = _schar;
   }
  }

  public static void Initiate()
  {

   LoadCharImageList();

   if (_NoxpszBase58 == null)
   {
    _NoxpszBase58 = new List<byte>();

    for (int i = 0; i < _CharImageList.Count(); i++)
    {
     var imgv = _CharImageList[i];
     byte[] BTS = Convert.FromBase64String(imgv.B64);

     if (i < 62)
     {
      if (imgv.DEC != 48 && imgv.DEC != 73 && imgv.DEC != 79 && imgv.DEC != 108)  //0//I//O//l//
      {
       _NoxpszBase58.Add(Convert.ToByte(imgv.DEC));
      }
     }
    }

    _NoxpszBase58 = _NoxpszBase58.OrderBy(i => i).ToList();
   }

  }

  static List<byte> _NoxpszBase58;

  public static void NoxWriteArray(byte[] array)
  {
   if (array == null) return;
   int WriteInt = 0;

   for (int i = 0; i < array.Length; i++)
   {
    array[i] = (byte)WriteInt;
    if (WriteInt == 0)
    {
     WriteInt = 1;
    }
    else
    {
     WriteInt = 0;
    }
   }
  }
  public static List<byte> EncodeB58NoxHash(byte[] data, int offset, int count)
  {
   BigInteger bn58 = BigInteger.ValueOf(58);
   BigInteger bn0 = BigInteger.Zero;

   var vchTmp = data.SafeSubarray(offset, count);
   var bn = new BigInteger(1, vchTmp);
   List<byte> builder = new List<byte>();

   while (bn.CompareTo(bn0) > 0)
   {
    var r = bn.DivideAndRemainder(bn58);
    var dv = r[0];
    BigInteger rem = r[1];
    bn = dv;
    var c = rem.IntValue;
    builder.Add(_NoxpszBase58[c]);
   }

   for (int i = offset; i < offset + count && data[i] == 0; i++)
   {
    builder.Add(_NoxpszBase58[0]);
   }

   builder.Reverse();
   return builder;
  }

  public static List<CharImages> _CharImageList;
  static void LoadCharImageList()
  {

   if (_CharImageList != null) return;

   _CharImageList = new List<CharImages>();
   CharImages item;
   item = new CharImages(); item.DEC = 97; item.CHAR = 'a';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAf0lEQVQoU7XQMQ4BUQCE4e9p9wA6tWKPsQlxAMdwBGuLrVQqF1ApFUKiUOu2cwitSsKGJ0HEdqb9M/knEzQk/AuW2i726D4VJy2ZserbOTFHR2IYYW4kmL2NW0dY6LnZCPpyWx/NCBcPzz1XOxxicypxtsQAR6yQvpw/nmh8qAZyxRj7W2dbdwAAAABJRU5ErkJggg=="; item.POS = 0; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 98; item.CHAR = 'b';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAdklEQVQoU2NkwAMY8Us2Megz/GPYw8DIEMNQz7ATWTEjA1GSDAxrGBgYMsA6GRk8QKYgdDIwnGLgZghj+MKQzMDIkMnAyuCAaWwjgzvDf4YlDEwMLiRIcjEcYfjKsApsLzdDGLKdIlBv3ADZx1DN8JJAIOAJIgAMNSqhvrDMygAAAABJRU5ErkJggg=="; item.POS = 1; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 99; item.CHAR = 'c';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAW0lEQVQoU2NkwAMYaSnZzcDN8JVhFQMDgxfUmhkMDQyZjAwICQYGboYwhlKGrzB3MDI0Megz/GPYw8DIEMNQz7AT2YEEJPEaCzKnlUGc4TfDAQYGBg1UB5EbQgDaZRgNSc6JlgAAAABJRU5ErkJggg=="; item.POS = 2; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 100; item.CHAR = 'd';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAdklEQVQoU2NkwAMYSZNsYJjOwMAgx8DNEIapE0OykcGd4T/DDiQrtkF0NjHoM/xj2MPAyBDDUM+wkwFFZz1DHgMjQyYDK4MDQzXDSxIkIfYtYWBicAHbCbKCgeEUwrUQezIYGBjeMPxn2M3AyMCP3StITsYbQgAa/x+9/HNMaAAAAABJRU5ErkJggg=="; item.POS = 3; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 101; item.CHAR = 'e';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAb0lEQVQoU2NkwAMYaSnZxKDP8I9hDwMDgwgDA8MbBiYGF4Y6houMDK0M4gy/GQ4wMDIUMNQz7GSoZ8hjYGTIZGBlcGBkaGRwZ/jPsAPNYTeQJZfAjEJWxMiAsG8NQwNDJqokiIdp9DYGboYw8gMBAIPzGdut/OpNAAAAAElFTkSuQmCC"; item.POS = 4; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 102; item.CHAR = 'f';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAY0lEQVQoU2NkwAMYiZNsYtBn+Mewh4GBQYSBgeENAxODC0JnA8N0BgYGOQZuhjCGUoavIBMhkt0M3AxfGVYxMDA8YmhgyIRZxchQz5DHwMgwEc3uGSBFBHTiNZYySRwhgTeEAMbBHb40o67XAAAAAElFTkSuQmCC"; item.POS = 5; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 103; item.CHAR = 'g';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAiUlEQVQoU7XOvQ1BARiF4ee7iQKtPaygkBAD0FhAwwAKodVgBg0DSCRKQygsYACtK9fPRSQ6pzxvzk/4ofgnHOkL84+J0AwTVRc7oatk72wttTW2eMKVREfR8RNOlW8GrUftQUHN0CmMNaRmT+N9N3wnM75R1g73p708eW9aStRfb6nklalB9vYKzBwpHL8PmZYAAAAASUVORK5CYII="; item.POS = 6; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 104; item.CHAR = 'h';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAUElEQVQoU2NkwAMY8Us2Megz/GPYw8DIEMNQz7ATWTEjA1GSDAxrGBgYMsA6GRk8QKYgdDIwnGLgZghj+MrQw8DAIAdiYxrbwDCd5pLkhhAAE8so+xYZ44wAAAAASUVORK5CYII="; item.POS = 7; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 105; item.CHAR = 'i';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAWUlEQVQoU2NkwAMY4XL1DHkMjAwTGRgZPBjqGXaCxImUxGI8I0MrgzjDb4YDDAwMGmB5rMY2Mrgz/GfYQRfJJgZ9hn8MexgYGESQHHyDgZXBAeFPrF7BE3wAK5AdqR8nWMoAAAAASUVORK5CYII="; item.POS = 8; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 106; item.CHAR = 'j';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAVElEQVQoU2NkwAMYUeTqGfIYGBkmMjAyeDDUM+wkQRLNCkaGJgZ9hn8MexgYGETgcliNbWRwZ/jPsAO7nQMnuYSBicGFoY7hIiMDzBEwf0C9AeICAJ5yJg0P6PaxAAAAAElFTkSuQmCC"; item.POS = 9; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 107; item.CHAR = 'k';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgUlEQVQoU2NkwAMY8Us2Megz/GPYw8DIEMNQz7ATWTEjA9GSLAwXGH4zHADrZmVwQNX5nyGAgYHBASTBUM3wEiHJwCDCwMDwhoGJwYWhjuEiSDOy5BoGBoYMhv8M+QyNDJNQJUGuhRgbAtONaicXwxGGrwyrGBgYzEAKCAQCniACANucKlrvC0tKAAAAAElFTkSuQmCC"; item.POS = 10; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 108; item.CHAR = 'l';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAASElEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoAFWyMjgwVDPsBPChIFGBneG/ww7BrlkE4M+wz+GPQwMDCJIfr7BwMrgQCAQ8AQRAId3H6ls4RG3AAAAAElFTkSuQmCC"; item.POS = 11; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 109; item.CHAR = 'm';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAV0lEQVQoU7XQoQ2EUBRE0fNIaAOxFSCRdIGhAtRWQCgBBwqPQ9HeJ4FgV2zyxx4xyQ0/FrlwsqBGgxOf+6rUhgcrYZOsQi8ZFYYXCYdklnTCLnyz4b+FLuJHH3pXgYYOAAAAAElFTkSuQmCC"; item.POS = 12; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 110; item.CHAR = 'n';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAT0lEQVQoU2NkwAMYaSXZxKDP8I9hJcN/hnMMjAyRYGsYGTwY6hl2MjJAJPcwMDCcYuBmCGP4ytDDwMAgB2IjJBkZYkCqGRoYptNcktwQAgCeHyH7o1qP7wAAAABJRU5ErkJggg=="; item.POS = 13; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 111; item.CHAR = 'o';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAcElEQVQoU2NkwAMYaSnZyODO8J9hB9SKNwxMDC4MdQwXGRmaGPQZ/jHsYWBkiGGoZ9jJ0MAwnYGBwYGBlcGBkaGeIY+BkSETxGGoZnjJADFlCUg3AUmYfYwMHkjGyjFwM4RB/AkxeiLUQTdgVpAfCAAhvx+hM6wubQAAAABJRU5ErkJggg=="; item.POS = 14; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 112; item.CHAR = 'p';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAdklEQVQoU2NkwAMYaSXZxKDP8I9hJcN/hnMMjAyRYGsYGTwY6hl2MjJAJPcwMDCcYuBmCGP4wpDMwMiQycDK4ICQZGSIAalmaGRwZ/jPsISBicGFBEkuhiMMXxlWge3lZghDtlME6ucbIPsYqhleYhqLFCp4JQGpuCuhy+vplgAAAABJRU5ErkJggg=="; item.POS = 15; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 113; item.CHAR = 'q';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAd0lEQVQoU2NkwAMYaSnZyODO8J9hB5IV2xi4GcIYGZoY9Bn+MexhYGSIYahn2MnQwDCdgYFBDiJZz5DHwMiQycDK4MBQzfCSBEmIfUsYmBhcwHaCrGBgOAUxFgQg9mQwMDC8YfjPsJuBkYEfIYkcEigOQg8iJEkAOvIfvVHMEhoAAAAASUVORK5CYII="; item.POS = 16; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 114; item.CHAR = 'r';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAUUlEQVQoU2NkwAMYaSXZyiDO8JvhAMN/hvMMjAyuDAwMIgwMDDMYGhgyGRlgkiDLWRkcGP4wGDD8Z1jCwMTggpD8zzCdoZFhErIDaSZJbggBANULH75eITdZAAAAAElFTkSuQmCC"; item.POS = 17; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 115; item.CHAR = 's';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAeElEQVQoU7XQsQnCcBTE4e/ZphVcwS5LBBQHcAeXUBNIZWejMziBgoUruIetlWAkEPLXxkLw2nfc796FL4p/HksTjWOPCFMrp1Abebho7JS27x3CRubugBluBgpL19aU2lZyT2cMsbe2+HwlpZCZJybjjtdH/z7CC7PyGaP9ESSbAAAAAElFTkSuQmCC"; item.POS = 18; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 116; item.CHAR = 't';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAATUlEQVQoU2NkwAMYCUu2Mogz/GY4wPCfYTpDI8MkmAaITpySDQzTGRgYMtCM38bAzRBGQCdeYymTBOmuZ8hjYGSYCHUYkoNwhATeEAIAnYAjOUvxHlkAAAAASUVORK5CYII="; item.POS = 19; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 117; item.CHAR = 'u';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAUUlEQVQoU2NkwAMYaSXZxKDP8I9hDwMjQwxDPcNOhgaG6QwMDHIM3AxhjAx0kGRkeAG2n4HhFMROEIA4IoOBgeENw3+G3QyMDPwISRwhgTeEAElhIZFRCuBPAAAAAElFTkSuQmCC"; item.POS = 20; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 118; item.CHAR = 'v';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAj0lEQVQoU7XQMQ4BcRTE4e+vXb3OFVxCsk5A4wA0Oq3dlah0GhWVSqHfxCUcwREUW0msbDaIRLbzqpf3m7zMTNAw4V8wtUVXZGiusNTzcBaMg0ysdNDSt3CRmAniShysRQpHpVzb7r1nNrXbl5oV9lpG1ZcarnTcnZSugpvUtDp/ctbGJoKBRP4Nf7TR2NATzKEha+GE9/cAAAAASUVORK5CYII="; item.POS = 21; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 119; item.CHAR = 'w';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAh0lEQVQoU7XQMQ4BYRiE4efbZCuu4AAqx3AC9BKNwhVYiUqDZu8gJBoFUTiBIziERCXxi+0pJKaazJvJJBO+KP4FJ/aSg8KqmpiruVu/s/AJhtUbllUj7CQLSU8oZYZhbCQ0cZM0hCM6cv1QaEuWkovMVjKrfN0gTLU8nbCRm3g446qm+/sJL/DuKDvRDCFbAAAAAElFTkSuQmCC"; item.POS = 22; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 120; item.CHAR = 'x';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgElEQVQoU7XQMQ5BURCF4W9uawG2YBkKhaglGgtQeVbgUahUT8Ma7ECisAWLUUk8ubkKiUQhMd2cP5n/ZMKXiX/B2lxohKHaycoeY8mgOEvQFxZajWRi6VrgVsfNESOtytouxwVudN1d0PuE5SzJwcNZmGZ/vBfIntc+ywV/f8ITi+QdhlYfPCsAAAAASUVORK5CYII="; item.POS = 23; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 121; item.CHAR = 'y';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAoElEQVQoU7XQLY4CYRCE4efbBAwHWIfGcQUECRcgqzgAGFCoTYAhoHBrBoXCwA1IEFyBK+A4wCiS/cgw/Kl127LfqlR1B39M+C84kaKq4stQZqru117QCRIt0dqHppGjN3GwUJHZinbKNi4OolTip2g71he0BCvRTEnDt3MB5z5vDmqiQe7K1687i6z2M/sJH7mcTPQejwn3dl0s30EuuALUoChrnuAQtAAAAABJRU5ErkJggg=="; item.POS = 24; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 122; item.CHAR = 'z';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAdElEQVQoU2NkwAMYaSXZyiDO8JvhAAMDgwbciv8M+QyNDJNQ7axnyGNgZMhkYGVwYKhmeImQbGLQZ/jHsIeBkSGGoZ5hJ8gUhGQDw3SwsQ0MmTDjIZKNDO4M/xmWMDAxuDDUMVxESGJzEAPDNgZuhjDyAwEAhe4VowcAVTsAAAAASUVORK5CYII="; item.POS = 25; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 65; item.CHAR = 'A';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAq0lEQVQoU43QLU4DYRjE8d9LQkXXknAJkl4BQbIJB+AONYBBIXa3FFWHAIeqAQ/JJgjE3qG+jgOsIuEl2/1ITSmPfCYz/8kEf1zYLy4kaq84F10pPDSm1jkz8eMRFU4kLtyoW7GQii4Fz6K5Q6dufbVi7km0MvLi26fgWqYM7h0Pj7GqY6/lpqHjfeBoq/l7ww2yDSvtS3T8pQNnQe5NVPb1h6To7h8j7JjpF/dxL0ERqrFuAAAAAElFTkSuQmCC"; item.POS = 26; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 66; item.CHAR = 'B';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAZklEQVQoU2NkwAMY8Us2Mrgz/GfYgaKIkcGDoZ5hJyMDTBIkwMVwhOErwyqwQm6GMFySjxgaGDIRkghz3zAwMbgw1DFcRNVZz7CToYFhOgMDgwMDK4MDpiTEDUtAuonUidVOckMIAAVNLIx7tpGCAAAAAElFTkSuQmCC"; item.POS = 27; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 67; item.CHAR = 'C';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAcUlEQVQoU2NkwAMYCUt2M3AzfGVYxcDA4AVVfIOBlcGBkQEhwcDAzRDGUMrwFWYaI0MjgzvDf4YdDIwMHgz1DDuRrWFkqGfIY2BkqGVgYnBhqGO4SIIkXmNbGcQZfjMcYGBguIfpIJAlCAUaqF4hN4QArr4eRdMRe3AAAAAASUVORK5CYII="; item.POS = 28; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 68; item.CHAR = 'D';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAbElEQVQoU2NkwAMY8UvWM+QxMDJMRFK0jYGbIYyhlOErIwNEMpOBlcGBgY3hC8NXhlVghdwMYaiS1QwvGRoZ3Bn+MyxhYGJwoaYkkhtQjQU55DfDAQYGhgMMDQyZMElkr8wASYDUEQgEPEEEAF0nJlIIa2wzAAAAAElFTkSuQmCC"; item.POS = 29; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 69; item.CHAR = 'E';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAARElEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoIGkcAZDA0MmIwNM8j/DdIZGhknIJhEpiWzsf4Z8kClE6qSOnQwMUK+QG0IAZeQmDdKQR90AAAAASUVORK5CYII="; item.POS = 30; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 70; item.CHAR = 'F';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAQElEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoIGkcAZDA0MmIwNM8j/DdIZGhknIJlFDEtnO/wz5ICuoYSxW15IbQgDESCcNYFTV0QAAAABJRU5ErkJggg=="; item.POS = 31; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 71; item.CHAR = 'G';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAi0lEQVQoU53QMQ4BYRRF4e+fRKWV6NQ6W1BI7ECtpKFSKcwQnUpjA5ahsAPrkGinkhiZMYIIEq89N/fcvODLhd9wqe5sj2YRzowl1sED7MWGBYz1RQ7BzEgwVNE2dXzWBLENGqp6JtLPMLXCACeRznttoiuzvcG5loudzCJf6AXmknuAWuksa//90BWiVCaC80UbnQAAAABJRU5ErkJggg=="; item.POS = 32; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 72; item.CHAR = 'H';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAO0lEQVQoU2NkwAMY8UvWM+QxMDJkMrAyODBUM7xkaGRwZ/jPsISBicGFkYFSyYlodr+hjrHkuJbcEAIAlh4qlcnouMoAAAAASUVORK5CYII="; item.POS = 33; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 73; item.CHAR = 'I';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAP0lEQVQoU2NkwAMY8Us2Megz/GPYw8DAIIKkcBsDN0MYQmc9Qx4DI0MmAyuDA0M1w0uQwkEoSZRXsIQG3hACAPjUGTlTnctLAAAAAElFTkSuQmCC"; item.POS = 34; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 74; item.CHAR = 'J';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAZklEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoAFXyMjgwVDPsBNVZxODPsM/hj0MjAwxAycJcu0fBhuGeoa1DI0M7gz/GZYwMDG4MNQxXIS4tp4hj4GRYSKY/Z8hn6GRYRKISSAQ8AQRAG7cIw1jd6KUAAAAAElFTkSuQmCC"; item.POS = 35; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 75; item.CHAR = 'K';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAeklEQVQoU53QoQ3CUBjE8d8jqelA6Iom7QBdABwTICpapoAZmAAcgiHQOAZAt3lB8ALhCc59+V8ud1+QUcjDncbkJGgNzkZ7dBbqIIUxJjGmsDNZ4260ib435IBKodJ7fMJ4337BFba4fMfGtplCrdLV0xHL15R/PzQDl00sb2j8HY4AAAAASUVORK5CYII="; item.POS = 36; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 76; item.CHAR = 'L';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAMklEQVQoU2NkwAMY8Ut2M3AzfGVYxcDA8IihgSETWTEjw1CV9ELyxjYGboYwAoGAJ4gATNAfoxkDPRoAAAAASUVORK5CYII="; item.POS = 37; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 77; item.CHAR = 'M';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAkUlEQVQoU53QLRLBYRAH4GfNaIKma24gao6gmxGY8XEDBBdQKJImiYLgFrobMCMJr/H+x0gEW5/97e5s+FHxG+fakoNkrGLjboe6slZ4I2tlMw+nPC3j1Eho4yIcJV1UlQwKpClcccMZvQ+GhuQs9CUdYVXgzCrvCHspTxli+8FXam6ZmxZq+agwKZJf8d8PPQG30i2ZsuWh0QAAAABJRU5ErkJggg=="; item.POS = 38; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 78; item.CHAR = 'N';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAjUlEQVQoU53QoQ6BYRQG4OdjkiAo7kKTBDbbfwckNyBwCYZpAi5Ap2mqu3ARikDjM37/hiB443m2s/ec4EfCbxzqCxaigZGlsaqbtZxOkCEHBQ1XlW/soSyayNt/YyI6CUqYYvW+NnkNt4K5qPuJRW1nMzSfF7wVSjzwoi7a4SinlbVN8ZGzDWop/vuhOxCJKJWT1DLNAAAAAElFTkSuQmCC"; item.POS = 39; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 79; item.CHAR = 'O';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAfElEQVQoU2NkwAMYCUs2Mrgz/GfYAVX4hoGJwYWhjuEiI0MTgz7DP4Y9DIwMMQz1DDsZGhimMzAwyDFwM4QxInMYShm+MkBMWQLSjSmJZBJZkisZmBjCGaF27GBgZPDAdBDI/fUMeQyMDBOhXrnBwMrgwFDN8JKIQMARTACXCCsrZux/vwAAAABJRU5ErkJggg=="; item.POS = 40; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 80; item.CHAR = 'P';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAZElEQVQoU2NkwAMY8Us2Megz/GPYw8DAIAJV+IaBicGFoY7hIiMDTJKRIYahnmEnQwPDdAYGBgcGVgYHTMlGBneG/wxLQLpx6ZRj4GYIQ0gi7LwBMpKhmuElpk4k51MiSW4IAQDQrCfwPuv5ewAAAABJRU5ErkJggg=="; item.POS = 41; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 81; item.CHAR = 'Q';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAlElEQVQoU53QvQkCURBF4e8tGG0PtmALBoJiAZpYg0ZGgn+pkZENmGgFCjZhZguC6aaurL5lBUHByYZzOdyZ4MuE33ChLXeIwZtEy9Q5WGq4OwkGZo7mNqhL9YKZoaBdLMayGN5J9MN78g0+TX/BqC2bBp3PQkX/V6l1POWipmniWj3hZdiWNxbBCq6kMnt0n4bc6AFhZjMYPsrxEQAAAABJRU5ErkJggg=="; item.POS = 42; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 82; item.CHAR = 'R';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAhUlEQVQoU4XQMQ4BURSF4e8JjY3YhkLCAmxCZQGCmS1oZg9WQDeFBdCpdRagknjyihcmI+N2N/89556coGNCNyxNRYevo6uBsZV7kGEw03f2VKNWWDTh0MnDHrc2zMqoUtp9lPlptEwgrU3bl5FgrWdi4/I7UMs2pd06KlSYJ/WfEjoqegPl6zO+8eGQFAAAAABJRU5ErkJggg=="; item.POS = 43; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 83; item.CHAR = 'S';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgUlEQVQoU43Quw3CAAyE4c8FVVr2YIV0IAagYgcGoCChTUfDDgyAQGIQVmAAWhIFCAEiHq4snf/z2eFLxW8xN1TaPQZLM7lVWBq4OAhTC/tnp9BQYdQVC4mzDcZX6m5Zt22gxp4+thKTbtrMGqme9FVsV7iR72dwrClzpz+e8OFNFdtPHtUoa4CZAAAAAElFTkSuQmCC"; item.POS = 44; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 84; item.CHAR = 'T';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAANElEQVQoU2NkwAMY8UvWM+QxMDJMRFP0hoGJwQWhE6Iok4GVwYGhmuElSPFQlcQSGnhDCAC7DRe+X5EYMwAAAABJRU5ErkJggg=="; item.POS = 45; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 85; item.CHAR = 'U';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAVklEQVQoU2NkwAMY8UvWM+QxMDJkMrAyODBUM7xkaGRwZ/jPsISBicGFkWHQSUJct4OBkcGDoZ5hJ0MDw3QGBgY5Bm6GMIg/IQIZUD/fgHmLQCDgCSIARycryTUNcQkAAAAASUVORK5CYII="; item.POS = 46; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 86; item.CHAR = 'V';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAApElEQVQoU4XQMWrCcBzF8c9f6BRHwS1XkN4gg1DxAE69gGdwMCl06tYlXdyc3BwEgx08SzcP0KnQlOafiFP8bT++j/ceL+i50A9zB3zJLRvhq7EfZ7UyyJXIPMisXBSe1LYGpqF9joKZtaoVpxKLcGtjaOPbTq1SeI+FonXaJLIXPP+7RNjl1E6Cxy4/whcTvz4xwkfXPMI3SZPF/FoMd0bomegPczUtQWkls48AAAAASUVORK5CYII="; item.POS = 47; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 87; item.CHAR = 'W';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAn0lEQVQoU53QMWoCURQF0PNHrBTS2Kd2A65AmAVYpxaCmMIyRcYypFPQaVLZpQ9oZ6sbsNc1WAXmhxmRgMIUueU79xXvBTUJ9Zj5Fm1Mzarih5azr3IWZJaiwx0GswuWSeQKCzxhrsI3Y0EXOwwxwestnvAg2CsMNLxfN1McsRKNBI8Sz8FUKloj15T5sUVHov+H0Yu2z+oMehf874d+AXqPLRZ4f5+YAAAAAElFTkSuQmCC"; item.POS = 48; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 88; item.CHAR = 'X';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAoklEQVQoU4XQP45BYRiF8d8nmeo2OsuwhSkmIWqxBgqj0U3iX6g0aFiASi8xicIW9LOBWYBK4sp1cVXX2705Oc85OUHOhXyx71swF1T1/Xr5g6nIyeZOaGOLg4FWih0pu9jjD0UfPv34zzIHlmiKdQwtEk8qZugaVgkyE4cqYmvBTGz8KBdMlJwdbiUi3We5SCNIs+oKvvQcpZRdkv1mhJyJrrw9KVhdnNc9AAAAAElFTkSuQmCC"; item.POS = 49; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 89; item.CHAR = 'Y';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgUlEQVQoU2NkwAMY8Us2MGwFK+BmCGMoZfjK0MSgz/CPYQ8DI0MMIzKHoZ5hJ0MDw3QGBgY5kGKIsTABBoZWBgaGjSBdIIUQSYhRK6H234VZAZHsZuBm+MqwioGBwYuBkcEDbDwDAwPCtfUMeQyMDJkMrAwODNUML6kliSU08IYQAIp0IWv0tuYPAAAAAElFTkSuQmCC"; item.POS = 50; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 90; item.CHAR = 'Z';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAb0lEQVQoU2NkwAMY8Us2Mrgz/GfYgaZoGwM3QxiqziYGfYZ/DHsYGBliGOoZdqJKNjBMZ2BgkAPpYihl+IqQRNMFsgYhiaYLIQnRtZKBiSGcoY7hIsxxEJ0QXSA6E9nVjAzYvXKDgZXBgUAg4AkiANP4Gz/RjFbrAAAAAElFTkSuQmCC"; item.POS = 51; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 48; item.CHAR = '0';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAm0lEQVQoU43QPa5BYRSF4ecjR2MOpqBTKyRmoFLpNOglF3cGNDqVyghINBrR6kxBopG4lcSR4+eKws9u19prv3sFbyZ8FnvKYtObcSel5Mc6+JV3MhdUdcx0DZGTVQk6GoK6SFHGwZ8JCsl2+HdGao5GgqVYK0l6iLE9VtIW9zP32L5YU8/AFW58jX0LlPA/v7K5wLVtvyjhRU1nQGctK1dZcvQAAAAASUVORK5CYII="; item.POS = 52; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 49; item.CHAR = '1';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAZElEQVQoU2NkwAMYiZOsZ8hjYGSYyMDI4MFQz7ATpAmis4FhOgMDQwaYjSLZxKDP8JehnIGRYQoDA8NGBkaGGFSdIB0gRf8Y9tBFspVBnOE3wwEGBgYNJD/fYGBlcCAyELAEFQBZCB6piO8/rAAAAABJRU5ErkJggg=="; item.POS = 53; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 50; item.CHAR = '2';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAh0lEQVQoU43QqxHCUBAF0PNEVIqhhTg8ghqgABQmAYlDkB4ogEEgKAONo4AoZvKYEJhMGH4r9/52b/Blwm8wV2LyIO6kxmaqYGGgtpEY3cGro6hUWPdtV1KVLc5y0z7YuhxEy76yU+kym5w3QLNubdtrM4nM3OX5XlAYivYv/54a4h8lfKjpBoXQIql7wZKDAAAAAElFTkSuQmCC"; item.POS = 54; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 51; item.CHAR = '3';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAf0lEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoAFVeIOBlcGBoZrhJapOhMIDDA0Mmdgl/zNMZ2hkmASRbGRwZ/jPsANq7DYGboYwhlKGr6g6uxm4Gb4yrAIr4mYIw3RtPUMeAyNDJshRuBx0D6KziUGf4R/DHgYGBhH8dqKFCN4QAgD8yx/V4jTeYAAAAABJRU5ErkJggg=="; item.POS = 55; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 52; item.CHAR = '4';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgUlEQVQoU43QMQ4BURQF0PNHVFoJu7AFEgmxAWvQ6CQqhkRlATagMb1CxxIsRiWMRPMNyY/Xntyb916QmPAfbrTcndGU6Vu4xmRuh967KTOOuDJQ2mOGecSthptC6aTm4ukQcWkqmKjremhXMXfE6GfrYFg9Za1TTX5GkvjVnfzQC1dQI9V+XwCvAAAAAElFTkSuQmCC"; item.POS = 56; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 53; item.CHAR = '5';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAbklEQVQoU2NkwAMY8Uu2Mogz/GY4wMDAoAFX+J8hn6GRYRIjA0ySkaGAoZ5hJ7JJREoijL3BwMrgwFDN8BLVQd0M3AxfGVaBjeZmCMN0bQPDdAYGBjlMSYTLDzA0MGQiHISwcwZIAmQygUDAE0QAegceOVV/3CYAAAAASUVORK5CYII="; item.POS = 57; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 54; item.CHAR = '6';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAn0lEQVQoU42QsQ3CQBAEZ18icg+0QAsOLEEFICQKIDIRERLGpESQ0AAJrgCEmyCjBQpwyqP/9wsChHzh7d3s3ok/pW7ihiMw98OWBSUHsSOhofLNhDFLmkgTWwa8OGOYsOb+bSNKhlhOWG6IqRfFiIKrKMgR++hD8E7pkX42DZnHRpIhi541YuZQ7WbfhQt3hulLG+bhkKx4dnzCj1e9AaolKPBOpw8aAAAAAElFTkSuQmCC"; item.POS = 58; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 55; item.CHAR = '7';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAj0lEQVQoU33QsQnCAABE0RfBKgM4hiukENzAygVs7KxEkyhWdjYuYOUEChZZwc4VHCCVYCSGECKYa//dcVygQ0E3TI0VLj+mh76ondwZeMmQSczasGo56RlZuzdwL5Q7f+tDEwt5AzeG3m4CU7Fr6Wlg4oioHGLp2cA6VdhKHerlVTI2F1jVQ9rwzxOdD30AsIweqRTyLYwAAAAASUVORK5CYII="; item.POS = 59; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 56; item.CHAR = '8';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAhklEQVQoU52QzQ1EYBCGn5E4KWZbcJBQgZMCuOjAfwVcKEARNtkm9KEGRL4Pu8mGxNxmnjfvvDPCRck9LHBZGA6h4JHxFkpezHwQgm1ARowQYWKfcKGioNHQxcJXO5W61rYTBg4po4I5LRB+hevIiQQVpt/V2iXZ+h0O6IS/gf7ZHqc8/dAKKOMljNRcn88AAAAASUVORK5CYII="; item.POS = 60; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 57; item.CHAR = '9';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAlElEQVQoU53QPQ4BARDF8d9IFKJRqF3BFRQS4gAqF9jGBcRnq1K5gMoNJA6g1SlUOgfQIuxugoLEVDPzf+9NMuFLxW841hcWmfCgqGHgHKZablYKmkqOLtY4mUhC6kpy9eucw+HTObI3sUTjIQ5z5Syqk93coZLCz0qdNWXddzhTd7UVesY2IV9QfYaE9gOk7b8fugMLKCF3EEc+8QAAAABJRU5ErkJggg=="; item.POS = 61; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 126; item.CHAR = '~';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAXElEQVQoU8XPoRXCMBgA4e+Pw/e9ztA92IEZQLBCU1EPMWEGFCN0CwxroBDBVNf27J25sEHsIme9nwWd5Ojg4+splDC6CoPw0tw0J6FKLiGrmrdJWcM7HrLzHit/al8ShoiuYwkAAAAASUVORK5CYII="; item.POS = 62; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 45; item.CHAR = '-';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAJUlEQVQoU2NkwAMYB6dkPUMeAyPDRKjj3jAwMbgw1DFcHHSuBQB4bAQNO86TKwAAAABJRU5ErkJggg=="; item.POS = 63; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 43; item.CHAR = '+';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAP0lEQVQoU2NkwAMYaS3ZyODO8J9hAgMrgwNDNcNLkHUIO7FK1jPkMTAyTERz2BsGJgYXAjphWvDaicXD5AcCAIXNEr75sjLlAAAAAElFTkSuQmCC"; item.POS = 64; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 61; item.CHAR = '=';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAKUlEQVQoU2NkwAMYB0SyiUGf4R/DHgYGBhEk+7cxcDOEEXAQ+TppEggAliIHOXzwa9sAAAAASUVORK5CYII="; item.POS = 65; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 123; item.CHAR = '{';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAcUlEQVQoU2NkwAMY4XLdDNwMXxlWMTAweDEwMngw1DPsREjWM+QxMDJkMrAyODBUM7wEaUKXdGfgZghjKGX4SoJkA8N0sP0NDJkwdzAyNDHoM/xj2MPwn6GZoZFhErLjEXZi1QlTCnEtOQ7CqxNLCAEAi3EjDePr1TUAAAAASUVORK5CYII="; item.POS = 66; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 125; item.CHAR = '}';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAeElEQVQoU2NkwAMYGboZuBm+MqxiYGDwYvjPkM/QyDAJpp4RrrGRwZ3hP8MEBlYGB4ZqhpcgcYRkE4M+wz+GlQxMDOEMdQwXSZBsZRBn+M1wgIGRoYChnmEnqk4QD+E4BgZuhjCEnXh1ku8giD+XMDAxuCC8gieEAI4VLKOremj+AAAAAElFTkSuQmCC"; item.POS = 67; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 91; item.CHAR = '[';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAOElEQVQoU2NkwAMYwXJNDPoM/xj2MDAwiDAwMGxj4GYIYyhl+IosuZKBiSGcoY7hIsywkSiJEUIAxS4uDX11us0AAAAASUVORK5CYII="; item.POS = 68; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 93; item.CHAR = ']';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAALElEQVQoU2NkwAMYwXL1DHkMjAwToepmMDQwZILYEEkYaGCYDmaOSEkcIQQAr4wYDa61zLwAAAAASUVORK5CYII="; item.POS = 69; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 47; item.CHAR = '/';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAi0lEQVQoU5XQsQnCQBSA4e8Kqwxg5wquYJcNrFzARmysLGIEKxuxyQ5uELBwHQdIJRg5glwiInjtz/fu3QU/Tvgv7lSYyMyHcm/q6SpYKNTD2FM2mhQPxh5ugnVUcZcUCytBHu+KKsW3alVK5/cLOtmppZGZrXuKR5nGRavuq25sKdc6farhQl++6gWuuiCjwKo9FgAAAABJRU5ErkJggg=="; item.POS = 70; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 62; item.CHAR = '>';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAWklEQVQoU2NkwAMYKZRsYJgONqGBIRPZJIixjQzuDP8ZdjAwMMxAVoCws4lBn+Efwx4GBoZTDNwMYQylDF9RHdTKIM7wm+EA2DRWBgcidOK1E69rcYQE3hACAEPVGKk3wOzzAAAAAElFTkSuQmCC"; item.POS = 71; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 60; item.CHAR = '<';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAY0lEQVQoU2NkwAMYqSTZyiDO8JvhAAMjQwFDPcNOhLFNDPoM/xj2MDAwnGLgZghjKGX4CpFESKxhaGDIhLmDkaGRwZ3hP8MOhv8M+QyNDJOQHUhAJ0wpTjthCnC6Fkto4A0hAB7HHw0TiRuBAAAAAElFTkSuQmCC"; item.POS = 72; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 63; item.CHAR = '?';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAcElEQVQoU2NkwAMYCUs2Mrgz/GfYAVbIyODBUM+wE8JEBvUMeQyMDBNhClAlWxnEGX4zHGBgYDjA0MCQiV3yP8N0hkaGSRDJbgZuhq8MqxgYGLwYGBi2MXAzhDGUMnwlwrVodsHcCNGJVxJHSOC1EwDSox0N8R+oSAAAAABJRU5ErkJggg=="; item.POS = 73; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 46; item.CHAR = '.';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAKklEQVQoU2NkwAMYh6NkA8N0BgaGDAZGBg+GeoadIC8i/IlXEkto4A0hAAJABg3zL+xEAAAAAElFTkSuQmCC"; item.POS = 74; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 44; item.CHAR = ',';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAUUlEQVQoU2NkwAMYh6NkA8N0BgaGDAZGBg+GeoadIC8i/FnPkMfAyDCR4T9DPkMjwyRUySYGfYZ/DHsYGBliEDq7GbgZvjKsYmBg8EI2EqQTADMTDw0n3ZMbAAAAAElFTkSuQmCC"; item.POS = 75; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 41; item.CHAR = ')';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAgUlEQVQoU2NkwAMYwXL1DHkMjAzuDNwMYQylDF9h6iGSrQziDL8ZDjAwMBxgaGDIRJUE8ZoY9Bn+MexhYGSIYahn2AkSguiEgQaG6QwMDHIw41ElGxncGf4zTGBgZXBgqGZ4SS1JiJ0MMBcjjMXp2m4GboavDKsYGBgeYfoTRwgBABkuJw3xoDNxAAAAAElFTkSuQmCC"; item.POS = 76; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 40; item.CHAR = '(';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAjUlEQVQoU2NkwAMYUeRaGcQZfjOsY2BiyGKoY7iIkIRIHGBgYDjA0MCQCdKEkGxgmM7AwCDHwM0QxlDK8BUhCdPFyFDAUM+wE2YVRGcjgzvDf4YlDEwMLiC7sElOYGBlcGCoZnhJgk6Ynf8ZpjM0MkxC1QniQVzrgGw0kf4E6W5i0Gf4x7CSgYkhHORqAFdgKw2Vdqc4AAAAAElFTkSuQmCC"; item.POS = 77; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 33; item.CHAR = '!';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAVElEQVQoU2NkwAMYiZNsZHBn+M+whIGJwYWhjuEiSBNCJ/mSTQz6DP8YVjIwMYRjGguRnMbAyhDEUM3wEtVOLM5GOKieIY+BkaEWu2vxSuI1FoskAMhoGg1JbiXjAAAAAElFTkSuQmCC"; item.POS = 78; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 64; item.CHAR = '@';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAtElEQVQoU4XQIU4DYRDF8d80QW0CprdA4FF1JD0AXKE1RaEQu8XW1SB6ApAkbMIB4AqgNxg0WbtLvm+XQBB01CT/92beTPinYj/cKLTuMc/i3qW1bfgBjcrSjROdOxMXYe1Mb6Vw7kqbnZVbvdfITXjXOx3H1sKLzmeCj2gwwwMOx73Z+SzU2Tk4joUjkQIlyA4bTMfT6pRhGBu2Sk9/bw6llbB0YObax2/B8KGUmMUI3r6FX9p3MeMWQnaXAAAAAElFTkSuQmCC"; item.POS = 79; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 35; item.CHAR = '#';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAiklEQVQoU53QsQ7BYADE8d/XpFOfxOQVbITNwDsYulkpq4VKxCsw2Awew8SLdGqiQickJG79310uF3xR+A3nmm42Yn2lTOViJq+TM22VJXpYC3JTp2AqFaw+6oNOnawNDbFM6SAyMnF+hZGtm53IsIaZI7pvtVexVrCQKOyfIx6qpBIDY8UP+O9Dd8/xJnHdSGmLAAAAAElFTkSuQmCC"; item.POS = 80; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 36; item.CHAR = '$';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAApklEQVQoU32QKw7CUBBFz4RUsQAcmmXgkKBQKFyTpl0A4VOBQILBoVgAqqlkCyi6BdaAGDJM4VFCGPXy7pw7d0b4U9LQVuxRruTs7N/FJSnC9t2oZNYg5AxQjsAQJaHFhgUXJ52aI0xQUiKmzLi5uKbDnTPQe9rWlmGmvdy+rOcWtBmHtGb/KiEmoh9EW0M4oYyArpPfa0BllIVy0kMdPpM2A/044wPpYCjVE4UxoAAAAABJRU5ErkJggg=="; item.POS = 81; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 37; item.CHAR = '%';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAn0lEQVQoU4XQPQ4BYRSF4eebhEahtAadJSiIVmUNotBpxRQ2MI09KHQSdqBVsQiRSKgkPpmR+GnG7W7enHPPPUHJhHKY6ok2OKpoq7q6WYq2wcxakIn6osPbKZX9Qlo4qxmYuAUf21x0kuiY2ufLJ9DMQlQXdAvrROcFX+qx6IKdoJnfD+Ya7lYSIw/DItQb5mmjrVT2db94608JJRU9AYyaMiEinEqzAAAAAElFTkSuQmCC"; item.POS = 82; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 94; item.CHAR = '^';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAbUlEQVQoU8XOMQqCAByF8d8/qLG16zQEncVF2qPUwd2trb0jtHkFQegMncFBEQcXCbfe9nh8jy/8SKwbM6lws3Fy14zQRJYOOk+02Msl85h76H3svHRq4SLzDoWzXmXr6Opruk/GvlJoQfsf5AAmvxOpdqbBRAAAAABJRU5ErkJggg=="; item.POS = 83; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 38; item.CHAR = '&';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAApUlEQVQoU43QIW5CYRCF0TNNUOBxteDYQh0JC6CGBYAhCBwJfSVhAaiyBepJKhBsAYfGsQAMJED+8p4hKXTUzXyTe2cmPKh4Dj/0hVk+OJfpJR0mGs4WXrz/wkKPbQq4EjrOakJPyZuR/S0z84UutgW42X5quvjJ87bCwMUsxYTMEjtlQwffaBUOCSbLV2VtRxUna6zTxmGqmjfqdzfP//GEP950Bb1xJ3HU9zYwAAAAAElFTkSuQmCC"; item.POS = 84; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 42; item.CHAR = '*';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAW0lEQVQoU2NkwAMYiZNsZHBn+M+Qx8DNEMZQyvAVpImRoZVBnOE3wwEGRoYlDP8YtMEmMTJ8ZGhgyEQY28AwnYGBIYOBkcGDoZ5hJwk6sdpJuVewmEBkIGDRCQB53xcNtuV1UgAAAABJRU5ErkJggg=="; item.POS = 85; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 59; item.CHAR = ';';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAYUlEQVQoU2NkwAMYaS3ZwDCdgYEhhIGJwYWhjuEiyDqEnXglsbiMSNdCjM1gYGTwYKhn2IlqZyODO8N/hh0MDAwzGBoYMlElmxj0Gf4x7GFgZIhB1Qkz8j9DPkMjwySY2wDbvxUNWbWqugAAAABJRU5ErkJggg=="; item.POS = 86; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 34; item.CHAR = '"';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAARElEQVQoU2NkwAMYCUs2MExnYGCQY2BgaGVgYJjHwMQQzlDHcBGis54hj4GRwR2fpCYDE8MMhn8MK1F14rCYCAcNIp0APa4NDUdRuj8AAAAASUVORK5CYII="; item.POS = 87; _CharImageList.Add(item);
   item = new CharImages(); item.DEC = 32; item.CHAR = ' ';
   item.B64 = "iVBORw0KGgoAAAANSUhEUgAAAAcAAAAMCAYAAACulacQAAAAE0lEQVQoU2NkwAMYRyUZGMgPBAAKMgANjdZyFAAAAABJRU5ErkJggg=="; item.POS = 88; _CharImageList.Add(item);

  }

  public struct CharImages
  {
   public int POS { get; set; }
   public int DEC { get; set; }
   public string B64 { get; set; }
   public char CHAR { get; set; }
  }

 }

}