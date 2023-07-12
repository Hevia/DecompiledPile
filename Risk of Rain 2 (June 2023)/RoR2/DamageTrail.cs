using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

public class DamageTrail : MonoBehaviour
{
	private struct TrailPoint
	{
		public Vector3 position;

		public float localStartTime;

		public float localEndTime;

		public Transform segmentTransform;
	}

	[FormerlySerializedAs("updateInterval")]
	[Tooltip("How often to drop a new point onto the trail.")]
	public float pointUpdateInterval = 0.2f;

	[Tooltip("How often the damage trail should deal damage.")]
	public float damageUpdateInterval = 0.2f;

	[Tooltip("How large the radius, or width, of the damage detection should be.")]
	public float radius = 0.5f;

	[Tooltip("How large the height of the damage detection should be.")]
	public float height = 0.5f;

	[Tooltip("How long a point on the trail should last.")]
	public float pointLifetime = 3f;

	[Tooltip("The line renderer to use for display.")]
	public LineRenderer lineRenderer;

	public bool active = true;

	[Tooltip("Prefab to use per segment.")]
	public GameObject segmentPrefab;

	public bool destroyTrailSegments;

	public float damagePerSecond;

	public GameObject owner;

	private Transform transform;

	private List<TrailPoint> pointsList;

	private float localTime;

	private float nextTrailPointUpdate;

	private float nextTrailDamageUpdate;

	private void Awake()
	{
		pointsList = new List<TrailPoint>();
		transform = ((Component)this).transform;
	}

	private void Start()
	{
		localTime = 0f;
		AddPoint();
		AddPoint();
	}

	private void FixedUpdate()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		localTime += Time.fixedDeltaTime;
		if (localTime >= nextTrailPointUpdate)
		{
			nextTrailPointUpdate += pointUpdateInterval;
			UpdateTrail(active);
		}
		if (localTime >= nextTrailDamageUpdate)
		{
			nextTrailDamageUpdate += damageUpdateInterval;
			DoDamage();
		}
		if (pointsList.Count > 0)
		{
			TrailPoint value = pointsList[pointsList.Count - 1];
			value.position = transform.position;
			value.localEndTime = localTime + pointLifetime;
			pointsList[pointsList.Count - 1] = value;
			if (Object.op_Implicit((Object)(object)value.segmentTransform))
			{
				value.segmentTransform.position = transform.position;
			}
			if (Object.op_Implicit((Object)(object)lineRenderer))
			{
				lineRenderer.SetPosition(pointsList.Count - 1, value.position);
			}
		}
		if (Object.op_Implicit((Object)(object)segmentPrefab))
		{
			Vector3 position = transform.position;
			Vector3 localScale = default(Vector3);
			for (int num = pointsList.Count - 1; num >= 0; num--)
			{
				Transform segmentTransform = pointsList[num].segmentTransform;
				segmentTransform.LookAt(position, Vector3.up);
				Vector3 val = pointsList[num].position - position;
				segmentTransform.position = position + val * 0.5f;
				float num2 = Mathf.Clamp01(Mathf.InverseLerp(pointsList[num].localStartTime, pointsList[num].localEndTime, localTime));
				((Vector3)(ref localScale))._002Ector(radius * (1f - num2), radius * (1f - num2), ((Vector3)(ref val)).magnitude);
				segmentTransform.localScale = localScale;
				position = pointsList[num].position;
			}
		}
	}

	private void UpdateTrail(bool addPoint)
	{
		while (pointsList.Count > 0 && pointsList[0].localEndTime <= localTime)
		{
			RemovePoint(0);
		}
		if (addPoint)
		{
			AddPoint();
		}
		if (Object.op_Implicit((Object)(object)lineRenderer))
		{
			UpdateLineRenderer(lineRenderer);
		}
	}

	private void DoDamage()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || pointsList.Count == 0)
		{
			return;
		}
		Vector3 val = pointsList[pointsList.Count - 1].position;
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		TeamIndex attackerTeamIndex = TeamIndex.Neutral;
		float damage = damagePerSecond * damageUpdateInterval;
		if (Object.op_Implicit((Object)(object)owner))
		{
			hashSet.Add(owner);
			attackerTeamIndex = TeamComponent.GetObjectTeam(owner);
		}
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.attacker = owner;
		damageInfo.inflictor = ((Component)this).gameObject;
		damageInfo.crit = false;
		damageInfo.damage = damage;
		damageInfo.damageColorIndex = DamageColorIndex.Item;
		damageInfo.damageType = DamageType.Generic;
		damageInfo.force = Vector3.zero;
		damageInfo.procCoefficient = 0f;
		Vector3 val2 = default(Vector3);
		for (int num = pointsList.Count - 2; num >= 0; num--)
		{
			Vector3 position = pointsList[num].position;
			Vector3 forward = position - val;
			((Vector3)(ref val2))._002Ector(radius, height, ((Vector3)(ref forward)).magnitude);
			Vector3 val3 = Vector3.Lerp(position, val, 0.5f);
			Quaternion val4 = Util.QuaternionSafeLookRotation(forward);
			Collider[] array = Physics.OverlapBox(val3, val2, val4, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)0);
			for (int i = 0; i < array.Length; i++)
			{
				HurtBox component = ((Component)array[i]).GetComponent<HurtBox>();
				if (!Object.op_Implicit((Object)(object)component))
				{
					continue;
				}
				HealthComponent healthComponent = component.healthComponent;
				if (!Object.op_Implicit((Object)(object)healthComponent))
				{
					continue;
				}
				GameObject gameObject = ((Component)healthComponent).gameObject;
				if (!hashSet.Contains(gameObject))
				{
					hashSet.Add(gameObject);
					if (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, attackerTeamIndex))
					{
						damageInfo.position = ((Component)array[i]).transform.position;
						healthComponent.TakeDamage(damageInfo);
					}
				}
			}
			val = position;
		}
	}

	private void UpdateLineRenderer(LineRenderer lineRenderer)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		lineRenderer.positionCount = pointsList.Count;
		for (int i = 0; i < pointsList.Count; i++)
		{
			lineRenderer.SetPosition(i, pointsList[i].position);
		}
	}

	private void AddPoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		TrailPoint trailPoint = default(TrailPoint);
		trailPoint.position = transform.position;
		trailPoint.localStartTime = localTime;
		trailPoint.localEndTime = localTime + pointLifetime;
		TrailPoint item = trailPoint;
		if (Object.op_Implicit((Object)(object)segmentPrefab))
		{
			item.segmentTransform = Object.Instantiate<GameObject>(segmentPrefab, transform).transform;
		}
		pointsList.Add(item);
	}

	private void RemovePoint(int pointIndex)
	{
		if (destroyTrailSegments && Object.op_Implicit((Object)(object)pointsList[pointIndex].segmentTransform))
		{
			Object.Destroy((Object)(object)((Component)pointsList[pointIndex].segmentTransform).gameObject);
		}
		pointsList.RemoveAt(pointIndex);
	}

	private void OnDrawGizmos()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pointsList[pointsList.Count - 1].position;
		Vector3 val2 = default(Vector3);
		for (int num = pointsList.Count - 2; num >= 0; num--)
		{
			Vector3 position = pointsList[num].position;
			Vector3 forward = position - val;
			((Vector3)(ref val2))._002Ector(radius, 0.5f, ((Vector3)(ref forward)).magnitude);
			Vector3 val3 = Vector3.Lerp(position, val, 0.5f);
			Quaternion val4 = Util.QuaternionSafeLookRotation(forward);
			Gizmos.matrix = Matrix4x4.TRS(val3, val4, val2);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = Matrix4x4.identity;
			val = position;
		}
	}
}
