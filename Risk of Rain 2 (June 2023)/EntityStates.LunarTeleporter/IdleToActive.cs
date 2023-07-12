using RoR2;

namespace EntityStates.LunarTeleporter;

public class IdleToActive : LunarTeleporterBaseState
{
	public static float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "IdleToActive", "playbackRate", duration);
		preferredInteractability = Interactability.Disabled;
		Util.PlaySound("Play_boss_spawn_rumble", base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new Active());
		}
	}

	public override void OnExit()
	{
		Chat.SendBroadcastChat(new Chat.SimpleChatMessage
		{
			baseToken = "LUNAR_TELEPORTER_ACTIVE"
		});
		base.OnExit();
	}
}
