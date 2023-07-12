using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class PulseController : NetworkBehaviour
{
	public delegate void PerformSearchDelegate(PulseController pulseController, Vector3 origin, float radius, List<PulseSearchResult> dest);

	public delegate void OnPulseHitDelegate(PulseController pulseController, PulseHit hitInfo);

	public struct PulseSearchResult
	{
		public Object hitObject;

		public Vector3 hitPos;
	}

	public struct PulseHit
	{
		public Object hitObject;

		public Vector3 hitPos;

		public Vector3 pulseOrigin;

		public float hitDistance;

		public float hitSeverity;
	}

	[Tooltip("How far the pulse can ultimately reach.")]
	[SyncVar]
	public float finalRadius;

	[SyncVar]
	[Tooltip("How long it takes for the pulse to complete.")]
	public float duration;

	[Tooltip("The curve by which normalized time will map to normalized radius.")]
	public AnimationCurveAsset normalizedRadiusCurve;

	[Tooltip("An object which will be enabled and scaled across the duration of the pulse.")]
	public Transform effectTransform;

	[Tooltip("Fires off the normalized time, useful for updating any VFX.")]
	public UnityEventFloat updateVfx;

	[Tooltip("Fired when the pulse is over.")]
	public UnityEvent onPulseEndServer;

	[SyncVar]
	private Run.FixedTimeStamp startTime;

	private List<Object> hitObjects;

	private float previousVfxNormalizedTime = float.NaN;

	public float NetworkfinalRadius
	{
		get
		{
			return finalRadius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref finalRadius, 1u);
		}
	}

	public float Networkduration
	{
		get
		{
			return duration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref duration, 2u);
		}
	}

	public Run.FixedTimeStamp NetworkstartTime
	{
		get
		{
			return startTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<Run.FixedTimeStamp>(value, ref startTime, 4u);
		}
	}

	public event PerformSearchDelegate performSearch;

	public event OnPulseHitDelegate onPulseHit;

	private void OnEnable()
	{
		if (NetworkServer.active)
		{
			NetworkstartTime = Run.FixedTimeStamp.positiveInfinity;
			hitObjects = CollectionPool<Object, List<Object>>.RentCollection();
		}
	}

	private void OnDisable()
	{
		if (NetworkServer.active)
		{
			NetworkstartTime = Run.FixedTimeStamp.positiveInfinity;
			hitObjects = CollectionPool<Object, List<Object>>.ReturnCollection(hitObjects);
		}
	}

	private void FixedUpdate()
	{
		float num = Mathf.Clamp01((Run.FixedTimeStamp.now - startTime) / duration);
		if (normalizedRadiusCurve?.value != null)
		{
			num *= normalizedRadiusCurve.value.Evaluate(num);
		}
		float radius = CalcRadius(num);
		if (NetworkServer.active)
		{
			StepPulse(radius);
		}
		if (num == 1f && NetworkServer.active)
		{
			NetworkstartTime = Run.FixedTimeStamp.positiveInfinity;
			UnityEvent obj = onPulseEndServer;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	private void Update()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01((Run.TimeStamp.now - startTime) / duration);
		if (normalizedRadiusCurve?.value != null)
		{
			num *= normalizedRadiusCurve.value.Evaluate(num);
		}
		float num2 = CalcRadius(num);
		if (Object.op_Implicit((Object)(object)effectTransform))
		{
			bool flag = num > 0f && num < 1f;
			((Component)effectTransform).gameObject.SetActive(flag);
			if (flag)
			{
				effectTransform.localScale = new Vector3(num2, num2, num2);
			}
		}
		if (previousVfxNormalizedTime != num)
		{
			previousVfxNormalizedTime = num;
			((UnityEvent<float>)(object)updateVfx)?.Invoke(num);
		}
	}

	[Server]
	public void StartPulseServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.PulseController::StartPulseServer()' called on client");
		}
		else if (((Behaviour)this).enabled)
		{
			NetworkstartTime = Run.FixedTimeStamp.now;
			hitObjects.Clear();
		}
	}

	private float CalcRadius(float normalizedTime)
	{
		return normalizedTime * finalRadius;
	}

	private void StepPulse(float radius)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		List<PulseSearchResult> list = CollectionPool<PulseSearchResult, List<PulseSearchResult>>.RentCollection();
		Vector3 position = ((Component)this).transform.position;
		try
		{
			this.performSearch?.Invoke(this, position, radius, list);
			for (int i = 0; i < list.Count; i++)
			{
				PulseSearchResult pulseSearchResult = list[i];
				if (hitObjects.Contains(pulseSearchResult.hitObject))
				{
					continue;
				}
				try
				{
					float num = Vector3.Distance(position, pulseSearchResult.hitPos);
					this.onPulseHit?.Invoke(this, new PulseHit
					{
						hitObject = pulseSearchResult.hitObject,
						hitPos = pulseSearchResult.hitPos,
						pulseOrigin = position,
						hitDistance = num,
						hitSeverity = Mathf.Clamp01(1f - num / finalRadius)
					});
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
				finally
				{
					hitObjects.Add(pulseSearchResult.hitObject);
				}
			}
		}
		finally
		{
			CollectionPool<PulseSearchResult, List<PulseSearchResult>>.RentCollection();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(finalRadius);
			writer.Write(duration);
			GeneratedNetworkCode._WriteFixedTimeStamp_Run(writer, startTime);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(finalRadius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(duration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteFixedTimeStamp_Run(writer, startTime);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			finalRadius = reader.ReadSingle();
			duration = reader.ReadSingle();
			startTime = GeneratedNetworkCode._ReadFixedTimeStamp_Run(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			finalRadius = reader.ReadSingle();
		}
		if (((uint)num & 2u) != 0)
		{
			duration = reader.ReadSingle();
		}
		if (((uint)num & 4u) != 0)
		{
			startTime = GeneratedNetworkCode._ReadFixedTimeStamp_Run(reader);
		}
	}

	public override void PreStartClient()
	{
	}
}
