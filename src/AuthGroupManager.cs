namespace Oxide.Plugins
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public partial class AuthGroups : RustPlugin
  {
    class AuthGroupManager : MonoBehaviour
    {
      AuthGroups Core;
      List<AuthGroup> Groups = new List<AuthGroup>();

      public void Init(AuthGroups core, IEnumerable<AuthGroupInfo> infos)
      {
        Core = core;

        foreach (AuthGroupInfo info in infos)
        {
          BasePlayer owner = BasePlayerEx.FindById(info.OwnerId);

          if (owner == null)
          {
            Core.PrintWarning($"Couldn't find owner {info.OwnerId} for auth group! Ignoring.");
            continue;
          }

          Groups.Add(new AuthGroup {
            Name = info.Name,
            Owner = owner,
            Flags = info.Flags,
            Members = FindPlayers(info.MemberIds).ToList(),
            Managers = FindPlayers(info.ManagerIds).ToList(),
            Entities = FindEntities(info.EntityIds).Select(ManagedEntity.Create).ToList()
          });
        }
      }

      public AuthGroup GetByOwnerOrManager(BasePlayer player, string name)
      {
        return GetAllByOwnerOrManager(player).FirstOrDefault(group => String.Compare(group.Name, name, true) == 0);
      }

      public AuthGroup[] GetAllByOwnerOrManager(BasePlayer player)
      {
        return Groups.Where(group => group.HasOwner(player) || group.HasManager(player)).ToArray();
      }

      public AuthGroup[] GetAllByMember(BasePlayer player)
      {
        return Groups.Where(group => group.HasMember(player)).ToArray();
      }

      public AuthGroup[] GetAllByEntity(BaseEntity entity)
      {
        return Groups.Where(group => group.HasEntity(entity)).ToArray();
      }

      public AuthGroup Create(BasePlayer owner, string name)
      {
        if (GetByOwnerOrManager(owner, name) != null)
          throw new InvalidOperationException($"Player ${owner.userID} is already the manager of an auth group named {name}!");

        var group = new AuthGroup(owner, name);
        Groups.Add(group);

        return group;
      }

      public void Delete(AuthGroup group)
      {
        group.DeauthorizeAll();
        Groups.Remove(group);
      }

      public AuthGroupInfo[] Serialize()
      {
        return Groups.Select(group => group.Serialize()).ToArray();
      }

      IEnumerable<BasePlayer> FindPlayers(IEnumerable<string> ids)
      {
        return ids.Select(FindPlayer).Where(player => player != null);
      }

      IEnumerable<BaseEntity> FindEntities(IEnumerable<uint> ids)
      {
        return ids.Select(FindEntity).Where(entity => entity != null);
      }

      BasePlayer FindPlayer(string id)
      {
        var player = BasePlayerEx.FindById(id);

        if (player == null)
          Core.PrintWarning($"Couldn't find player {id} for an auth group!");

        return player;
      }

      BaseEntity FindEntity(uint id)
      {
        var entity = BaseNetworkable.serverEntities.Find(id) as BaseEntity;

        if (entity == null)
          Core.PrintWarning($"Couldn't find entity {id} for an auth group!");

        return entity;
      }
    }
  }
}
