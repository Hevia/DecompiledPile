using System;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class NetworkParent : NetworkBehaviour
{
	[Serializable]
	private struct ParentIdentifier : IEquatable<ParentIdentifier>
	{
		public byte indexInParentChildLocatorPlusOne;

		public NetworkInstanceId parentNetworkInstanceId;

		public int indexInParentChildLocator
		{
			get
			{
				return indexInParentChildLocatorPlusOne - 1;
			}
			set
			{
				indexInParentChildLocatorPlusOne = (byte)(value + 1);
			}
		}

		private static ChildLocator LookUpChildLocator(Transform rootObject)
		{
			ModelLocator component = ((Component)rootObject).GetComponent<ModelLocator>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				return null;
			}
			Transform modelTransform = component.modelTransform;
			if (!Object.op_Implicit((Object)(object)modelTransform))
			{
				return null;
			}
			return ((Component)modelTransform).GetComponent<ChildLocator>();
		}

		public ParentIdentifier(Transform parent)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			parentNetworkInstanceId = NetworkInstanceId.Invalid;
			indexInParentChildLocatorPlusOne = 0;
			if (!Object.op_Implicit((Object)(object)parent))
			{
				return;
			}
			NetworkIdentity componentInParent = ((Component)parent).GetComponentInParent<NetworkIdentity>();
			if (!Object.op_Implicit((Object)(object)componentInParent))
			{
				Debug.LogWarningFormat("NetworkParent cannot accept a non-null parent without a NetworkIdentity! parent={0}", new object[1] { parent });
				return;
			}
			parentNetworkInstanceId = componentInParent.netId;
			if (((Component)componentInParent).gameObject == ((Component)parent).gameObject)
			{
				return;
			}
			ChildLocator childLocator = LookUpChildLocator(((Component)componentInParent).transform);
			if (!Object.op_Implicit((Object)(object)childLocator))
			{
				Debug.LogWarningFormat("NetworkParent can only be parented directly to another object with a NetworkIdentity or an object registered in the ChildLocator of a a model of an object with a NetworkIdentity. parent={0}", new object[1] { parent });
				return;
			}
			indexInParentChildLocator = childLocator.FindChildIndex(parent);
			if (indexInParentChildLocatorPlusOne == 0)
			{
				Debug.LogWarningFormat("NetworkParent parent={0} is not registered in a ChildLocator.", new object[1] { parent });
			}
		}

		public bool Equals(ParentIdentifier other)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (indexInParentChildLocatorPlusOne == other.indexInParentChildLocatorPlusOne)
			{
				return ((NetworkInstanceId)(ref parentNetworkInstanceId)).Equals(other.parentNetworkInstanceId);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is ParentIdentifier other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (indexInParentChildLocatorPlusOne.GetHashCode() * 397) ^ ((object)(NetworkInstanceId)(ref parentNetworkInstanceId)).GetHashCode();
		}

		public Transform Resolve()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = Util.FindNetworkObject(parentNetworkInstanceId);
			NetworkIdentity val2 = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<NetworkIdentity>() : null);
			if (!Object.op_Implicit((Object)(object)val2))
			{
				return null;
			}
			if (indexInParentChildLocatorPlusOne == 0)
			{
				return ((Component)val2).transform;
			}
			ChildLocator childLocator = LookUpChildLocator(((Component)val2).transform);
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				return childLocator.FindChild(indexInParentChildLocator);
			}
			return null;
		}
	}

	private Transform cachedServerParentTransform;

	private Transform transform;

	[SyncVar(hook = "SetParentIdentifier")]
	private ParentIdentifier parentIdentifier;

	public ParentIdentifier NetworkparentIdentifier
	{
		get
		{
			return parentIdentifier;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetParentIdentifier(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<ParentIdentifier>(value, ref parentIdentifier, 1u);
		}
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	public override void OnStartServer()
	{
		ServerUpdateParent();
	}

	private void OnTransformParentChanged()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			ServerUpdateParent();
		}
		if (Object.op_Implicit((Object)(object)transform.parent))
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}
	}

	[Server]
	private void ServerUpdateParent()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkParent::ServerUpdateParent()' called on client");
			return;
		}
		Transform val = transform.parent;
		if (val != cachedServerParentTransform)
		{
			if (!Object.op_Implicit((Object)(object)val))
			{
				val = null;
			}
			cachedServerParentTransform = val;
			SetParentIdentifier(new ParentIdentifier(val));
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		SetParentIdentifier(parentIdentifier);
	}

	private void SetParentIdentifier(ParentIdentifier newParentIdentifier)
	{
		NetworkparentIdentifier = newParentIdentifier;
		if (!NetworkServer.active)
		{
			transform.parent = parentIdentifier.Resolve();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteParentIdentifier_NetworkParent(writer, parentIdentifier);
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
			GeneratedNetworkCode._WriteParentIdentifier_NetworkParent(writer, parentIdentifier);
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
			parentIdentifier = GeneratedNetworkCode._ReadParentIdentifier_NetworkParent(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetParentIdentifier(GeneratedNetworkCode._ReadParentIdentifier_NetworkParent(reader));
		}
	}

	public override void PreStartClient()
	{
	}
}
