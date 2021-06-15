using Newtonsoft.Json;
using System;
using static NeoGasLibrary.NeoAPI;

namespace NeoGasLibrary.Neoscan
{
  public class NeoscanApi
  {
    private readonly string TemplateUrl = "";

    public NeoscanApi(Net net)
    {
      TemplateUrl = String.Format("https://neoscan.io/api/{0}/{1}", net.ToString().ToLower() + "_net", "{0}/{1}/{2}");
    }

    public NeoscanBalance GetBalance(string address)
    {
            return null;
    }

    public NeoscanUnclaimed GetUnclaimed(string address)
    {
            return null;
    }

    public NeoscanEndpointInfo[] GetRpcEndpointsFull()
    {
            return null;
    }

    public string[] GetRPCEndpoints()
    {
            return null;
    }
  }
}
