using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FirePistol2 : BaseSkillState, SteppedSkillDef.IStepSetter
{
	public static GameObject muzzleEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float damageCoefficient;

	public static float force;

	public static float baseDuration = 2f;

	public static string firePistolSoundString;

	public static float recoilAmplitude = 1f;

	public static float spreadBloomValue = 0.3f;

	public static float commandoBoostBuffCoefficient = 0.4f;

	private int pistol;

	private Ray aimRay;

	private float duration;

	void SteppedSkillDef.IStepSetter.SetStep(int i)
	{
		pistol = i;
	}

	private void FireBullet(string targetMuzzle)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(firePistolSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.radius = 0.1f;
			bulletAttack.smartCollision = true;
			bulletAttack.Fire();
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnEnter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		aimRay = GetAimRay();
		StartAimMode(aimRay, 3f);
		if (pistol % 2 == 0)
		{
			PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
			FireBullet("MuzzleLeft");
		}
		else
		{
			PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
			FireBullet("MuzzleRight");
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			if (base.activatorSkillSlot.stock <= 0)
			{
				outer.SetNextState(new ReloadPistols());
			}
			else
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
