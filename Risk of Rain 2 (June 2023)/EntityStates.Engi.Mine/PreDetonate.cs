using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.Mine;

public class PreDetonate : BaseMineState
{
	public static float duration;

	public static string pathToPrepForExplosionChildEffect;

	public static float detachForce;

	protected override bool shouldStick => false;

	protected override bool shouldRevertToWaitForStickOnSurfaceLost => false;

	public override void OnEnter()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		((Component)base.transform.Find(pathToPrepForExplosionChildEffect)).gameObject.SetActive(true);
		base.rigidbody.AddForce(base.transform.forward * detachForce);
		base.rigidbody.AddTorque(Random.onUnitSphere * 200f);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && duration <= base.fixedAge)
		{
			outer.SetNextState(new Detonate());
		}
	}
}
