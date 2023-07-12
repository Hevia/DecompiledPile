namespace EntityStates.LunarExploderMonster.Weapon;

public class FireExploderShards : GenericProjectileBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "FireExploderShards");
		base.characterBody.SetAimTimer(0f);
	}
}
