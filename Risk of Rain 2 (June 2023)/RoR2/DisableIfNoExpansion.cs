using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace RoR2;

public class DisableIfNoExpansion : MonoBehaviour
{
	[SerializeField]
	private ExpansionDef expansionDef;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)expansionDef) && !EntitlementManager.localUserEntitlementTracker.AnyUserHasEntitlement(expansionDef.requiredEntitlement))
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
