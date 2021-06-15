using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.EosCore.Serialization
{
  public abstract class BaseCustomType : JsonConverter
  {
    public BaseCustomType() { }

    public override bool CanConvert(Type objectType)
    {
      return (objectType == typeof(JTokenType));
    }

    public abstract void WriteToStream(Stream stream);
  }
}
