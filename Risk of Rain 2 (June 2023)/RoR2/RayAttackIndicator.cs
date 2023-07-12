using System;
using System.Collections.Generic;
using HG;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RoR2;

public class RayAttackIndicator : MonoBehaviour
{
	private struct HitIndicatorInfo
	{
		public GameObject gameObject;

		public Transform transform;
	}

	private struct RaycastInfo
	{
		public Ray ray;

		public float maxDistance;

		public int hitsStart;

		public int maxHits;
	}

	private struct HitPointIndicatorData
	{
		public Vector3 position;

		public Quaternion rotation;

		public float scale;
	}

	private struct Updater
	{
		private bool running;

		public void ScheduleUpdate()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			if (running)
			{
				return;
			}
			List<RayAttackIndicator> instancesList = InstanceTracker.GetInstancesList<RayAttackIndicator>();
			if (instancesList.Count != 0)
			{
				requestsBuffer = new NativeArray<RaycastInfo>(instancesList.Count, (Allocator)3, (NativeArrayOptions)0);
				raycastCommandBuffer = new NativeArray<RaycastCommand>(requestsBuffer.Length, (Allocator)3, (NativeArrayOptions)0);
				raycastRequestersList = CollectionPool<RayAttackIndicator, List<RayAttackIndicator>>.RentCollection();
				int num = 1;
				resultsBuffer = new NativeArray<RaycastHit>(requestsBuffer.Length * num, (Allocator)3, (NativeArrayOptions)0);
				for (int i = 0; i < instancesList.Count; i++)
				{
					RayAttackIndicator rayAttackIndicator = instancesList[i];
					raycastRequestersList.Add(rayAttackIndicator);
					requestsBuffer[i] = new RaycastInfo
					{
						ray = rayAttackIndicator.attackRay,
						maxDistance = rayAttackIndicator.attackRange,
						hitsStart = i * num,
						maxHits = 1
					};
					raycastCommandBuffer[i] = new RaycastCommand(((Ray)(ref rayAttackIndicator.attackRay)).origin, ((Ray)(ref rayAttackIndicator.attackRay)).direction, rayAttackIndicator.attackRange, LayerMask.op_Implicit(rayAttackIndicator.layerMask), 1);
				}
				raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommandBuffer, resultsBuffer, 4, default(JobHandle));
				running = true;
			}
		}

		public void CompleteUpdate()
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			if (!running)
			{
				return;
			}
			((JobHandle)(ref raycastJobHandle)).Complete();
			for (int i = 0; i < requestsBuffer.Length; i++)
			{
				RaycastInfo raycastInfo = requestsBuffer[i];
				RayAttackIndicator rayAttackIndicator = raycastRequestersList[i];
				if (!Object.op_Implicit((Object)(object)rayAttackIndicator))
				{
					continue;
				}
				List<HitPointIndicatorData> list = CollectionPool<HitPointIndicatorData, List<HitPointIndicatorData>>.RentCollection();
				try
				{
					Vector3 up = ((Component)rayAttackIndicator).transform.up;
					Ray ray = raycastInfo.ray;
					Vector3 origin = ((Ray)(ref ray)).origin;
					Vector3 direction = ((Ray)(ref ray)).direction;
					if (Object.op_Implicit((Object)(object)rayAttackIndicator.originRecipient))
					{
						rayAttackIndicator.originRecipient.SetPositionAndRotation(origin, Quaternion.LookRotation(direction, rayAttackIndicator.originRecipient.up));
					}
					float num = raycastInfo.maxDistance;
					float num2 = 0f;
					for (int j = 0; j < raycastInfo.maxHits; j++)
					{
						RaycastHit val = resultsBuffer[raycastInfo.hitsStart + j];
						bool num3 = ((RaycastHit)(ref val)).collider != null;
						float num4 = (num3 ? ((RaycastHit)(ref val)).distance : raycastInfo.maxDistance);
						num = ((num4 < num) ? num4 : num);
						num2 = ((num4 > num2) ? num4 : num2);
						Vector3 position = origin + direction * num4;
						if (num3)
						{
							list.Add(new HitPointIndicatorData
							{
								position = position,
								rotation = Quaternion.LookRotation(-direction, up),
								scale = rayAttackIndicator.attackRadius
							});
						}
					}
					rayAttackIndicator.SetHits(list);
					if (Object.op_Implicit((Object)(object)rayAttackIndicator.nearestHitRecipient))
					{
						rayAttackIndicator.nearestHitRecipient.SetPositionAndRotation(origin + direction * num, Quaternion.LookRotation(-direction, rayAttackIndicator.nearestHitRecipient.up));
					}
					if (Object.op_Implicit((Object)(object)rayAttackIndicator.furthestHitRecipient))
					{
						rayAttackIndicator.furthestHitRecipient.SetPositionAndRotation(origin + direction * num2, Quaternion.LookRotation(-direction, rayAttackIndicator.furthestHitRecipient.up));
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					((Behaviour)rayAttackIndicator).enabled = false;
				}
				finally
				{
					list = CollectionPool<HitPointIndicatorData, List<HitPointIndicatorData>>.ReturnCollection(list);
				}
			}
			resultsBuffer.Dispose();
			requestsBuffer.Dispose();
			raycastCommandBuffer.Dispose();
			raycastRequestersList = CollectionPool<RayAttackIndicator, List<RayAttackIndicator>>.ReturnCollection(raycastRequestersList);
			running = false;
		}
	}

	[Tooltip("The prefab that will be instantiated and moved to wherever the ray hits.")]
	public GameObject hitIndicatorPrefab;

	[Tooltip("Transform which will be moved to the start of the ray.")]
	public Transform originRecipient;

	[Tooltip("Transform which will be moved to the nearest hit.")]
	public Transform nearestHitRecipient;

	[Tooltip("Transform which will be moved to the furthest hit.")]
	public Transform furthestHitRecipient;

	[Range(1f, 1f)]
	[Tooltip("How many hits this raycast is allowed to make. This is currently limited to 1.")]
	public int maxHits = 1;

	private static readonly Ray defaultRay = new Ray(Vector3.zero, Vector3.up);

	[NonSerialized]
	public Ray attackRay = defaultRay;

	[NonSerialized]
	public float attackRange = float.PositiveInfinity;

	[NonSerialized]
	public float attackRadius = 1f;

	[NonSerialized]
	public LayerMask layerMask;

	private List<HitIndicatorInfo> hitIndicators = new List<HitIndicatorInfo>();

	private static bool raysDirty = false;

	private static List<RayAttackIndicator> raycastRequestersList;

	private static NativeArray<RaycastCommand> raycastCommandBuffer;

	private static NativeArray<RaycastHit> resultsBuffer;

	private static NativeArray<RaycastInfo> requestsBuffer;

	private static JobHandle raycastJobHandle;

	private static Updater updater;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		layerMask = LayerIndex.CommonMasks.bullet;
	}

	private void OnEnable()
	{
		InstanceTracker.Add(this);
	}

	private void OnDisable()
	{
		AllocateHitIndicators(0);
		InstanceTracker.Remove(this);
	}

	private void AllocateHitIndicators(int newHitIndicatorCount)
	{
		while (newHitIndicatorCount < hitIndicators.Count)
		{
			Object.Destroy((Object)(object)ListUtils.TakeLast<HitIndicatorInfo>(hitIndicators).gameObject);
		}
		while (newHitIndicatorCount > hitIndicators.Count)
		{
			GameObject val = Object.Instantiate<GameObject>(hitIndicatorPrefab);
			val.SetActive(true);
			hitIndicators.Add(new HitIndicatorInfo
			{
				gameObject = val,
				transform = val.transform
			});
		}
	}

	private void SetHits(List<HitPointIndicatorData> hits)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			AllocateHitIndicators(hits.Count);
			for (int i = 0; i < hits.Count; i++)
			{
				HitIndicatorInfo hitIndicatorInfo = hitIndicators[i];
				HitPointIndicatorData hitPointIndicatorData = hits[i];
				hitIndicatorInfo.transform.SetPositionAndRotation(hitPointIndicatorData.position, hitPointIndicatorData.rotation);
				hitIndicatorInfo.transform.localScale = new Vector3(hitPointIndicatorData.scale, hitPointIndicatorData.scale, hitPointIndicatorData.scale);
			}
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		RoR2Application.onUpdate += StaticUpdate;
		RoR2Application.onLateUpdate += StaticLateUpdate;
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
	}

	private static void StaticUpdate()
	{
		if (raysDirty)
		{
			raysDirty = false;
			updater.ScheduleUpdate();
		}
	}

	private static void StaticLateUpdate()
	{
		updater.CompleteUpdate();
	}

	private static void StaticFixedUpdate()
	{
		raysDirty = true;
	}
}
