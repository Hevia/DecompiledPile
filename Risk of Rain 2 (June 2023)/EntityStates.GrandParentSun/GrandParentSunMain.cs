using RoR2;
using UnityEngine;

namespace EntityStates.GrandParentSun;

public class GrandParentSunMain : GrandParentSunBase
{
	private GenericOwnership ownership;

	protected override bool shouldEnableSunController => true;

	protected override float desiredVfxScale => 1f;

	public override void OnEnter()
	{
		base.OnEnter();
		ownership = GetComponent<GenericOwnership>();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && !Object.op_Implicit((Object)(object)ownership.ownerObject))
		{
			outer.SetNextState(new GrandParentSunDeath());
		}
	}
}
