using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class WeaponSlam : BaseState
{
	public static float duration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float upwardForce;

	public static float radius = 3f;

	public static string attackSoundString;

	public static string muzzleString;

	public static GameObject slamImpactEffect;

	public static float durationBeforePriorityReduces;

	public static GameObject waveProjectilePrefab;

	public static float waveProjectileArc;

	public static int waveProjectileCount;

	public static float waveProjectileDamageCoefficient;

	public static float waveProjectileForce;

	public static float weaponDamageCoefficient;

	public static float weaponForce;

	public static GameObject pillarProjectilePrefab;

	public static float pillarDamageCoefficient;

	public static GameObject weaponHitEffectPrefab;

	public static NetworkSoundEventDef weaponImpactSound;

	private BlastAttack blastAttack;

	private OverlapAttack weaponAttack;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasDoneBlastAttack;

	public override void OnEnter()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("FullBody Override", "WeaponSlam", "WeaponSlam.playbackRate", duration, 0.1f);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = aimRay.direction;
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
		if (base.isAuthority)
		{
			weaponAttack = new OverlapAttack
			{
				attacker = base.gameObject,
				damage = damageCoefficient * damageStat,
				damageColorIndex = DamageColorIndex.Default,
				damageType = DamageType.Generic,
				hitEffectPrefab = weaponHitEffectPrefab,
				hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "WeaponBig"),
				impactSound = weaponImpactSound.index,
				inflictor = base.gameObject,
				procChainMask = default(ProcChainMask),
				pushAwayForce = weaponForce,
				procCoefficient = 1f,
				teamIndex = GetTeam()
			};
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.skillLocator) && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed)
		{
			base.skillLocator.utility.ExecuteIfReady();
			return;
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			if (base.isAuthority && modelAnimator.GetFloat("weapon.hitBoxActive") > 0.5f)
			{
				weaponAttack.Fire();
			}
			if (modelAnimator.GetFloat("blast.hitBoxActive") > 0.5f && !hasDoneBlastAttack)
			{
				hasDoneBlastAttack = true;
				EffectManager.SimpleMuzzleFlash(slamImpactEffect, base.gameObject, muzzleString, transmit: false);
				if (base.isAuthority)
				{
					if (Object.op_Implicit((Object)(object)base.characterDirection))
					{
						base.characterDirection.moveVector = base.characterDirection.forward;
					}
					if (Object.op_Implicit((Object)(object)modelTransform))
					{
						Transform val = FindModelChild(muzzleString);
						if (Object.op_Implicit((Object)(object)val))
						{
							blastAttack = new BlastAttack();
							blastAttack.attacker = base.gameObject;
							blastAttack.inflictor = base.gameObject;
							blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
							blastAttack.baseDamage = damageStat * damageCoefficient;
							blastAttack.baseForce = forceMagnitude;
							blastAttack.position = val.position;
							blastAttack.radius = radius;
							blastAttack.bonusForce = new Vector3(0f, upwardForce, 0f);
							blastAttack.Fire();
						}
					}
					if (Object.op_Implicit((Object)(object)PhaseCounter.instance) && PhaseCounter.instance.phase == 3)
					{
						Transform val2 = FindModelChild(muzzleString);
						float num = waveProjectileArc / (float)waveProjectileCount;
						Vector3 val3 = Vector3.ProjectOnPlane(base.characterDirection.forward, Vector3.up);
						Vector3 position = base.characterBody.footPosition;
						if (Object.op_Implicit((Object)(object)val2))
						{
							position = val2.position;
						}
						for (int i = 0; i < waveProjectileCount; i++)
						{
							Vector3 forward = Quaternion.AngleAxis(num * ((float)i - (float)waveProjectileCount / 2f), Vector3.up) * val3;
							ProjectileManager.instance.FireProjectile(waveProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.characterBody.damage * waveProjectileDamageCoefficient, waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
						}
						ProjectileManager.instance.FireProjectile(pillarProjectilePrefab, position, Quaternion.identity, base.gameObject, base.characterBody.damage * pillarDamageCoefficient, 0f, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
					}
				}
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge > durationBeforePriorityReduces))
		{
			return InterruptPriority.PrioritySkill;
		}
		return InterruptPriority.Skill;
	}
}
