using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RoR2.UI;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class PingerController : NetworkBehaviour
{
	[Serializable]
	public struct PingInfo : IEquatable<PingInfo>
	{
		public bool active;

		public Vector3 origin;

		public Vector3 normal;

		public NetworkIdentity targetNetworkIdentity;

		public GameObject targetGameObject
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)targetNetworkIdentity))
				{
					return null;
				}
				return ((Component)targetNetworkIdentity).gameObject;
			}
		}

		public bool Equals(PingInfo other)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (active.Equals(other.active) && ((Vector3)(ref origin)).Equals(other.origin) && ((Vector3)(ref normal)).Equals(other.normal))
			{
				return targetNetworkIdentity == other.targetNetworkIdentity;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return (obj as PingInfo?)?.Equals(this) ?? false;
		}

		public override int GetHashCode()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			return (((-1814869148 * -1521134295 + active.GetHashCode()) * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(origin)) * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(normal)) * -1521134295 + EqualityComparer<NetworkIdentity>.Default.GetHashCode(targetNetworkIdentity);
		}
	}

	private int pingStock = 3;

	private float pingRechargeStopwatch;

	private const int maximumPingStock = 3;

	private const float pingRechargeInterval = 1.5f;

	private static readonly PingInfo emptyPing;

	private PingIndicator pingIndicator;

	[SyncVar(hook = "OnSyncCurrentPing")]
	public PingInfo currentPing;

	private static int kCmdCmdPing;

	public PingInfo NetworkcurrentPing
	{
		get
		{
			return currentPing;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncCurrentPing(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<PingInfo>(value, ref currentPing, 1u);
		}
	}

	private void RebuildPing(PingInfo pingInfo)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!pingInfo.active && pingIndicator != null)
		{
			if (Object.op_Implicit((Object)(object)pingIndicator))
			{
				Object.Destroy((Object)(object)((Component)pingIndicator).gameObject);
			}
			pingIndicator = null;
			return;
		}
		if (!Object.op_Implicit((Object)(object)pingIndicator))
		{
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/PingIndicator"));
			pingIndicator = val.GetComponent<PingIndicator>();
			pingIndicator.pingOwner = ((Component)this).gameObject;
		}
		pingIndicator.pingOrigin = pingInfo.origin;
		pingIndicator.pingNormal = pingInfo.normal;
		pingIndicator.pingTarget = pingInfo.targetGameObject;
		pingIndicator.RebuildPing();
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)pingIndicator))
		{
			Object.Destroy((Object)(object)((Component)pingIndicator).gameObject);
		}
	}

	private void OnSyncCurrentPing(PingInfo newPingInfo)
	{
		if (!((NetworkBehaviour)this).hasAuthority)
		{
			SetCurrentPing(newPingInfo);
		}
	}

	private void SetCurrentPing(PingInfo newPingInfo)
	{
		NetworkcurrentPing = newPingInfo;
		RebuildPing(currentPing);
		if (((NetworkBehaviour)this).hasAuthority)
		{
			CallCmdPing(currentPing);
		}
	}

	[Command]
	private void CmdPing(PingInfo incomingPing)
	{
		NetworkcurrentPing = incomingPing;
	}

	private void FixedUpdate()
	{
		if (((NetworkBehaviour)this).hasAuthority)
		{
			pingRechargeStopwatch -= Time.fixedDeltaTime;
			if (pingRechargeStopwatch <= 0f)
			{
				pingStock = Mathf.Min(pingStock + 1, 3);
				pingRechargeStopwatch = 1.5f;
			}
		}
	}

	private static bool GeneratePingInfo(Ray aimRay, GameObject bodyObject, out PingInfo result)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		result = new PingInfo
		{
			active = true,
			origin = Vector3.zero,
			normal = Vector3.zero,
			targetNetworkIdentity = null
		};
		aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, bodyObject, out var extraRaycastDistance);
		float maxDistance = 1000f + extraRaycastDistance;
		if (Util.CharacterRaycast(bodyObject, aimRay, out var hitInfo, maxDistance, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.entityPrecise.mask) | LayerMask.op_Implicit(LayerIndex.world.mask)), (QueryTriggerInteraction)0))
		{
			HurtBox component = ((Component)((RaycastHit)(ref hitInfo)).collider).GetComponent<HurtBox>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.healthComponent))
			{
				CharacterBody body = component.healthComponent.body;
				result.origin = body.corePosition;
				result.normal = Vector3.zero;
				result.targetNetworkIdentity = body.networkIdentity;
				return true;
			}
		}
		if (Util.CharacterRaycast(bodyObject, aimRay, out hitInfo, maxDistance, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask) | LayerMask.op_Implicit(LayerIndex.pickups.mask)), (QueryTriggerInteraction)2))
		{
			GameObject gameObject = ((Component)((RaycastHit)(ref hitInfo)).collider).gameObject;
			NetworkIdentity val = gameObject.GetComponentInParent<NetworkIdentity>();
			if (!Object.op_Implicit((Object)(object)val))
			{
				Transform parent = gameObject.transform.parent;
				EntityLocator entityLocator = (Object.op_Implicit((Object)(object)parent) ? ((Component)parent).GetComponentInChildren<EntityLocator>() : gameObject.GetComponent<EntityLocator>());
				if (Object.op_Implicit((Object)(object)entityLocator))
				{
					gameObject = entityLocator.entity;
					val = gameObject.GetComponent<NetworkIdentity>();
				}
			}
			result.origin = ((RaycastHit)(ref hitInfo)).point;
			result.normal = ((RaycastHit)(ref hitInfo)).normal;
			result.targetNetworkIdentity = val;
			return true;
		}
		return false;
	}

	public void AttemptPing(Ray aimRay, GameObject bodyObject)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (pingStock <= 0)
		{
			Chat.AddMessage(Language.GetString("PLAYER_PING_COOLDOWN"));
			return;
		}
		if (!RoR2Application.isInSinglePlayer)
		{
			pingStock--;
		}
		if (GeneratePingInfo(aimRay, bodyObject, out var result))
		{
			if (result.targetNetworkIdentity != null && result.targetNetworkIdentity == currentPing.targetNetworkIdentity)
			{
				result = emptyPing;
				pingStock++;
			}
			SetCurrentPing(result);
		}
	}

	static PingerController()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kCmdCmdPing = 1170265357;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PingerController), kCmdCmdPing, new CmdDelegate(InvokeCmdCmdPing));
		NetworkCRC.RegisterBehaviour("PingerController", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPing(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdPing called on client.");
		}
		else
		{
			((PingerController)(object)obj).CmdPing(GeneratedNetworkCode._ReadPingInfo_PingerController(reader));
		}
	}

	public void CallCmdPing(PingInfo incomingPing)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdPing called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdPing(incomingPing);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdPing);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WritePingInfo_PingerController(val, incomingPing);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdPing");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WritePingInfo_PingerController(writer, currentPing);
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
			GeneratedNetworkCode._WritePingInfo_PingerController(writer, currentPing);
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
			currentPing = GeneratedNetworkCode._ReadPingInfo_PingerController(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncCurrentPing(GeneratedNetworkCode._ReadPingInfo_PingerController(reader));
		}
	}

	public override void PreStartClient()
	{
	}
}
