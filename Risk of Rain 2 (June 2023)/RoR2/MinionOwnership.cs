using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DisallowMultipleComponent]
public class MinionOwnership : NetworkBehaviour
{
	public class MinionGroup : IDisposable
	{
		private class MinionGroupDestroyer : MonoBehaviour
		{
			public MinionGroup group;

			private void OnDestroy()
			{
				group.Dispose();
				group.refCount--;
			}
		}

		private static readonly List<MinionGroup> instancesList = new List<MinionGroup>();

		public readonly NetworkInstanceId ownerId;

		private MinionOwnership[] _members;

		private int _memberCount;

		private int refCount;

		private bool resolved;

		private GameObject resolvedOwnerGameObject;

		private CharacterMaster resolvedOwnerMaster;

		public MinionOwnership[] members => _members;

		public int memberCount => _memberCount;

		public bool isMinion => ownerId != NetworkInstanceId.Invalid;

		public static MinionGroup FindGroup(NetworkInstanceId ownerId)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			foreach (MinionGroup instances in instancesList)
			{
				if (instances.ownerId == ownerId)
				{
					return instances;
				}
			}
			return null;
		}

		public static void SetMinionOwner(MinionOwnership minion, NetworkInstanceId ownerId)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (minion.group != null)
			{
				if (minion.group.ownerId == ownerId)
				{
					return;
				}
				RemoveMinion(minion.group.ownerId, minion);
			}
			if (ownerId != NetworkInstanceId.Invalid)
			{
				AddMinion(ownerId, minion);
			}
		}

		private static void AddMinion(NetworkInstanceId ownerId, MinionOwnership minion)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			MinionGroup minionGroup = null;
			for (int i = 0; i < instancesList.Count; i++)
			{
				MinionGroup minionGroup2 = instancesList[i];
				if (instancesList[i].ownerId == ownerId)
				{
					minionGroup = minionGroup2;
					break;
				}
			}
			if (minionGroup == null)
			{
				minionGroup = new MinionGroup(ownerId);
			}
			minionGroup.AddMember(minion);
			minionGroup.AttemptToResolveOwner();
			CharacterMaster component = ((Component)minion).GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.inventory.GiveItem(RoR2Content.Items.MinionLeash);
			}
		}

		private static void RemoveMinion(NetworkInstanceId ownerId, MinionOwnership minion)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			CharacterMaster component = ((Component)minion).GetComponent<CharacterMaster>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.inventory.RemoveItem(RoR2Content.Items.MinionLeash);
			}
			MinionGroup minionGroup = null;
			for (int i = 0; i < instancesList.Count; i++)
			{
				MinionGroup minionGroup2 = instancesList[i];
				if (instancesList[i].ownerId == ownerId)
				{
					minionGroup = minionGroup2;
					break;
				}
			}
			if (minionGroup == null)
			{
				throw new InvalidOperationException(string.Format("{0}.{1} Could not find group to which {2} belongs", "MinionGroup", "RemoveMinion", minion));
			}
			minionGroup.RemoveMember(minion);
			if (minionGroup.refCount == 0 && !Object.op_Implicit((Object)(object)minionGroup.resolvedOwnerGameObject))
			{
				minionGroup.Dispose();
			}
		}

		private MinionGroup(NetworkInstanceId ownerId)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			this.ownerId = ownerId;
			_members = new MinionOwnership[4];
			_memberCount = 0;
			instancesList.Add(this);
		}

		public void Dispose()
		{
			for (int num = _memberCount - 1; num >= 0; num--)
			{
				RemoveMemberAt(num);
			}
			instancesList.Remove(this);
		}

		private void AttemptToResolveOwner()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (resolved)
			{
				return;
			}
			resolvedOwnerGameObject = Util.FindNetworkObject(ownerId);
			if (Object.op_Implicit((Object)(object)resolvedOwnerGameObject))
			{
				resolved = true;
				resolvedOwnerMaster = resolvedOwnerGameObject.GetComponent<CharacterMaster>();
				resolvedOwnerGameObject.AddComponent<MinionGroupDestroyer>().group = this;
				refCount++;
				for (int i = 0; i < _memberCount; i++)
				{
					_members[i].HandleOwnerDiscovery(resolvedOwnerMaster);
				}
			}
		}

		public void AddMember(MinionOwnership minion)
		{
			ArrayUtils.ArrayAppend<MinionOwnership>(ref _members, ref _memberCount, ref minion);
			refCount++;
			minion.HandleGroupDiscovery(this);
			if (Object.op_Implicit((Object)(object)resolvedOwnerMaster))
			{
				minion.HandleOwnerDiscovery(resolvedOwnerMaster);
			}
		}

		public void RemoveMember(MinionOwnership minion)
		{
			RemoveMemberAt(Array.IndexOf(_members, minion));
			refCount--;
		}

		private void RemoveMemberAt(int i)
		{
			MinionOwnership obj = _members[i];
			ArrayUtils.ArrayRemoveAt<MinionOwnership>(_members, ref _memberCount, i, 1);
			obj.HandleOwnerDiscovery(null);
			obj.HandleGroupDiscovery(null);
		}

		[ConCommand(commandName = "minion_dump", flags = ConVarFlags.None, helpText = "Prints debug information about all active minion groups.")]
		private static void CCMinionPrint(ConCommandArgs args)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < instancesList.Count; i++)
			{
				MinionGroup minionGroup = instancesList[i];
				stringBuilder.Append("group [").Append(i).Append("] size=")
					.Append(minionGroup._memberCount)
					.Append(" id=")
					.Append(minionGroup.ownerId)
					.Append(" resolvedOwnerGameObject=")
					.Append(Object.op_Implicit((Object)(object)minionGroup.resolvedOwnerGameObject))
					.AppendLine();
				for (int j = 0; j < minionGroup._memberCount; j++)
				{
					stringBuilder.Append("  ").Append("[").Append(j)
						.Append("] member.name=")
						.Append(((Object)minionGroup._members[j]).name)
						.AppendLine();
				}
			}
			Debug.Log((object)stringBuilder.ToString());
		}
	}

	[SyncVar(hook = "OnSyncOwnerMasterId")]
	private NetworkInstanceId ownerMasterId = NetworkInstanceId.Invalid;

	public CharacterMaster ownerMaster { get; private set; }

	public MinionGroup group { get; private set; }

	public NetworkInstanceId NetworkownerMasterId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ownerMasterId;
		}
		[param: In]
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnSyncOwnerMasterId(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<NetworkInstanceId>(value, ref ownerMasterId, 1u);
		}
	}

	public event Action<CharacterMaster> onOwnerDiscovered;

	public event Action<CharacterMaster> onOwnerLost;

	public static event Action<MinionOwnership> onMinionGroupChangedGlobal;

	public static event Action<MinionOwnership> onMinionOwnerChangedGlobal;

	[Server]
	public void SetOwner(CharacterMaster newOwnerMaster)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.MinionOwnership::SetOwner(RoR2.CharacterMaster)' called on client");
			return;
		}
		NetworkownerMasterId = (Object.op_Implicit((Object)(object)newOwnerMaster) ? ((NetworkBehaviour)newOwnerMaster).netId : NetworkInstanceId.Invalid);
		MinionGroup.SetMinionOwner(this, ownerMasterId);
	}

	private void OnSyncOwnerMasterId(NetworkInstanceId newOwnerMasterId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		MinionGroup.SetMinionOwner(this, ownerMasterId);
	}

	public override void OnStartClient()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartClient();
		if (!NetworkServer.active)
		{
			MinionGroup.SetMinionOwner(this, ownerMasterId);
		}
	}

	private void HandleGroupDiscovery(MinionGroup newGroup)
	{
		group = newGroup;
		MinionOwnership.onMinionGroupChangedGlobal?.Invoke(this);
	}

	private void HandleOwnerDiscovery(CharacterMaster newOwner)
	{
		if (ownerMaster != null)
		{
			this.onOwnerLost?.Invoke(ownerMaster);
		}
		ownerMaster = newOwner;
		if (ownerMaster != null)
		{
			this.onOwnerDiscovered?.Invoke(ownerMaster);
		}
		MinionOwnership.onMinionOwnerChangedGlobal?.Invoke(this);
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		MinionGroup.SetMinionOwner(this, NetworkInstanceId.Invalid);
	}

	[AssetCheck(typeof(CharacterMaster))]
	private static void AddMinionOwnershipComponent(AssetCheckArgs args)
	{
		CharacterMaster characterMaster = args.asset as CharacterMaster;
		if (!Object.op_Implicit((Object)(object)((Component)characterMaster).GetComponent<MinionOwnership>()))
		{
			((Component)characterMaster).gameObject.AddComponent<MinionOwnership>();
			args.UpdatePrefab();
		}
	}

	private void OnValidate()
	{
		if (((Component)this).GetComponents<MinionOwnership>().Length > 1)
		{
			Debug.LogError((object)"Only one MinionOwnership is allowed per object!", (Object)(object)this);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(ownerMasterId);
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
			writer.Write(ownerMasterId);
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
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			ownerMasterId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnSyncOwnerMasterId(reader.ReadNetworkId());
		}
	}

	public override void PreStartClient()
	{
	}
}
