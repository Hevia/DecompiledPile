using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates;

public class BumpityBoom2 : BaseSkillState
{
	public float damageCoefficient = 2.5f;

	public float baseDuration = 0.5f;

	public float recoil = 1f;

	public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

	private float duration;

	private float fireDuration;

	private bool hasFired;

	private Animator animator;

	private string muzzleString;

	private string muzzleString2;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration;
		fireDuration = 0.25f * duration;
		((EntityState)this).characterBody.SetAimTimer(2f);
		animator = ((EntityState)this).GetModelAnimator();
		muzzleString = "HandL";
		Util.PlaySound(Sounds.vileFragDrop, ((EntityState)this).gameObject);
		((EntityState)this).PlayAnimation("LeftArm, Override", "GranadeL", "attackSpeed", duration);
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	private void FireES()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!hasFired)
		{
			hasFired = true;
			((EntityState)this).characterBody.AddSpreadBloom(0.15f);
			Ray aimRay = ((BaseState)this).GetAimRay();
			EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, ((EntityState)this).gameObject, muzzleString, false);
			if (((EntityState)this).isAuthority)
			{
				ProjectileManager.instance.FireProjectile(Projectiles.BumpityBombProjectile, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), ((EntityState)this).gameObject, damageCoefficient * ((BaseState)this).damageStat, 0f, Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
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
