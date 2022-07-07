using System;
using NoxService.NWS;
using NoxService.SWS;

namespace BitfiWallet.DeviceManager
{
 public class NoxTManager
 {
  private readonly Device _Device;


  public BatStatus batStatus = new BatStatus() { IsCharging = false, IsError = true, Level = 0 };

  public UpdateStatus updateStatus = new UpdateStatus() { Available = false, Progress = UpdateProgress.UNKNOWN };

  public readonly SGADWS walletServ;

  public readonly NOXWS2 serv;

  public NoxTManager(string DeviceKey, string DeviceID)
  {
   _Device = new Device(DeviceKey, DeviceID);

   walletServ = new SGADWS();

   serv = new NOXWS2();
  }

  public Device noxDevice
  {
   get
   {
    return _Device;
   }
  }
 }
}
