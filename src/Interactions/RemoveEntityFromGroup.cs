namespace Oxide.Plugins
{
  public partial class AuthGroups : RustPlugin
  {
    class RemoveEntityFromGroup : Interaction
    {
      public RemoveEntityFromGroup(AuthGroups core, BasePlayer player, AuthGroup group)
        : base(core, player, group)
      {
      }

      public override bool TryComplete(HitInfo hit)
      {
        if (hit == null || hit.HitEntity == null)
          return false;

        ManagedEntity entity = Group.GetManagedEntity(hit.HitEntity);

        if (entity == null)
        {
          Core.SendReply(Player, Messages.CannotRemoveEntityNotEnrolledInGroup, Group.Name);
          return false;
        }

        if (!entity.IsAuthorized(Player))
        {
          Core.SendReply(Player, Messages.CannotRemoveEntityNotAuthorized);
          return false;
        }

        Group.RemoveEntity(entity);
        Core.SendReply(Player, Messages.EntityRemoved, Group.Name);

        return true;
      }
    }
  }
}
