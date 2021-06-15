using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.Core.Utils
{
  public static class Common
  {
    public static string ToBase64(this object obj, JsonSerializerSettings settings = null)
    {
      var data = settings != null ?
        JsonConvert.SerializeObject(obj, settings) : JsonConvert.SerializeObject(obj);
      byte[] bts = System.Text.Encoding.UTF8.GetBytes(data.ToString());
      return Convert.ToBase64String(bts);
    }
  }
}
