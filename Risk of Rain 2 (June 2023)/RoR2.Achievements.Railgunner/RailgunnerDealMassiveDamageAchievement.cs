namespace RoR2.Achievements.Railgunner;

[RegisterAchievement("RailgunnerDealMassiveDamage", "Skills.Railgunner.UtilityAlt1", null, null)]
public class RailgunnerDealMassiveDamageAchievement : BaseAchievement
{
	private const float minimumDamage = 1000000f;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("RailgunnerBody");
	}

	protected override void OnBodyRequirementMet()
	{
		GlobalEventManager.onClientDamageNotified += onClientDamageNotified;
	}

	protected override void OnBodyRequirementBroken()
	{
		GlobalEventManager.onClientDamageNotified -= onClientDamageNotified;
	}

	private void onClientDamageNotified(DamageDealtMessage message)
	{
		if (message.attacker == base.localUser.cachedBodyObject && message.damage >= 1000000f)
		{
			Grant();
		}
	}
}
