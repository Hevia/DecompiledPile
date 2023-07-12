using System;
using UnityEngine;

namespace RoR2;

public class StriderLegController : MonoBehaviour
{
	[Serializable]
	public struct FootInfo
	{
		public Transform transform;

		public Transform referenceTransform;

		[HideInInspector]
		public Vector3 velocity;

		[HideInInspector]
		public FootState footState;

		[HideInInspector]
		public Vector3 plantPosition;

		[HideInInspector]
		public Vector3 trailingTargetPosition;

		[HideInInspector]
		public float stopwatch;

		[HideInInspector]
		public float currentYOffsetFromRaycast;

		[HideInInspector]
		public float lastYOffsetFromRaycast;

		[HideInInspector]
		public float footRaycastTimer;
	}

	public enum FootState
	{
		Planted,
		Replanting
	}

	[Header("Foot Settings")]
	public Transform centerOfGravity;

	public FootInfo[] feet;

	public Vector3 footRaycastDirection;

	public float raycastVerticalOffset;

	public float maxRaycastDistance;

	public float footDampTime;

	public float stabilityRadius;

	public float replantDuration;

	public float replantHeight;

	public float overstepDistance;

	public AnimationCurve lerpCurve;

	public GameObject footPlantEffect;

	public string footPlantString;

	public string footMoveString;

	public float footRaycastFrequency = 0.2f;

	public int maxFeetReplantingAtOnce = 9999;

	[Header("Root Settings")]
	public Transform rootTransform;

	public float rootSpringConstant;

	public float rootDampingConstant;

	public float rootOffsetHeight;

	public float rootSmoothDamp;

	private float rootVelocity;

	public Vector3 GetCenterOfStance()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < feet.Length; i++)
		{
			val += feet[i].transform.position;
		}
		return val / (float)feet.Length;
	}

	private void Awake()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i].footState = FootState.Planted;
			feet[i].plantPosition = feet[i].referenceTransform.position;
			feet[i].trailingTargetPosition = feet[i].plantPosition;
			feet[i].footRaycastTimer = Random.Range(0f, 1f / footRaycastFrequency);
		}
	}

	private void Update()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < feet.Length; i++)
		{
			if (feet[i].footState == FootState.Replanting)
			{
				num2++;
			}
		}
		RaycastHit val6 = default(RaycastHit);
		for (int j = 0; j < feet.Length; j++)
		{
			feet[j].footRaycastTimer -= Time.deltaTime;
			Transform transform = feet[j].transform;
			Transform referenceTransform = feet[j].referenceTransform;
			_ = transform.position;
			Vector3 val = Vector3.zero;
			float num3 = 0f;
			switch (feet[j].footState)
			{
			case FootState.Planted:
			{
				num++;
				val = feet[j].plantPosition;
				Vector3 val4 = referenceTransform.position - val;
				if (((Vector3)(ref val4)).sqrMagnitude > stabilityRadius * stabilityRadius && num2 < maxFeetReplantingAtOnce)
				{
					feet[j].footState = FootState.Replanting;
					Util.PlaySound(footMoveString, ((Component)transform).gameObject);
					num2++;
				}
				break;
			}
			case FootState.Replanting:
			{
				feet[j].stopwatch += Time.deltaTime;
				Vector3 plantPosition = feet[j].plantPosition;
				Vector3 position = referenceTransform.position;
				Vector3 val2 = position;
				Vector3 val3 = Vector3.ProjectOnPlane(position - plantPosition, Vector3.up);
				position = val2 + ((Vector3)(ref val3)).normalized * overstepDistance;
				float num4 = lerpCurve.Evaluate(feet[j].stopwatch / replantDuration);
				val = Vector3.Lerp(plantPosition, position, num4);
				num3 = Mathf.Sin(num4 * MathF.PI) * replantHeight;
				if (feet[j].stopwatch >= replantDuration)
				{
					feet[j].plantPosition = position;
					feet[j].stopwatch = 0f;
					feet[j].footState = FootState.Planted;
					Util.PlaySound(footPlantString, ((Component)transform).gameObject);
					if (Object.op_Implicit((Object)(object)footPlantEffect))
					{
						EffectManager.SimpleEffect(footPlantEffect, transform.position, Quaternion.identity, transmit: false);
					}
				}
				break;
			}
			}
			Ray val5 = default(Ray);
			((Ray)(ref val5)).direction = transform.TransformDirection(((Vector3)(ref footRaycastDirection)).normalized);
			((Ray)(ref val5)).origin = val - ((Ray)(ref val5)).direction * raycastVerticalOffset;
			if (feet[j].footRaycastTimer <= 0f)
			{
				feet[j].footRaycastTimer = 1f / footRaycastFrequency;
				feet[j].lastYOffsetFromRaycast = feet[j].currentYOffsetFromRaycast;
				if (Physics.Raycast(val5, ref val6, maxRaycastDistance + raycastVerticalOffset, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					feet[j].currentYOffsetFromRaycast = ((RaycastHit)(ref val6)).point.y - val.y;
				}
				else
				{
					feet[j].currentYOffsetFromRaycast = 0f;
				}
			}
			float num5 = Mathf.Lerp(feet[j].currentYOffsetFromRaycast, feet[j].lastYOffsetFromRaycast, feet[j].footRaycastTimer / (1f / footRaycastFrequency));
			val.y += num3 + num5;
			feet[j].trailingTargetPosition = Vector3.SmoothDamp(feet[j].trailingTargetPosition, val, ref feet[j].velocity, footDampTime);
			transform.position = feet[j].trailingTargetPosition;
		}
		if (Object.op_Implicit((Object)(object)rootTransform))
		{
			Vector3 localPosition = rootTransform.localPosition;
			float num6 = (1f - (float)num / (float)feet.Length) * rootOffsetHeight;
			float num7 = localPosition.z - num6;
			float num8 = Mathf.SmoothDamp(localPosition.z, num7, ref rootVelocity, rootSmoothDamp);
			rootTransform.localPosition = new Vector3(localPosition.x, localPosition.y, num8);
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

	public void OnDrawGizmos()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < feet.Length; i++)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(feet[i].transform.position, feet[i].transform.TransformVector(footRaycastDirection));
		}
	}
}
