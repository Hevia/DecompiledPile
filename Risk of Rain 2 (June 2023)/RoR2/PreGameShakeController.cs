using UnityEngine;

namespace RoR2;

public class PreGameShakeController : MonoBehaviour
{
	public ShakeEmitter shakeEmitter;

	public float minInterval = 0.5f;

	public float maxInterval = 7f;

	public Rigidbody[] physicsBodies;

	public float physicsForce;

	private float timer;

	private void ResetTimer()
	{
		timer = Random.Range(minInterval, maxInterval);
	}

	private void DoShake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		shakeEmitter.StartShake();
		Vector3 onUnitSphere = Random.onUnitSphere;
		Rigidbody[] array = physicsBodies;
		foreach (Rigidbody val in array)
		{
			if (Object.op_Implicit((Object)(object)val))
			{
				Vector3 val2 = onUnitSphere * ((0.75f + Random.value * 0.25f) * physicsForce);
				Bounds bounds = ((Component)val).GetComponent<Collider>().bounds;
				float y = ((Bounds)(ref bounds)).min.y;
				Vector3 centerOfMass = val.centerOfMass;
				centerOfMass.y = y;
				val.AddForceAtPosition(val2, centerOfMass);
			}
		}
	}

	private void Awake()
	{
		ResetTimer();
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			ResetTimer();
			DoShake();
		}
	}
}
