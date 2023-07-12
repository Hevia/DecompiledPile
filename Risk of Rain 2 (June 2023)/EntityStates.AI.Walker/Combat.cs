using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AI.Walker;

public class Combat : BaseAIState
{
	private float strafeDirection;

	private const float strafeDuration = 0.25f;

	private float strafeTimer;

	private float activeSoundTimer;

	private float aiUpdateTimer;

	private const float minUpdateInterval = 1f / 6f;

	private const float maxUpdateInterval = 0.2f;

	private AISkillDriver dominantSkillDriver;

	protected bool currentSkillMeetsActivationConditions;

	protected SkillSlot currentSkillSlot = SkillSlot.None;

	protected Vector3 myBodyFootPosition;

	private float lastPathUpdate;

	private float fallbackNodeStartAge;

	private readonly float fallbackNodeDuration = 4f;

	public override void OnEnter()
	{
		base.OnEnter();
		activeSoundTimer = Random.Range(3f, 8f);
		if (Object.op_Implicit((Object)(object)base.ai))
		{
			lastPathUpdate = base.ai.broadNavigationAgent.output.lastPathUpdate;
			base.ai.broadNavigationAgent.InvalidatePath();
		}
		fallbackNodeStartAge = float.NegativeInfinity;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!Object.op_Implicit((Object)(object)base.ai) || !Object.op_Implicit((Object)(object)base.body))
		{
			return;
		}
		aiUpdateTimer -= Time.fixedDeltaTime;
		strafeTimer -= Time.fixedDeltaTime;
		UpdateFootPosition();
		if (aiUpdateTimer <= 0f)
		{
			aiUpdateTimer = BaseAIState.cvAIUpdateInterval.value;
			UpdateAI(BaseAIState.cvAIUpdateInterval.value);
			if (!Object.op_Implicit((Object)(object)dominantSkillDriver))
			{
				outer.SetNextState(new LookBusy());
			}
		}
		UpdateBark();
	}

	protected void UpdateFootPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		myBodyFootPosition = base.body.footPosition;
		BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
		broadNavigationAgent.currentPosition = myBodyFootPosition;
	}

	protected void UpdateAI(float deltaTime)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		BaseAI.SkillDriverEvaluation skillDriverEvaluation = base.ai.skillDriverEvaluation;
		dominantSkillDriver = skillDriverEvaluation.dominantSkillDriver;
		currentSkillSlot = SkillSlot.None;
		currentSkillMeetsActivationConditions = false;
		bodyInputs.moveVector = Vector3.zero;
		AISkillDriver.MovementType movementType = AISkillDriver.MovementType.Stop;
		float num = 1f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (!Object.op_Implicit((Object)(object)base.body) || !Object.op_Implicit((Object)(object)base.bodyInputBank))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)dominantSkillDriver))
		{
			movementType = dominantSkillDriver.movementType;
			currentSkillSlot = dominantSkillDriver.skillSlot;
			flag = dominantSkillDriver.activationRequiresTargetLoS;
			flag2 = dominantSkillDriver.activationRequiresAimTargetLoS;
			flag3 = dominantSkillDriver.activationRequiresAimConfirmation;
			num = dominantSkillDriver.moveInputScale;
		}
		Vector3 position = base.bodyTransform.position;
		_ = base.bodyInputBank.aimOrigin;
		BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
		BroadNavigationSystem.AgentOutput output = broadNavigationAgent.output;
		BaseAI.Target target = skillDriverEvaluation.target;
		if (Object.op_Implicit((Object)(object)target?.gameObject))
		{
			target.GetBullseyePosition(out var position2);
			Vector3 val = position2;
			if (fallbackNodeStartAge + fallbackNodeDuration < base.fixedAge)
			{
				base.ai.SetGoalPosition(target);
			}
			Vector3 val2 = position;
			bool allowWalkOffCliff = true;
			Vector3 val3 = (Vector3)((!Object.op_Implicit((Object)(object)dominantSkillDriver) || !dominantSkillDriver.ignoreNodeGraph) ? (((_003F?)output.nextPosition) ?? myBodyFootPosition) : ((!base.body.isFlying) ? val : position2));
			Vector3 val4 = val3 - myBodyFootPosition;
			Vector3 val5 = ((Vector3)(ref val4)).normalized * 10f;
			Vector3 val6 = Vector3.Cross(Vector3.up, val5);
			switch (movementType)
			{
			case AISkillDriver.MovementType.ChaseMoveTarget:
				val2 = val3 + (position - myBodyFootPosition);
				break;
			case AISkillDriver.MovementType.FleeMoveTarget:
				val2 -= val5;
				break;
			case AISkillDriver.MovementType.StrafeMovetarget:
				if (strafeTimer <= 0f)
				{
					if (strafeDirection == 0f)
					{
						strafeDirection = ((Random.Range(0, 1) == 0) ? (-1f) : 1f);
					}
					strafeTimer = 0.25f;
				}
				val2 += val6 * strafeDirection;
				allowWalkOffCliff = false;
				break;
			}
			base.ai.localNavigator.targetPosition = val2;
			base.ai.localNavigator.allowWalkOffCliff = allowWalkOffCliff;
			base.ai.localNavigator.Update(deltaTime);
			if (base.ai.localNavigator.wasObstructedLastUpdate)
			{
				strafeDirection *= -1f;
			}
			bodyInputs.moveVector = base.ai.localNavigator.moveVector;
			ref Vector3 moveVector = ref bodyInputs.moveVector;
			moveVector *= num;
			if (!flag3 || base.ai.hasAimConfirmation)
			{
				bool flag4 = true;
				if (skillDriverEvaluation.target == skillDriverEvaluation.aimTarget && flag && flag2)
				{
					flag2 = false;
				}
				if (flag4 && flag)
				{
					flag4 = skillDriverEvaluation.target.TestLOSNow();
				}
				if (flag4 && flag2)
				{
					flag4 = skillDriverEvaluation.aimTarget.TestLOSNow();
				}
				if (flag4)
				{
					currentSkillMeetsActivationConditions = true;
				}
			}
		}
		if (output.lastPathUpdate > lastPathUpdate && !output.targetReachable && fallbackNodeStartAge + fallbackNodeDuration < base.fixedAge)
		{
			broadNavigationAgent.goalPosition = PickRandomNearbyReachablePosition();
			broadNavigationAgent.InvalidatePath();
		}
		lastPathUpdate = output.lastPathUpdate;
	}

	public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		bool pressSkill = false;
		bool pressSkill2 = false;
		bool pressSkill3 = false;
		bool pressSkill4 = false;
		if (Object.op_Implicit((Object)(object)base.bodyInputBank))
		{
			AISkillDriver.ButtonPressType buttonPressType = AISkillDriver.ButtonPressType.Abstain;
			if (Object.op_Implicit((Object)(object)dominantSkillDriver))
			{
				buttonPressType = dominantSkillDriver.buttonPressType;
			}
			bool flag = false;
			switch (currentSkillSlot)
			{
			case SkillSlot.Primary:
				flag = previousBodyInputs.pressSkill1;
				break;
			case SkillSlot.Secondary:
				flag = previousBodyInputs.pressSkill2;
				break;
			case SkillSlot.Utility:
				flag = previousBodyInputs.pressSkill3;
				break;
			case SkillSlot.Special:
				flag = previousBodyInputs.pressSkill4;
				break;
			}
			bool flag2 = currentSkillMeetsActivationConditions;
			switch (buttonPressType)
			{
			case AISkillDriver.ButtonPressType.Abstain:
				flag2 = false;
				break;
			case AISkillDriver.ButtonPressType.TapContinuous:
				flag2 = flag2 && !flag;
				break;
			}
			switch (currentSkillSlot)
			{
			case SkillSlot.Primary:
				pressSkill = flag2;
				break;
			case SkillSlot.Secondary:
				pressSkill2 = flag2;
				break;
			case SkillSlot.Utility:
				pressSkill3 = flag2;
				break;
			case SkillSlot.Special:
				pressSkill4 = flag2;
				break;
			}
		}
		bodyInputs.pressSkill1 = pressSkill;
		bodyInputs.pressSkill2 = pressSkill2;
		bodyInputs.pressSkill3 = pressSkill3;
		bodyInputs.pressSkill4 = pressSkill4;
		bodyInputs.pressSprint = false;
		bodyInputs.pressActivateEquipment = false;
		bodyInputs.desiredAimDirection = Vector3.zero;
		if (Object.op_Implicit((Object)(object)dominantSkillDriver))
		{
			bodyInputs.pressSprint = dominantSkillDriver.shouldSprint;
			bodyInputs.pressActivateEquipment = dominantSkillDriver.shouldFireEquipment && !previousBodyInputs.pressActivateEquipment;
			AISkillDriver.AimType aimType = dominantSkillDriver.aimType;
			BaseAI.Target aimTarget = base.ai.skillDriverEvaluation.aimTarget;
			if (aimType == AISkillDriver.AimType.MoveDirection)
			{
				AimInDirection(ref bodyInputs, bodyInputs.moveVector);
			}
			if (aimTarget != null)
			{
				AimAt(ref bodyInputs, aimTarget);
			}
		}
		ModifyInputsForJumpIfNeccessary(ref bodyInputs);
		return bodyInputs;
	}

	protected void UpdateBark()
	{
		activeSoundTimer -= Time.fixedDeltaTime;
		if (activeSoundTimer <= 0f)
		{
			activeSoundTimer = Random.Range(3f, 8f);
			base.body.CallRpcBark();
		}
	}
}
