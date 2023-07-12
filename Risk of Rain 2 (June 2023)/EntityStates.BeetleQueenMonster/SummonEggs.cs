using System;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleQueenMonster;

public class SummonEggs : BaseState
{
	public static float baseDuration = 3.5f;

	public static string attackSoundString;

	public static float randomRadius = 8f;

	public static GameObject spitPrefab;

	public static int maxSummonCount = 5;

	public static float summonInterval = 1f;

	private static float summonDuration = 3.26f;

	public static SpawnCard spawnCard;

	private Animator animator;

	private Transform modelTransform;

	private ChildLocator childLocator;

	private float duration;

	private float summonTimer;

	private int summonCount;

	private bool isSummoning;

	private BullseyeSearch enemySearch;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		duration = baseDuration;
		PlayCrossfade("Gesture", "SummonEggs", 0.5f);
		Util.PlaySound(attackSoundString, base.gameObject);
		if (NetworkServer.active)
		{
			enemySearch = new BullseyeSearch();
			enemySearch.filterByDistinctEntity = false;
			enemySearch.filterByLoS = false;
			enemySearch.maxDistanceFilter = float.PositiveInfinity;
			enemySearch.minDistanceFilter = 0f;
			enemySearch.minAngleFilter = 0f;
			enemySearch.maxAngleFilter = 180f;
			enemySearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
			enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
			enemySearch.viewer = base.characterBody;
		}
	}

	private void SummonEgg()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Vector3 searchOrigin = ((Ray)(ref aimRay)).origin;
		if (Object.op_Implicit((Object)(object)base.inputBank) && base.inputBank.GetAimRaycast(float.PositiveInfinity, out var hitInfo))
		{
			searchOrigin = ((RaycastHit)(ref hitInfo)).point;
		}
		if (enemySearch == null)
		{
			return;
		}
		enemySearch.searchOrigin = searchOrigin;
		enemySearch.RefreshCandidates();
		HurtBox hurtBox = enemySearch.GetResults().FirstOrDefault();
		Transform val = ((Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent)) ? hurtBox.healthComponent.body.coreTransform : base.characterBody.coreTransform);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			minDistance = 3f,
			maxDistance = 20f,
			spawnOnTarget = val
		}, RoR2Application.rng);
		directorSpawnRequest.summonerBodyObject = base.gameObject;
		directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult spawnResult)
		{
			if (spawnResult.success && Object.op_Implicit((Object)(object)spawnResult.spawnedInstance) && Object.op_Implicit((Object)(object)base.characterBody))
			{
				Inventory component = spawnResult.spawnedInstance.GetComponent<Inventory>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.CopyEquipmentFrom(base.characterBody.inventory);
				}
			}
		});
		DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		bool flag = animator.GetFloat("SummonEggs.active") > 0.9f;
		if (flag && !isSummoning)
		{
			string muzzleName = "Mouth";
			EffectManager.SimpleMuzzleFlash(spitPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (isSummoning)
		{
			summonTimer += Time.fixedDeltaTime;
			if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
			{
				summonCount++;
				summonTimer -= summonInterval;
				SummonEgg();
			}
		}
		isSummoning = flag;
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
