using RoR2;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InterpolatedTransformUpdater))]
public class InterpolatedTransform : MonoBehaviour, ITeleportHandler, IEventSystemHandler
{
	private struct TransformData
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}
	}

	private TransformData[] m_lastTransforms;

	private int m_newTransformIndex;

	private void OnEnable()
	{
		ForgetPreviousTransforms();
	}

	public void ForgetPreviousTransforms()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_lastTransforms = new TransformData[2];
		TransformData transformData = new TransformData(((Component)this).transform.localPosition, ((Component)this).transform.localRotation, ((Component)this).transform.localScale);
		m_lastTransforms[0] = transformData;
		m_lastTransforms[1] = transformData;
		m_newTransformIndex = 0;
	}

	private void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		TransformData transformData = m_lastTransforms[m_newTransformIndex];
		((Component)this).transform.localPosition = transformData.position;
		((Component)this).transform.localRotation = transformData.rotation;
		((Component)this).transform.localScale = transformData.scale;
	}

	public void LateFixedUpdate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		m_newTransformIndex = OldTransformIndex();
		m_lastTransforms[m_newTransformIndex] = new TransformData(((Component)this).transform.localPosition, ((Component)this).transform.localRotation, ((Component)this).transform.localScale);
	}

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		TransformData transformData = m_lastTransforms[m_newTransformIndex];
		TransformData transformData2 = m_lastTransforms[OldTransformIndex()];
		((Component)this).transform.localPosition = Vector3.Lerp(transformData2.position, transformData.position, InterpolationController.InterpolationFactor);
		((Component)this).transform.localRotation = Quaternion.Slerp(transformData2.rotation, transformData.rotation, InterpolationController.InterpolationFactor);
		((Component)this).transform.localScale = Vector3.Lerp(transformData2.scale, transformData.scale, InterpolationController.InterpolationFactor);
	}

	private int OldTransformIndex()
	{
		if (m_newTransformIndex != 0)
		{
			return 0;
		}
		return 1;
	}

	public void OnTeleport(Vector3 oldPosition, Vector3 newPosition)
	{
		ForgetPreviousTransforms();
	}
}
