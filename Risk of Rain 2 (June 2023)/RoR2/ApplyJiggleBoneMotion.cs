using UnityEngine;

namespace RoR2;

public class ApplyJiggleBoneMotion : MonoBehaviour
{
	public float forceScale = 100f;

	public Transform rootTransform;

	public Rigidbody[] rigidbodies;

	private Vector3 lastRootPosition;

	private void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = rootTransform.position;
		Rigidbody[] array = rigidbodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AddForce((lastRootPosition - position) * forceScale * Time.fixedDeltaTime);
		}
		lastRootPosition = position;
	}
}
