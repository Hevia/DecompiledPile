using EntityStates.Engi.EngiBubbleShield;
using UnityEngine;

namespace EntityStates.Engi.EngiWallShield;

public class Undeployed : EntityStates.Engi.EngiBubbleShield.Undeployed
{
	public override void OnEnter()
	{
		base.OnEnter();
	}

	protected override void SetNextState()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = base.transform.forward;
		Vector3 forward2 = default(Vector3);
		((Vector3)(ref forward2))._002Ector(forward.x, 0f, forward.z);
		base.transform.forward = forward2;
		outer.SetNextState(new Deployed());
	}
}
