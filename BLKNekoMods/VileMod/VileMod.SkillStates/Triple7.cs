using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates;

public class Triple7 : BaseSkillState
{
	public static float damageCoefficient = 0.5f;

	public static float procCoefficient = 1f;

	public static float baseDuration = 0.6f;

	public static float force = 100f;

	public static float recoil = 0f;

	public static float range = 180f;

	public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoShotgun");

	private float duration;

	private float fireTime;

	private bool hasFired;

	private string muzzleString;

	public float shootdelay = 1.5f;

	public float timer = 2f;

	public bool shootsfx = true;

	public static bool heat;

	public static int buffSkillIndex;

	public static float Chilldelay;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		fireTime = 0.2f * duration;
		((EntityState)this).characterBody.SetAimTimer(2f);
		muzzleString = "Weapon";
		((EntityState)this).PlayAnimation("Gesture, Override", "CannonShoot", "attackSpeed", duration);
		if (heat)
		{
			shootdelay = Chilldelay - 0.3f;
		}
		else
		{
			shootdelay = Chilldelay;
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	private void Fire()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		if (hasFired)
		{
			return;
		}
		hasFired = true;
		((EntityState)this).characterBody.AddSpreadBloom(1.5f);
		EffectManager.SimpleMuzzleFlash(FireShotgun.effectPrefab, ((EntityState)this).gameObject, muzzleString, false);
		if (shootsfx)
		{
			Util.PlaySound(Sounds.vileCherryBlast, ((EntityState)this).gameObject);
		}
		if (((EntityState)this).isAuthority)
		{
			Ray aimRay = ((BaseState)this).GetAimRay();
			BulletAttack val = new BulletAttack();
			val.bulletCount = 1u;
			val.aimVector = aimRay.direction;
			val.origin = aimRay.origin;
			val.damage = damageCoefficient * ((BaseState)this).damageStat;
			val.damageColorIndex = (DamageColorIndex)0;
			val.falloffModel = (FalloffModel)1;
			val.maxDistance = range;
			val.force = force;
			val.hitMask = CommonMasks.bullet;
			val.minSpread = 0.1f;
			val.maxSpread = 2.5f;
			val.isCrit = ((BaseState)this).RollCrit();
			val.owner = ((EntityState)this).gameObject;
			val.muzzleName = muzzleString;
			val.smartCollision = false;
			val.procChainMask = default(ProcChainMask);
			val.procCoefficient = procCoefficient;
			val.radius = 0.75f;
			val.sniper = false;
			val.stopperMask = CommonMasks.bullet;
			val.weapon = null;
			val.tracerEffectPrefab = tracerEffectPrefab;
			val.spreadPitchScale = 1.5f;
			val.spreadYawScale = 2f;
			val.queryTriggerInteraction = (QueryTriggerInteraction)0;
			val.hitEffectPrefab = FireShotgun.hitEffectPrefab;
			switch (buffSkillIndex)
			{
			case 0:
				val.damageType = (DamageType)(Util.CheckRoll(5f, ((EntityState)this).characterBody.master) ? 8 : 0);
				break;
			case 1:
				val.damageType = (DamageType)(Util.CheckRoll(8f, ((EntityState)this).characterBody.master) ? 32 : 0);
				break;
			case 2:
				val.damageType = (DamageType)(Util.CheckRoll(8f, ((EntityState)this).characterBody.master) ? 16777216 : 0);
				break;
			case 3:
				val.damageType = (DamageType)(Util.CheckRoll(8f, ((EntityState)this).characterBody.master) ? 128 : 0);
				break;
			default:
				val.damageType = (DamageType)(Util.CheckRoll(5f, ((EntityState)this).characterBody.master) ? 8 : 0);
				break;
			}
			val.Fire();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		timer += Time.deltaTime;
		if (((EntityState)this).inputBank.skill1.down)
		{
			if (((EntityState)this).characterBody.HasBuff(Buffs.VileFuryBuff))
			{
				if (timer > shootdelay)
				{
					shootdelay = 0.05f;
					if (shootsfx)
					{
						shootsfx = false;
					}
					else
					{
						shootsfx = true;
					}
					timer = 0f;
					hasFired = false;
					((EntityState)this).characterBody.SetAimTimer(1f);
					((EntityState)this).PlayAnimation("Gesture, Override", "CannonShoot", "attackSpeed", duration);
					((EntityState)this).characterBody.isSprinting = false;
					Fire();
				}
			}
			else if (timer > shootdelay)
			{
				if (shootdelay <= 0.085f)
				{
					shootdelay = 0.085f;
					if (shootsfx)
					{
						shootsfx = false;
					}
					else
					{
						shootsfx = true;
					}
				}
				else if ((double)shootdelay <= 1.6 && (double)shootdelay >= 1.2)
				{
					shootdelay -= 0.25f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else if ((double)shootdelay <= 1.19 && shootdelay >= 1f)
				{
					shootdelay -= 0.18f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else if ((double)shootdelay <= 0.99 && (double)shootdelay >= 0.8)
				{
					shootdelay -= 0.085f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else if ((double)shootdelay <= 0.79 && (double)shootdelay >= 0.5)
				{
					shootdelay -= 0.045f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else if ((double)shootdelay <= 0.49 && (double)shootdelay >= 0.2)
				{
					shootdelay -= 0.015f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else if ((double)shootdelay <= 0.19)
				{
					shootdelay -= 0.008f + ((BaseState)this).attackSpeedStat / 50f;
				}
				else
				{
					shootdelay -= 0.01f + ((BaseState)this).attackSpeedStat / 50f;
				}
				timer = 0f;
				hasFired = false;
				((EntityState)this).characterBody.SetAimTimer(1f);
				((EntityState)this).PlayAnimation("Gesture, Override", "CannonShoot", "attackSpeed", duration);
				((EntityState)this).characterBody.isSprinting = false;
				Fire();
			}
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority && !((EntityState)this).inputBank.skill1.down)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
