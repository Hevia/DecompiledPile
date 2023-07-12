using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class DeployableMinionSpawner : IDisposable
{
	public float respawnInterval = 30f;

	[CanBeNull]
	public SpawnCard spawnCard;

	public float minSpawnDistance = 3f;

	public float maxSpawnDistance = 40f;

	[NotNull]
	private Xoroshiro128Plus rng;

	private float respawnStopwatch = float.PositiveInfinity;

	[CanBeNull]
	public CharacterMaster ownerMaster { get; }

	public DeployableSlot deployableSlot { get; }

	public event Action<SpawnCard.SpawnResult> onMinionSpawnedServer;

	public DeployableMinionSpawner([CanBeNull] CharacterMaster ownerMaster, DeployableSlot deployableSlot, [NotNull] Xoroshiro128Plus rng)
	{
		this.ownerMaster = ownerMaster;
		this.deployableSlot = deployableSlot;
		this.rng = rng;
		RoR2Application.onFixedUpdate += FixedUpdate;
	}

	public void Dispose()
	{
		RoR2Application.onFixedUpdate -= FixedUpdate;
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)ownerMaster))
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		int deployableCount = ownerMaster.GetDeployableCount(deployableSlot);
		int deployableSameSlotLimit = ownerMaster.GetDeployableSameSlotLimit(deployableSlot);
		if (deployableCount >= deployableSameSlotLimit)
		{
			return;
		}
		respawnStopwatch += fixedDeltaTime;
		if (!(respawnStopwatch >= respawnInterval))
		{
			return;
		}
		CharacterBody body = ownerMaster.GetBody();
		if (Object.op_Implicit((Object)(object)body) && Object.op_Implicit((Object)(object)spawnCard))
		{
			try
			{
				SpawnMinion(spawnCard, body);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
			respawnStopwatch = 0f;
		}
	}

	private void SpawnMinion([NotNull] SpawnCard spawnCard, [NotNull] CharacterBody spawnTarget)
	{
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			minDistance = minSpawnDistance,
			maxDistance = maxSpawnDistance,
			spawnOnTarget = ((Component)spawnTarget).transform
		}, RoR2Application.rng);
		directorSpawnRequest.summonerBodyObject = ownerMaster.GetBodyObject();
		directorSpawnRequest.onSpawnedServer = OnMinionSpawnedServerInternal;
		DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
	}

	private void OnMinionSpawnedServerInternal([NotNull] SpawnCard.SpawnResult spawnResult)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		GameObject spawnedInstance = spawnResult.spawnedInstance;
		if (Object.op_Implicit((Object)(object)spawnedInstance))
		{
			CharacterMaster component = spawnedInstance.GetComponent<CharacterMaster>();
			Deployable deployable = spawnedInstance.AddComponent<Deployable>();
			deployable.onUndeploy = (UnityEvent)(((object)deployable.onUndeploy) ?? ((object)new UnityEvent()));
			if (Object.op_Implicit((Object)(object)component))
			{
				deployable.onUndeploy.AddListener(new UnityAction(component.TrueKill));
			}
			else
			{
				deployable.onUndeploy.AddListener((UnityAction)delegate
				{
					Object.Destroy((Object)(object)spawnedInstance);
				});
			}
			ownerMaster.AddDeployable(deployable, deployableSlot);
		}
		this.onMinionSpawnedServer?.Invoke(spawnResult);
	}
}
