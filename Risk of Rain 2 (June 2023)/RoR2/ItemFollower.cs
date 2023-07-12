using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(ItemDisplay))]
public class ItemFollower : MonoBehaviour
{
	public GameObject followerPrefab;

	public GameObject targetObject;

	public BezierCurveLine followerCurve;

	public LineRenderer followerLineRenderer;

	public float distanceDampTime;

	public float distanceMaxSpeed;

	private ItemDisplay itemDisplay;

	private Vector3 velocityDistance;

	private Vector3 v0;

	private Vector3 v1;

	[HideInInspector]
	public GameObject followerInstance;

	private void Start()
	{
		itemDisplay = ((Component)this).GetComponent<ItemDisplay>();
		followerLineRenderer = ((Component)this).GetComponent<LineRenderer>();
		Rebuild();
	}

	private void Rebuild()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (itemDisplay.GetVisibilityLevel() == VisibilityLevel.Invisible)
		{
			if (Object.op_Implicit((Object)(object)followerInstance))
			{
				Object.Destroy((Object)(object)followerInstance);
			}
			if (Object.op_Implicit((Object)(object)followerLineRenderer))
			{
				((Renderer)followerLineRenderer).enabled = false;
			}
			return;
		}
		if (!Object.op_Implicit((Object)(object)followerInstance))
		{
			followerInstance = Object.Instantiate<GameObject>(followerPrefab, targetObject.transform.position, Quaternion.identity);
			followerInstance.transform.localScale = ((Component)this).transform.localScale;
			if (Object.op_Implicit((Object)(object)followerCurve))
			{
				v0 = followerCurve.v0;
				v1 = followerCurve.v1;
			}
		}
		if (Object.op_Implicit((Object)(object)followerLineRenderer))
		{
			((Renderer)followerLineRenderer).enabled = true;
		}
	}

	private void Update()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Rebuild();
		if (Object.op_Implicit((Object)(object)followerInstance))
		{
			Transform transform = followerInstance.transform;
			Transform transform2 = targetObject.transform;
			transform.position = Vector3.SmoothDamp(transform.position, transform2.position, ref velocityDistance, distanceDampTime);
			transform.rotation = transform2.rotation;
			if (Object.op_Implicit((Object)(object)followerCurve))
			{
				followerCurve.v0 = ((Component)this).transform.TransformVector(v0);
				followerCurve.v1 = transform.TransformVector(v1);
				followerCurve.p1 = transform.position;
			}
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)followerInstance))
		{
			Object.Destroy((Object)(object)followerInstance);
		}
	}
}
