using UnityEngine;

public class HoverOverHead : MonoBehaviour
{
	private Transform parentTransform;

	private Collider bodyCollider;

	public Vector3 bonusOffset;

	private void Start()
	{
		parentTransform = ((Component)this).transform.parent;
		bodyCollider = ((Component)((Component)this).transform.parent).GetComponent<Collider>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = parentTransform.position;
		if (Object.op_Implicit((Object)(object)bodyCollider))
		{
			Bounds bounds = bodyCollider.bounds;
			Vector3 center = ((Bounds)(ref bounds)).center;
			bounds = bodyCollider.bounds;
			val = center + new Vector3(0f, ((Bounds)(ref bounds)).extents.y, 0f);
		}
		((Component)this).transform.position = val + bonusOffset;
	}
}
