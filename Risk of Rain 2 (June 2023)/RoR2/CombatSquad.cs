using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class CombatSquad : NetworkBehaviour
{
	private List<CharacterMaster> membersList = new List<CharacterMaster>();

	private List<NetworkInstanceId> memberHistory = new List<NetworkInstanceId>();

	public bool propagateMembershipToSummons;

	[Tooltip("Grants a bonus health boost to members of the combat squad, depending on the number of players in the game.")]
	public bool grantBonusHealthInMultiplayer = true;

	private List<OnDestroyCallback> onDestroyCallbacksServer;

	private bool defeatedServer;

	private const uint membersListDirtyBit = 1u;

	private const uint allDirtyBits = 1u;

	[NonSerialized]
	public Run.FixedTimeStamp awakeTime;

	public UnityEvent onDefeatedClientLogicEvent;

	public UnityEvent onDefeatedServerLogicEvent;

	private static int kRpcRpcOnDefeatedClient;

	public ReadOnlyCollection<CharacterMaster> readOnlyMembersList { get; private set; }

	public int memberCount => membersList.Count;

	public event Action onDefeatedServer;

	public event Action<CharacterMaster, DamageReport> onMemberDeathServer;

	public event Action<CharacterMaster, DamageReport> onMemberDefeatedServer;

	public event Action<CharacterMaster> onMemberAddedServer;

	public event Action<CharacterMaster> onMemberDiscovered;

	public event Action<CharacterMaster> onMemberLost;

	private void Awake()
	{
		if (NetworkServer.active)
		{
			onDestroyCallbacksServer = new List<OnDestroyCallback>();
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathCallback;
			MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
		}
		readOnlyMembersList = new ReadOnlyCollection<CharacterMaster>(membersList);
		awakeTime = Run.FixedTimeStamp.now;
		InstanceTracker.Add<CombatSquad>(this);
	}

	private void OnEnable()
	{
		if (NetworkServer.active)
		{
			((NetworkBehaviour)this).SetDirtyBit(1u);
		}
	}

	private void OnDestroy()
	{
		InstanceTracker.Remove<CombatSquad>(this);
		if (NetworkServer.active)
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathCallback;
			MasterSummon.onServerMasterSummonGlobal -= OnServerMasterSummonGlobal;
		}
		for (int num = membersList.Count - 1; num >= 0; num--)
		{
			RemoveMemberAt(num);
		}
		onDestroyCallbacksServer = null;
	}

	[Server]
	private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport report)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CombatSquad::OnServerMasterSummonGlobal(RoR2.MasterSummon/MasterSummonReport)' called on client");
		}
		else if (propagateMembershipToSummons && Object.op_Implicit((Object)(object)report.leaderMasterInstance) && HasContainedMember(((NetworkBehaviour)report.leaderMasterInstance).netId))
		{
			AddMember(report.summonMasterInstance);
		}
	}

	[Server]
	public void AddMember(CharacterMaster memberMaster)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CombatSquad::AddMember(RoR2.CharacterMaster)' called on client");
			return;
		}
		if (membersList.Count >= 255)
		{
			Debug.LogFormat("Cannot add character {0} to CombatGroup! Limit of {1} members already reached.", new object[2]
			{
				memberMaster,
				byte.MaxValue
			});
			return;
		}
		membersList.Add(memberMaster);
		memberHistory.Add(((NetworkBehaviour)memberMaster).netId);
		((NetworkBehaviour)this).SetDirtyBit(1u);
		onDestroyCallbacksServer.Add(OnDestroyCallback.AddCallback(((Component)memberMaster).gameObject, OnMemberDestroyedServer));
		this.onMemberAddedServer?.Invoke(memberMaster);
		this.onMemberDiscovered?.Invoke(memberMaster);
	}

	[Server]
	private void OnCharacterDeathCallback(DamageReport damageReport)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CombatSquad::OnCharacterDeathCallback(RoR2.DamageReport)' called on client");
			return;
		}
		CharacterMaster victimMaster = damageReport.victimMaster;
		if (!Object.op_Implicit((Object)(object)victimMaster))
		{
			return;
		}
		int num = membersList.IndexOf(victimMaster);
		if (num < 0)
		{
			return;
		}
		this.onMemberDeathServer?.Invoke(victimMaster, damageReport);
		if (victimMaster.IsDeadAndOutOfLivesServer())
		{
			this.onMemberDefeatedServer?.Invoke(victimMaster, damageReport);
			RemoveMemberAt(num);
			if (!defeatedServer && membersList.Count == 0)
			{
				TriggerDefeat();
			}
		}
	}

	[Server]
	private void RemoveMember(CharacterMaster memberMaster)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CombatSquad::RemoveMember(RoR2.CharacterMaster)' called on client");
			return;
		}
		int num = membersList.IndexOf(memberMaster);
		if (num != -1)
		{
			RemoveMemberAt(num);
		}
	}

	private void RemoveMemberAt(int memberIndex)
	{
		CharacterMaster obj = membersList[memberIndex];
		membersList.RemoveAt(memberIndex);
		if (onDestroyCallbacksServer != null)
		{
			onDestroyCallbacksServer.RemoveAt(memberIndex);
		}
		((NetworkBehaviour)this).SetDirtyBit(1u);
		this.onMemberLost?.Invoke(obj);
	}

	[Server]
	public void OnMemberDestroyedServer(OnDestroyCallback onDestroyCallback)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.CombatSquad::OnMemberDestroyedServer(RoR2.OnDestroyCallback)' called on client");
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)onDestroyCallback))
			{
				return;
			}
			GameObject gameObject = ((Component)onDestroyCallback).gameObject;
			CharacterMaster characterMaster = (Object.op_Implicit((Object)(object)gameObject) ? gameObject.GetComponent<CharacterMaster>() : null);
			for (int i = 0; i < membersList.Count; i++)
			{
				if (membersList[i] == characterMaster)
				{
					RemoveMemberAt(i);
					break;
				}
			}
		}
	}

	public bool ContainsMember(CharacterMaster master)
	{
		for (int i = 0; i < membersList.Count; i++)
		{
			if (membersList[i] == master)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasContainedMember(NetworkInstanceId id)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return memberHistory.Contains(id);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 1u;
		}
		bool num2 = (num & 1) != 0;
		writer.Write((byte)num);
		if (num2)
		{
			writer.Write((byte)membersList.Count);
			for (int i = 0; i < membersList.Count; i++)
			{
				CharacterMaster characterMaster = membersList[i];
				GameObject val = (Object.op_Implicit((Object)(object)characterMaster) ? ((Component)characterMaster).gameObject : null);
				writer.Write(val);
			}
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if ((reader.ReadByte() & 1) == 0)
		{
			return;
		}
		List<CharacterMaster> list = CollectionPool<CharacterMaster, List<CharacterMaster>>.RentCollection();
		List<CharacterMaster> list2 = CollectionPool<CharacterMaster, List<CharacterMaster>>.RentCollection();
		List<CharacterMaster> a = CollectionPool<CharacterMaster, List<CharacterMaster>>.RentCollection();
		byte b = reader.ReadByte();
		for (byte b2 = 0; b2 < b; b2 = (byte)(b2 + 1))
		{
			GameObject val = reader.ReadGameObject();
			CharacterMaster item = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<CharacterMaster>() : null);
			a.Add(item);
		}
		ListUtils.FindExclusiveEntriesByReference<CharacterMaster>(membersList, a, list, list2);
		Util.Swap(ref a, ref membersList);
		CollectionPool<CharacterMaster, List<CharacterMaster>>.ReturnCollection(a);
		for (int i = 0; i < list.Count; i++)
		{
			CharacterMaster characterMaster = list[i];
			if (Object.op_Implicit((Object)(object)characterMaster))
			{
				try
				{
					this.onMemberLost?.Invoke(characterMaster);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			CharacterMaster characterMaster2 = list2[j];
			if (Object.op_Implicit((Object)(object)characterMaster2))
			{
				try
				{
					this.onMemberDiscovered?.Invoke(characterMaster2);
				}
				catch (Exception ex2)
				{
					Debug.LogError((object)ex2);
				}
			}
		}
		CollectionPool<CharacterMaster, List<CharacterMaster>>.ReturnCollection(list2);
		CollectionPool<CharacterMaster, List<CharacterMaster>>.ReturnCollection(list);
	}

	private void FixedUpdate()
	{
		if (!NetworkServer.active || defeatedServer || memberHistory.Count <= 0)
		{
			return;
		}
		bool flag = false;
		foreach (CharacterMaster members in membersList)
		{
			if (members.hasBody || members.IsExtraLifePendingServer())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogError((object)"CombatSquad has no living members.  Triggering defeat...");
			while (membersList.Count > 0)
			{
				RemoveMember(membersList[0]);
			}
			TriggerDefeat();
		}
	}

	private void TriggerDefeat()
	{
		defeatedServer = true;
		this.onDefeatedServer?.Invoke();
		UnityEvent obj = onDefeatedServerLogicEvent;
		if (obj != null)
		{
			obj.Invoke();
		}
		membersList?.Clear();
		CallRpcOnDefeatedClient();
	}

	[ClientRpc]
	private void RpcOnDefeatedClient()
	{
		UnityEvent obj = onDefeatedClientLogicEvent;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcOnDefeatedClient(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcOnDefeatedClient called on server.");
		}
		else
		{
			((CombatSquad)(object)obj).RpcOnDefeatedClient();
		}
	}

	public void CallRpcOnDefeatedClient()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcOnDefeatedClient called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcOnDefeatedClient);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcOnDefeatedClient");
	}

	static CombatSquad()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcOnDefeatedClient = -1235734536;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CombatSquad), kRpcRpcOnDefeatedClient, new CmdDelegate(InvokeRpcRpcOnDefeatedClient));
		NetworkCRC.RegisterBehaviour("CombatSquad", 0);
	}

	public override void PreStartClient()
	{
	}
}
