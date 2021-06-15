using System;
using System.Collections.Generic;
using System.Text;

namespace WalletLibrary.NoxShared.WebSockets
{
  public class WebSocketClientOptions
  {
    public TimeSpan KeepAliveInterval { get; set; }
    public bool NoDelay { get; set; }
    public Dictionary<string, string> AdditionalHttpHeaders { get; set; }
    public bool IncludeExceptionInCloseResponse { get; set; }
    public string SecWebSocketExtensions { get; set; }
    public string SecWebSocketProtocol { get; set; }
    public WebSocketClientOptions()
    {
      KeepAliveInterval = TimeSpan.FromSeconds(20);
      NoDelay = true;
      AdditionalHttpHeaders = new Dictionary<string, string>();
      IncludeExceptionInCloseResponse = false;
      SecWebSocketProtocol = null;
    }
  }
}
