using System;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2;

[Serializable]
public class DirectorCard
{
	public SpawnCard spawnCard;

	public int selectionWeight;

	public DirectorCore.MonsterSpawnDistance spawnDistance;

	public bool preventOverhead;

	public int minimumStageCompletions;

	[Tooltip("'requiredUnlockable' will be discontinued. Use 'requiredUnlockableDef' instead.")]
	[Obsolete("'requiredUnlockable' will be discontinued. Use 'requiredUnlockableDef' instead.", false)]
	public string requiredUnlockable;

	[Obsolete("'forbiddenUnlockable' will be discontinued. Use 'forbiddenUnlockableDef' instead.", false)]
	[Tooltip("'forbiddenUnlockable' will be discontinued. Use 'forbiddenUnlockableDef' instead.")]
	public string forbiddenUnlockable;

	public UnlockableDef requiredUnlockableDef;

	public UnlockableDef forbiddenUnlockableDef;

	public int cost => spawnCard.directorCreditCost;

	[Obsolete("'CardIsValid' is confusingly named. Use 'IsAvailable' instead.", false)]
	public bool CardIsValid()
	{
		return IsAvailable();
	}

	public bool IsAvailable()
	{
		ref string reference = ref requiredUnlockable;
		ref string reference2 = ref forbiddenUnlockable;
		if (!Object.op_Implicit((Object)(object)requiredUnlockableDef) && !string.IsNullOrEmpty(reference))
		{
			requiredUnlockableDef = UnlockableCatalog.GetUnlockableDef(reference);
			reference = null;
		}
		if (!Object.op_Implicit((Object)(object)forbiddenUnlockableDef) && !string.IsNullOrEmpty(reference2))
		{
			forbiddenUnlockableDef = UnlockableCatalog.GetUnlockableDef(reference2);
			reference2 = null;
		}
		bool flag = !Object.op_Implicit((Object)(object)requiredUnlockableDef) || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef);
		bool flag2 = Object.op_Implicit((Object)(object)forbiddenUnlockableDef) && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef);
		if (Object.op_Implicit((Object)(object)Run.instance) && Run.instance.stageClearCount >= minimumStageCompletions && flag && !flag2 && Object.op_Implicit((Object)(object)spawnCard) && Object.op_Implicit((Object)(object)spawnCard.prefab))
		{
			ExpansionRequirementComponent component = spawnCard.prefab.GetComponent<ExpansionRequirementComponent>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				CharacterMaster component2 = spawnCard.prefab.GetComponent<CharacterMaster>();
				if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component2.bodyPrefab))
				{
					component = component2.bodyPrefab.GetComponent<ExpansionRequirementComponent>();
				}
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				return Run.instance.IsExpansionEnabled(component.requiredExpansion);
			}
			return true;
		}
		return false;
	}
}
