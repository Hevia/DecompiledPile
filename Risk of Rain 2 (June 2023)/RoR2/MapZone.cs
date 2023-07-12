using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class MapZone : MonoBehaviour
{
	public enum TriggerType
	{
		TriggerExit,
		TriggerEnter
	}

	public enum ZoneType
	{
		OutOfBounds,
		KickOutPlayers
	}

	private struct CollisionInfo : IEquatable<CollisionInfo>
	{
		public readonly MapZone mapZone;

		public readonly Collider otherCollider;

		public CollisionInfo(MapZone mapZone, Collider otherCollider)
		{
			this.mapZone = mapZone;
			this.otherCollider = otherCollider;
		}

		public bool Equals(CollisionInfo other)
		{
			if (mapZone == other.mapZone)
			{
				return otherCollider == other.otherCollider;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is CollisionInfo)
			{
				return Equals((CollisionInfo)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((object)otherCollider).GetHashCode();
		}
	}

	public TriggerType triggerType;

	public ZoneType zoneType;

	public Transform explicitDestination;

	public GameObject explicitSpawnEffectPrefab;

	public float destinationIdealRadius;

	private Collider collider;

	private readonly List<CollisionInfo> queuedCollisions = new List<CollisionInfo>();

	private static readonly Queue<Collider> collidersToCheckInFixedUpdate;

	public event Action<CharacterBody> onBodyTeleport;

	static MapZone()
	{
		collidersToCheckInFixedUpdate = new Queue<Collider>();
		VehicleSeat.onPassengerExitGlobal += CheckCharacterOnVehicleExit;
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
	}

	private static bool TestColliders(Collider characterCollider, Collider triggerCollider)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		float num = default(float);
		return Physics.ComputePenetration(characterCollider, ((Component)characterCollider).transform.position, ((Component)characterCollider).transform.rotation, triggerCollider, ((Component)triggerCollider).transform.position, ((Component)triggerCollider).transform.rotation, ref val, ref num);
	}

	private void Awake()
	{
		collider = ((Component)this).GetComponent<Collider>();
	}

	public void OnTriggerEnter(Collider other)
	{
		if (triggerType == TriggerType.TriggerEnter)
		{
			TryZoneStart(other);
		}
		else
		{
			TryZoneEnd(other);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (triggerType == TriggerType.TriggerExit)
		{
			TryZoneStart(other);
		}
		else
		{
			TryZoneEnd(other);
		}
	}

	private void TryZoneStart(Collider other)
	{
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)component.currentVehicle))
		{
			queuedCollisions.Add(new CollisionInfo(this, other));
			return;
		}
		TeamComponent teamComponent = component.teamComponent;
		switch (zoneType)
		{
		case ZoneType.OutOfBounds:
		{
			bool flag = false;
			if (Object.op_Implicit((Object)(object)component.inventory))
			{
				flag = component.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0 || component.inventory.GetItemCount(RoR2Content.Items.TeleportWhenOob) > 0;
			}
			if (teamComponent.teamIndex != TeamIndex.Player && !flag)
			{
				if (!NetworkServer.active || Physics.GetIgnoreLayerCollision(((Component)this).gameObject.layer, ((Component)other).gameObject.layer))
				{
					break;
				}
				Debug.LogFormat("Killing {0} for being out of bounds.", new object[1] { ((Component)component).gameObject });
				HealthComponent healthComponent = component.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					if (Object.op_Implicit((Object)(object)component.master))
					{
						component.master.TrueKill(healthComponent.lastHitAttacker, ((Component)this).gameObject, DamageType.OutOfBounds);
					}
					else
					{
						healthComponent.Suicide(healthComponent.lastHitAttacker, ((Component)this).gameObject, DamageType.OutOfBounds);
					}
				}
				else if (Object.op_Implicit((Object)(object)component.master))
				{
					component.master.TrueKill(null, null, DamageType.OutOfBounds);
				}
			}
			else
			{
				TeleportBody(component);
			}
			break;
		}
		case ZoneType.KickOutPlayers:
			if (teamComponent.teamIndex == TeamIndex.Player)
			{
				TeleportBody(component);
			}
			break;
		}
	}

	private void TryZoneEnd(Collider other)
	{
		if (queuedCollisions.Count != 0 && queuedCollisions.Contains(new CollisionInfo(this, other)))
		{
			queuedCollisions.Remove(new CollisionInfo(this, other));
		}
	}

	private void ProcessQueuedCollisionsForCollider(Collider collider)
	{
		for (int num = queuedCollisions.Count - 1; num >= 0; num--)
		{
			if (queuedCollisions[num].otherCollider == collider)
			{
				queuedCollisions.RemoveAt(num);
				TryZoneStart(collider);
			}
		}
	}

	private static void StaticFixedUpdate()
	{
		int i = 0;
		for (int count = collidersToCheckInFixedUpdate.Count; i < count; i++)
		{
			Collider val = collidersToCheckInFixedUpdate.Dequeue();
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			foreach (MapZone instances in InstanceTracker.GetInstancesList<MapZone>())
			{
				instances.ProcessQueuedCollisionsForCollider(val);
			}
		}
	}

	private static void CheckCharacterOnVehicleExit(VehicleSeat vehicleSeat, GameObject passengerObject)
	{
		Collider component = passengerObject.GetComponent<Collider>();
		collidersToCheckInFixedUpdate.Enqueue(component);
	}

	private void OnEnable()
	{
		InstanceTracker.Add(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove(this);
	}

	private void TeleportBody(CharacterBody characterBody)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (Util.HasEffectiveAuthority(((Component)characterBody).gameObject) && !Physics.GetIgnoreLayerCollision(((Component)this).gameObject.layer, ((Component)characterBody).gameObject.layer))
		{
			Vector3 val = Run.instance.FindSafeTeleportPosition(characterBody, explicitDestination, 0f, destinationIdealRadius);
			Debug.Log((object)"tp back");
			TeleportHelper.TeleportBody(characterBody, val);
			GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(((Component)characterBody).gameObject);
			if (Object.op_Implicit((Object)(object)explicitSpawnEffectPrefab))
			{
				teleportEffectPrefab = explicitSpawnEffectPrefab;
			}
			if (Object.op_Implicit((Object)(object)teleportEffectPrefab))
			{
				EffectManager.SimpleEffect(teleportEffectPrefab, val, Quaternion.identity, transmit: true);
			}
			this.onBodyTeleport?.Invoke(characterBody);
		}
	}
}
