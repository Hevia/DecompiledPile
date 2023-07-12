using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public sealed class NetworkedBodyAttachment : NetworkBehaviour
{
	[SyncVar(hook = "OnSyncAttachedBodyObject")]
	private GameObject _attachedBodyObject;

	[SyncVar(hook = "OnSyncAttachedBodyChildName")]
	private string attachedBodyChildName;

	public bool shouldParentToAttachedBody = true;

	public bool forceHostAuthority;

	private NetworkIdentity networkIdentity;

	private CharacterBody attachmentBody;

	private bool attached;

	private NetworkInstanceId ____attachedBodyObjectNetId;

	public GameObject attachedBodyObject => _attachedBodyObject;

	public CharacterBody attachedBody { get; private set; }

	public bool hasEffectiveAuthority { get; private set; }

	public GameObject Network_attachedBodyObject
	{
		get
		{
			return _attachedBodyObject;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncAttachedBodyObject(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref _attachedBodyObject, 1u, ref ____attachedBodyObjectNetId);
		}
	}

	public string NetworkattachedBodyChildName
	{
		get
		{
			return attachedBodyChildName;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncAttachedBodyChildName(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<string>(value, ref attachedBodyChildName, 2u);
		}
	}

	private void OnSyncAttachedBodyObject(GameObject value)
	{
		if (!NetworkServer.active)
		{
			Network_attachedBodyObject = value;
			OnAttachedBodyObjectAssigned();
		}
	}

	private void OnSyncAttachedBodyChildName(string newName)
	{
		NetworkattachedBodyChildName = newName;
		if (shouldParentToAttachedBody)
		{
			ParentToBody();
		}
	}

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		attachmentBody = ((Component)this).GetComponent<CharacterBody>();
	}

	[Server]
	public void AttachToGameObjectAndSpawn([NotNull] GameObject newAttachedBodyObject, string attachedBodyChildName = null)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkedBodyAttachment::AttachToGameObjectAndSpawn(UnityEngine.GameObject,System.String)' called on client");
		}
		else if (attached)
		{
			Debug.LogErrorFormat("Can't attach object '{0}' to object '{1}', it's already been assigned to object '{2}'.", new object[3]
			{
				((Component)this).gameObject,
				newAttachedBodyObject,
				attachedBodyObject
			});
		}
		else if (Object.op_Implicit((Object)(object)newAttachedBodyObject))
		{
			NetworkIdentity component = newAttachedBodyObject.GetComponent<NetworkIdentity>();
			NetworkInstanceId netId = component.netId;
			if (((NetworkInstanceId)(ref netId)).Value == 0)
			{
				Debug.LogWarningFormat("Network Identity for object {0} has a zero netID. Attachment will fail over the network.", new object[1] { newAttachedBodyObject });
			}
			NetworkattachedBodyChildName = attachedBodyChildName;
			Network_attachedBodyObject = newAttachedBodyObject;
			OnAttachedBodyObjectAssigned();
			NetworkConnection val = null;
			val = component.clientAuthorityOwner;
			if (val == null || forceHostAuthority)
			{
				NetworkServer.Spawn(((Component)this).gameObject);
			}
			else
			{
				NetworkServer.SpawnWithClientAuthority(((Component)this).gameObject, val);
			}
		}
	}

	private void OnAttachedBodyObjectAssigned()
	{
		if (attached)
		{
			return;
		}
		attached = true;
		if (Object.op_Implicit((Object)(object)_attachedBodyObject))
		{
			attachedBody = _attachedBodyObject.GetComponent<CharacterBody>();
			if (shouldParentToAttachedBody)
			{
				ParentToBody();
			}
		}
		if (!Object.op_Implicit((Object)(object)attachedBody))
		{
			return;
		}
		List<INetworkedBodyAttachmentListener> list = CollectionPool<INetworkedBodyAttachmentListener, List<INetworkedBodyAttachmentListener>>.RentCollection();
		((Component)this).GetComponents<INetworkedBodyAttachmentListener>(list);
		foreach (INetworkedBodyAttachmentListener item in list)
		{
			item.OnAttachedBodyDiscovered(this, attachedBody);
		}
		list = CollectionPool<INetworkedBodyAttachmentListener, List<INetworkedBodyAttachmentListener>>.ReturnCollection(list);
	}

	private void ParentToBody()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_attachedBodyObject))
		{
			return;
		}
		Transform val = _attachedBodyObject.transform;
		if (!string.IsNullOrEmpty(attachedBodyChildName))
		{
			ModelLocator component = _attachedBodyObject.GetComponent<ModelLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.modelTransform))
			{
				ChildLocator component2 = ((Component)component.modelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					Transform val2 = component2.FindChild(attachedBodyChildName);
					if (Object.op_Implicit((Object)(object)val2))
					{
						val = val2;
					}
				}
			}
		}
		((Component)this).transform.SetParent(val, false);
		((Component)this).transform.localPosition = Vector3.zero;
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		OnSyncAttachedBodyObject(attachedBodyObject);
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)attachedBodyObject) && NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)attachmentBody) && Object.op_Implicit((Object)(object)attachmentBody.healthComponent))
			{
				attachmentBody.healthComponent.Suicide();
			}
			else
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	private void OnValidate()
	{
		if (!((Component)this).GetComponent<NetworkIdentity>().localPlayerAuthority && !forceHostAuthority)
		{
			Debug.LogWarningFormat("NetworkedBodyAttachment: Object {0} NetworkIdentity needs localPlayerAuthority=true", new object[1] { ((Object)((Component)this).gameObject).name });
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<NetworkedBodyAttachment>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<NetworkedBodyAttachment>(this);
	}

	public static void FindBodyAttachments(CharacterBody body, List<NetworkedBodyAttachment> output)
	{
		foreach (NetworkedBodyAttachment instances in InstanceTracker.GetInstancesList<NetworkedBodyAttachment>())
		{
			if (instances.attachedBody == body)
			{
				output.Add(instances);
			}
		}
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
	}

	public override void OnStopAuthority()
	{
		((NetworkBehaviour)this).OnStopAuthority();
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_attachedBodyObject);
			writer.Write(attachedBodyChildName);
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
			writer.Write(_attachedBodyObject);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(attachedBodyChildName);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			____attachedBodyObjectNetId = reader.ReadNetworkId();
			attachedBodyChildName = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncAttachedBodyObject(reader.ReadGameObject());
		}
		if (((uint)num & 2u) != 0)
		{
			OnSyncAttachedBodyChildName(reader.ReadString());
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ____attachedBodyObjectNetId)).IsEmpty())
		{
			Network_attachedBodyObject = ClientScene.FindLocalObject(____attachedBodyObjectNetId);
		}
	}
}
