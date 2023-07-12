using UnityEngine;

namespace EntityStates.Mage;

public class JetpackOn : BaseState
{
	public static float hoverVelocity;

	public static float hoverAcceleration;

	private Transform jetOnEffect;

	public override void OnEnter()
	{
		base.OnEnter();
		jetOnEffect = FindModelChild("JetOn");
		if (Object.op_Implicit((Object)(object)jetOnEffect))
		{
			((Component)jetOnEffect).gameObject.SetActive(true);
		}
	}

	public override void FixedUpdate()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			float y = base.characterMotor.velocity.y;
			y = Mathf.MoveTowards(y, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
			base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, y, base.characterMotor.velocity.z);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)jetOnEffect))
		{
			((Component)jetOnEffect).gameObject.SetActive(false);
		}
	}
}
