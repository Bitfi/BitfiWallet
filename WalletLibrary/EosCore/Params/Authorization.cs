using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.EosCore.Serialization;

namespace WalletLibrary.EosCore.Params
{
  public class Authorization
  {
    [JsonConverter(typeof(AccountName))]
    public AccountName actor { get; set; }
    [JsonConverter(typeof(PermissionName))]
    public PermissionName permission { get; set; }
  }
}
