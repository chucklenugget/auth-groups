namespace Oxide.Plugins
{
  public partial class AuthGroups : RustPlugin
  {
    class AddEntityToGroup : Interaction
    {
      public AddEntityToGroup(AuthGroups core, BasePlayer player, AuthGroup group)
        : base(core, player, group)
      {
      }

      public override bool TryComplete(HitInfo hit)
      {
        if (hit == null || hit.HitEntity == null)
          return false;

        if (Group.HasEntity(hit.HitEntity))
        {
          Core.SendReply(Player, Messages.CannotAddEntityAlreadyEnrolledInGroup, Group.Name);
          return false;
        }

        var entity = ManagedEntity.Create(hit.HitEntity);

        if (entity == null)
        {
          Core.SendReply(Player, Messages.InvalidEntitySelected);
          return false;
        }

        if (!entity.IsAuthorized(Player))
        {
          Core.SendReply(Player, Messages.CannotAddEntityNotAuthorized);
          return false;
        }

        Group.AddEntity(entity);
        Core.SendReply(Player, Messages.EntityAdded, Group.Name, Group.Members.Count);

        return true;
      }
    }
  }
}
