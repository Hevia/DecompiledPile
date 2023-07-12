using UnityEngine;

[ExecuteAlways]
public class MaintainRotation : MonoBehaviour
{
	public Vector3 eulerAngles;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.eulerAngles = eulerAngles;
	}
}
