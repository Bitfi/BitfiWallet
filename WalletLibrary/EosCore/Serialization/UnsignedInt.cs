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
  public class UnsignedInt : BaseCustomType
  {
    public uint value { get; set; }

    public UnsignedInt() { }

    public UnsignedInt(uint value)
    {
      this.value = value;
    }

    public override bool CanConvert(Type objectType)
    {
      return (objectType == typeof(JTokenType));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      value = serializer.Deserialize<uint>(reader);
      var entity = (UnsignedInt)Activator.CreateInstance(objectType, value);
      return entity;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      //serialize as actual JSON and not string data
      var token = JToken.Parse(this.value.ToString());
      writer.WriteToken(token.CreateReader());
    }

    public override void WriteToStream(Stream stream)
    {
      var v = value;
      while (v >= 0x80)
      {
        stream.WriteByte((byte)(0x80 | (v & 0x7f)));
        v >>= 7;
      }
      stream.WriteByte((byte)v);
    }
  }
}
