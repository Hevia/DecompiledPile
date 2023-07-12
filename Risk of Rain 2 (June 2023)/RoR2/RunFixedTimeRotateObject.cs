using UnityEngine;

namespace RoR2;

public class RunFixedTimeRotateObject : MonoBehaviour
{
	[SerializeField]
	private Vector3 eulerVelocity;

	[SerializeField]
	private bool isLocal = true;

	private Quaternion initialRotation;

	private void Start()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (isLocal)
		{
			initialRotation = ((Component)this).transform.localRotation;
		}
		else
		{
			initialRotation = ((Component)this).transform.rotation;
		}
	}

	private void FixedUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Quaternion val = initialRotation * Quaternion.Euler(eulerVelocity * Run.instance.fixedTime);
			if (isLocal)
			{
				((Component)this).transform.localRotation = val;
			}
			else
			{
				((Component)this).transform.rotation = val;
			}
		}
	}
}
