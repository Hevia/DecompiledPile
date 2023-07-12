using UnityEngine;
using UnityEngine.Events;

namespace RoR2.Orbs;

[RequireComponent(typeof(EffectComponent))]
public class OrbEffect : MonoBehaviour
{
	private Transform targetTransform;

	private float duration;

	private Vector3 startPosition;

	private Vector3 previousPosition;

	private Vector3 lastKnownTargetPosition;

	private float age;

	[HideInInspector]
	public Transform parentObjectTransform;

	[Header("Curve Parameters")]
	public Vector3 startVelocity1;

	public Vector3 startVelocity2;

	public Vector3 endVelocity1;

	public Vector3 endVelocity2;

	private Vector3 startVelocity;

	private Vector3 endVelocity;

	public AnimationCurve movementCurve;

	public BezierCurveLine bezierCurveLine;

	public bool faceMovement = true;

	public bool callArrivalIfTargetIsGone;

	[Header("Start Effect")]
	[Tooltip("An effect prefab to spawn on Start")]
	public GameObject startEffect;

	public float startEffectScale = 1f;

	public bool startEffectCopiesRotation;

	[Header("End Effect")]
	[Tooltip("An effect prefab to spawn on end")]
	public GameObject endEffect;

	public float endEffectScale = 1f;

	public bool endEffectCopiesRotation;

	[Header("Arrival Behavior")]
	public UnityEvent onArrival;

	private void Start()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		EffectComponent component = ((Component)this).GetComponent<EffectComponent>();
		startPosition = component.effectData.origin;
		previousPosition = startPosition;
		GameObject val = component.effectData.ResolveHurtBoxReference();
		targetTransform = (Object.op_Implicit((Object)(object)val) ? val.transform : null);
		duration = component.effectData.genericFloat;
		if (duration == 0f)
		{
			Debug.LogFormat("Zero duration for effect \"{0}\"", new object[1] { ((Object)((Component)this).gameObject).name });
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		lastKnownTargetPosition = (Object.op_Implicit((Object)(object)targetTransform) ? targetTransform.position : startPosition);
		if (Object.op_Implicit((Object)(object)startEffect))
		{
			EffectData effectData = new EffectData
			{
				origin = ((Component)this).transform.position,
				scale = startEffectScale
			};
			if (startEffectCopiesRotation)
			{
				effectData.rotation = ((Component)this).transform.rotation;
			}
			EffectManager.SpawnEffect(startEffect, effectData, transmit: false);
		}
		startVelocity.x = Mathf.Lerp(startVelocity1.x, startVelocity2.x, Random.value);
		startVelocity.y = Mathf.Lerp(startVelocity1.y, startVelocity2.y, Random.value);
		startVelocity.z = Mathf.Lerp(startVelocity1.z, startVelocity2.z, Random.value);
		endVelocity.x = Mathf.Lerp(endVelocity1.x, endVelocity2.x, Random.value);
		endVelocity.y = Mathf.Lerp(endVelocity1.y, endVelocity2.y, Random.value);
		endVelocity.z = Mathf.Lerp(endVelocity1.z, endVelocity2.z, Random.value);
		UpdateOrb(0f);
	}

	private void Update()
	{
		UpdateOrb(Time.deltaTime);
	}

	private void UpdateOrb(float deltaTime)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)parentObjectTransform))
		{
			startPosition = parentObjectTransform.position;
		}
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			lastKnownTargetPosition = targetTransform.position;
		}
		float num = Mathf.Clamp01(age / duration);
		float num2 = movementCurve.Evaluate(num);
		Vector3 val = Vector3.Lerp(startPosition + startVelocity * num2, lastKnownTargetPosition + endVelocity * (1f - num2), num2);
		((Component)this).transform.position = val;
		if (faceMovement && val != previousPosition)
		{
			((Component)this).transform.forward = val - previousPosition;
		}
		UpdateBezier();
		if (num == 1f || (callArrivalIfTargetIsGone && (Object)(object)targetTransform == (Object)null))
		{
			onArrival.Invoke();
			if (Object.op_Implicit((Object)(object)endEffect))
			{
				EffectData effectData = new EffectData
				{
					origin = ((Component)this).transform.position,
					scale = endEffectScale
				};
				if (endEffectCopiesRotation)
				{
					effectData.rotation = ((Component)this).transform.rotation;
				}
				EffectManager.SpawnEffect(endEffect, effectData, transmit: false);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		previousPosition = val;
		age += deltaTime;
	}

	private void UpdateBezier()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)bezierCurveLine))
		{
			bezierCurveLine.p1 = startPosition;
			bezierCurveLine.v0 = endVelocity;
			bezierCurveLine.v1 = startVelocity;
			bezierCurveLine.UpdateBezier(0f);
		}
	}

	public void InstantiatePrefab(GameObject prefab)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(prefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
	}

	public void InstantiateEffect(GameObject prefab)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(prefab, new EffectData
		{
			origin = ((Component)this).transform.position
		}, transmit: false);
	}

	public void InstantiateEffectCopyRotation(GameObject prefab)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(prefab, new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = ((Component)this).transform.rotation
		}, transmit: false);
	}

	public void InstantiateEffectOppositeFacing(GameObject prefab)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(prefab, new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Util.QuaternionSafeLookRotation(-((Component)this).transform.forward)
		}, transmit: false);
	}

	public void InstantiatePrefabOppositeFacing(GameObject prefab)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(prefab, ((Component)this).transform.position, Util.QuaternionSafeLookRotation(-((Component)this).transform.forward));
	}
}
