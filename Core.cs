namespace Oxide.Plugins
{
  using System;
  using System.Collections.Generic;
  using Oxide.Core;
  using Oxide.Core.Configuration;
  using UnityEngine;

  [Info("AuthGroups", "chucklenugget", "0.2.0")]
  public partial class AuthGroups : RustPlugin
  {
    const string FAILURE_EFFECT = "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab";

    DynamicConfigFile DataFile;
    AuthGroupManager Groups;
    Dictionary<ulong, Interaction> PendingInteractions = new Dictionary<ulong, Interaction>();

    void Init()
    {
      DataFile = Interface.Oxide.DataFileSystem.GetFile("AuthGroups");
      Groups = new GameObject().AddComponent<AuthGroupManager>();
    }

    void OnServerInitialized()
    {
      List<AuthGroupInfo> infos = new List<AuthGroupInfo>();

      try
      {
        infos = DataFile.ReadObject<List<AuthGroupInfo>>();
        Puts($"Loaded {infos.Count} auth groups.");
      }
      catch (Exception ex)
      {
        PrintError(ex.ToString());
        PrintWarning("Couldn't load data, defaulting to empty list of groups.");
      }

      Groups.Init(this, infos);
    }

    void OnServerSave()
    {
      DataFile.WriteObject(Groups.Serialize());
    }

    void OnHammerHit(BasePlayer player, HitInfo hit)
    {
      Interaction interaction;

      if (PendingInteractions.TryGetValue(player.userID, out interaction))
        interaction.TryComplete(hit);
    }

    void OnEntityKill(BaseNetworkable networkable)
    {
      var entity = networkable as BaseEntity;

      if (entity == null || !ManagedEntity.IsSupportedEntity(entity))
        return;

      foreach (AuthGroup group in Groups.GetAllByEntity(entity))
      {
        group.RemoveEntity(entity);
        Puts($"Managed entity {entity.net.ID} was removed from auth group {group.Name} of player {group.Owner.net.ID} since it was destroyed.");
      }
    }

    object CanPickupEntity(BaseCombatEntity entity, BasePlayer player)
    {
      var turret = entity as AutoTurret;

      if (turret != null && ShouldBlockAccess(turret, player))
      {
        PlayEffectAtEntity(turret, FAILURE_EFFECT);
        return false;
      }

      return null;
    }

    void OnLootEntity(BasePlayer player, BaseEntity entity)
    {
      var turret = entity.GetComponent<AutoTurret>();

      if (turret != null && ShouldBlockAccess(turret, player))
      {
        PlayEffectAtEntity(turret, FAILURE_EFFECT);
        NextTick(player.EndLooting);
      }
    }

    object OnTurretAuthorize(AutoTurret turret, BasePlayer player)
    {
      if (turret != null && ShouldBlockAccess(turret, player))
      {
        PlayEffectAtEntity(turret, FAILURE_EFFECT);
        return false;
      }

      return null;
    }

    object OnTurretDeauthorize(AutoTurret turret, BasePlayer player)
    {
      if (turret != null && ShouldBlockAccess(turret, player))
      {
        PlayEffectAtEntity(turret, FAILURE_EFFECT);
        return false;
      }

      return null;
    }

    bool ShouldBlockAccess(AutoTurret turret, BasePlayer player)
    {
      foreach (AuthGroup group in Groups.GetAllByEntity(turret))
      {
        if (group.HasFlag(AuthGroupFlag.TurretsLocked) && !group.HasOwner(player) && !group.HasManager(player))
          return true;
      }

      return false;
    }

    void PlayEffectAtEntity(BaseEntity entity, string effectPrefab)
    {
      Effect.server.Run(effectPrefab, entity, 0, Vector3.zero, Vector3.forward);
    }
  }
}
