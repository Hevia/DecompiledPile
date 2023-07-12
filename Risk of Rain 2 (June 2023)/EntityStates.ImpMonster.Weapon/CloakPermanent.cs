using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpMonster.Weapon;

public class CloakPermanent : BaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.Cloak);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
