using RoR2;
using UnityEngine;

namespace EntityStates.Headstompers;

public class HeadstompersCooldown : BaseHeadstompersState
{
	public static float baseDuration = 10f;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration;
		if (Object.op_Implicit((Object)(object)body))
		{
			Inventory inventory = body.inventory;
			int num = ((!Object.op_Implicit((Object)(object)inventory)) ? 1 : inventory.GetItemCount(RoR2Content.Items.FallBoots));
			if (num > 0)
			{
				duration /= num;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			FixedUpdateAuthority();
		}
	}

	private void FixedUpdateAuthority()
	{
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new HeadstompersIdle());
		}
	}
}
