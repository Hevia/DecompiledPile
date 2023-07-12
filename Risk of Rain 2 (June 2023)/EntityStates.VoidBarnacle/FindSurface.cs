using System;
using RoR2;
using UnityEngine;

namespace EntityStates.VoidBarnacle;

public class FindSurface : NoCastSpawn
{
	public static int raycastCount;

	public static float maxRaycastLength;

	public static float raycastSphereYOffset;

	public static float raycastMinimumAngle;

	public static float raycastMaximumAngle;

	private const float _cRadianConversionCoeficcient = MathF.PI / 180f;

	public override void OnEnter()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		RaycastHit val = default(RaycastHit);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(base.characterBody.corePosition.x, base.characterBody.corePosition.y + raycastSphereYOffset, base.characterBody.corePosition.z);
		if (!base.isAuthority)
		{
			return;
		}
		raycastMinimumAngle = Mathf.Clamp(raycastMinimumAngle, 0f, raycastMaximumAngle);
		raycastMaximumAngle = Mathf.Clamp(raycastMaximumAngle, raycastMinimumAngle, 90f);
		raycastCount = Mathf.Abs(raycastCount);
		float num = 360f / (float)raycastCount;
		Vector3 val3 = default(Vector3);
		for (int i = 0; i < raycastCount; i++)
		{
			float num2 = num * (float)i;
			float num3 = num * (float)(i + 1) - 1f;
			float num4 = Random.Range(num2, num3) * (MathF.PI / 180f);
			float num5 = Random.Range(raycastMinimumAngle, raycastMaximumAngle) * (MathF.PI / 180f);
			float num6 = Mathf.Cos(num4);
			float num7 = Mathf.Sin(num5);
			float num8 = Mathf.Sin(num4);
			((Vector3)(ref val3))._002Ector(num6, num7, num8);
			if (Physics.Raycast(val2, val3, ref val, maxRaycastLength, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				base.transform.position = ((RaycastHit)(ref val)).point;
				base.transform.up = ((RaycastHit)(ref val)).normal;
				break;
			}
		}
	}
}
