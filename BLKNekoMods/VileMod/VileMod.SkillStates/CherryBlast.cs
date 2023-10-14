using EntityStates;
using RoR2;
using UnityEngine;
using VileMod.Modules;
using VileMod.SkillStates.BaseStates;

namespace VileMod.SkillStates;

public class CherryBlast : BaseSkillState
{
	public float damageCoefficient = 0.25f;

	public float baseDuration = 1f;

	public float recoil = 1f;

	public static float procCoefficient = 1f;

	public static float force = 100f;

	public static float range = 200f;

	public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerClayBruiserMinigun");

	public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark1");

	public int bulletcount;

	public bool shootsfx = true;

	public static bool heat;

	public static int buffSkillIndex;

	public static float Chilldelay;

	public float shootdelay = 1.5f;

	public float timer = 2f;

	private float duration;

	private float fireDuration;

	private bool hasFired = true;

	private Animator animator;

	private string muzzleString;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		if (bulletcount == 0)
		{
			bulletcount = 1;
		}
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		fireDuration = 0.25f * duration;
		((EntityState)this).characterBody.SetAimTimer(2f);
		animator = ((EntityState)this).GetModelAnimator();
		muzzleString = "Weapon";
		((EntityState)this).characterBody.isSprinting = false;
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

	private void FireArrow()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		if (hasFired)
		{
			return;
		}
		hasFired = true;
		((EntityState)this).characterBody.SetSpreadBloom(0.8f, true);
		Ray aimRay = ((BaseState)this).GetAimRay();
		if (((EntityState)this).isAuthority)
		{
			BulletAttack val = new BulletAttack();
			val.owner = ((EntityState)this).gameObject;
			val.origin = ((Ray)(ref aimRay)).origin;
			val.aimVector = ((Ray)(ref aimRay)).direction;
			val.minSpread = 0.1f;
			val.maxSpread = 1f;
			val.damage = damageCoefficient * ((BaseState)this).damageStat;
			val.spreadPitchScale = 0.1f;
			val.spreadYawScale = 1f;
			val.tracerEffectPrefab = tracerEffectPrefab;
			val.muzzleName = muzzleString;
			val.hitEffectPrefab = hitEffectPrefab;
			val.isCrit = Fury.isCrit;
			val.bulletCount = 1u;
			val.damageColorIndex = (DamageColorIndex)0;
			val.falloffModel = (FalloffModel)1;
			val.maxDistance = range;
			val.force = force;
			val.hitMask = CommonMasks.bullet;
			val.smartCollision = false;
			val.procChainMask = default(ProcChainMask);
			val.procCoefficient = procCoefficient;
			val.radius = 0.75f;
			val.sniper = false;
			val.stopperMask = CommonMasks.bullet;
			val.weapon = null;
			val.queryTriggerInteraction = (QueryTriggerInteraction)0;
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
			if (shootsfx)
			{
				Util.PlaySound(Sounds.vileCherryBlast, ((EntityState)this).gameObject);
			}
			val.Fire();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		timer += Time.deltaTime;
		if (((EntityState)this).inputBank.skill1.down && hasFired)
		{
			if (((EntityState)this).characterBody.HasBuff(Buffs.VileFuryBuff))
			{
				if (timer > shootdelay)
				{
					shootdelay = 0.045f;
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
					FireArrow();
				}
			}
			else if (timer > shootdelay)
			{
				if (shootdelay <= 0.075f)
				{
					shootdelay = 0.075f;
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
				FireArrow();
			}
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority && !((EntityState)this).inputBank.skill1.down)
		{
			shootdelay = 1.5f;
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
	}
}
