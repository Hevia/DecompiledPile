using System;
using EntityStates.GummyClone;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class GummyCloneProjectile : MonoBehaviour, IProjectileImpactBehavior
{
	[SerializeField]
	private int damageBoostCount = 10;

	[SerializeField]
	private int hpBoostCount = 10;

	[SerializeField]
	private float maxLifetime = 10f;

	private float stopwatch;

	private bool isAlive = true;

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		if (isAlive)
		{
			SpawnGummyClone();
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

	public void SpawnGummyClone()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		ProjectileController component = ((Component)this).GetComponent<ProjectileController>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.owner))
		{
			return;
		}
		CharacterBody component2 = component.owner.GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		CharacterMaster characterMaster = component2.master;
		if (!Object.op_Implicit((Object)(object)characterMaster) || characterMaster.IsDeployableLimited(DeployableSlot.GummyClone))
		{
			return;
		}
		MasterCopySpawnCard masterCopySpawnCard = MasterCopySpawnCard.FromMaster(characterMaster, copyItems: false, copyEquipment: false);
		if (!Object.op_Implicit((Object)(object)masterCopySpawnCard))
		{
			return;
		}
		masterCopySpawnCard.GiveItem(DLC1Content.Items.GummyCloneIdentifier);
		masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostDamage, damageBoostCount);
		masterCopySpawnCard.GiveItem(RoR2Content.Items.BoostHp, hpBoostCount);
		Transform transform = ((Component)this).transform;
		DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
		{
			spawnOnTarget = transform,
			placementMode = DirectorPlacementRule.PlacementMode.Direct
		};
		DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(masterCopySpawnCard, directorPlacementRule, new Xoroshiro128Plus(Run.instance.seed + (ulong)Run.instance.fixedTime));
		directorSpawnRequest.teamIndexOverride = characterMaster.teamIndex;
		directorSpawnRequest.ignoreTeamMemberLimit = true;
		directorSpawnRequest.summonerBodyObject = ((Component)component2).gameObject;
		directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult result)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			CharacterMaster component3 = result.spawnedInstance.GetComponent<CharacterMaster>();
			Deployable deployable = result.spawnedInstance.AddComponent<Deployable>();
			characterMaster.AddDeployable(deployable, DeployableSlot.GummyClone);
			deployable.onUndeploy = (UnityEvent)(((object)deployable.onUndeploy) ?? ((object)new UnityEvent()));
			deployable.onUndeploy.AddListener(new UnityAction(component3.TrueKill));
			GameObject bodyObject = component3.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				EntityStateMachine[] components = bodyObject.GetComponents<EntityStateMachine>();
				foreach (EntityStateMachine entityStateMachine in components)
				{
					if (entityStateMachine.customName == "Body")
					{
						entityStateMachine.SetState(new GummyCloneSpawnState());
						break;
					}
				}
			}
		});
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		Object.Destroy((Object)(object)masterCopySpawnCard);
	}
}
