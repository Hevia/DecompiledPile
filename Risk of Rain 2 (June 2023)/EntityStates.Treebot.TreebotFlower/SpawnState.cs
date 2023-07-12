using RoR2;

namespace EntityStates.Treebot.TreebotFlower;

public class SpawnState : BaseState
{
	public static float duration;

	public static string enterSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Base", "Spawn", "Spawn.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new TreebotFlower2Projectile());
		}
	}
}
