using RoR2.Skills;
using UnityEngine;

namespace RoR2;

public class CrocoDamageTypeController : MonoBehaviour
{
	public SkillDef poisonSkillDef;

	public SkillDef blightSkillDef;

	public GenericSkill passiveSkillSlot;

	public DamageType GetDamageType()
	{
		if (Object.op_Implicit((Object)(object)passiveSkillSlot))
		{
			if ((Object)(object)passiveSkillSlot.skillDef == (Object)(object)poisonSkillDef)
			{
				return DamageType.PoisonOnHit;
			}
			if ((Object)(object)passiveSkillSlot.skillDef == (Object)(object)blightSkillDef)
			{
				return DamageType.BlightOnHit;
			}
		}
		return DamageType.Generic;
	}
}
