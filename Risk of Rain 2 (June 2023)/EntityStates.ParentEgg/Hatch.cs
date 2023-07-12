using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ParentEgg;

public class Hatch : GenericCharacterDeath
{
	private SpawnerPodsController controller;

	public override void OnEnter()
	{
		controller = GetComponent<SpawnerPodsController>();
		base.OnEnter();
		if (NetworkServer.active)
		{
			DoHatch();
		}
	}

	protected override void PlayDeathAnimation(float crossfadeDuration)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		PlayAnimation("Body", "Hatch");
		EffectManager.SimpleEffect(controller.hatchEffect, base.gameObject.transform.position, base.transform.rotation, transmit: false);
	}

	protected override void PlayDeathSound()
	{
		Util.PlaySound(controller.podHatchSound, base.gameObject);
	}

	private void DoHatch()
	{
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscParent"), new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Direct,
			minDistance = 0f,
			maxDistance = 1f,
			spawnOnTarget = base.transform
		}, RoR2Application.rng);
		directorSpawnRequest.summonerBodyObject = base.gameObject;
		directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, new Action<SpawnCard.SpawnResult>(OnHatchlingSpawned));
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
	}

	private void OnHatchlingSpawned(SpawnCard.SpawnResult spawnResult)
	{
		CharacterMaster component = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
		component.teamIndex = base.characterBody.teamComponent.teamIndex;
		CharacterMaster master = base.characterBody.master;
		CharacterMaster characterMaster = (Object.op_Implicit((Object)(object)master) ? master.minionOwnership.ownerMaster : null);
		if (Object.op_Implicit((Object)(object)component))
		{
			Inventory inventory = base.characterBody.master.inventory;
			Inventory inventory2 = component.inventory;
			inventory2.CopyItemsFrom(inventory);
			inventory2.CopyEquipmentFrom(inventory);
			GameObject bodyObject = component.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject) && Object.op_Implicit((Object)(object)characterMaster))
			{
				Deployable component2 = bodyObject.GetComponent<Deployable>();
				characterMaster.AddDeployable(component2, DeployableSlot.ParentAlly);
			}
		}
	}
}
