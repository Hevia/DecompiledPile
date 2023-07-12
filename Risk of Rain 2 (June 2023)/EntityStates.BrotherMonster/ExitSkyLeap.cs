using System;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class ExitSkyLeap : BaseState
{
	public static float baseDuration;

	public static float baseDurationUntilRecastInterrupt;

	public static string soundString;

	public static GameObject waveProjectilePrefab;

	public static int waveProjectileCount;

	public static float waveProjectileDamageCoefficient;

	public static float waveProjectileForce;

	public static float recastChance;

	public static int cloneCount;

	public static int cloneDuration;

	public static SkillDef replacementSkillDef;

	private float duration;

	private float durationUntilRecastInterrupt;

	private bool recast;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(soundString, base.gameObject);
		PlayAnimation("Body", "ExitSkyLeap", "SkyLeap.playbackRate", duration);
		PlayAnimation("FullBody Override", "BufferEmpty");
		base.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, baseDuration);
		AimAnimator aimAnimator = GetAimAnimator();
		if (Object.op_Implicit((Object)(object)aimAnimator))
		{
			((Behaviour)aimAnimator).enabled = true;
		}
		if (base.isAuthority)
		{
			FireRingAuthority();
		}
		if (!Object.op_Implicit((Object)(object)PhaseCounter.instance) || PhaseCounter.instance.phase != 3)
		{
			return;
		}
		if (Random.value < recastChance)
		{
			recast = true;
		}
		for (int i = 0; i < cloneCount; i++)
		{
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscBrotherGlass"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = 3f,
				maxDistance = 20f,
				spawnOnTarget = base.gameObject.transform
			}, RoR2Application.rng);
			directorSpawnRequest.summonerBodyObject = base.gameObject;
			directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate(SpawnCard.SpawnResult spawnResult)
			{
				spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, cloneDuration);
			});
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
		GenericSkill genericSkill = (Object.op_Implicit((Object)(object)base.skillLocator) ? base.skillLocator.special : null);
		if (Object.op_Implicit((Object)(object)genericSkill))
		{
			genericSkill.SetSkillOverride(outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
	}

	private void FireRingAuthority()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		float num = 360f / (float)waveProjectileCount;
		Vector3 val = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
		Vector3 footPosition = base.characterBody.footPosition;
		for (int i = 0; i < waveProjectileCount; i++)
		{
			Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * val;
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.characterBody.damage * waveProjectileDamageCoefficient, waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (recast && base.fixedAge > baseDurationUntilRecastInterrupt)
			{
				outer.SetNextState(new EnterSkyLeap());
			}
			if (base.fixedAge > duration)
			{
				outer.SetNextStateToMain();
			}
		}
	}
}
