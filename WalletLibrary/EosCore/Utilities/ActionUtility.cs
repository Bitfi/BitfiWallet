
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletLibrary.EosCore.Params;
using WalletLibrary.EosCore.Serialization;

namespace WalletLibrary.EosCore.Utilities
{
  public class ActionUtility
  {
    private ChainAPI chainAPI;
    public ActionUtility(string host)
    {
      chainAPI = new ChainAPI(host);
    }

    public Action GetActionObject(string actionName, string permissionActor, string permissionName, string code, object args)
    {
      //prepare action object
      Action action = new Action()
      {
        account = new AccountName(code),
        name = new ActionName(actionName),
        authorization = new[] {
          new Authorization {
              actor = new AccountName(permissionActor),
              permission = new PermissionName(permissionName)
          }
        }
      };

      //convert action arguments to binary and save it in action.datareturn await new EOS_Object<PushTransaction>(HOST).GetObjectsFromAPIAsync(new PushTransactionParam { packed_trx = Hex.ToString(packedTransaction), signatures = signatures, packed_context_free_data = string.Empty, compression = "none" });
      //var abiJsonToBin = chainAPI.GetAbiJsonToBin(code, actionName, args);
      //action.data = new BinaryString(abiJsonToBin.binargs);
      return action;
    }
  }
}
