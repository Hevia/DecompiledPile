using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class PositionAlongBasicBezierSpline : MonoBehaviour
{
	public BasicBezierSpline curve;

	[Range(0f, 1f)]
	public float normalizedPositionAlongCurve;

	private void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)curve))
		{
			((Component)this).transform.position = curve.Evaluate(normalizedPositionAlongCurve);
		}
	}
}
