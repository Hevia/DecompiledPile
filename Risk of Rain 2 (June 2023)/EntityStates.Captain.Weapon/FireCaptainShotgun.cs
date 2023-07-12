using RoR2;

namespace EntityStates.Captain.Weapon;

public class FireCaptainShotgun : GenericBulletBaseState
{
	public static float tightSoundSwitchThreshold;

	public static string wideSoundString;

	public static string tightSoundString;

	public override void OnEnter()
	{
		fireSoundString = ((base.characterBody.spreadBloomAngle <= tightSoundSwitchThreshold) ? tightSoundString : wideSoundString);
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "FireCaptainShotgun");
		PlayAnimation("Gesture, Override", "FireCaptainShotgun");
	}

	protected override void ModifyBullet(BulletAttack bulletAttack)
	{
		base.ModifyBullet(bulletAttack);
		bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
