using System;
using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GrandParentBoss;

public class Offspring : BaseState
{
	[SerializeField]
	public GameObject SpawnerPodsPrefab;

	public static float randomRadius = 8f;

	public static float maxRange = 9999f;

	private Animator animator;

	private ChildLocator childLocator;

	private Transform modelTransform;

	private float duration;

	public static float baseDuration = 3.5f;

	public static string attackSoundString;

	private float summonInterval;

	private static float summonDuration = 3.26f;

	public static int maxSummonCount = 5;

	private float summonTimer;

	private bool isSummoning;

	private int summonCount;

	public static GameObject spawnEffect;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		duration = baseDuration;
		Util.PlaySound(attackSoundString, base.gameObject);
		summonInterval = summonDuration / (float)maxSummonCount;
		isSummoning = true;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (isSummoning)
		{
			summonTimer += Time.fixedDeltaTime;
			if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
			{
				summonCount++;
				summonTimer -= summonInterval;
				SpawnPods();
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void SpawnPods()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		Ray aimRay = GetAimRay();
		aimRay.origin = aimRay.origin + Random.insideUnitSphere * randomRadius;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			zero = ((RaycastHit)(ref val)).point;
		}
		zero = base.transform.position;
		Transform val2 = FindTargetFarthest(zero, GetTeam());
		if (Object.op_Implicit((Object)(object)val2))
		{
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscParentPod"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = 3f,
				maxDistance = 20f,
				spawnOnTarget = val2
			}, RoR2Application.rng);
			directorSpawnRequest.summonerBodyObject = base.gameObject;
			directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult spawnResult)
			{
				Inventory inventory = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().inventory;
				Inventory inventory2 = base.characterBody.inventory;
				inventory.CopyEquipmentFrom(inventory2);
			});
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			PlayAnimation("Body", "SpawnPodWarn", "spawnPodWarn.playbackRate", duration);
		}
	}

	private Transform FindTargetFarthest(Vector3 point, TeamIndex myTeam)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Transform result = null;
		TeamMask enemyTeams = TeamMask.GetEnemyTeams(myTeam);
		for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
		{
			if (!enemyTeams.HasTeam(teamIndex))
			{
				ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
				for (int i = 0; i < teamMembers.Count; i++)
				{
					float num2 = Vector3.Magnitude(((Component)teamMembers[i]).transform.position - point);
					if (num2 > num && num2 < maxRange)
					{
						num = num2;
						result = ((Component)teamMembers[i]).transform;
					}
				}
			}
		}
		return result;
	}
}
