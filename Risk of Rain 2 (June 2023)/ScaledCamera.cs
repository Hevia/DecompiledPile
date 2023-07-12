using UnityEngine;

public class ScaledCamera : MonoBehaviour
{
	public float scale = 1f;

	private bool foundCamera;

	private Vector3 offset;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if ((Object)(object)main != (Object)null)
		{
			if (!foundCamera)
			{
				foundCamera = true;
				offset = ((Component)main).transform.position - ((Component)this).transform.position;
			}
			((Component)this).transform.eulerAngles = ((Component)main).transform.eulerAngles;
			((Component)this).transform.position = ((Component)main).transform.position / scale - offset;
		}
	}
}
