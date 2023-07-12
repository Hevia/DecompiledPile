using RoR2;
using UnityEngine;

namespace EntityStates.Assassin2;

public class ChargeDash : BaseState
{
	public static float baseDuration = 1.5f;

	public static string enterSoundString;

	private Animator modelAnimator;

	private float duration;

	private int slashCount;

	private Transform modelTransform;

	private Vector3 oldVelocity;

	private bool dashComplete;

	public override void OnEnter()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = ((Ray)(ref aimRay)).direction;
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "PreAttack");
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration && base.isAuthority)
		{
			outer.SetNextState(new DashStrike());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
