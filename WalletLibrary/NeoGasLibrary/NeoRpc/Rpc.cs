using NeoGasLibrary.Neoscan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeoGasLibrary.NeoAPI;

namespace NeoGasLibrary.NeoRpc
{
  public class Rpc
  {
    private readonly NeoscanApi NeoscanApi;
    private JsonSerializer Serializer = new JsonSerializer()
    {
      ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public Rpc(Net net)
    {
      NeoscanApi = new NeoscanApi(net);
    }

    private string GetBestRpcEndpoint()
    {
      var endpoints = NeoscanApi.GetRpcEndpointsFull();
      var ordered = endpoints.OrderByDescending(v => v.Height).ToArray();
      return ordered[0].Url;
      /*
      var endpoints = NeoscanApi.GetRPCEndpoints();
      if (endpoints == null || endpoints.Count() == 0)
      {
        throw new Exception("There is no available endpoints, try later!");
      }

      //Random rnd = new Random();
      //int index = rnd.Next(0, endpoints.Length - 1);

      return endpoints[0];
      */
    }

    public bool SendRawTransaction(string hexTx)
    {
            return true;
    }
  }
}
