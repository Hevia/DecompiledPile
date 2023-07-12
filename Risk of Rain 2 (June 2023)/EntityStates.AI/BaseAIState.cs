using System.Collections.Generic;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ConVar;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AI;

public abstract class BaseAIState : EntityState
{
	protected static FloatConVar cvAIUpdateInterval = new FloatConVar("ai_update_interval", ConVarFlags.Cheat, "0.2", "Frequency that the local navigator refreshes.");

	protected BaseAI.BodyInputs bodyInputs;

	protected bool isInJump;

	protected Vector3? jumpLockedMoveVector;

	protected CharacterMaster characterMaster { get; private set; }

	protected BaseAI ai { get; private set; }

	protected CharacterBody body { get; private set; }

	protected Transform bodyTransform { get; private set; }

	protected InputBankTest bodyInputBank { get; private set; }

	protected CharacterMotor bodyCharacterMotor { get; private set; }

	protected SkillLocator bodySkillLocator { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		characterMaster = GetComponent<CharacterMaster>();
		ai = GetComponent<BaseAI>();
		if (Object.op_Implicit((Object)(object)ai))
		{
			body = ai.body;
			bodyTransform = ai.bodyTransform;
			bodyInputBank = ai.bodyInputBank;
			bodyCharacterMotor = ai.bodyCharacterMotor;
			bodySkillLocator = ai.bodySkillLocator;
		}
		bodyInputs = default(BaseAI.BodyInputs);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public virtual BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
	{
		return bodyInputs;
	}

	protected void ModifyInputsForJumpIfNeccessary(ref BaseAI.BodyInputs bodyInputs)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ai))
		{
			return;
		}
		BroadNavigationSystem.AgentOutput output = ai.broadNavigationAgent.output;
		bodyInputs.pressJump = false;
		if (!Object.op_Implicit((Object)(object)bodyCharacterMotor))
		{
			return;
		}
		bool isGrounded = bodyCharacterMotor.isGrounded;
		bool flag = isGrounded || bodyCharacterMotor.isFlying || !bodyCharacterMotor.useGravity;
		if (isInJump && flag)
		{
			isInJump = false;
			jumpLockedMoveVector = null;
		}
		if (isGrounded)
		{
			float num = Mathf.Max(output.desiredJumpVelocity, ai.localNavigator.jumpSpeed);
			if (num > 0f && body.jumpPower > 0f)
			{
				bool num2 = output.desiredJumpVelocity >= ai.localNavigator.jumpSpeed;
				num = body.jumpPower;
				bodyInputs.pressJump = true;
				if (num2 && output.nextPosition.HasValue)
				{
					Vector3 val = output.nextPosition.Value - bodyTransform.position;
					Vector3 val2 = val;
					val2.y = 0f;
					float num3 = Trajectory.CalculateFlightDuration(0f, val.y, num);
					float walkSpeed = bodyCharacterMotor.walkSpeed;
					if (num3 > 0f && walkSpeed > 0f)
					{
						float magnitude = ((Vector3)(ref val2)).magnitude;
						float num4 = Mathf.Max(magnitude / num3 / bodyCharacterMotor.walkSpeed, 0f);
						jumpLockedMoveVector = val2 * (num4 / magnitude);
						bodyCharacterMotor.moveDirection = jumpLockedMoveVector.Value;
					}
				}
				isInJump = true;
			}
		}
		if (jumpLockedMoveVector.HasValue)
		{
			bodyInputs.moveVector = jumpLockedMoveVector.Value;
		}
	}

	protected Vector3? PickRandomNearbyReachablePosition()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ai) || !Object.op_Implicit((Object)(object)body))
		{
			return null;
		}
		NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
		NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(nodeGraph, (HullMask)(1 << (int)body.hullClassification));
		NodeGraph.NodeIndex nodeIndex = nodeGraph.FindClosestNode(bodyTransform.position, body.hullClassification, 50f);
		nodeGraphSpider.AddNodeForNextStep(nodeIndex);
		for (int i = 0; i < 6; i++)
		{
			nodeGraphSpider.PerformStep();
		}
		List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
		if (collectedSteps.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, collectedSteps.Count);
		NodeGraph.NodeIndex node = collectedSteps[index].node;
		if (nodeGraph.GetNodePosition(node, out var position))
		{
			return position;
		}
		return null;
	}

	protected void AimAt(ref BaseAI.BodyInputs dest, BaseAI.Target aimTarget)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (aimTarget != null && aimTarget.GetBullseyePosition(out var position))
		{
			Vector3 val = position - bodyInputBank.aimOrigin;
			dest.desiredAimDirection = ((Vector3)(ref val)).normalized;
		}
	}

	protected void AimInDirection(ref BaseAI.BodyInputs dest, Vector3 aimDirection)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (aimDirection != Vector3.zero)
		{
			dest.desiredAimDirection = aimDirection;
		}
	}
}
