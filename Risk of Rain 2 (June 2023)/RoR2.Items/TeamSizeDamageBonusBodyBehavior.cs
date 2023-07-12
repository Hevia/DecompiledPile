namespace RoR2.Items;

public class TeamSizeDamageBonusBodyBehavior : BaseItemBodyBehavior
{
	[ItemDefAssociation(useOnServer = true, useOnClient = true)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.TeamSizeDamageBonus;
	}

	private void OnJoinTeamGlobal(TeamComponent teamComponent, TeamIndex newTeamIndex)
	{
		if (teamComponent == base.body.teamComponent || newTeamIndex == base.body.teamComponent.teamIndex)
		{
			base.body.MarkAllStatsDirty();
		}
	}

	private void OnLeaveTeamGlobal(TeamComponent teamComponent, TeamIndex oldTeamIndex)
	{
		if (teamComponent == base.body.teamComponent || oldTeamIndex == base.body.teamComponent.teamIndex)
		{
			base.body.MarkAllStatsDirty();
		}
	}

	private void OnEnable()
	{
		TeamComponent.onJoinTeamGlobal += OnJoinTeamGlobal;
		TeamComponent.onLeaveTeamGlobal += OnLeaveTeamGlobal;
	}

	private void OnDisable()
	{
		TeamComponent.onJoinTeamGlobal -= OnJoinTeamGlobal;
		TeamComponent.onLeaveTeamGlobal -= OnLeaveTeamGlobal;
	}
}
