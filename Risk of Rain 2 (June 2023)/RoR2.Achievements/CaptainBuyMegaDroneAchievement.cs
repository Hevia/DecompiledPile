using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("CaptainBuyMegaDrone", "Skills.Captain.CaptainSupplyDropHacking", "CompleteMainEnding", typeof(CaptainBuyMegaDroneServerAchievement))]
public class CaptainBuyMegaDroneAchievement : BaseAchievement
{
	private class CaptainBuyMegaDroneServerAchievement : BaseServerAchievement
	{
		private GameObject megaDroneMasterPrefab;

		public override void OnInstall()
		{
			base.OnInstall();
			megaDroneMasterPrefab = MasterCatalog.FindMasterPrefab("MegaDroneMaster");
			GlobalEventManager.OnInteractionsGlobal += OnInteractionsGlobal;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.OnInteractionsGlobal -= OnInteractionsGlobal;
			megaDroneMasterPrefab = null;
			base.OnUninstall();
		}

		private void OnInteractionsGlobal(Interactor interactor, IInteractable interactable, GameObject interactableObject)
		{
			if (IsCurrentBody(((Component)interactor).gameObject) && (Object)(object)interactableObject.GetComponent<SummonMasterBehavior>()?.masterPrefab == (Object)(object)megaDroneMasterPrefab)
			{
				Grant();
			}
		}
	}

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CaptainBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		SetServerTracked(shouldTrack: true);
	}

	protected override void OnBodyRequirementBroken()
	{
		SetServerTracked(shouldTrack: false);
		base.OnBodyRequirementBroken();
	}
}
