using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class NailgunFinalBurst : BaseNailgunState
{
	public static int finalBurstBulletCount = 8;

	public static float burstTimeCostCoefficient = 1.2f;

	public static string burstSound;

	public static float selfForce = 1000f;

	protected override float GetBaseDuration()
	{
		return (float)finalBurstBulletCount * FireNailgun.baseRefireInterval * burstTimeCostCoefficient;
	}

	public override void OnEnter()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetSpreadBloom(1f, canOnlyIncreaseBloom: false);
		}
		Ray aimRay = GetAimRay();
		FireBullet(GetAimRay(), finalBurstBulletCount, BaseNailgunState.spreadPitchScale, BaseNailgunState.spreadYawScale);
		if (!base.isInDualWield)
		{
			PlayAnimation("Gesture, Additive", "FireGrenadeLauncher", "FireGrenadeLauncher.playbackRate", 0.45f / attackSpeedStat);
		}
		else
		{
			BaseToolbotPrimarySkillStateMethods.PlayGenericFireAnim(this, base.gameObject, base.skillLocator, 0.45f / attackSpeedStat);
		}
		Util.PlaySound(burstSound, base.gameObject);
		if (base.isAuthority)
		{
			float num = selfForce * (base.characterMotor.isGrounded ? 0.5f : 1f) * base.characterMotor.mass;
			base.characterMotor.ApplyForce(((Ray)(ref aimRay)).direction * (0f - num));
		}
		Util.PlaySound(BaseNailgunState.fireSoundString, base.gameObject);
		Util.PlaySound(BaseNailgunState.fireSoundString, base.gameObject);
		Util.PlaySound(BaseNailgunState.fireSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
