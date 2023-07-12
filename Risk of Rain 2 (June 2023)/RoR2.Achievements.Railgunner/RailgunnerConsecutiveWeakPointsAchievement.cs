using EntityStates.Railgunner.Weapon;

namespace RoR2.Achievements.Railgunner;

[RegisterAchievement("RailgunnerConsecutiveWeakPoints", "Skills.Railgunner.SecondaryAlt1", null, null)]
public class RailgunnerConsecutiveWeakPointsAchievement : BaseAchievement
{
	private const int requirement = 30;

	private int consecutiveCount;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("RailgunnerBody");
	}

	protected override void OnBodyRequirementMet()
	{
		BaseFireSnipe.onWeakPointHit += OnWeakPointHit;
		BaseFireSnipe.onWeakPointMissed += OnWeakPointMissed;
	}

	protected override void OnBodyRequirementBroken()
	{
		BaseFireSnipe.onWeakPointHit -= OnWeakPointHit;
		BaseFireSnipe.onWeakPointMissed -= OnWeakPointMissed;
		consecutiveCount = 0;
	}

	private void OnWeakPointMissed()
	{
		consecutiveCount = 0;
	}

	private void OnWeakPointHit(DamageInfo damageInfo)
	{
		consecutiveCount++;
		if (consecutiveCount >= 30)
		{
			Grant();
		}
	}
}
