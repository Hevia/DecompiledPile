using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates;

public class CerberusPhantom : BaseSkillState
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

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		duration = baseDuration;
		fireDuration = 0.25f * duration;
		((EntityState)this).characterBody.SetAimTimer(2f);
		animator = ((EntityState)this).GetModelAnimator();
		muzzleString = "Weapon";
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
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (!hasFired)
		{
			hasFired = true;
			((EntityState)this).characterBody.SetSpreadBloom(0.8f, true);
			Ray aimRay = ((BaseState)this).GetAimRay();
			EffectManager.SimpleMuzzleFlash(FireShotgun.effectPrefab, ((EntityState)this).gameObject, muzzleString, false);
			float num = ((Ray)(ref aimRay)).direction.x - ((Ray)(ref aimRay)).direction.x * 3f;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(((Ray)(ref aimRay)).direction.x + 0.2f, ((Ray)(ref aimRay)).direction.y, ((Ray)(ref aimRay)).direction.z);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(((Ray)(ref aimRay)).direction.x - 0.2f, ((Ray)(ref aimRay)).direction.y, ((Ray)(ref aimRay)).direction.z);
			if (((EntityState)this).isAuthority)
			{
				Util.PlaySound(Sounds.vileCerberusPhantom, ((EntityState)this).gameObject);
				ProjectileManager.instance.FireProjectile(Projectiles.CerberusPhantonFMJProjectile, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Vector3)(ref val)).normalized), ((EntityState)this).gameObject, damageCoefficient * ((BaseState)this).damageStat, 0f, Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
				ProjectileManager.instance.FireProjectile(Projectiles.CerberusPhantonFMJProjectile, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Vector3)(ref val2)).normalized), ((EntityState)this).gameObject, damageCoefficient * ((BaseState)this).damageStat, 0f, Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
				ProjectileManager.instance.FireProjectile(Projectiles.CerberusPhantonFMJProjectile, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), ((EntityState)this).gameObject, damageCoefficient * ((BaseState)this).damageStat, 0f, Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
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
