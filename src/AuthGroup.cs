namespace Oxide.Plugins
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;

  public partial class AuthGroups : RustPlugin
  {
    class AuthGroup
    {
      public BasePlayer Owner;
      public string Name;
      public AuthGroupFlag Flags;

      public List<BasePlayer> Members;
      public List<BasePlayer> Managers;
      public List<ManagedEntity> Entities;

      public AuthGroup()
      {
        Members = new List<BasePlayer>();
        Managers = new List<BasePlayer>();
        Entities = new List<ManagedEntity>();
      }

      public AuthGroup(BasePlayer owner, string name)
        : this()
      {
        Owner = owner;
        Name = name;
        Members.Add(owner);
      }

      public bool AddMember(BasePlayer player)
      {
        if (HasMember(player))
          return false;

        Members.Add(player);

        foreach (ManagedEntity entity in Entities.Where(entity => !entity.IsAuthorized(player)))
          entity.Authorize(player);

        return true;
      }

      public bool RemoveMember(BasePlayer player)
      {
        if (HasOwner(player))
          throw new InvalidOperationException($"Cannot remove player {player.net.ID} from auth group, since they are the owner");

        if (!HasMember(player))
          return false;

        Members.RemoveAll(member => member.userID == player.userID);
        Managers.RemoveAll(manager => manager.userID == player.userID);

        foreach (ManagedEntity entity in Entities)
          entity.Deauthorize(player);

        return true;
      }

      public void Promote(BasePlayer player)
      {
        if (!Members.Contains(player))
          throw new InvalidOperationException($"Tried to promote player {player.userID} in group {Name} owned by {Owner.userID}, but they are not a member!");

        Managers.Add(player);
      }

      public void Demote(BasePlayer player)
      {
        if (!Managers.Contains(player))
          throw new InvalidOperationException($"Tried to demote player {player.userID} in group {Name} owned by {Owner.userID}, but they are not a manager!");

        Managers.Remove(player);
      }

      public bool AddEntity(ManagedEntity entity)
      {
        if (Entities.Any(e => e.Id == entity.Id))
          return false;

        Entities.Add(entity);

        entity.DeauthorizeAll();
        entity.Authorize(Members);
        return true;
      }

      public bool RemoveEntity(ManagedEntity entity)
      {
        if (!Entities.Remove(entity))
          return false;

        entity.DeauthorizeAll();
        return true;
      }

      public bool RemoveEntity(BaseEntity entity)
      {
        ManagedEntity enrolled = GetManagedEntity(entity);

        if (enrolled == null)
          return false;

        return RemoveEntity(enrolled);
      }

      public void SynchronizeAll()
      {
        foreach (ManagedEntity entity in Entities)
          entity.SetAuthorizedPlayers(Members);
      }

      public void DeauthorizeAll()
      {
        foreach (ManagedEntity entity in Entities)
          entity.DeauthorizeAll();
      }

      public ManagedEntity GetManagedEntity(BaseEntity entity)
      {
        return Entities.FirstOrDefault(e => e.Id == entity.net.ID);
      }

      public T[] GetAllManagedEntitiesOfType<T>() where T : ManagedEntity
      {
        return Entities.OfType<T>().ToArray();
      }

      public bool HasEntity(BaseEntity entity)
      {
        return GetManagedEntity(entity) != null;
      }

      public bool HasOwner(BasePlayer player)
      {
        return Owner.userID == player.userID;
      }

      public bool HasManager(BasePlayer player)
      {
        return Managers.Any(manager => manager.userID == player.userID);
      }

      public bool HasMember(BasePlayer player)
      {
        return Members.Any(member => member.userID == player.userID);
      }

      public bool HasFlag(AuthGroupFlag flag)
      {
        return (Flags & flag) != 0;
      }

      public void AddFlag(AuthGroupFlag flag)
      {
        Flags |= flag;
      }

      public void RemoveFlag(AuthGroupFlag flag)
      {
        Flags &= ~flag;
      }

      public AuthGroupInfo Serialize()
      {
        return new AuthGroupInfo {
          Name = Name,
          OwnerId = Owner.UserIDString,
          Flags = Flags,
          ManagerIds = Managers.Select(player => player.UserIDString).ToArray(),
          MemberIds = Members.Select(player => player.UserIDString).ToArray(),
          EntityIds = Entities.Select(entity => entity.Id).ToArray()
        };
      }
    }

    public class AuthGroupInfo
    {
      [JsonProperty("name")]
      public string Name;

      [JsonProperty("ownerId")]
      public string OwnerId;

      [JsonProperty("flags"), JsonConverter(typeof(StringEnumConverter))]
      public AuthGroupFlag Flags;

      [JsonProperty("memberIds")]
      public string[] MemberIds;

      [JsonProperty("managerIds")]
      public string[] ManagerIds;

      [JsonProperty("entityIds")]
      public uint[] EntityIds;
    }

    [Flags]
    public enum AuthGroupFlag
    {
      TurretsLocked = 1
    }
  }
}
