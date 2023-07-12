using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class NetworkedBodySpawnSlot : MonoBehaviour, MasterSpawnSlotController.ISlot
{
	[SerializeField]
	private SpawnCard spawnCard;

	[SerializeField]
	private CharacterBody ownerBody;

	[SerializeField]
	private ChildLocator ownerChildLocator;

	[SerializeField]
	private string ownerAttachChildName;

	[SerializeField]
	private GameObject spawnEffectPrefab;

	[SerializeField]
	private GameObject killEffectPrefab;

	private CharacterMaster spawnedMaster;

	private bool isSpawnPending;

	public bool IsOpen()
	{
		if (!isSpawnPending)
		{
			return !Object.op_Implicit((Object)(object)spawnedMaster);
		}
		return false;
	}

	public void Spawn(GameObject summonerBodyObject, Xoroshiro128Plus rng, Action<MasterSpawnSlotController.ISlot, SpawnCard.SpawnResult> callback = null)
	{
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)ownerBody))
		{
			return;
		}
		Transform transform = ((Component)ownerBody).transform;
		if (!string.IsNullOrEmpty(ownerAttachChildName) && Object.op_Implicit((Object)(object)ownerChildLocator))
		{
			Transform val = ownerChildLocator.FindChild(ownerAttachChildName);
			if (Object.op_Implicit((Object)(object)val))
			{
				transform = ((Component)val).transform;
			}
		}
		DirectorPlacementRule placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Direct,
			spawnOnTarget = transform
		};
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, rng);
		directorSpawnRequest.summonerBodyObject = summonerBodyObject;
		directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult spawnResult)
		{
			OnSpawnedServer(((Component)ownerBody).gameObject, spawnResult, callback);
		});
		isSpawnPending = true;
		DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
	}

	public void Kill()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && Object.op_Implicit((Object)(object)spawnedMaster))
		{
			GameObject bodyObject = spawnedMaster.GetBodyObject();
			if (Object.op_Implicit((Object)(object)killEffectPrefab) && Object.op_Implicit((Object)(object)bodyObject))
			{
				EffectData effectData = new EffectData
				{
					origin = bodyObject.transform.position,
					rotation = bodyObject.transform.rotation
				};
				EffectManager.SpawnEffect(killEffectPrefab, effectData, transmit: true);
			}
			spawnedMaster.TrueKill();
			spawnedMaster = null;
		}
	}

	private void OnSpawnedServer(GameObject ownerBodyObject, SpawnCard.SpawnResult spawnResult, Action<MasterSpawnSlotController.ISlot, SpawnCard.SpawnResult> callback)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		isSpawnPending = false;
		if (spawnResult.success)
		{
			spawnedMaster = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
			spawnedMaster.onBodyDestroyed += OnBodyDestroyed;
			GameObject bodyObject = spawnedMaster.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
				{
					EffectData effectData = new EffectData
					{
						origin = bodyObject.transform.position,
						rotation = bodyObject.transform.rotation
					};
					EffectManager.SpawnEffect(spawnEffectPrefab, effectData, transmit: true);
				}
				NetworkedBodyAttachment component = bodyObject.GetComponent<NetworkedBodyAttachment>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.AttachToGameObjectAndSpawn(ownerBodyObject, ownerAttachChildName);
				}
			}
		}
		callback?.Invoke(this, spawnResult);
	}

	private void OnBodyDestroyed(CharacterBody body)
	{
		if (body.master.IsDeadAndOutOfLivesServer() && Object.op_Implicit((Object)(object)spawnedMaster))
		{
			spawnedMaster.onBodyDestroyed -= OnBodyDestroyed;
			spawnedMaster = null;
		}
	}
}
