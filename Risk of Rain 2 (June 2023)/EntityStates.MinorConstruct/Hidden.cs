using RoR2;
using UnityEngine;

namespace EntityStates.MinorConstruct;

public class Hidden : BaseHideState
{
	[SerializeField]
	public BuffDef buffDef;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)buffDef))
		{
			base.characterBody.AddBuff(buffDef);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.characterBody.outOfCombat || !base.characterBody.outOfDanger)
		{
			outer.SetNextState(new Revealed());
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)buffDef))
		{
			base.characterBody.RemoveBuff(buffDef);
		}
		base.OnExit();
	}
}
