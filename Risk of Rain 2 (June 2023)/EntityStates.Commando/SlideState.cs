using RoR2;
using UnityEngine;

namespace EntityStates.Commando;

public class SlideState : BaseState
{
	public static float slideDuration;

	public static float jumpDuration;

	public static AnimationCurve forwardSpeedCoefficientCurve;

	public static AnimationCurve jumpforwardSpeedCoefficientCurve;

	public static string soundString;

	public static GameObject jetEffectPrefab;

	public static GameObject slideEffectPrefab;

	private Vector3 forwardDirection;

	private GameObject slideEffectInstance;

	private bool startedStateGrounded;

	public override void OnEnter()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(soundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Vector3 val = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector);
			obj.forward = ((Vector3)(ref val)).normalized;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			startedStateGrounded = base.characterMotor.isGrounded;
		}
		if (Object.op_Implicit((Object)(object)jetEffectPrefab))
		{
			Transform val2 = FindModelChild("LeftJet");
			Transform val3 = FindModelChild("RightJet");
			if (Object.op_Implicit((Object)(object)val2))
			{
				Object.Instantiate<GameObject>(jetEffectPrefab, val2);
			}
			if (Object.op_Implicit((Object)(object)val3))
			{
				Object.Instantiate<GameObject>(jetEffectPrefab, val3);
			}
		}
		base.characterBody.SetSpreadBloom(0f, canOnlyIncreaseBloom: false);
		if (!startedStateGrounded)
		{
			PlayAnimation("Body", "Jump");
			Vector3 velocity = base.characterMotor.velocity;
			velocity.y = base.characterBody.jumpPower;
			base.characterMotor.velocity = velocity;
			return;
		}
		PlayAnimation("Body", "SlideForward", "SlideForward.playbackRate", slideDuration);
		if (Object.op_Implicit((Object)(object)slideEffectPrefab))
		{
			Transform val4 = FindModelChild("Base");
			slideEffectInstance = Object.Instantiate<GameObject>(slideEffectPrefab, val4);
		}
	}

	public override void FixedUpdate()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			float num = (startedStateGrounded ? slideDuration : jumpDuration);
			if (Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				base.characterDirection.moveVector = base.inputBank.moveVector;
				forwardDirection = base.characterDirection.forward;
			}
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				float num2 = 0f;
				num2 = ((!startedStateGrounded) ? jumpforwardSpeedCoefficientCurve.Evaluate(base.fixedAge / num) : forwardSpeedCoefficientCurve.Evaluate(base.fixedAge / num));
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += num2 * moveSpeedStat * forwardDirection * Time.fixedDeltaTime;
			}
			if (base.fixedAge >= num)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		PlayImpactAnimation();
		if (Object.op_Implicit((Object)(object)slideEffectInstance))
		{
			EntityState.Destroy((Object)(object)slideEffectInstance);
		}
		base.OnExit();
	}

	private void PlayImpactAnimation()
	{
		Animator modelAnimator = GetModelAnimator();
		int layerIndex = modelAnimator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			modelAnimator.SetLayerWeight(layerIndex, 1f);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
