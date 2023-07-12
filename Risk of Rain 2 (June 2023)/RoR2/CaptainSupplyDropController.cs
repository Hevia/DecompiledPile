using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class CaptainSupplyDropController : NetworkBehaviour
{
	[Header("Referenced Components")]
	public GenericSkill orbitalStrikeSkill;

	public GenericSkill prepSupplyDropSkill;

	public GenericSkill supplyDrop1Skill;

	public GenericSkill supplyDrop2Skill;

	[Header("Skill Defs")]
	public SkillDef usedUpSkillDef;

	public SkillDef lostConnectionSkillDef;

	[SyncVar]
	private byte netEnabledSkillsMask;

	private byte authorityEnabledSkillsMask;

	private bool hasDoneInitialReset;

	private CharacterBody characterBody;

	[CanBeNull]
	private SkillDef currentSupplyDrop1SkillDef;

	[CanBeNull]
	private SkillDef currentSupplyDrop2SkillDef;

	[CanBeNull]
	private SkillDef currentPrepSupplyDropSkillDef;

	[CanBeNull]
	private SkillDef currentOrbitalStrikeSkillDef;

	private static int kCmdCmdSetSkillMask;

	private bool canUseOrbitalSkills => SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage;

	private bool hasEffectiveAuthority => characterBody.hasEffectiveAuthority;

	public byte NetworknetEnabledSkillsMask
	{
		get
		{
			return netEnabledSkillsMask;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<byte>(value, ref netEnabledSkillsMask, 1u);
		}
	}

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		hasDoneInitialReset = false;
	}

	private void FixedUpdate()
	{
		UpdateSkillOverrides();
		if (hasEffectiveAuthority && !hasDoneInitialReset)
		{
			hasDoneInitialReset = true;
			if (Object.op_Implicit((Object)(object)supplyDrop1Skill))
			{
				supplyDrop1Skill.Reset();
			}
			if (Object.op_Implicit((Object)(object)supplyDrop2Skill))
			{
				supplyDrop2Skill.Reset();
			}
		}
	}

	private void OnDisable()
	{
		SetSkillOverride(ref currentSupplyDrop1SkillDef, null, supplyDrop1Skill);
		SetSkillOverride(ref currentSupplyDrop2SkillDef, null, supplyDrop2Skill);
	}

	private void SetSkillOverride([CanBeNull] ref SkillDef currentSkillDef, [CanBeNull] SkillDef newSkillDef, [NotNull] GenericSkill component)
	{
		if (currentSkillDef != newSkillDef)
		{
			if (currentSkillDef != null)
			{
				component.UnsetSkillOverride(this, currentSkillDef, GenericSkill.SkillOverridePriority.Contextual);
			}
			currentSkillDef = newSkillDef;
			if (currentSkillDef != null)
			{
				component.SetSkillOverride(this, currentSkillDef, GenericSkill.SkillOverridePriority.Contextual);
			}
		}
	}

	private void UpdateSkillOverrides()
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		byte b = 0;
		if (hasEffectiveAuthority)
		{
			byte b2 = 0;
			if (supplyDrop1Skill.stock > 0 || !Object.op_Implicit((Object)(object)supplyDrop1Skill.skillDef))
			{
				b2 = (byte)(b2 | 1u);
			}
			if (supplyDrop2Skill.stock > 0 || !Object.op_Implicit((Object)(object)supplyDrop2Skill.skillDef))
			{
				b2 = (byte)(b2 | 2u);
			}
			if (b2 != authorityEnabledSkillsMask)
			{
				authorityEnabledSkillsMask = b2;
				if (NetworkServer.active)
				{
					NetworknetEnabledSkillsMask = authorityEnabledSkillsMask;
				}
				else
				{
					CallCmdSetSkillMask(authorityEnabledSkillsMask);
				}
			}
			b = authorityEnabledSkillsMask;
		}
		else
		{
			b = netEnabledSkillsMask;
		}
		bool flag = (b & 1) != 0;
		bool flag2 = (b & 2) != 0;
		SetSkillOverride(ref currentSupplyDrop1SkillDef, flag ? null : usedUpSkillDef, supplyDrop1Skill);
		SetSkillOverride(ref currentSupplyDrop2SkillDef, flag2 ? null : usedUpSkillDef, supplyDrop2Skill);
	}

	[Command]
	private void CmdSetSkillMask(byte newMask)
	{
		NetworknetEnabledSkillsMask = newMask;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetSkillMask(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSetSkillMask called on client.");
		}
		else
		{
			((CaptainSupplyDropController)(object)obj).CmdSetSkillMask((byte)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdSetSkillMask(byte newMask)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSetSkillMask called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSetSkillMask(newMask);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSetSkillMask);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.WritePackedUInt32((uint)newMask);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSetSkillMask");
	}

	static CaptainSupplyDropController()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kCmdCmdSetSkillMask = 176967897;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CaptainSupplyDropController), kCmdCmdSetSkillMask, new CmdDelegate(InvokeCmdCmdSetSkillMask));
		NetworkCRC.RegisterBehaviour("CaptainSupplyDropController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)netEnabledSkillsMask);
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
			writer.WritePackedUInt32((uint)netEnabledSkillsMask);
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
			netEnabledSkillsMask = (byte)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			netEnabledSkillsMask = (byte)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
