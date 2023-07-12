using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Skills;

public class LunarDetonatorSkill : SkillDef
{
	private class InstanceData : BaseSkillInstanceData
	{
		private GenericSkill _skillSlot;

		private LunarDetonatorPassiveAttachment lunarDetonatorPassiveAttachment;

		public GenericSkill skillSlot
		{
			get
			{
				return _skillSlot;
			}
			set
			{
				if (_skillSlot == value)
				{
					return;
				}
				if (_skillSlot != null)
				{
					_skillSlot.characterBody.onInventoryChanged -= OnInventoryChanged;
				}
				if (Object.op_Implicit((Object)(object)lunarDetonatorPassiveAttachment))
				{
					Object.Destroy((Object)(object)((Component)lunarDetonatorPassiveAttachment).gameObject);
				}
				lunarDetonatorPassiveAttachment = null;
				_skillSlot = value;
				if (_skillSlot != null)
				{
					_skillSlot.characterBody.onInventoryChanged += OnInventoryChanged;
					if (NetworkServer.active && Object.op_Implicit((Object)(object)_skillSlot.characterBody))
					{
						GameObject val = Object.Instantiate<GameObject>(lunarDetonatorPassiveAttachmentPrefab);
						lunarDetonatorPassiveAttachment = val.GetComponent<LunarDetonatorPassiveAttachment>();
						lunarDetonatorPassiveAttachment.monitoredSkill = skillSlot;
						val.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)_skillSlot.characterBody).gameObject);
					}
				}
			}
		}

		public void OnInventoryChanged()
		{
			skillSlot.RecalculateValues();
		}
	}

	private static GameObject lunarDetonatorPassiveAttachmentPrefab;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		lunarDetonatorPassiveAttachmentPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/LunarDetonatorPassiveAttachment");
	}

	public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
	{
		return new InstanceData
		{
			skillSlot = skillSlot
		};
	}

	public override void OnUnassigned(GenericSkill skillSlot)
	{
		((InstanceData)skillSlot.skillInstanceData).skillSlot = null;
	}

	public override float GetRechargeInterval(GenericSkill skillSlot)
	{
		return (float)skillSlot.characterBody.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) * baseRechargeInterval;
	}
}
