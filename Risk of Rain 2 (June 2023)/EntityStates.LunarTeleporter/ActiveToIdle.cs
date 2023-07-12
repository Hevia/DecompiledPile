using RoR2;

namespace EntityStates.LunarTeleporter;

public class ActiveToIdle : LunarTeleporterBaseState
{
	public static float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "ActiveToIdle", "playbackRate", duration);
		preferredInteractability = Interactability.Disabled;
		Util.PlaySound("Play_boss_spawn_rumble", base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new Idle());
		}
	}

	public override void OnExit()
	{
		Chat.SendBroadcastChat(new Chat.SimpleChatMessage
		{
			baseToken = "LUNAR_TELEPORTER_IDLE"
		});
		base.OnExit();
	}
}
