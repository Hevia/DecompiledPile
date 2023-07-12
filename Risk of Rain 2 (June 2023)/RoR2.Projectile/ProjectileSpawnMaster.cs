using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileSpawnMaster : MonoBehaviour, IProjectileImpactBehavior
{
	[SerializeField]
	private float maxLifetime = 10f;

	[SerializeField]
	private CharacterSpawnCard spawnCard;

	[SerializeField]
	public DeployableSlot deployableSlot = DeployableSlot.None;

	private float stopwatch;

	private bool isAlive = true;

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		if (isAlive)
		{
			SpawnMaster();
			isAlive = false;
		}
	}

	protected void FixedUpdate()
	{
		stopwatch += Time.fixedDeltaTime;
		if (NetworkServer.active && (!isAlive || stopwatch > maxLifetime))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void SpawnMaster()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		ProjectileController component = ((Component)this).GetComponent<ProjectileController>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.owner))
		{
			return;
		}
		CharacterBody component2 = component.owner.GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component2) || !Object.op_Implicit((Object)(object)component2.master) || !Object.op_Implicit((Object)(object)spawnCard))
		{
			return;
		}
		CharacterMaster characterMaster = component2.master;
		if (!Object.op_Implicit((Object)(object)characterMaster) || (deployableSlot != DeployableSlot.None && characterMaster.IsDeployableLimited(deployableSlot)))
		{
			return;
		}
		Transform transform = ((Component)this).transform;
		DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
		{
			spawnOnTarget = transform,
			placementMode = DirectorPlacementRule.PlacementMode.Direct
		};
		DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, new Xoroshiro128Plus(Run.instance.seed + (ulong)Run.instance.fixedTime));
		directorSpawnRequest.teamIndexOverride = characterMaster.teamIndex;
		directorSpawnRequest.ignoreTeamMemberLimit = false;
		directorSpawnRequest.summonerBodyObject = component.owner;
		if (deployableSlot != DeployableSlot.None)
		{
			directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult result)
			{
				if (result.success && Object.op_Implicit((Object)(object)result.spawnedInstance))
				{
					result.spawnedInstance.GetComponent<CharacterMaster>();
					Deployable deployable = result.spawnedInstance.AddComponent<Deployable>();
					characterMaster.AddDeployable(deployable, deployableSlot);
				}
			});
		}
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
	}
}
