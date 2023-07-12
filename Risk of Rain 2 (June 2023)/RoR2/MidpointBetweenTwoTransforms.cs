using UnityEngine;

namespace RoR2;

[ExecuteAlways]
public class MidpointBetweenTwoTransforms : MonoBehaviour
{
	public Transform transform1;

	public Transform transform2;

	public void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector3.Lerp(transform1.position, transform2.position, 0.5f);
	}
}
