using UnityEngine;

namespace RoR2;

public class AlignToNormal : MonoBehaviour
{
	[Tooltip("The amount to raycast down from.")]
	public float maxDistance;

	[Tooltip("The amount to pull the object out of the ground initially to test.")]
	public float offsetDistance;

	[Tooltip("Send to floor only - don't change normals.")]
	public bool changePositionOnly;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position + ((Component)this).transform.up * offsetDistance, -((Component)this).transform.up, ref val, maxDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			((Component)this).transform.position = ((RaycastHit)(ref val)).point;
			if (!changePositionOnly)
			{
				((Component)this).transform.up = ((RaycastHit)(ref val)).normal;
			}
		}
	}
}
