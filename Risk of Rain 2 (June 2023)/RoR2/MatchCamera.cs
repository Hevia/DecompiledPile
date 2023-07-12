using UnityEngine;

namespace RoR2;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MatchCamera : MonoBehaviour
{
	private Camera destCamera;

	public Camera srcCamera;

	public bool matchFOV = true;

	public bool matchRect = true;

	public bool matchPosition;

	private void Awake()
	{
		destCamera = ((Component)this).GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)srcCamera))
		{
			if (matchRect)
			{
				destCamera.rect = srcCamera.rect;
			}
			if (matchFOV)
			{
				destCamera.fieldOfView = srcCamera.fieldOfView;
			}
			if (matchPosition)
			{
				((Component)destCamera).transform.position = ((Component)srcCamera).transform.position;
				((Component)destCamera).transform.rotation = ((Component)srcCamera).transform.rotation;
			}
		}
	}
}
