using EntityStates.Treebot.Weapon;
using RoR2;

namespace EntityStates.ClayBruiser.Weapon;

public class FireSonicBoom : EntityStates.Treebot.Weapon.FireSonicBoom
{
	public override void OnEnter()
	{
		base.OnEnter();
		GetModelAnimator().SetBool("WeaponIsReady", true);
	}

	protected override void AddDebuff(CharacterBody body)
	{
		body.AddTimedBuff(RoR2Content.Buffs.ClayGoo, slowDuration);
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			GetModelAnimator().SetBool("WeaponIsReady", false);
		}
		base.OnExit();
	}
}
