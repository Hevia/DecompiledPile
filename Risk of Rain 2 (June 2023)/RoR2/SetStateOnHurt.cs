using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DisallowMultipleComponent]
public class SetStateOnHurt : NetworkBehaviour, IOnTakeDamageServerReceiver
{
	[Tooltip("The percentage of their max HP they need to take to get stunned. Ranges from 0-1.")]
	public float hitThreshold = 0.1f;

	[Tooltip("The state machine to set the state of when this character is hurt.")]
	public EntityStateMachine targetStateMachine;

	[Tooltip("The state machine to set to idle when this character is hurt.")]
	public EntityStateMachine[] idleStateMachine;

	[Tooltip("The state to enter when this character is hurt.")]
	public SerializableEntityStateType hurtState;

	public bool canBeHitStunned = true;

	public bool canBeStunned = true;

	public bool canBeFrozen = true;

	private bool hasEffectiveAuthority = true;

	private static readonly float stunChanceOnHitBaseChancePercent;

	private static int kRpcRpcSetStun;

	private static int kRpcRpcSetFrozen;

	private static int kRpcRpcSetShock;

	private static int kRpcRpcSetPain;

	private static int kRpcRpcCleanse;

	private bool spawnedOverNetwork => ((NetworkBehaviour)this).isServer;

	public static void SetStunOnObject(GameObject target, float duration)
	{
		SetStateOnHurt component = target.GetComponent<SetStateOnHurt>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetStun(duration);
		}
	}

	public override void OnStartAuthority()
	{
		((NetworkBehaviour)this).OnStartAuthority();
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		((NetworkBehaviour)this).OnStopAuthority();
		UpdateAuthority();
	}

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(((Component)this).gameObject);
	}

	private void Start()
	{
		UpdateAuthority();
	}

	public void OnTakeDamageServer(DamageReport damageReport)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)targetStateMachine) || !spawnedOverNetwork)
		{
			return;
		}
		HealthComponent victim = damageReport.victim;
		DamageInfo damageInfo = damageReport.damageInfo;
		CharacterMaster attackerMaster = damageReport.attackerMaster;
		int num = (Object.op_Implicit((Object)(object)attackerMaster) ? attackerMaster.inventory.GetItemCount(RoR2Content.Items.StunChanceOnHit) : 0);
		if (num > 0 && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(stunChanceOnHitBaseChancePercent * (float)num * damageReport.damageInfo.procCoefficient), attackerMaster))
		{
			EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), damageInfo.position, -damageInfo.force, transmit: true);
			SetStun(2f);
		}
		bool flag = damageInfo.procCoefficient >= Mathf.Epsilon;
		float damageDealt = damageReport.damageDealt;
		if (flag && canBeFrozen && (damageInfo.damageType & DamageType.Freeze2s) != 0)
		{
			SetFrozen(2f * damageInfo.procCoefficient);
		}
		else if (!victim.isInFrozenState)
		{
			if (flag && canBeStunned && (damageInfo.damageType & DamageType.Shock5s) != 0)
			{
				SetShock(5f * damageReport.damageInfo.procCoefficient);
			}
			else if (flag && canBeStunned && (damageInfo.damageType & DamageType.Stun1s) != 0)
			{
				SetStun(1f);
			}
			else if (canBeHitStunned && damageDealt > victim.fullCombinedHealth * hitThreshold)
			{
				SetPain();
			}
		}
	}

	[Server]
	public void SetStun(float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SetStateOnHurt::SetStun(System.Single)' called on client");
		}
		else if (canBeStunned)
		{
			if (hasEffectiveAuthority)
			{
				SetStunInternal(duration);
			}
			else
			{
				CallRpcSetStun(duration);
			}
		}
	}

	[Server]
	public void SetFrozen(float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SetStateOnHurt::SetFrozen(System.Single)' called on client");
		}
		else if (canBeFrozen)
		{
			if (hasEffectiveAuthority)
			{
				SetFrozenInternal(duration);
			}
			else
			{
				CallRpcSetFrozen(duration);
			}
		}
	}

	[Server]
	public void SetShock(float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SetStateOnHurt::SetShock(System.Single)' called on client");
		}
		else if (canBeStunned)
		{
			if (hasEffectiveAuthority)
			{
				SetShockInternal(duration);
			}
			else
			{
				CallRpcSetShock(duration);
			}
		}
	}

	[Server]
	public void SetPain()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SetStateOnHurt::SetPain()' called on client");
		}
		else if (canBeHitStunned)
		{
			if (hasEffectiveAuthority)
			{
				SetPainInternal();
			}
			else
			{
				CallRpcSetPain();
			}
		}
	}

	[Server]
	public void Cleanse()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SetStateOnHurt::Cleanse()' called on client");
		}
		else if (hasEffectiveAuthority)
		{
			CleanseInternal();
		}
		else
		{
			CallRpcCleanse();
		}
	}

	[ClientRpc]
	private void RpcSetStun(float duration)
	{
		if (hasEffectiveAuthority)
		{
			SetStunInternal(duration);
		}
	}

	private void SetStunInternal(float duration)
	{
		if (Object.op_Implicit((Object)(object)targetStateMachine))
		{
			if (targetStateMachine.state is StunState)
			{
				StunState stunState = targetStateMachine.state as StunState;
				if (stunState.timeRemaining < duration)
				{
					stunState.ExtendStun(duration - stunState.timeRemaining);
				}
			}
			else
			{
				StunState stunState2 = new StunState();
				stunState2.stunDuration = duration;
				targetStateMachine.SetInterruptState(stunState2, InterruptPriority.Pain);
			}
		}
		EntityStateMachine[] array = idleStateMachine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetNextStateToMain();
		}
	}

	[ClientRpc]
	private void RpcSetFrozen(float duration)
	{
		if (hasEffectiveAuthority)
		{
			SetFrozenInternal(duration);
		}
	}

	private void SetFrozenInternal(float duration)
	{
		if (Object.op_Implicit((Object)(object)targetStateMachine))
		{
			FrozenState frozenState = new FrozenState();
			frozenState.freezeDuration = duration;
			targetStateMachine.SetInterruptState(frozenState, InterruptPriority.Frozen);
		}
		EntityStateMachine[] array = idleStateMachine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetNextState(new Idle());
		}
	}

	[ClientRpc]
	private void RpcSetShock(float duration)
	{
		if (hasEffectiveAuthority)
		{
			SetShockInternal(duration);
		}
	}

	private void SetShockInternal(float duration)
	{
		if (Object.op_Implicit((Object)(object)targetStateMachine))
		{
			ShockState shockState = new ShockState();
			shockState.shockDuration = duration;
			targetStateMachine.SetInterruptState(shockState, InterruptPriority.Pain);
		}
		EntityStateMachine[] array = idleStateMachine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetNextStateToMain();
		}
	}

	[ClientRpc]
	private void RpcSetPain()
	{
		if (hasEffectiveAuthority)
		{
			SetPainInternal();
		}
	}

	private void SetPainInternal()
	{
		if (Object.op_Implicit((Object)(object)targetStateMachine))
		{
			targetStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(hurtState), InterruptPriority.Pain);
		}
		EntityStateMachine[] array = idleStateMachine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetNextStateToMain();
		}
	}

	[ClientRpc]
	private void RpcCleanse()
	{
		if (hasEffectiveAuthority)
		{
			CleanseInternal();
		}
	}

	private void CleanseInternal()
	{
		if (Object.op_Implicit((Object)(object)targetStateMachine) && (targetStateMachine.state is FrozenState || targetStateMachine.state is StunState || targetStateMachine.state is ShockState))
		{
			targetStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(targetStateMachine.mainStateType), InterruptPriority.Frozen);
		}
	}

	static SetStateOnHurt()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		stunChanceOnHitBaseChancePercent = 5f;
		kRpcRpcSetStun = 788834249;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SetStateOnHurt), kRpcRpcSetStun, new CmdDelegate(InvokeRpcRpcSetStun));
		kRpcRpcSetFrozen = 1781279215;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SetStateOnHurt), kRpcRpcSetFrozen, new CmdDelegate(InvokeRpcRpcSetFrozen));
		kRpcRpcSetShock = -1316305549;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SetStateOnHurt), kRpcRpcSetShock, new CmdDelegate(InvokeRpcRpcSetShock));
		kRpcRpcSetPain = 788726245;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SetStateOnHurt), kRpcRpcSetPain, new CmdDelegate(InvokeRpcRpcSetPain));
		kRpcRpcCleanse = -339360280;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SetStateOnHurt), kRpcRpcCleanse, new CmdDelegate(InvokeRpcRpcCleanse));
		NetworkCRC.RegisterBehaviour("SetStateOnHurt", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcSetStun(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcSetStun called on server.");
		}
		else
		{
			((SetStateOnHurt)(object)obj).RpcSetStun(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcSetFrozen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcSetFrozen called on server.");
		}
		else
		{
			((SetStateOnHurt)(object)obj).RpcSetFrozen(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcSetShock(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcSetShock called on server.");
		}
		else
		{
			((SetStateOnHurt)(object)obj).RpcSetShock(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcSetPain(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcSetPain called on server.");
		}
		else
		{
			((SetStateOnHurt)(object)obj).RpcSetPain();
		}
	}

	protected static void InvokeRpcRpcCleanse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcCleanse called on server.");
		}
		else
		{
			((SetStateOnHurt)(object)obj).RpcCleanse();
		}
	}

	public void CallRpcSetStun(float duration)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcSetStun called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcSetStun);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(duration);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcSetStun");
	}

	public void CallRpcSetFrozen(float duration)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcSetFrozen called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcSetFrozen);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(duration);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcSetFrozen");
	}

	public void CallRpcSetShock(float duration)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcSetShock called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcSetShock);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(duration);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcSetShock");
	}

	public void CallRpcSetPain()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcSetPain called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcSetPain);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcSetPain");
	}

	public void CallRpcCleanse()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcCleanse called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcCleanse);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcCleanse");
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
