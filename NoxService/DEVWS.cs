using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using DevMsgForm.BITFIDEV;

namespace NoxService.DEVWS
{
  public class DEVWS
  {
    DEVTOOL dev;
    public DEVWS()
    {
      dev = new DEVTOOL();
     
    }
    public NoxMessagesBasic[] GetTagList()
    {
      try
      {    
        dev.Timeout = 5000;

        return dev.GetSupportTagList();
      }
      catch (WebException)
      {
        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public SupportArticle GetArticle(string id)
    {
      try
      {
        dev.Timeout = 5000;
        return dev.GetSupportArticleByID(id);
      }
      catch (WebException)
      {
        return null;
      }
      catch (Exception)
      {
        return null;
      }
    }

  }
}

 
