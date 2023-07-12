using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class Interactor : NetworkBehaviour
{
	public float maxInteractionDistance = 1f;

	private static int kCmdCmdInteract;

	private static int kRpcRpcInteractionResult;

	public GameObject FindBestInteractableObject(Ray raycastRay, float maxRaycastDistance, Vector3 overlapPosition, float overlapRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		LayerMask interactable = LayerIndex.CommonMasks.interactable;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(raycastRay, ref val, maxRaycastDistance, LayerMask.op_Implicit(interactable), (QueryTriggerInteraction)2))
		{
			GameObject entity = EntityLocator.GetEntity(((Component)((RaycastHit)(ref val)).collider).gameObject);
			if (Object.op_Implicit((Object)(object)entity))
			{
				IInteractable component = entity.GetComponent<IInteractable>();
				if (component != null && ((Behaviour)(MonoBehaviour)component).isActiveAndEnabled && component.GetInteractability(this) != 0)
				{
					return entity;
				}
			}
		}
		Collider[] array = Physics.OverlapSphere(overlapPosition, overlapRadius, LayerMask.op_Implicit(interactable), (QueryTriggerInteraction)2);
		int num = array.Length;
		GameObject result = null;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			Collider val2 = array[i];
			GameObject entity2 = EntityLocator.GetEntity(((Component)val2).gameObject);
			if (!Object.op_Implicit((Object)(object)entity2))
			{
				continue;
			}
			IInteractable component2 = entity2.GetComponent<IInteractable>();
			if (component2 != null && ((Behaviour)(MonoBehaviour)component2).isActiveAndEnabled && component2.GetInteractability(this) != 0 && !component2.ShouldIgnoreSpherecastForInteractibility(this))
			{
				Vector3 val3 = ((Component)val2).transform.position - overlapPosition;
				float num3 = Vector3.Dot(((Vector3)(ref val3)).normalized, ((Ray)(ref raycastRay)).direction);
				if (num3 > num2)
				{
					num2 = num3;
					result = entity2.gameObject;
				}
			}
		}
		return result;
	}

	[Command]
	public void CmdInteract(GameObject interactableObject)
	{
		PerformInteraction(interactableObject);
	}

	[Server]
	private void PerformInteraction(GameObject interactableObject)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Interactor::PerformInteraction(UnityEngine.GameObject)' called on client");
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)interactableObject))
			{
				return;
			}
			bool flag = false;
			bool anyInteractionSucceeded = false;
			IInteractable[] components = interactableObject.GetComponents<IInteractable>();
			foreach (IInteractable interactable in components)
			{
				Interactability interactability = interactable.GetInteractability(this);
				if (interactability == Interactability.Available)
				{
					interactable.OnInteractionBegin(this);
					GlobalEventManager.instance.OnInteractionBegin(this, interactable, interactableObject);
					anyInteractionSucceeded = true;
				}
				flag = flag || interactability != Interactability.Disabled;
			}
			if (flag)
			{
				CallRpcInteractionResult(anyInteractionSucceeded);
			}
		}
	}

	[ClientRpc]
	private void RpcInteractionResult(bool anyInteractionSucceeded)
	{
		if (!anyInteractionSucceeded && CameraRigController.IsObjectSpectatedByAnyCamera(((Component)this).gameObject))
		{
			Util.PlaySound("Play_UI_insufficient_funds", ((Component)RoR2Application.instance).gameObject);
		}
	}

	public void AttemptInteraction(GameObject interactableObject)
	{
		if (NetworkServer.active)
		{
			PerformInteraction(interactableObject);
		}
		else
		{
			CallCmdInteract(interactableObject);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdInteract(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdInteract called on client.");
		}
		else
		{
			((Interactor)(object)obj).CmdInteract(reader.ReadGameObject());
		}
	}

	public void CallCmdInteract(GameObject interactableObject)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdInteract called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdInteract(interactableObject);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdInteract);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(interactableObject);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdInteract");
	}

	protected static void InvokeRpcRpcInteractionResult(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcInteractionResult called on server.");
		}
		else
		{
			((Interactor)(object)obj).RpcInteractionResult(reader.ReadBoolean());
		}
	}

	public void CallRpcInteractionResult(bool anyInteractionSucceeded)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcInteractionResult called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcInteractionResult);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(anyInteractionSucceeded);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcInteractionResult");
	}

	static Interactor()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		kCmdCmdInteract = 591229007;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Interactor), kCmdCmdInteract, new CmdDelegate(InvokeCmdCmdInteract));
		kRpcRpcInteractionResult = 804118976;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Interactor), kRpcRpcInteractionResult, new CmdDelegate(InvokeRpcRpcInteractionResult));
		NetworkCRC.RegisterBehaviour("Interactor", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
