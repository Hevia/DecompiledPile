using RoR2;
using UnityEngine;

public class BobObject : MonoBehaviour
{
	public float bobDelay;

	public Vector3 bobDistance = Vector3.zero;

	private Vector3 initialPosition;

	private void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)((Component)this).transform.parent))
		{
			initialPosition = ((Component)this).transform.localPosition;
		}
		else
		{
			initialPosition = ((Component)this).transform.position;
		}
	}

	private void FixedUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Vector3 val = initialPosition + bobDistance * Mathf.Sin(Run.instance.fixedTime - bobDelay);
			if (Object.op_Implicit((Object)(object)((Component)this).transform.parent))
			{
				((Component)this).transform.localPosition = val;
			}
			else
			{
				((Component)this).transform.position = val;
			}
		}
	}
}
