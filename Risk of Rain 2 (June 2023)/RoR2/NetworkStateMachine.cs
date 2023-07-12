using System;
using System.Collections.Generic;
using EntityStates;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DisallowMultipleComponent]
public class NetworkStateMachine : NetworkBehaviour
{
	[SerializeField]
	[Tooltip("The sibling state machine components to network.")]
	private EntityStateMachine[] stateMachines = Array.Empty<EntityStateMachine>();

	private NetworkIdentity networkIdentity;

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		for (int i = 0; i < stateMachines.Length; i++)
		{
			EntityStateMachine obj = stateMachines[i];
			obj.networkIndex = i;
			obj.networker = this;
			obj.networkIdentity = networkIdentity;
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < stateMachines.Length; i++)
			{
				EntityStateMachine entityStateMachine = stateMachines[i];
				writer.Write(EntityStateCatalog.GetStateIndex(entityStateMachine.state.GetType()));
				entityStateMachine.state.OnSerialize(writer);
			}
			return true;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (!initialState)
		{
			return;
		}
		for (int i = 0; i < stateMachines.Length; i++)
		{
			EntityStateMachine entityStateMachine = stateMachines[i];
			EntityStateIndex entityStateIndex = reader.ReadEntityStateIndex();
			if (((NetworkBehaviour)this).hasAuthority)
			{
				continue;
			}
			EntityState entityState = EntityStateCatalog.InstantiateState(entityStateIndex);
			if (entityState != null)
			{
				entityState.outer = entityStateMachine;
				entityState.OnDeserialize(reader);
				if (!Object.op_Implicit((Object)(object)stateMachines[i]))
				{
					Debug.LogErrorFormat("State machine [{0}] on object {1} is not set! incoming state = {2}", new object[3]
					{
						i,
						((Component)this).gameObject,
						entityState.GetType()
					});
				}
				entityStateMachine.SetNextState(entityState);
			}
		}
	}

	[NetworkMessageHandler(msgType = 48, client = true, server = true)]
	public static void HandleSetEntityState(NetworkMessage netMsg)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		NetworkIdentity val = netMsg.reader.ReadNetworkIdentity();
		byte b = netMsg.reader.ReadByte();
		EntityStateIndex entityStateIndex = netMsg.reader.ReadEntityStateIndex();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		NetworkStateMachine component = ((Component)val).gameObject.GetComponent<NetworkStateMachine>();
		if ((Object)(object)component == (Object)null || b >= component.stateMachines.Length)
		{
			return;
		}
		EntityStateMachine entityStateMachine = component.stateMachines[b];
		if ((Object)(object)entityStateMachine == (Object)null)
		{
			return;
		}
		if (val.isServer)
		{
			HashSet<NetworkInstanceId> clientOwnedObjects = netMsg.conn.clientOwnedObjects;
			if (clientOwnedObjects == null || !clientOwnedObjects.Contains(val.netId))
			{
				return;
			}
		}
		else if (val.hasAuthority)
		{
			return;
		}
		EntityState entityState = EntityStateCatalog.InstantiateState(entityStateIndex);
		if (entityState != null)
		{
			entityState.outer = entityStateMachine;
			entityState.OnDeserialize(netMsg.reader);
			entityStateMachine.SetState(entityState);
		}
	}

	public void SendSetEntityState(int stateMachineIndex)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (NetworkServer.active || ((NetworkBehaviour)this).hasAuthority)
		{
			NetworkWriter val = new NetworkWriter();
			EntityStateMachine entityStateMachine = stateMachines[stateMachineIndex];
			val.StartMessage((short)48);
			val.Write(networkIdentity);
			val.Write((byte)stateMachineIndex);
			val.Write(EntityStateCatalog.GetStateIndex(entityStateMachine.state.GetType()));
			entityStateMachine.state.OnSerialize(val);
			val.FinishMessage();
			if (NetworkServer.active)
			{
				NetworkServer.SendWriterToReady(((Component)this).gameObject, val, ((NetworkBehaviour)this).GetNetworkChannel());
			}
			else if (ClientScene.readyConnection != null)
			{
				ClientScene.readyConnection.SendWriter(val, ((NetworkBehaviour)this).GetNetworkChannel());
			}
		}
	}

	private void OnValidate()
	{
		for (int i = 0; i < stateMachines.Length; i++)
		{
			if (!Object.op_Implicit((Object)(object)stateMachines[i]))
			{
				Debug.LogErrorFormat("{0} has a blank entry for NetworkStateMachine!", new object[1] { ((Component)this).gameObject });
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
