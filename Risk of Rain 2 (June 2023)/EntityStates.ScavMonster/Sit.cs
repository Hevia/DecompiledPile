using UnityEngine;

namespace EntityStates.ScavMonster;

public class Sit : BaseSitState
{
	public static string soundString;

	public static float minimumDuration;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayCrossfade("Body", "SitLoop", 0.1f);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (((Vector3)(ref base.inputBank.moveVector)).sqrMagnitude >= Mathf.Epsilon && base.fixedAge >= minimumDuration)
		{
			outer.SetNextState(new ExitSit());
		}
	}
}
