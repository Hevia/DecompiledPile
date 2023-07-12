using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

[RequireComponent(typeof(EffectComponent))]
public class Tracer : MonoBehaviour
{
	[Tooltip("A child transform which will be placed at the start of the tracer path upon creation.")]
	public Transform startTransform;

	[Tooltip("Child object to scale to the length of this tracer and burst particles on based on that length. Optional.")]
	public GameObject beamObject;

	[Tooltip("The number of particles to emit per meter of length if using a beam object.")]
	public float beamDensity = 10f;

	[Tooltip("The travel speed of this tracer.")]
	public float speed = 1f;

	[Tooltip("Child transform which will be moved to the head of the tracer.")]
	public Transform headTransform;

	[Tooltip("Child transform which will be moved to the tail of the tracer.")]
	public Transform tailTransform;

	[Tooltip("The maximum distance between head and tail transforms.")]
	public float length = 1f;

	[Tooltip("Reverses the travel direction of the tracer.")]
	public bool reverse;

	[Tooltip("The event that runs when the tail reaches the destination.")]
	public UnityEvent onTailReachedDestination;

	private Vector3 startPos;

	private Vector3 endPos;

	private float distanceTraveled;

	private float totalDistance;

	private Vector3 normal;

	private void Start()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		EffectComponent component = ((Component)this).GetComponent<EffectComponent>();
		endPos = component.effectData.origin;
		Transform val = component.effectData.ResolveChildLocatorTransformReference();
		startPos = (Object.op_Implicit((Object)(object)val) ? val.position : component.effectData.start);
		if (reverse)
		{
			Util.Swap(ref endPos, ref startPos);
		}
		Vector3 val2 = endPos - startPos;
		distanceTraveled = 0f;
		totalDistance = Vector3.Magnitude(val2);
		if (totalDistance != 0f)
		{
			normal = val2 * (1f / totalDistance);
			((Component)this).transform.rotation = Util.QuaternionSafeLookRotation(normal);
		}
		else
		{
			normal = Vector3.zero;
		}
		if (Object.op_Implicit((Object)(object)beamObject))
		{
			beamObject.transform.position = startPos + val2 * 0.5f;
			ParticleSystem component2 = beamObject.GetComponent<ParticleSystem>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				ShapeModule shape = component2.shape;
				((ShapeModule)(ref shape)).radius = totalDistance * 0.5f;
				component2.Emit(Mathf.FloorToInt(totalDistance * beamDensity) - 1);
			}
		}
		if (Object.op_Implicit((Object)(object)startTransform))
		{
			startTransform.position = startPos;
		}
	}

	private void Update()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (distanceTraveled > totalDistance)
		{
			onTailReachedDestination.Invoke();
			return;
		}
		distanceTraveled += speed * Time.deltaTime;
		float num = Mathf.Clamp(distanceTraveled, 0f, totalDistance);
		float num2 = Mathf.Clamp(distanceTraveled - length, 0f, totalDistance);
		if (Object.op_Implicit((Object)(object)headTransform))
		{
			headTransform.position = startPos + num * normal;
		}
		if (Object.op_Implicit((Object)(object)tailTransform))
		{
			tailTransform.position = startPos + num2 * normal;
		}
	}
}
