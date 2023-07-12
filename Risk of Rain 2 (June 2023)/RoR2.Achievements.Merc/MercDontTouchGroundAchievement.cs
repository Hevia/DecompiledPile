using UnityEngine;

namespace RoR2.Achievements.Merc;

[RegisterAchievement("MercDontTouchGround", "Skills.Merc.Uppercut", "CompleteUnknownEnding", null)]
public class MercDontTouchGroundAchievement : BaseAchievement
{
	private static readonly float requirement = 30f;

	private CharacterMotor motor;

	private CharacterBody body;

	private float stopwatch;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("MercBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		RoR2Application.onFixedUpdate += MercFixedUpdate;
		base.localUser.onBodyChanged += OnBodyChanged;
		OnBodyChanged();
	}

	protected override void OnBodyRequirementBroken()
	{
		base.localUser.onBodyChanged -= OnBodyChanged;
		RoR2Application.onFixedUpdate -= MercFixedUpdate;
		base.OnBodyRequirementBroken();
	}

	private void OnBodyChanged()
	{
		body = base.localUser.cachedBody;
		motor = (Object.op_Implicit((Object)(object)body) ? body.characterMotor : null);
	}

	private void MercFixedUpdate()
	{
		bool flag = Object.op_Implicit((Object)(object)motor) && !motor.isGrounded && !Object.op_Implicit((Object)(object)body.currentVehicle);
		stopwatch = (flag ? (stopwatch + Time.fixedDeltaTime) : 0f);
		if (requirement <= stopwatch)
		{
			Grant();
		}
	}
}
