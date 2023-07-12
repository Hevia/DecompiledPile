using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ChestRevealer : NetworkBehaviour
{
	private struct PendingReveal : IComparable<PendingReveal>
	{
		public GameObject gameObject;

		public Run.FixedTimeStamp time;

		public float duration;

		public int CompareTo(PendingReveal other)
		{
			return time.CompareTo(other.time);
		}
	}

	private class RevealedObject : MonoBehaviour
	{
		private float lifetime;

		private static readonly Dictionary<GameObject, RevealedObject> currentlyRevealedObjects = new Dictionary<GameObject, RevealedObject>();

		private PositionIndicator positionIndicator;

		public static void RevealObject(GameObject gameObject, float duration)
		{
			if (!currentlyRevealedObjects.TryGetValue(gameObject, out var value))
			{
				value = gameObject.AddComponent<RevealedObject>();
			}
			if (value.lifetime < duration)
			{
				value.lifetime = duration;
			}
		}

		private void OnEnable()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/PositionIndicators/PoiPositionIndicator"), ((Component)this).transform.position, ((Component)this).transform.rotation);
			positionIndicator = val.GetComponent<PositionIndicator>();
			positionIndicator.insideViewObject.GetComponent<SpriteRenderer>().sprite = PingIndicator.GetInteractableIcon(((Component)this).gameObject);
			positionIndicator.targetTransform = ((Component)this).transform;
			currentlyRevealedObjects[((Component)this).gameObject] = this;
		}

		private void OnDisable()
		{
			currentlyRevealedObjects.Remove(((Component)this).gameObject);
			if (Object.op_Implicit((Object)(object)positionIndicator))
			{
				Object.Destroy((Object)(object)((Component)positionIndicator).gameObject);
			}
			positionIndicator = null;
		}

		private void FixedUpdate()
		{
			lifetime -= Time.fixedDeltaTime;
			if (lifetime <= 0f)
			{
				Object.Destroy((Object)(object)this);
			}
		}
	}

	[SyncVar]
	public float radius;

	public float pulseTravelSpeed = 10f;

	public float revealDuration = 10f;

	public float pulseInterval = 1f;

	private Run.FixedTimeStamp nextPulse = Run.FixedTimeStamp.negativeInfinity;

	public GameObject pulseEffectPrefab;

	public float pulseEffectScale = 1f;

	private static readonly List<PendingReveal> pendingReveals = new List<PendingReveal>();

	private static Type[] typesToCheck;

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref radius, 1u);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
		typesToCheck = (from t in typeof(ChestRevealer).Assembly.GetTypes()
			where typeof(IInteractable).IsAssignableFrom(t)
			select t).ToArray();
	}

	private static void StaticFixedUpdate()
	{
		pendingReveals.Sort();
		while (pendingReveals.Count > 0)
		{
			PendingReveal pendingReveal = pendingReveals[0];
			if (pendingReveal.time.hasPassed)
			{
				if (Object.op_Implicit((Object)(object)pendingReveal.gameObject))
				{
					RevealedObject.RevealObject(pendingReveal.gameObject, pendingReveal.duration);
				}
				pendingReveals.RemoveAt(0);
				continue;
			}
			break;
		}
	}

	public void Pulse()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Vector3 origin = ((Component)this).transform.position;
		float radiusSqr = radius * radius;
		float invPulseTravelSpeed = 1f / pulseTravelSpeed;
		Type[] array = typesToCheck;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (MonoBehaviour item2 in InstanceTracker.FindInstancesEnumerable(array[i]))
			{
				if (((IInteractable)item2).ShouldShowOnScanner())
				{
					TryAddRevealable(((Component)item2).transform);
				}
			}
		}
		EffectManager.SpawnEffect(pulseEffectPrefab, new EffectData
		{
			origin = origin,
			scale = radius * pulseEffectScale
		}, transmit: false);
		void TryAddRevealable(Transform revealableTransform)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = revealableTransform.position - origin;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (!(sqrMagnitude > radiusSqr))
			{
				float num = Mathf.Sqrt(sqrMagnitude) * invPulseTravelSpeed;
				PendingReveal pendingReveal = default(PendingReveal);
				pendingReveal.gameObject = ((Component)revealableTransform).gameObject;
				pendingReveal.time = Run.FixedTimeStamp.now + num;
				pendingReveal.duration = revealDuration;
				PendingReveal item = pendingReveal;
				pendingReveals.Add(item);
			}
		}
	}

	private void FixedUpdate()
	{
		if (nextPulse.hasPassed)
		{
			Pulse();
			nextPulse = Run.FixedTimeStamp.now + pulseInterval;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(radius);
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
			writer.Write(radius);
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
			radius = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			radius = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
