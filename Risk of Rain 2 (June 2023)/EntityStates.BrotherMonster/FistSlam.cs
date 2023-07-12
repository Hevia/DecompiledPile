using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class FistSlam : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float upwardForce;

	public static float radius = 3f;

	public static string attackSoundString;

	public static string muzzleString;

	public static float healthCostFraction;

	public static GameObject chargeEffectPrefab;

	public static GameObject slamImpactEffect;

	public static GameObject waveProjectilePrefab;

	public static int waveProjectileCount;

	public static float waveProjectileDamageCoefficient;

	public static float waveProjectileForce;

	private BlastAttack attack;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private float duration;

	private GameObject chargeInstance;

	public override void OnEnter()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("FullBody Override", "FistSlam", "FistSlam.playbackRate", duration, 0.1f);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
		Transform val = FindModelChild("MuzzleRight");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeInstance.transform.parent = val;
			ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = duration / 2.8f;
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
		PlayAnimation("FullBody Override", "BufferEmpty");
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f && !hasAttacked)
		{
			if (Object.op_Implicit((Object)(object)chargeInstance))
			{
				EntityState.Destroy((Object)(object)chargeInstance);
			}
			EffectManager.SimpleMuzzleFlash(slamImpactEffect, base.gameObject, muzzleString, transmit: false);
			if (NetworkServer.active && Object.op_Implicit((Object)(object)base.healthComponent))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
				damageInfo.position = base.characterBody.corePosition;
				damageInfo.force = Vector3.zero;
				damageInfo.damageColorIndex = DamageColorIndex.Default;
				damageInfo.crit = false;
				damageInfo.attacker = null;
				damageInfo.inflictor = null;
				damageInfo.damageType = DamageType.NonLethal | DamageType.BypassArmor;
				damageInfo.procCoefficient = 0f;
				damageInfo.procChainMask = default(ProcChainMask);
				base.healthComponent.TakeDamage(damageInfo);
			}
			if (base.isAuthority)
			{
				if (Object.op_Implicit((Object)(object)modelTransform))
				{
					Transform val = FindModelChild(muzzleString);
					if (Object.op_Implicit((Object)(object)val))
					{
						attack = new BlastAttack();
						attack.attacker = base.gameObject;
						attack.inflictor = base.gameObject;
						attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
						attack.baseDamage = damageStat * damageCoefficient;
						attack.baseForce = forceMagnitude;
						attack.position = val.position;
						attack.radius = radius;
						attack.bonusForce = new Vector3(0f, upwardForce, 0f);
						attack.Fire();
					}
				}
				float num = 360f / (float)waveProjectileCount;
				Vector3 val2 = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
				Vector3 footPosition = base.characterBody.footPosition;
				for (int i = 0; i < waveProjectileCount; i++)
				{
					Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * val2;
					ProjectileManager.instance.FireProjectile(waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.characterBody.damage * waveProjectileDamageCoefficient, waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
				}
			}
			hasAttacked = true;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
