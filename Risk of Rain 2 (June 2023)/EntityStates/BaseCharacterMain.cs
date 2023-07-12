using RoR2;
using UnityEngine;

namespace EntityStates;

public class BaseCharacterMain : BaseState
{
	private RootMotionAccumulator rootMotionAccumulator;

	private Vector3 previousPosition;

	protected CharacterAnimParamAvailability characterAnimParamAvailability;

	private CharacterAnimatorWalkParamCalculator animatorWalkParamCalculator;

	protected BodyAnimatorSmoothingParameters.SmoothingParameters smoothingParameters;

	protected bool useRootMotion;

	private bool wasGrounded;

	private float lastYSpeed;

	protected bool hasCharacterMotor;

	protected bool hasCharacterDirection;

	protected bool hasCharacterBody;

	protected bool hasRailMotor;

	protected bool hasCameraTargetParams;

	protected bool hasSkillLocator;

	protected bool hasModelAnimator;

	protected bool hasInputBank;

	protected bool hasRootMotionAccumulator;

	protected Animator modelAnimator { get; private set; }

	protected Vector3 estimatedVelocity { get; private set; }

	public override void OnEnter()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		rootMotionAccumulator = GetModelRootMotionAccumulator();
		if (Object.op_Implicit((Object)(object)rootMotionAccumulator))
		{
			rootMotionAccumulator.ExtractRootMotion();
		}
		GetBodyAnimatorSmoothingParameters(out smoothingParameters);
		previousPosition = base.transform.position;
		hasCharacterMotor = Object.op_Implicit((Object)(object)base.characterMotor);
		hasCharacterDirection = Object.op_Implicit((Object)(object)base.characterDirection);
		hasCharacterBody = Object.op_Implicit((Object)(object)base.characterBody);
		hasRailMotor = Object.op_Implicit((Object)(object)base.railMotor);
		hasCameraTargetParams = Object.op_Implicit((Object)(object)base.cameraTargetParams);
		hasSkillLocator = Object.op_Implicit((Object)(object)base.skillLocator);
		hasModelAnimator = Object.op_Implicit((Object)(object)modelAnimator);
		hasInputBank = Object.op_Implicit((Object)(object)base.inputBank);
		hasRootMotionAccumulator = Object.op_Implicit((Object)(object)rootMotionAccumulator);
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			characterAnimParamAvailability = CharacterAnimParamAvailability.FromAnimator(modelAnimator);
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			if (characterAnimParamAvailability.isGrounded)
			{
				wasGrounded = base.isGrounded;
				modelAnimator.SetBool(AnimationParameters.isGrounded, wasGrounded);
			}
			if (base.isGrounded || !hasCharacterMotor)
			{
				modelAnimator.CrossFadeInFixedTime("Idle", 0.1f, layerIndex);
			}
			else
			{
				modelAnimator.CrossFadeInFixedTime("AscendDescend", 0.1f, layerIndex);
			}
			modelAnimator.Update(0f);
		}
	}

	public override void OnExit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rootMotionAccumulator))
		{
			rootMotionAccumulator.ExtractRootMotion();
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			if (characterAnimParamAvailability.isMoving)
			{
				modelAnimator.SetBool(AnimationParameters.isMoving, false);
			}
			if (characterAnimParamAvailability.turnAngle)
			{
				modelAnimator.SetFloat(AnimationParameters.turnAngle, 0f);
			}
		}
		base.OnExit();
	}

	public override void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!(Time.deltaTime <= 0f))
		{
			Vector3 position = base.transform.position;
			estimatedVelocity = (position - previousPosition) / Time.deltaTime;
			previousPosition = position;
			useRootMotion = (Object.op_Implicit((Object)(object)base.characterBody) && base.characterBody.rootMotionInMainState && base.isGrounded) || Object.op_Implicit((Object)(object)base.railMotor);
			UpdateAnimationParameters();
		}
	}

	public override void FixedUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (hasCharacterMotor)
		{
			float num = estimatedVelocity.y - lastYSpeed;
			if (base.isGrounded && !wasGrounded && hasModelAnimator)
			{
				int layerIndex = modelAnimator.GetLayerIndex("Impact");
				if (layerIndex >= 0)
				{
					modelAnimator.SetLayerWeight(layerIndex, Mathf.Clamp01(Mathf.Max(new float[3]
					{
						0.3f,
						num / 5f,
						modelAnimator.GetLayerWeight(layerIndex)
					})));
					modelAnimator.PlayInFixedTime("LightImpact", layerIndex, 0f);
				}
			}
			wasGrounded = base.isGrounded;
			lastYSpeed = estimatedVelocity.y;
		}
		if (!hasRootMotionAccumulator)
		{
			return;
		}
		Vector3 val = rootMotionAccumulator.ExtractRootMotion();
		if (useRootMotion && val != Vector3.zero && base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += val;
			}
			if (Object.op_Implicit((Object)(object)base.railMotor))
			{
				RailMotor obj2 = base.railMotor;
				obj2.rootMotion += val;
			}
		}
	}

	protected virtual void UpdateAnimationParameters()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		if (hasRailMotor || !hasModelAnimator)
		{
			return;
		}
		Vector3 val = (Object.op_Implicit((Object)(object)base.inputBank) ? base.inputBank.moveVector : Vector3.zero);
		bool flag = val != Vector3.zero && base.characterBody.moveSpeed > Mathf.Epsilon;
		animatorWalkParamCalculator.Update(val, Object.op_Implicit((Object)(object)base.characterDirection) ? base.characterDirection.animatorForward : base.transform.forward, in smoothingParameters, Time.fixedDeltaTime);
		if (useRootMotion)
		{
			if (characterAnimParamAvailability.mainRootPlaybackRate)
			{
				float num = 1f;
				if (Object.op_Implicit((Object)(object)base.modelLocator) && Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
				{
					num = base.modelLocator.modelTransform.localScale.x;
				}
				float num2 = base.characterBody.moveSpeed / (base.characterBody.mainRootSpeed * num);
				modelAnimator.SetFloat(AnimationParameters.mainRootPlaybackRate, num2);
			}
		}
		else if (characterAnimParamAvailability.walkSpeed)
		{
			modelAnimator.SetFloat(AnimationParameters.walkSpeed, base.characterBody.moveSpeed);
		}
		if (characterAnimParamAvailability.isGrounded)
		{
			modelAnimator.SetBool(AnimationParameters.isGrounded, base.isGrounded);
		}
		if (characterAnimParamAvailability.isMoving)
		{
			modelAnimator.SetBool(AnimationParameters.isMoving, flag);
		}
		if (characterAnimParamAvailability.turnAngle)
		{
			modelAnimator.SetFloat(AnimationParameters.turnAngle, animatorWalkParamCalculator.remainingTurnAngle, smoothingParameters.turnAngleSmoothDamp, Time.fixedDeltaTime);
		}
		if (characterAnimParamAvailability.isSprinting)
		{
			modelAnimator.SetBool(AnimationParameters.isSprinting, base.characterBody.isSprinting);
		}
		if (characterAnimParamAvailability.forwardSpeed)
		{
			modelAnimator.SetFloat(AnimationParameters.forwardSpeed, animatorWalkParamCalculator.animatorWalkSpeed.x, smoothingParameters.forwardSpeedSmoothDamp, Time.deltaTime);
		}
		if (characterAnimParamAvailability.rightSpeed)
		{
			modelAnimator.SetFloat(AnimationParameters.rightSpeed, animatorWalkParamCalculator.animatorWalkSpeed.y, smoothingParameters.rightSpeedSmoothDamp, Time.deltaTime);
		}
		if (characterAnimParamAvailability.upSpeed)
		{
			modelAnimator.SetFloat(AnimationParameters.upSpeed, estimatedVelocity.y, 0.1f, Time.deltaTime);
		}
	}
}
