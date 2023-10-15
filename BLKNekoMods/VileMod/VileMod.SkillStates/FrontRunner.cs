using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates;

public class FrontRunner : BaseSkillState
{
	public float damageCoefficient = 3f;

	public float baseDuration = 0.4f;

	public float recoil = 1f;

	public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

	private float duration;

	private float fireDuration;

	private bool hasFired;

	private Animator animator;

	private string muzzleString;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		fireDuration = 0.25f * duration;
		((EntityState)this).characterBody.SetAimTimer(2f);
		animator = ((EntityState)this).GetModelAnimator();
		muzzleString = "Weapon";
		Util.PlaySound(Sounds.vileAttack, ((EntityState)this).gameObject);
		((EntityState)this).PlayAnimation("Gesture, Override", "CannonShoot", "attackSpeed", duration);
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	private void FireES()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!hasFired)
		{
			hasFired = true;
			((EntityState)this).characterBody.SetSpreadBloom(0.8f, true);
			Ray aimRay = ((BaseState)this).GetAimRay();
			EffectManager.SimpleMuzzleFlash(FireRoller.fireMuzzleflashEffectPrefab, ((EntityState)this).gameObject, muzzleString, false);
			if (((EntityState)this).isAuthority)
			{
				ProjectileManager.instance.FireProjectile(Projectiles.FrontRunnerFireBallProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), ((EntityState)this).gameObject, damageCoefficient * ((BaseState)this).damageStat, 0f, Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
			}
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).fixedAge >= fireDuration)
		{
			FireES();
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority)
		{
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
