using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Achievements.Engi;

[RegisterAchievement("EngiArmy", "Skills.Engi.WalkerTurret", "Complete30StagesCareer", null)]
public class EngiArmyAchievement : BaseAchievement
{
	private static readonly int requirement = 12;

	private ToggleAction monitorMinions;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("EngiBody");
	}

	private void SubscribeToMinionChanges()
	{
		MinionOwnership.onMinionGroupChangedGlobal += OnMinionGroupChangedGlobal;
	}

	private void UnsubscribeFromMinionChanges()
	{
		MinionOwnership.onMinionGroupChangedGlobal -= OnMinionGroupChangedGlobal;
	}

	private void OnMinionGroupChangedGlobal(MinionOwnership minion)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (requirement > (minion.group?.memberCount ?? 0))
		{
			return;
		}
		CharacterMaster master = base.localUser.cachedMasterController.master;
		if (Object.op_Implicit((Object)(object)master))
		{
			NetworkInstanceId netId = ((NetworkBehaviour)master).netId;
			if (minion.group.ownerId == netId)
			{
				Grant();
			}
		}
	}

	public override void OnInstall()
	{
		base.OnInstall();
		monitorMinions = new ToggleAction(SubscribeToMinionChanges, UnsubscribeFromMinionChanges);
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		monitorMinions.SetActive(newActive: true);
	}

	protected override void OnBodyRequirementBroken()
	{
		monitorMinions.SetActive(newActive: false);
		base.OnBodyRequirementBroken();
	}
}
