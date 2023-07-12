using RoR2;
using UnityEngine;

namespace EntityStates.Huntress.Weapon;

public class FireArrowSnipe : GenericBulletBaseState
{
	public float charge;

	public static float recoilAmplitude;

	protected override void ModifyBullet(BulletAttack bulletAttack)
	{
		base.ModifyBullet(bulletAttack);
		bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		Object.op_Implicit((Object)(object)(Object.op_Implicit((Object)(object)base.skillLocator) ? base.skillLocator.primary : null));
	}

	protected override void FireBullet(Ray aimRay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.FireBullet(aimRay);
		base.characterBody.SetSpreadBloom(0.2f, canOnlyIncreaseBloom: false);
		AddRecoil(-0.6f * recoilAmplitude, -0.8f * recoilAmplitude, -0.1f * recoilAmplitude, 0.1f * recoilAmplitude);
		PlayAnimation("Body", "FireArrowSnipe", "FireArrowSnipe.playbackRate", duration);
		base.healthComponent.TakeDamageForce(((Ray)(ref aimRay)).direction * -400f, alwaysApply: true);
	}
}
