using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(NetworkedBodyAttachment))]
public class LunarDetonatorPassiveAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener
{
	private class DamageListener : MonoBehaviour, IOnDamageDealtServerReceiver
	{
		public LunarDetonatorPassiveAttachment passiveController;

		public void OnDamageDealtServer(DamageReport damageReport)
		{
			if (passiveController.skillAvailable && damageReport.victim.alive && Util.CheckRoll(damageReport.damageInfo.procCoefficient * 100f, damageReport.attackerMaster))
			{
				damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.LunarDetonationCharge, 10f);
			}
		}
	}

	private GenericSkill _monitoredSkill;

	[SyncVar(hook = "SetSkillSlotIndexPlusOne")]
	private uint skillSlotIndexPlusOne;

	private bool skillAvailable;

	private NetworkedBodyAttachment networkedBodyAttachment;

	private DamageListener damageListener;

	private static int kCmdCmdSetSkillAvailable;

	public GenericSkill monitoredSkill
	{
		get
		{
			return _monitoredSkill;
		}
		set
		{
			if (_monitoredSkill == value)
			{
				return;
			}
			_monitoredSkill = value;
			int num = -1;
			if (Object.op_Implicit((Object)(object)_monitoredSkill))
			{
				SkillLocator component = ((Component)_monitoredSkill).GetComponent<SkillLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					num = component.GetSkillSlotIndex(_monitoredSkill);
				}
			}
			SetSkillSlotIndexPlusOne((uint)(num + 1));
		}
	}

	public uint NetworkskillSlotIndexPlusOne
	{
		get
		{
			return skillSlotIndexPlusOne;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetSkillSlotIndexPlusOne(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<uint>(value, ref skillSlotIndexPlusOne, 1u);
		}
	}

	private void Awake()
	{
		networkedBodyAttachment = ((Component)this).GetComponent<NetworkedBodyAttachment>();
	}

	private void FixedUpdate()
	{
		if (networkedBodyAttachment.hasEffectiveAuthority)
		{
			FixedUpdateAuthority();
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)damageListener))
		{
			Object.Destroy((Object)(object)damageListener);
		}
		damageListener = null;
	}

	public override void OnStartClient()
	{
		SetSkillSlotIndexPlusOne(skillSlotIndexPlusOne);
	}

	private void SetSkillSlotIndexPlusOne(uint newSkillSlotIndexPlusOne)
	{
		NetworkskillSlotIndexPlusOne = newSkillSlotIndexPlusOne;
		if (!NetworkServer.active)
		{
			ResolveMonitoredSkill();
		}
	}

	private void ResolveMonitoredSkill()
	{
		if (Object.op_Implicit((Object)(object)networkedBodyAttachment.attachedBody))
		{
			SkillLocator component = ((Component)networkedBodyAttachment.attachedBody).GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				monitoredSkill = component.GetSkillAtIndex((int)(skillSlotIndexPlusOne - 1));
			}
		}
	}

	private void FixedUpdateAuthority()
	{
		bool flag = false;
		if (Object.op_Implicit((Object)(object)monitoredSkill))
		{
			flag = monitoredSkill.stock > 0;
		}
		if (skillAvailable != flag)
		{
			skillAvailable = flag;
			if (!NetworkServer.active)
			{
				CallCmdSetSkillAvailable(skillAvailable);
			}
		}
	}

	[Command]
	private void CmdSetSkillAvailable(bool newSkillAvailable)
	{
		skillAvailable = newSkillAvailable;
	}

	public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
	{
		if (NetworkServer.active)
		{
			damageListener = ((Component)attachedBody).gameObject.AddComponent<DamageListener>();
			damageListener.passiveController = this;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetSkillAvailable(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"Command CmdSetSkillAvailable called on client.");
		}
		else
		{
			((LunarDetonatorPassiveAttachment)(object)obj).CmdSetSkillAvailable(reader.ReadBoolean());
		}
	}

	public void CallCmdSetSkillAvailable(bool newSkillAvailable)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"Command function CmdSetSkillAvailable called on server.");
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			CmdSetSkillAvailable(newSkillAvailable);
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)5);
		val.WritePackedUInt32((uint)kCmdCmdSetSkillAvailable);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		val.Write(newSkillAvailable);
		((NetworkBehaviour)this).SendCommandInternal(val, 0, "CmdSetSkillAvailable");
	}

	static LunarDetonatorPassiveAttachment()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kCmdCmdSetSkillAvailable = -1453655134;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LunarDetonatorPassiveAttachment), kCmdCmdSetSkillAvailable, new CmdDelegate(InvokeCmdCmdSetSkillAvailable));
		NetworkCRC.RegisterBehaviour("LunarDetonatorPassiveAttachment", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32(skillSlotIndexPlusOne);
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
			writer.WritePackedUInt32(skillSlotIndexPlusOne);
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
			skillSlotIndexPlusOne = reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetSkillSlotIndexPlusOne(reader.ReadPackedUInt32());
		}
	}

	public override void PreStartClient()
	{
	}
}
