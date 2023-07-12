using RoR2;

namespace EntityStates.BrotherMonster;

public class SkySpawnState : BaseState
{
	public static float duration;

	public static string soundString;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "ExitSkyLeap", "SkyLeap.playbackRate", duration);
		PlayAnimation("FullBody Override", "BufferEmpty");
		Util.PlaySound(soundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
