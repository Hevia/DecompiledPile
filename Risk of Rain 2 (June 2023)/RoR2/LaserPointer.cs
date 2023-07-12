using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(LineRenderer))]
public class LaserPointer : MonoBehaviour
{
	public float laserDistance;

	private LineRenderer line;

	private void Start()
	{
		line = ((Component)this).GetComponent<LineRenderer>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref val, laserDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			line.SetPosition(0, ((Component)this).transform.position);
			line.SetPosition(1, ((RaycastHit)(ref val)).point);
		}
	}
}
