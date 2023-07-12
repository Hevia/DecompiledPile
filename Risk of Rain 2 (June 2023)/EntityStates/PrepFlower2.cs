using RoR2;

namespace EntityStates;

public class PrepFlower2 : BaseState
{
	public static float baseDuration;

	public static string enterSoundString;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Gesture, Additive", "PrepFlower", "PrepFlower.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new FireFlower2());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
