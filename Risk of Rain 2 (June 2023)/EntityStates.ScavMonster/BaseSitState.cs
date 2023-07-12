using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ScavMonster;

public class BaseSitState : BaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
		}
		base.OnExit();
	}
}
