namespace Oxide.Plugins
{
  public partial class AuthGroups : RustPlugin
  {
    void OnAuthGroupCancelCommand(BasePlayer player)
    {
      if (PendingInteractions.Remove(player.userID))
        SendReply(player, Messages.InteractionCanceled);
      else
        OnAuthGroupHelpCommand(player);
    }
  }
}
