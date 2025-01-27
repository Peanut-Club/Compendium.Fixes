using System.Collections.Generic;
using System.Linq;
using CentralAuth;
using Compendium.Attributes;
using Compendium.Enums;
using Compendium.Features;
using helpers;
using helpers.Attributes;
using helpers.Pooling.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace Compendium.Fixes.RoleSpawn;

public static class RoleSpawnHandler
{
	public static readonly RoleTypeId[] ScpRoles = new RoleTypeId[6]
	{
		RoleTypeId.Scp049,
		RoleTypeId.Scp173,
		RoleTypeId.Scp106,
		RoleTypeId.Scp096,
		RoleTypeId.Scp079,
		RoleTypeId.Scp939
	};

	public static readonly RoleTypeId[] PossibleRoles = new RoleTypeId[3]
	{
		RoleTypeId.Scientist,
		RoleTypeId.ClassD,
		RoleTypeId.FacilityGuard
	};

	[Load]
	public static void Load()
	{
		if (!typeof(PlayerRoleManager).TryAddHandler<PlayerRoleManager.RoleChanged>("OnRoleChanged", OnRoleChanged))
		{
			FLog.Warn("Failed to register role spawn handler!");
		}
		else
		{
			FLog.Info("Succesfully registered role spawn handler.");
		}
	}

	[Unload]
	public static void Unload()
	{
		if (!typeof(PlayerRoleManager).TryRemoveHandler<PlayerRoleManager.RoleChanged>("OnRoleChanged", OnRoleChanged))
		{
			FLog.Warn("Failed to remove role spawn handler!");
		}
		else
		{
			FLog.Info("Succesfully removed role spawn handler.");
		}
	}

	public static void FixPosition(ReferenceHub hub, Vector3 position, Vector3 rotation)
	{
		hub.TryOverridePosition(position, rotation);
	}

	[RoundStateChanged(new RoundState[] { RoundState.InProgress })]
	private static void OnRoundStarted()
	{
		Calls.Delay(1.5f, delegate
		{
			ScpRoles.ForEach(delegate(RoleTypeId scpRole)
			{
				if (Hub.Hubs.Count((ReferenceHub hub) => hub.RoleId() == scpRole) > 1)
				{
					List<ReferenceHub> plysToRemove = ListPool<ReferenceHub>.Pool.Get();
					while (Hub.Hubs.Count((ReferenceHub hub) => hub.RoleId() == scpRole && !plysToRemove.Contains(hub)) > 1)
					{
						plysToRemove.Add(Hub.Hubs.Last((ReferenceHub hub) => hub.RoleId() == scpRole && !plysToRemove.Contains(hub)));
					}
					if (plysToRemove.Count > 0)
					{
						plysToRemove.ForEach(delegate(ReferenceHub hub)
						{
							FLog.Debug($"Removing duplicate SCP role from {hub.GetLogName(includeIp: false, includeRole: false)}: {scpRole}");
							RoleTypeId roleTypeId = PossibleRoles.RandomItem();
							hub.RoleId(roleTypeId);
							hub.Hint(string.Format("\n\n<b><color={0}>Your role was set to <color={1}>{2}</color> to prevent duplicate SCPs.</color></b>", "#33FFA5", "#FF0000", roleTypeId));
						});
					}
					ListPool<ReferenceHub>.Pool.Push(plysToRemove);
				}
			});
		});
	}

	private static void OnRoleChanged(ReferenceHub hub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
	{
		Calls.Delay(0.4f, delegate
		{
			if (newRole != null && newRole.Is<IFpcRole>() && newRole.ServerSpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint))
			{
				RoleTypeId roleId = hub.GetRoleId();
				if (RoleSpawnValidator.IsEnabled(roleId, RoleSpawnValidationType.YAxis, out var axisValue) && !RoleSpawnValidator.TryValidate(((Component)(object)hub).transform.position, RoleSpawnValidationType.YAxis, axisValue))
				{
					if ((object)hub != null && hub.roleManager != null && hub.roleManager.CurrentRole != null && hub.Mode == ClientInstanceMode.ReadyClient)
					{
						RoleTypeId roleId2 = hub.GetRoleId();
						if (roleId2 != RoleTypeId.Scp079 && roleId2 != RoleTypeId.Scp0492 && hub.roleManager.CurrentRole is IFpcRole fpcRole && fpcRole != null && fpcRole.SpawnpointHandler != null)
						{
							if (!fpcRole.SpawnpointHandler.TryGetSpawnpoint(out var position, out var horizontalRot))
							{
								FLog.Warn($"Failed to retrieve a spawnpoint of role {roleId2}!");
							}
							else
							{
								FixPosition(hub, position, new Vector3(horizontalRot, horizontalRot, horizontalRot));
							}
						}
					}
				}
				else if (RoleSpawnValidator.IsEnabled(roleId, RoleSpawnValidationType.SpawnpointDistance, out axisValue) && (object)hub != null && hub.roleManager != null && hub.roleManager.CurrentRole != null && hub.Mode == ClientInstanceMode.ReadyClient)
				{
					RoleTypeId roleId3 = hub.GetRoleId();
					if (roleId3 != RoleTypeId.Scp079 && roleId3 != RoleTypeId.Scp0492 && hub.roleManager.CurrentRole is IFpcRole fpcRole2 && fpcRole2 != null && fpcRole2.SpawnpointHandler != null)
					{
						if (!fpcRole2.SpawnpointHandler.TryGetSpawnpoint(out var position2, out var horizontalRot2))
						{
							FLog.Warn($"Failed to retrieve a spawnpoint of role {roleId3}!");
						}
						else
						{
							FixPosition(hub, position2, new Vector3(horizontalRot2, horizontalRot2, horizontalRot2));
						}
					}
				}
			}
		});
	}
}
