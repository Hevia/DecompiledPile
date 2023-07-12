using UnityEngine;

namespace RoR2;

public class SurfaceDefProvider : MonoBehaviour
{
	[Tooltip("The primary surface definition. Use this when not tying to a splatmap.")]
	public SurfaceDef surfaceDef;

	public static SurfaceDef GetObjectSurfaceDef(Collider collider, Vector3 position)
	{
		SurfaceDefProvider component = ((Component)collider).GetComponent<SurfaceDefProvider>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return null;
		}
		return component.surfaceDef;
	}
}
