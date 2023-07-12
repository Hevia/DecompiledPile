using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2;

public class DetachTrailOnDestroy : MonoBehaviour
{
	[FormerlySerializedAs("trail")]
	public TrailRenderer[] targetTrailRenderers;

	private void OnDestroy()
	{
		for (int i = 0; i < targetTrailRenderers.Length; i++)
		{
			TrailRenderer val = targetTrailRenderers[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).transform.SetParent((Transform)null);
				val.autodestruct = true;
			}
		}
		targetTrailRenderers = null;
	}
}
