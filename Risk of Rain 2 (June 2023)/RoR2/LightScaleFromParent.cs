using UnityEngine;

namespace RoR2;

public class LightScaleFromParent : MonoBehaviour
{
	private void Start()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Light component = ((Component)this).GetComponent<Light>();
		if (Object.op_Implicit((Object)(object)component))
		{
			float range = component.range;
			Vector3 lossyScale = ((Component)this).transform.lossyScale;
			component.range = range * Mathf.Max(new float[3] { lossyScale.x, lossyScale.y, lossyScale.z });
		}
	}
}
