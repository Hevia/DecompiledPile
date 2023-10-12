using EntityStates;
using EntityStates.VoidSurvivor.Weapon;
using RoR2.Skills;

namespace IndiesSkills.MyEntityStates;

public class VoidmandoMeleeAttack : BaseSkillState, IStepSetter
{
	public int step;

	void IStepSetter.SetStep(int i)
	{
		step = i;
	}

	public override void OnEnter()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		((BaseState)this).OnEnter();
		if (((EntityState)this).isAuthority)
		{
			switch (step % 3)
			{
			case 0:
				((EntityState)this).outer.SetNextState((EntityState)new SwingMelee1());
				break;
			case 1:
				((EntityState)this).outer.SetNextState((EntityState)new SwingMelee2());
				break;
			case 2:
				((EntityState)this).outer.SetNextState((EntityState)new SwingMelee3());
				break;
			default:
				((EntityState)this).outer.SetNextState((EntityState)new SwingMelee1());
				break;
			}
		}
	}
}
