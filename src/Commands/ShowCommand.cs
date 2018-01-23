namespace Oxide.Plugins
{
  using System;
  using System.Linq;
  using System.Text;

  public partial class AuthGroups : RustPlugin
  {
    void OnAuthGroupShowCommand(BasePlayer player, string groupName)
    {
      AuthGroup group = Groups.GetByOwnerOrManager(player, groupName);

      if (group == null)
      {
        SendReply(player, Messages.YouAreNotOwnerOrManagerOfGroup, groupName);
        return;
      }

      var sb = new StringBuilder();

      sb.Append($"Auth group <color=#ffd479>{group.Name}</color> ");

      if (group.Entities.Count == 0)
        sb.AppendLine("isn't managing any items.");
      else
        sb.AppendLine($"is managing <color=#ffd479>{group.Entities.Count}</color> items:");

      var cupboards = group.GetAllManagedEntitiesOfType<ManagedToolCupboard>();
      if (cupboards.Length > 0)
        sb.AppendLine($"  <color=#ffd479>{cupboards.Length}</color> tool cupboard(s)");

      var turrets = group.GetAllManagedEntitiesOfType<ManagedAutoTurret>();
      if (turrets.Length > 0)
        sb.AppendLine($"  <color=#ffd479>{turrets.Length}</color> turret(s)");

      var codeLocks = group.GetAllManagedEntitiesOfType<ManagedCodeLock>();
      if (codeLocks.Length > 0)
        sb.AppendLine($"  <color=#ffd479>{codeLocks.Length}</color> code lock(s)");

      sb.AppendLine($"Owner: {group.Owner.displayName}");

      if (group.Managers.Count > 0)
      {
        sb.Append($"<color=#ffd479>{group.Managers.Count}</color> managers: ");
        sb.AppendLine(String.Join(", ", group.Managers.Select(m => m.displayName).ToArray()));
      }

      sb.Append($"<color=#ffd479>{group.Members.Count}</color> members: ");
      sb.AppendLine(String.Join(", ", group.Members.Select(m => m.displayName).ToArray()));

      SendReply(player, sb.ToString());
    }
  }
}
