namespace EntityStates.Squid;

public class SquidDeathState : GenericCharacterDeath
{
	public static float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Death");
	}
}
