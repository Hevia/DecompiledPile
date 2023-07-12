using RoR2;
using UnityEngine;

namespace EntityStates.Gup;

public class GupSpikesState : BasicMeleeAttack
{
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string playbackRateParam;

	[SerializeField]
	public float crossfadeDuration;

	[SerializeField]
	public string initialHitboxActiveParameter;

	[SerializeField]
	public string initialHitboxName;

	private HitBox initialHitBox;

	public override void OnEnter()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		StartAimMode(0f);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		if (!Object.op_Implicit((Object)(object)hitBoxGroup))
		{
			return;
		}
		HitBox[] hitBoxes = hitBoxGroup.hitBoxes;
		foreach (HitBox hitBox in hitBoxes)
		{
			if (((Object)((Component)hitBox).gameObject).name == initialHitboxName)
			{
				initialHitBox = hitBox;
				break;
			}
		}
	}

	protected override void PlayAnimation()
	{
		PlayCrossfade(animationLayerName, animationStateName, playbackRateParam, duration, crossfadeDuration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)initialHitBox))
		{
			((Behaviour)initialHitBox).enabled = animator.GetFloat(initialHitboxActiveParameter) > 0.5f;
		}
	}
}
