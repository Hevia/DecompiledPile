using UnityEngine;

namespace RoR2;

public class ExperienceOrbBehavior : MonoBehaviour
{
	public GameObject hitEffectPrefab;

	private Transform transform;

	private TrailRenderer trail;

	private Light light;

	private float localTime;

	private Vector3 startPos;

	private Vector3 previousPos;

	private Vector3 initialVelocity;

	private float scale;

	private bool consumed;

	private static readonly string expSoundString = "Play_UI_xp_gain";

	public Transform targetTransform { get; set; }

	public float travelTime { get; set; }

	public ulong exp { get; set; }

	private void Awake()
	{
		transform = ((Component)this).transform;
		trail = ((Component)this).GetComponent<TrailRenderer>();
		light = ((Component)this).GetComponent<Light>();
	}

	private void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		localTime = 0f;
		consumed = false;
		startPos = transform.position;
		previousPos = startPos;
		scale = 2f * Mathf.Log((float)exp + 1f, 6f);
		initialVelocity = (Vector3.up * 4f + Random.insideUnitSphere * 1f) * scale;
		transform.localScale = new Vector3(scale, scale, scale);
		trail.startWidth = 0.05f * scale;
		if (Object.op_Implicit((Object)(object)light))
		{
			light.range = 1f * scale;
		}
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		localTime += Time.deltaTime;
		if (!Object.op_Implicit((Object)(object)targetTransform))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		float num = Mathf.Clamp01(localTime / travelTime);
		previousPos = transform.position;
		transform.position = CalculatePosition(startPos, initialVelocity, targetTransform.position, num);
		if (num >= 1f)
		{
			OnHitTarget();
		}
	}

	private static Vector3 CalculatePosition(Vector3 startPos, Vector3 initialVelocity, Vector3 targetPos, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = startPos + initialVelocity * t;
		float num = t * t * t;
		return Vector3.LerpUnclamped(val, targetPos, num);
	}

	private void OnTriggerStay(Collider other)
	{
		if ((Object)(object)((Component)other).transform == (Object)(object)targetTransform)
		{
			OnHitTarget();
		}
	}

	private void OnHitTarget()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!consumed)
		{
			consumed = true;
			Util.PlaySound(expSoundString, ((Component)targetTransform).gameObject);
			Transform obj = Object.Instantiate<GameObject>(hitEffectPrefab, transform.position, Util.QuaternionSafeLookRotation(previousPos - startPos)).transform;
			obj.localScale *= scale;
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
