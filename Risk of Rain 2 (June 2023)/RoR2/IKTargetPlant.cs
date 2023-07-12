using System;
using UnityEngine;

namespace RoR2;

public class IKTargetPlant : MonoBehaviour, IIKTargetBehavior
{
	public enum IKState
	{
		Plant,
		Reset
	}

	[Tooltip("The max offset to step up")]
	public float minHeight = -0.3f;

	[Tooltip("The max offset to step down")]
	public float maxHeight = 1f;

	[Tooltip("The strength of the IK as a lerp (0-1)")]
	public float ikWeight = 1f;

	[Tooltip("The time to restep")]
	public float timeToReset = 0.6f;

	[Tooltip("The max positional IK error before restepping")]
	public float maxXZPositionalError = 4f;

	public GameObject plantEffect;

	public Animator animator;

	[Tooltip("The IK weight float parameter if used")]
	public string animatorIKWeightFloat;

	[Tooltip("The lift animation trigger string if used")]
	public string animatorLiftTrigger;

	[Tooltip("The scale of the leg for calculating if the leg is too short to reach the IK target")]
	public float legScale = 1f;

	[Tooltip("The height of the step arc")]
	public float arcHeight = 1f;

	[Tooltip("The smoothing duration for the IK. Higher will be smoother but will be delayed.")]
	public float smoothDampTime = 0.1f;

	[Tooltip("Spherecasts will have more hits but take higher performance.")]
	public bool useSpherecast;

	public float spherecastRadius = 0.5f;

	public IKState ikState;

	private bool isPlanted;

	private Vector3 lastTransformPosition;

	private Vector3 smoothDampRefVelocity;

	private Vector3 targetPosition;

	private Vector3 plantPosition;

	private IKSimpleChain ikChain;

	private float resetTimer;

	private void Awake()
	{
		ikChain = ((Component)this).GetComponent<IKSimpleChain>();
	}

	public void UpdateIKState(int targetState)
	{
		if (ikState != IKState.Reset)
		{
			ikState = (IKState)targetState;
		}
	}

	public Vector3 GetArcPosition(Vector3 start, Vector3 end, float arcHeight, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(start, end, Mathf.Sin(t * MathF.PI * 0.5f)) + new Vector3(0f, Mathf.Sin(t * MathF.PI) * arcHeight, 0f);
	}

	public void UpdateIKTargetPosition()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)animator))
		{
			ikWeight = animator.GetFloat(animatorIKWeightFloat);
		}
		else
		{
			ikWeight = 1f;
		}
		switch (ikState)
		{
		case IKState.Reset:
			resetTimer += Time.deltaTime;
			isPlanted = false;
			RaycastIKTarget(((Component)this).transform.position);
			((Component)this).transform.position = GetArcPosition(plantPosition, targetPosition, arcHeight, resetTimer / timeToReset);
			if (resetTimer >= timeToReset)
			{
				ikState = IKState.Plant;
				isPlanted = true;
				plantPosition = targetPosition;
				Object.Instantiate<GameObject>(plantEffect, plantPosition, Quaternion.identity);
			}
			break;
		case IKState.Plant:
		{
			Vector3 position = ((Component)this).transform.position;
			RaycastIKTarget(position);
			if (!isPlanted)
			{
				plantPosition = targetPosition;
				((Component)this).transform.position = plantPosition;
				isPlanted = true;
				if (Object.op_Implicit((Object)(object)plantEffect))
				{
					Object.Instantiate<GameObject>(plantEffect, plantPosition, Quaternion.identity);
				}
			}
			else
			{
				((Component)this).transform.position = Vector3.Lerp(position, plantPosition, ikWeight);
			}
			Vector3 val = position - ((Component)this).transform.position;
			val.y = 0f;
			if (ikChain.LegTooShort(legScale) || ((Vector3)(ref val)).sqrMagnitude >= maxXZPositionalError * maxXZPositionalError)
			{
				plantPosition = ((Component)this).transform.position;
				ikState = IKState.Reset;
				if (Object.op_Implicit((Object)(object)animator))
				{
					animator.SetTrigger(animatorLiftTrigger);
				}
				resetTimer = 0f;
			}
			break;
		}
		}
		((Component)this).transform.position = Vector3.SmoothDamp(lastTransformPosition, ((Component)this).transform.position, ref smoothDampRefVelocity, smoothDampTime);
		lastTransformPosition = ((Component)this).transform.position;
	}

	public void RaycastIKTarget(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (useSpherecast)
		{
			Physics.SphereCast(position + Vector3.up * (0f - minHeight), spherecastRadius, Vector3.down, ref val, maxHeight - minHeight, LayerMask.op_Implicit(LayerIndex.world.mask));
		}
		else
		{
			Physics.Raycast(position + Vector3.up * (0f - minHeight), Vector3.down, ref val, maxHeight - minHeight, LayerMask.op_Implicit(LayerIndex.world.mask));
		}
		if (Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider))
		{
			targetPosition = ((RaycastHit)(ref val)).point;
		}
		else
		{
			targetPosition = position;
		}
	}
}
