namespace Oxide.Plugins
{
  public partial class AuthGroups : RustPlugin
  {
    abstract class Interaction
    {
      protected AuthGroups Core;

      public BasePlayer Player { get; private set; }
      public AuthGroup Group { get; private set; }

      protected Interaction(AuthGroups core, BasePlayer player, AuthGroup group)
      {
        Core = core;
        Player = player;
        Group = group;
      }

      public abstract bool TryComplete(HitInfo hit);
    }
  }
}
