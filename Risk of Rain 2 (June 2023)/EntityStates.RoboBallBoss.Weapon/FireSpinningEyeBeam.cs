using EntityStates.RoboBallMini.Weapon;
using UnityEngine;

namespace EntityStates.RoboBallBoss.Weapon;

public class FireSpinningEyeBeam : FireEyeBeam
{
	private Transform eyeBeamOriginTransform;

	public override void OnEnter()
	{
		string customName = outer.customName;
		eyeBeamOriginTransform = FindModelChild(customName);
		muzzleString = customName;
		base.OnEnter();
	}

	public override Ray GetLaserRay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Ray result = default(Ray);
		if (Object.op_Implicit((Object)(object)eyeBeamOriginTransform))
		{
			((Ray)(ref result)).origin = eyeBeamOriginTransform.position;
			((Ray)(ref result)).direction = eyeBeamOriginTransform.forward;
		}
		return result;
	}

	public override bool ShouldFireLaser()
	{
		return true;
	}
}
