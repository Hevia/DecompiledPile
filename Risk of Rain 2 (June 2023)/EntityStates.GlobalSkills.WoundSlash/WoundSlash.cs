using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GlobalSkills.WoundSlash;

public class WoundSlash : BaseSkillState
{
	public static float blastRadius;

	public static float blastDamageCoefficient;

	public static float blastForce;

	public static float blastProcCoefficient;

	public static GameObject blastImpactEffectPrefab;

	public static GameObject slashEffectPrefab;

	public static float slashEffectOffset;

	public static float baseDuration;

	public static string soundString;

	public static float shortHopVelocity;

	private float duration => baseDuration / attackSpeedStat;

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Ray aimRay = GetAimRay();
		Vector3 val = base.characterBody.corePosition + ((Ray)(ref aimRay)).direction * slashEffectOffset;
		Util.PlaySound(soundString, base.gameObject);
		EffectData effectData = new EffectData
		{
			origin = val,
			rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction)
		};
		EffectManager.SpawnEffect(slashEffectPrefab, effectData, transmit: true);
		if (NetworkServer.active)
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.baseDamage = blastDamageCoefficient * damageStat;
			blastAttack.baseForce = blastForce;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.crit = RollCrit();
			blastAttack.damageType = DamageType.Generic;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.inflictor = base.gameObject;
			blastAttack.position = val;
			blastAttack.procChainMask = default(ProcChainMask);
			blastAttack.procCoefficient = blastProcCoefficient;
			blastAttack.radius = blastRadius;
			blastAttack.teamIndex = base.teamComponent.teamIndex;
			blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab);
			blastAttack.Fire();
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity.y = Mathf.Max(base.characterMotor.velocity.y, shortHopVelocity);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
