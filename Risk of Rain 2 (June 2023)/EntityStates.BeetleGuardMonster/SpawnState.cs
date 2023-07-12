using RoR2;

namespace EntityStates.BeetleGuardMonster;

public class SpawnState : BaseState
{
	public static float duration = 4f;

	public static string spawnSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn1", "Spawn1.playbackRate", duration);
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
		return InterruptPriority.Death;
	}
}
