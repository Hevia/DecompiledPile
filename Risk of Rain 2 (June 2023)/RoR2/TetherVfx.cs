using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(BezierCurveLine))]
public class TetherVfx : MonoBehaviour
{
	public AnimateShaderAlpha fadeOut;

	[Tooltip("The transform to position at the target.")]
	public Transform tetherEndTransform;

	[Tooltip("The transform to position the end to.")]
	public Transform tetherTargetTransform;

	public void Update()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)tetherTargetTransform) && Object.op_Implicit((Object)(object)tetherEndTransform))
		{
			tetherEndTransform.position = tetherTargetTransform.position;
		}
	}

	public void Terminate()
	{
		if (Object.op_Implicit((Object)(object)fadeOut))
		{
			((Behaviour)fadeOut).enabled = true;
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
