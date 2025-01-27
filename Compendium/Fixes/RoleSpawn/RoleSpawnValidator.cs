using System.Collections.Generic;
using Compendium.Extensions;
using Compendium.Features;
using helpers.Configuration;
using PlayerRoles;
using UnityEngine;

namespace Compendium.Fixes.RoleSpawn;

public static class RoleSpawnValidator
{
	[Config(Name = "Enabled Validators", Description = "A list of enabled spawn point validators.")]
	public static List<RoleSpawnValidationType> EnabledValidators { get; set; } = new List<RoleSpawnValidationType>
	{
		RoleSpawnValidationType.SpawnpointDistance,
		RoleSpawnValidationType.YAxis
	};


	[Config(Name = "Blacklisted Roles", Description = "A list of blacklisted roles.")]
	public static Dictionary<RoleSpawnValidationType, List<RoleTypeId>> BlacklistedRoles { get; set; } = new Dictionary<RoleSpawnValidationType, List<RoleTypeId>>
	{
		[RoleSpawnValidationType.SpawnpointDistance] = new List<RoleTypeId> { RoleTypeId.Tutorial },
		[RoleSpawnValidationType.YAxis] = new List<RoleTypeId> { RoleTypeId.Tutorial }
	};


	[Config(Name = "Minimum Axis Values", Description = "A list of minimum axis values.")]
	public static Dictionary<Team, Dictionary<RoleSpawnValidationType, float>> MinimumAxisValues { get; set; } = new Dictionary<Team, Dictionary<RoleSpawnValidationType, float>>
	{
		[Team.ClassD] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		},
		[Team.Scientists] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		},
		[Team.OtherAlive] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		},
		[Team.SCPs] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		},
		[Team.ChaosInsurgency] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		},
		[Team.FoundationForces] = new Dictionary<RoleSpawnValidationType, float>
		{
			[RoleSpawnValidationType.YAxis] = -1000f,
			[RoleSpawnValidationType.SpawnpointDistance] = 2f
		}
	};


	public static bool IsEnabled(RoleTypeId role, RoleSpawnValidationType roleSpawnValidationType, out float axisValue)
	{
		axisValue = 0f;
		if (BlacklistedRoles.TryGetValue(roleSpawnValidationType, out var value) && value.Contains(role))
		{
			return false;
		}
		if (!EnabledValidators.Contains(roleSpawnValidationType))
		{
			return false;
		}
		Team team = role.GetTeam();
		if (!MinimumAxisValues.TryGetValue(team, out var value2))
		{
			FLog.Warn($"Role {role} with validator {roleSpawnValidationType} is enabled, but doesn't have any registered axis values for team {team}.");
			return false;
		}
		if (!value2.TryGetValue(roleSpawnValidationType, out axisValue))
		{
			FLog.Warn($"Role {role} with validator {roleSpawnValidationType} is enabled, but doesn't have any registered axis values for team {team}.");
			return false;
		}
		return true;
	}

	public static bool TryValidate(Vector3 position, RoleSpawnValidationType roleSpawnValidationType, float axis, Vector3? spawnpoint = null)
	{
		switch (roleSpawnValidationType)
		{
		case RoleSpawnValidationType.YAxis:
			if (position.y <= axis)
			{
				return false;
			}
			return true;
		case RoleSpawnValidationType.SpawnpointDistance:
			if (!spawnpoint.HasValue)
			{
				return false;
			}
			if (!position.IsWithinDistance(spawnpoint.Value, axis))
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}
}
