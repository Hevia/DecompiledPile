using UnityEngine;

namespace RoR2;

public class FollowerItemDisplayComponent : MonoBehaviour
{
	public Transform target;

	public Vector3 localPosition;

	public Quaternion localRotation;

	public Vector3 localScale;

	private Transform transform;

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void LateUpdate()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Quaternion rotation = target.rotation;
		transform.position = target.position + rotation * localPosition;
		transform.rotation = rotation * localRotation;
		transform.localScale = localScale;
	}
}
