using UnityEngine;

public class PositionFromParentRaycast : MonoBehaviour
{
	public float maxLength;

	public LayerMask mask;

	private void Update()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.parent.position, ((Component)this).transform.parent.forward, ref val, maxLength, LayerMask.op_Implicit(mask)))
		{
			((Component)this).transform.position = ((RaycastHit)(ref val)).point;
		}
		else
		{
			((Component)this).transform.position = ((Component)this).transform.parent.position + ((Component)this).transform.parent.forward * maxLength;
		}
	}
}
