using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionAccumulator : MonoBehaviour
{
	private Animator animator;

	[NonSerialized]
	public Vector3 accumulatedRootMotion;

	public Quaternion accumulatedRootRotation;

	public bool accumulateRotation;

	public Vector3 ExtractRootMotion()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = accumulatedRootMotion;
		accumulatedRootMotion = Vector3.zero;
		return result;
	}

	public Quaternion ExtractRootRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion result = accumulatedRootRotation;
		accumulatedRootRotation = Quaternion.identity;
		return result;
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		animator = ((Component)this).GetComponent<Animator>();
		accumulatedRootRotation = Quaternion.identity;
	}

	private void OnAnimatorMove()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		accumulatedRootMotion += animator.deltaPosition;
		if (accumulateRotation)
		{
			accumulatedRootRotation *= animator.deltaRotation;
		}
	}
}
