using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2.Hologram;

public class HologramProjector : MonoBehaviour
{
	[Tooltip("The range in meters at which the hologram begins to display.")]
	public float displayDistance = 15f;

	[Tooltip("The position at which to display the hologram.")]
	public Transform hologramPivot;

	[Tooltip("Whether or not the hologram will pivot to the player")]
	public bool disableHologramRotation;

	private float transformDampVelocity;

	private IHologramContentProvider contentProvider;

	private float viewerReselectTimer;

	private float viewerReselectInterval = 0.25f;

	private Transform cachedViewer;

	private Transform viewer;

	private GameObject hologramContentInstance;

	private void Awake()
	{
		contentProvider = ((Component)this).GetComponent<IHologramContentProvider>();
	}

	private Transform FindViewer(Vector3 position)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (viewerReselectTimer > 0f)
		{
			return cachedViewer;
		}
		viewerReselectTimer = viewerReselectInterval;
		cachedViewer = null;
		float num = float.PositiveInfinity;
		ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
		int i = 0;
		for (int count = instances.Count; i < count; i++)
		{
			GameObject bodyObject = instances[i].master.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				Vector3 val = bodyObject.transform.position - position;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					cachedViewer = bodyObject.transform;
				}
			}
		}
		return cachedViewer;
	}

	private void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		viewerReselectTimer -= Time.deltaTime;
		Vector3 val = (Object.op_Implicit((Object)(object)hologramPivot) ? hologramPivot.position : ((Component)this).transform.position);
		viewer = FindViewer(val);
		Vector3 val2 = (Object.op_Implicit((Object)(object)viewer) ? viewer.position : ((Component)this).transform.position);
		bool flag = false;
		Vector3 forward = Vector3.zero;
		if (Object.op_Implicit((Object)(object)viewer))
		{
			forward = val - val2;
			if (((Vector3)(ref forward)).sqrMagnitude <= displayDistance * displayDistance)
			{
				flag = true;
			}
		}
		if (flag)
		{
			flag = contentProvider.ShouldDisplayHologram(((Component)viewer).gameObject);
		}
		if (flag)
		{
			if (!Object.op_Implicit((Object)(object)hologramContentInstance))
			{
				BuildHologram();
			}
			if (Object.op_Implicit((Object)(object)hologramContentInstance) && contentProvider != null)
			{
				contentProvider.UpdateHologramContent(hologramContentInstance);
				if (!disableHologramRotation)
				{
					hologramContentInstance.transform.rotation = Util.SmoothDampQuaternion(hologramContentInstance.transform.rotation, Util.QuaternionSafeLookRotation(forward), ref transformDampVelocity, 0.2f);
				}
			}
		}
		else
		{
			DestroyHologram();
		}
	}

	private void BuildHologram()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		DestroyHologram();
		if (contentProvider == null)
		{
			return;
		}
		GameObject hologramContentPrefab = contentProvider.GetHologramContentPrefab();
		if (Object.op_Implicit((Object)(object)hologramContentPrefab))
		{
			hologramContentInstance = Object.Instantiate<GameObject>(hologramContentPrefab);
			hologramContentInstance.transform.parent = (Object.op_Implicit((Object)(object)hologramPivot) ? hologramPivot : ((Component)this).transform);
			hologramContentInstance.transform.localPosition = Vector3.zero;
			hologramContentInstance.transform.localRotation = Quaternion.identity;
			hologramContentInstance.transform.localScale = Vector3.one;
			if (Object.op_Implicit((Object)(object)viewer) && !disableHologramRotation)
			{
				Vector3 val = (Object.op_Implicit((Object)(object)hologramPivot) ? hologramPivot.position : ((Component)this).transform.position);
				_ = viewer.position;
				Vector3 forward = val - viewer.position;
				hologramContentInstance.transform.rotation = Util.QuaternionSafeLookRotation(forward);
			}
			contentProvider.UpdateHologramContent(hologramContentInstance);
		}
	}

	private void DestroyHologram()
	{
		if (Object.op_Implicit((Object)(object)hologramContentInstance))
		{
			Object.Destroy((Object)(object)hologramContentInstance);
		}
		hologramContentInstance = null;
	}
}
