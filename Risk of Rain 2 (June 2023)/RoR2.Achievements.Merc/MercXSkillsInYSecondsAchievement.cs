namespace RoR2.Achievements.Merc;

[RegisterAchievement("MercXSkillsInYSeconds", "Skills.Merc.FocusedAssault", "CompleteUnknownEnding", null)]
public class MercXSkillsInYSecondsAchievement : BaseAchievement
{
	private static readonly int requiredSkillCount = 20;

	private static readonly float windowSecconds = 10f;

	private DoXInYSecondsTracker tracker;

	private CharacterBody _trackedBody;

	private CharacterBody trackedBody
	{
		get
		{
			return _trackedBody;
		}
		set
		{
			if (_trackedBody != value)
			{
				if (_trackedBody != null)
				{
					_trackedBody.onSkillActivatedAuthority -= OnSkillActivated;
				}
				_trackedBody = value;
				if (_trackedBody != null)
				{
					_trackedBody.onSkillActivatedAuthority += OnSkillActivated;
				}
			}
		}
	}

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("MercBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		trackedBody = base.localUser.cachedBody;
		base.localUser.onBodyChanged += OnBodyChanged;
		tracker.Clear();
	}

	protected override void OnBodyRequirementBroken()
	{
		if (base.localUser != null)
		{
			base.localUser.onBodyChanged -= OnBodyChanged;
		}
		trackedBody = null;
		base.OnBodyRequirementBroken();
		if (tracker != null)
		{
			tracker.Clear();
		}
	}

	private void OnBodyChanged()
	{
		trackedBody = base.localUser.cachedBody;
		tracker.Clear();
	}

	public override void OnInstall()
	{
		base.OnInstall();
		tracker = new DoXInYSecondsTracker(requiredSkillCount, windowSecconds);
	}

	public override void OnUninstall()
	{
		tracker = null;
		base.OnUninstall();
	}

	private void OnSkillActivated(GenericSkill skill)
	{
		if (tracker.Push(Run.FixedTimeStamp.now.t))
		{
			Grant();
		}
	}
}
