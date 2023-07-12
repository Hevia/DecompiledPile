using System;
using HG;
using RoR2.Networking;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[RequireComponent(typeof(NetworkIdentity))]
public class SkillLocator : NetworkBehaviour
{
	[Serializable]
	public struct PassiveSkill
	{
		public bool enabled;

		public string skillNameToken;

		public string skillDescriptionToken;

		public string keywordToken;

		public Sprite icon;
	}

	[FormerlySerializedAs("skill1")]
	public GenericSkill primary;

	[FormerlySerializedAs("skill2")]
	public GenericSkill secondary;

	[FormerlySerializedAs("skill3")]
	public GenericSkill utility;

	[FormerlySerializedAs("skill4")]
	public GenericSkill special;

	public PassiveSkill passiveSkill;

	[SerializeField]
	private GenericSkill primaryBonusStockOverrideSkill;

	[SerializeField]
	private GenericSkill secondaryBonusStockOverrideSkill;

	[SerializeField]
	private GenericSkill utilityBonusStockOverrideSkill;

	[SerializeField]
	private GenericSkill specialBonusStockOverrideSkill;

	private NetworkIdentity networkIdentity;

	private GenericSkill[] allSkills;

	private bool hasEffectiveAuthority;

	private uint skillDefDirtyFlags;

	private bool inDeserialize;

	private static int kRpcRpcDeductCooldownFromAllSkillsServer;

	public GenericSkill primaryBonusStockSkill
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)primaryBonusStockOverrideSkill))
			{
				return primary;
			}
			return primaryBonusStockOverrideSkill;
		}
	}

	public GenericSkill secondaryBonusStockSkill
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)secondaryBonusStockOverrideSkill))
			{
				return secondary;
			}
			return secondaryBonusStockOverrideSkill;
		}
	}

	public GenericSkill utilityBonusStockSkill
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)utilityBonusStockOverrideSkill))
			{
				return utility;
			}
			return utilityBonusStockOverrideSkill;
		}
	}

	public GenericSkill specialBonusStockSkill
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)specialBonusStockOverrideSkill))
			{
				return special;
			}
			return specialBonusStockOverrideSkill;
		}
	}

	public int skillSlotCount => allSkills.Length;

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		allSkills = ((Component)this).GetComponents<GenericSkill>();
	}

	private void Start()
	{
		UpdateAuthority();
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
		hasEffectiveAuthority = Util.HasEffectiveAuthority(networkIdentity);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			for (int i = 0; i < allSkills.Length; i++)
			{
				GenericSkill genericSkill = allSkills[i];
				if ((Object)(object)genericSkill.baseSkill != (Object)(object)genericSkill.defaultSkillDef)
				{
					num |= (uint)(1 << i);
				}
			}
		}
		writer.WritePackedUInt32(num);
		for (int j = 0; j < allSkills.Length; j++)
		{
			if ((num & (uint)(1 << j)) != 0)
			{
				GenericSkill genericSkill2 = allSkills[j];
				writer.WritePackedUInt32((uint)((genericSkill2.baseSkill?.skillIndex ?? (-1)) + 1));
			}
		}
		return num != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		inDeserialize = true;
		uint num = reader.ReadPackedUInt32();
		for (int i = 0; i < allSkills.Length; i++)
		{
			if ((num & (uint)(1 << i)) != 0)
			{
				GenericSkill genericSkill = allSkills[i];
				SkillDef skillDef = SkillCatalog.GetSkillDef((int)(reader.ReadPackedUInt32() - 1));
				if (initialState || !hasEffectiveAuthority)
				{
					genericSkill.SetBaseSkill(skillDef);
				}
			}
		}
		inDeserialize = false;
	}

	public GenericSkill FindSkill(string skillName)
	{
		for (int i = 0; i < allSkills.Length; i++)
		{
			if (allSkills[i].skillName == skillName)
			{
				return allSkills[i];
			}
		}
		return null;
	}

	public GenericSkill FindSkillByFamilyName(string skillFamilyName)
	{
		for (int i = 0; i < allSkills.Length; i++)
		{
			if (SkillCatalog.GetSkillFamilyName(allSkills[i].skillFamily.catalogIndex) == skillFamilyName)
			{
				return allSkills[i];
			}
		}
		return null;
	}

	public GenericSkill FindSkillByDef(SkillDef skillDef)
	{
		for (int i = 0; i < allSkills.Length; i++)
		{
			if (allSkills[i].skillDef == skillDef)
			{
				return allSkills[i];
			}
		}
		return null;
	}

	public GenericSkill GetSkill(SkillSlot skillSlot)
	{
		return skillSlot switch
		{
			SkillSlot.Primary => primary, 
			SkillSlot.Secondary => secondary, 
			SkillSlot.Utility => utility, 
			SkillSlot.Special => special, 
			_ => null, 
		};
	}

	public GenericSkill GetSkillAtIndex(int index)
	{
		return ArrayUtils.GetSafe<GenericSkill>(allSkills, index);
	}

	public int GetSkillSlotIndex(GenericSkill skillSlot)
	{
		return Array.IndexOf(allSkills, skillSlot);
	}

	public SkillSlot FindSkillSlot(GenericSkill skillComponent)
	{
		if (!Object.op_Implicit((Object)(object)skillComponent))
		{
			return SkillSlot.None;
		}
		if ((Object)(object)skillComponent == (Object)(object)primary)
		{
			return SkillSlot.Primary;
		}
		if ((Object)(object)skillComponent == (Object)(object)secondary)
		{
			return SkillSlot.Secondary;
		}
		if ((Object)(object)skillComponent == (Object)(object)utility)
		{
			return SkillSlot.Utility;
		}
		if ((Object)(object)skillComponent == (Object)(object)special)
		{
			return SkillSlot.Special;
		}
		return SkillSlot.None;
	}

	[Server]
	public void ApplyLoadoutServer(Loadout loadout, BodyIndex bodyIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SkillLocator::ApplyLoadoutServer(RoR2.Loadout,RoR2.BodyIndex)' called on client");
		}
		else
		{
			if (bodyIndex == BodyIndex.None)
			{
				return;
			}
			for (int i = 0; i < allSkills.Length; i++)
			{
				uint num = loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, i);
				GenericSkill obj = allSkills[i];
				SkillFamily.Variant[] variants = obj.skillFamily.variants;
				if (!ArrayUtils.IsInBounds<SkillFamily.Variant>(variants, num))
				{
					num = 0u;
				}
				obj.SetBaseSkill(variants[num].skillDef);
			}
		}
	}

	public void ResetSkills()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (NetworkServer.active && networkIdentity.clientAuthorityOwner != null)
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)56);
			val.Write(((Component)this).gameObject);
			val.FinishMessage();
			networkIdentity.clientAuthorityOwner.SendWriter(val, QosChannelIndex.defaultReliable.intVal);
		}
		for (int i = 0; i < allSkills.Length; i++)
		{
			allSkills[i].Reset();
		}
	}

	public void ApplyAmmoPack()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (NetworkServer.active && !networkIdentity.hasAuthority)
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)63);
			val.Write(((Component)this).gameObject);
			val.FinishMessage();
			NetworkConnection clientAuthorityOwner = networkIdentity.clientAuthorityOwner;
			if (clientAuthorityOwner != null)
			{
				clientAuthorityOwner.SendWriter(val, QosChannelIndex.defaultReliable.intVal);
			}
			return;
		}
		GenericSkill[] array = allSkills;
		foreach (GenericSkill genericSkill in array)
		{
			if (genericSkill.CanApplyAmmoPack())
			{
				genericSkill.ApplyAmmoPack();
			}
		}
	}

	[NetworkMessageHandler(msgType = 56, client = true)]
	private static void HandleResetSkills(NetworkMessage netMsg)
	{
		GameObject val = netMsg.reader.ReadGameObject();
		if (!NetworkServer.active && Object.op_Implicit((Object)(object)val))
		{
			SkillLocator component = val.GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.ResetSkills();
			}
		}
	}

	[NetworkMessageHandler(msgType = 63, client = true)]
	private static void HandleAmmoPackPickup(NetworkMessage netMsg)
	{
		GameObject val = netMsg.reader.ReadGameObject();
		if (!NetworkServer.active && Object.op_Implicit((Object)(object)val))
		{
			SkillLocator component = val.GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.ApplyAmmoPack();
			}
		}
	}

	public void DeductCooldownFromAllSkillsServer(float deduction)
	{
		if (hasEffectiveAuthority)
		{
			DeductCooldownFromAllSkillsAuthority(deduction);
		}
		else
		{
			CallRpcDeductCooldownFromAllSkillsServer(deduction);
		}
	}

	[ClientRpc]
	private void RpcDeductCooldownFromAllSkillsServer(float deduction)
	{
		if (hasEffectiveAuthority)
		{
			DeductCooldownFromAllSkillsAuthority(deduction);
		}
	}

	private void DeductCooldownFromAllSkillsAuthority(float deduction)
	{
		for (int i = 0; i < allSkills.Length; i++)
		{
			GenericSkill genericSkill = allSkills[i];
			if (genericSkill.stock < genericSkill.maxStock)
			{
				genericSkill.rechargeStopwatch += deduction;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDeductCooldownFromAllSkillsServer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcDeductCooldownFromAllSkillsServer called on server.");
		}
		else
		{
			((SkillLocator)(object)obj).RpcDeductCooldownFromAllSkillsServer(reader.ReadSingle());
		}
	}

	public void CallRpcDeductCooldownFromAllSkillsServer(float deduction)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcDeductCooldownFromAllSkillsServer called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcDeductCooldownFromAllSkillsServer);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(deduction);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcDeductCooldownFromAllSkillsServer");
	}

	static SkillLocator()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcDeductCooldownFromAllSkillsServer = -2090076365;
		NetworkBehaviour.RegisterRpcDelegate(typeof(SkillLocator), kRpcRpcDeductCooldownFromAllSkillsServer, new CmdDelegate(InvokeRpcRpcDeductCooldownFromAllSkillsServer));
		NetworkCRC.RegisterBehaviour("SkillLocator", 0);
	}

	public override void PreStartClient()
	{
	}
}
