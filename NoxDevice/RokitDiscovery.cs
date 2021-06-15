using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using EthereumLibrary.Hex.HexConvertors.Extensions;

namespace BitfiWallet
{
 class RokitDiscovery
 {

  public async static Task<List<string>> RunScan(CancellationTokenSource cancellationTokenSource)
  {

   var work = new RokitWorkDelegate(() =>
   {
    try
    {
     byte[] muticast = new byte[4];

     var host = Dns.GetHostEntry(Dns.GetHostName());
     foreach (var ip in host.AddressList)
     {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
       var ipbytes = ip.GetAddressBytes();
       muticast[0] = ipbytes[0];
       muticast[1] = ipbytes[1];
       muticast[2] = ipbytes[2];
       muticast[3] = 255;

       break;
      }
     }

     RokitsBroadcaster rBroadcaster = new RokitsBroadcaster();
     return rBroadcaster.Broadcast(muticast);

    }
    catch (Exception ex)
    {
     return null;

    }

   });

   var resp = await work.ToRokit(10, cancellationTokenSource);

   if (resp.Error != null)
    return null;

   return (List<string>)resp.Content;

  }

 }

 public class RokitsBroadcaster
 {
  private static readonly byte[] DISCOVERY_REQUEST_PACKET = System.Text.Encoding.ASCII.GetBytes("rokits");

  private int initialDiscoveryResponseDelay = 0x7d0;
  private int lateResponseArrivalDelay = 500;

  const int UDP_SERVER_PORT = 4201;

  private void InitialDiscoveryResponseWait(Socket mcastSocket)
  {
   for (int i = (this.initialDiscoveryResponseDelay / 50) + 1;
    (mcastSocket.Available == 0) && (i > 0); i--)
   {
    Thread.Sleep(50);
   }

  }

  public List<string> Broadcast(byte[] directedIpAddress)
  {
   List<string> list = new List<string>();

   IPAddress address = new IPAddress(directedIpAddress);

   using (Socket mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
   {

    mcastSocket.EnableBroadcast = true;

    IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
    mcastSocket.Bind(localEP);

    IPEndPoint remoteEP = new IPEndPoint(address, UDP_SERVER_PORT);
    mcastSocket.SendTo(DISCOVERY_REQUEST_PACKET, remoteEP);

    this.InitialDiscoveryResponseWait(mcastSocket);

    bool flag = mcastSocket.Available != 0;
    while (flag)
    {
     EndPoint point;
     if (mcastSocket.Available != 0)
     {
      goto Label_00B8;
     }
     flag = false;
     continue;

    Label_0031:
     point = new IPEndPoint(IPAddress.Any, 0);
     byte[] buffer = new byte[mcastSocket.Available];
     mcastSocket.ReceiveFrom(buffer, ref point);

     if (buffer.Length == 24)
     {
      byte[] server_ip = new byte[4];
      byte[] pub_key = new byte[20];

      Buffer.BlockCopy(buffer, 0, server_ip, 0, 4);
      Buffer.BlockCopy(buffer, 4, pub_key, 0, 20);

      IPAddress new_address = new IPAddress(server_ip);

      list.Add("wss://" + new_address.ToString() + ":4201");
      list.Add(pub_key.ToHex());
     }



    Label_00B8:
     if (mcastSocket.Available > 0)
     {
      goto Label_0031;
     }
     Thread.Sleep(this.lateResponseArrivalDelay);

    }

    mcastSocket.Close();

   }

   return list;

  }
 }

}

