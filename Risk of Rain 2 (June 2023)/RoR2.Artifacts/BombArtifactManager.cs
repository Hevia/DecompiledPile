using System;
using System.Collections.Generic;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Artifacts;

public static class BombArtifactManager
{
	private struct BombRequest
	{
		public Vector3 spawnPosition;

		public Vector3 raycastOrigin;

		public float bombBaseDamage;

		public GameObject attacker;

		public TeamIndex teamIndex;

		public float velocityY;
	}

	private static FloatConVar cvSpiteBombCoefficient = new FloatConVar("spite_bomb_coefficient", ConVarFlags.Cheat, "0.5", "Multiplier for number of spite bombs.");

	private static GameObject bombPrefab;

	private static readonly int maxBombCount = 30;

	private static readonly float extraBombPerRadius = 4f;

	private static readonly float bombSpawnBaseRadius = 3f;

	private static readonly float bombSpawnRadiusCoefficient = 4f;

	private static readonly float bombDamageCoefficient = 1.5f;

	private static readonly Queue<BombRequest> bombRequestQueue = new Queue<BombRequest>();

	private static readonly float bombBlastRadius = 7f;

	private static readonly float bombFuseTimeout = 8f;

	private static readonly float maxBombStepUpDistance = 8f;

	private static readonly float maxBombFallDistance = 60f;

	private static ArtifactDef myArtifact => RoR2Content.Artifacts.bombArtifactDef;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
		bombPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SpiteBomb");
	}

	private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (NetworkServer.active && !((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			GlobalEventManager.onCharacterDeathGlobal += OnServerCharacterDeath;
			RoR2Application.onFixedUpdate += ProcessBombQueue;
		}
	}

	private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			bombRequestQueue.Clear();
			RoR2Application.onFixedUpdate -= ProcessBombQueue;
			GlobalEventManager.onCharacterDeathGlobal -= OnServerCharacterDeath;
		}
	}

	private static void SpawnBomb(BombRequest bombRequest, float groundY)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 spawnPosition = bombRequest.spawnPosition;
		if (spawnPosition.y < groundY + 4f)
		{
			spawnPosition.y = groundY + 4f;
		}
		Vector3 raycastOrigin = bombRequest.raycastOrigin;
		raycastOrigin.y = groundY;
		GameObject val = Object.Instantiate<GameObject>(bombPrefab, spawnPosition, Random.rotation);
		SpiteBombController component = val.GetComponent<SpiteBombController>();
		DelayBlast delayBlast = component.delayBlast;
		TeamFilter component2 = val.GetComponent<TeamFilter>();
		component.bouncePosition = raycastOrigin;
		component.initialVelocityY = bombRequest.velocityY;
		delayBlast.position = spawnPosition;
		delayBlast.baseDamage = bombRequest.bombBaseDamage;
		delayBlast.baseForce = 2300f;
		delayBlast.attacker = bombRequest.attacker;
		delayBlast.radius = bombBlastRadius;
		delayBlast.crit = false;
		delayBlast.procCoefficient = 0.75f;
		delayBlast.maxTimer = bombFuseTimeout;
		delayBlast.timerStagger = 0f;
		delayBlast.falloffModel = BlastAttack.FalloffModel.None;
		component2.teamIndex = bombRequest.teamIndex;
		NetworkServer.Spawn(val);
	}

	private static void OnServerCharacterDeath(DamageReport damageReport)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (damageReport.victimTeamIndex == TeamIndex.Monster)
		{
			CharacterBody victimBody = damageReport.victimBody;
			Vector3 corePosition = victimBody.corePosition;
			int num = Mathf.Min(maxBombCount, Mathf.CeilToInt(victimBody.bestFitRadius * extraBombPerRadius * cvSpiteBombCoefficient.value));
			for (int i = 0; i < num; i++)
			{
				Vector3 val = Random.insideUnitSphere * (bombSpawnBaseRadius + victimBody.bestFitRadius * bombSpawnRadiusCoefficient);
				BombRequest bombRequest = default(BombRequest);
				bombRequest.spawnPosition = corePosition;
				bombRequest.raycastOrigin = corePosition + val;
				bombRequest.bombBaseDamage = victimBody.damage * bombDamageCoefficient;
				bombRequest.attacker = ((Component)victimBody).gameObject;
				bombRequest.teamIndex = damageReport.victimTeamIndex;
				bombRequest.velocityY = Random.Range(5f, 25f);
				BombRequest item = bombRequest;
				bombRequestQueue.Enqueue(item);
			}
		}
	}

	private static void ProcessBombQueue()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (bombRequestQueue.Count > 0)
		{
			BombRequest bombRequest = bombRequestQueue.Dequeue();
			Ray val = new Ray(bombRequest.raycastOrigin + new Vector3(0f, maxBombStepUpDistance, 0f), Vector3.down);
			float num = maxBombStepUpDistance + maxBombFallDistance;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(val, ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				SpawnBomb(bombRequest, ((RaycastHit)(ref val2)).point.y);
			}
		}
	}
}
