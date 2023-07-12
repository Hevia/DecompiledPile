using System.Collections.Generic;
using EntityStates.Engi.MineDeployer;
using UnityEngine;

namespace RoR2.Skills;

public class EngiMineDeployerSkill : SkillDef
{
	public override bool CanExecute(GenericSkill skillSlot)
	{
		List<BaseMineDeployerState> instancesList = BaseMineDeployerState.instancesList;
		for (int i = 0; i < instancesList.Count; i++)
		{
			if ((Object)(object)instancesList[i].owner == (Object)(object)((Component)skillSlot).gameObject)
			{
				return false;
			}
		}
		return base.CanExecute(skillSlot);
	}
}
