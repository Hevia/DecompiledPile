using UnityEngine;

namespace RoR2;

public class IKTargetPassive : MonoBehaviour, IIKTargetBehavior
{
	private float smoothedTargetHeightOffset;

	private float targetHeightOffset;

	private float smoothdampVelocity;

	public float minHeight = -0.3f;

	public float maxHeight = 1f;

	public float dampTime = 0.1f;

	public float baseOffset;

	[Tooltip("The IK weight float parameter if used")]
	public string animatorIKWeightFloat = "";

	public Animator animator;

	[Tooltip("The target transform will plant without any calls from external IK chains")]
	public bool selfPlant;

	public float selfPlantFrequency = 5f;

	[Tooltip("Whether or not to cache where the raycast begins. Used when not attached to bones, who reset themselves via animator.")]
	public bool cacheFirstPosition;

	private Vector3 cachedLocalPosition;

	private float selfPlantTimer;

	public void UpdateIKState(int targetState)
	{
	}

	private void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (cacheFirstPosition)
		{
			cachedLocalPosition = ((Component)this).transform.localPosition;
		}
	}

	private void LateUpdate()
	{
		selfPlantTimer -= Time.deltaTime;
		if (selfPlant && selfPlantTimer <= 0f)
		{
			selfPlantTimer = 1f / selfPlantFrequency;
			UpdateIKTargetPosition();
		}
		UpdateYOffset();
	}

	public void UpdateIKTargetPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ResetTransformToCachedPosition();
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position + Vector3.up * (0f - minHeight), Vector3.down, ref val, maxHeight - minHeight, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			targetHeightOffset = ((RaycastHit)(ref val)).point.y - ((Component)this).transform.position.y;
		}
		else
		{
			targetHeightOffset = 0f;
		}
		targetHeightOffset += baseOffset;
	}

	public void UpdateYOffset()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (Object.op_Implicit((Object)(object)animator) && animatorIKWeightFloat.Length > 0)
		{
			num = animator.GetFloat(animatorIKWeightFloat);
		}
		smoothedTargetHeightOffset = Mathf.SmoothDamp(smoothedTargetHeightOffset, targetHeightOffset, ref smoothdampVelocity, dampTime, float.PositiveInfinity, Time.deltaTime);
		ResetTransformToCachedPosition();
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y + Mathf.Lerp(0f, smoothedTargetHeightOffset, num), ((Component)this).transform.position.z);
	}

	private void ResetTransformToCachedPosition()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (cacheFirstPosition)
		{
			((Component)this).transform.localPosition = new Vector3(cachedLocalPosition.x, cachedLocalPosition.y, cachedLocalPosition.z);
		}
	}
}
