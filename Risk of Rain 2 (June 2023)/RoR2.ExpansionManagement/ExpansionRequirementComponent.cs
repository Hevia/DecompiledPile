using RoR2.EntitlementManagement;
using UnityEngine;

namespace RoR2.ExpansionManagement;

public class ExpansionRequirementComponent : MonoBehaviour
{
	public ExpansionDef requiredExpansion;

	public bool requireEntitlementIfPlayerControlled;

	private void Start()
	{
		CharacterBody component = ((Component)this).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && component.isPlayerControlled && !PlayerCanUseBody(component.master.playerCharacterMasterController))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public bool PlayerCanUseBody(PlayerCharacterMasterController playerCharacterMasterController)
	{
		Run instance = Run.instance;
		if (!Object.op_Implicit((Object)(object)instance))
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)requiredExpansion))
		{
			if (!instance.IsExpansionEnabled(requiredExpansion))
			{
				return false;
			}
			if (requireEntitlementIfPlayerControlled)
			{
				EntitlementDef requiredEntitlement = requiredExpansion.requiredEntitlement;
				if (Object.op_Implicit((Object)(object)requiredEntitlement))
				{
					PlayerCharacterMasterControllerEntitlementTracker component = ((Component)playerCharacterMasterController).GetComponent<PlayerCharacterMasterControllerEntitlementTracker>();
					if (!Object.op_Implicit((Object)(object)component))
					{
						Debug.LogWarning((object)"Rejecting body because the playerCharacterMasterController doesn't have a sibling PlayerCharacterMasterControllerEntitlementTracker");
						return false;
					}
					if (!component.HasEntitlement(requiredEntitlement))
					{
						Debug.LogWarning((object)("Rejecting body because the player doesn't have entitlement " + ((Object)requiredEntitlement).name));
						return false;
					}
				}
			}
		}
		return true;
	}
}
