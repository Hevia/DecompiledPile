using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AI.Walker;

public class Wander : BaseAIState
{
	private Vector3? targetPosition;

	private float lookTimer;

	private const float minLookDuration = 0.5f;

	private const float maxLookDuration = 4f;

	private const int lookTries = 1;

	private const float lookRaycastLength = 25f;

	private Vector3 targetLookPosition;

	private float aiUpdateTimer;

	private void PickNewTargetLookPosition()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.bodyInputBank))
		{
			float num = 0f;
			Vector3 aimOrigin = base.bodyInputBank.aimOrigin;
			Vector3 val = base.bodyInputBank.moveVector;
			if (val == Vector3.zero)
			{
				val = Random.onUnitSphere;
			}
			Ray val3 = default(Ray);
			RaycastHit val4 = default(RaycastHit);
			for (int i = 0; i < 1; i++)
			{
				Vector3 val2 = Util.ApplySpread(val, 0f, 60f, 0f, 0f);
				float num2 = 25f;
				((Ray)(ref val3))._002Ector(aimOrigin, val2);
				if (Physics.Raycast(val3, ref val4, 25f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					num2 = ((RaycastHit)(ref val4)).distance;
				}
				if (num2 > num)
				{
					num = num2;
					targetLookPosition = ((Ray)(ref val3)).GetPoint(num2);
				}
			}
		}
		lookTimer = Random.Range(0.5f, 4f);
	}

	public override void OnEnter()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.ai) && Object.op_Implicit((Object)(object)base.body))
		{
			BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
			targetPosition = PickRandomNearbyReachablePosition();
			if (targetPosition.HasValue)
			{
				broadNavigationAgent.goalPosition = targetPosition.Value;
				broadNavigationAgent.InvalidatePath();
			}
			PickNewTargetLookPosition();
			aiUpdateTimer = 0.5f;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		aiUpdateTimer -= Time.fixedDeltaTime;
		if (!Object.op_Implicit((Object)(object)base.ai) || !Object.op_Implicit((Object)(object)base.body))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)base.ai.skillDriverEvaluation.dominantSkillDriver))
		{
			outer.SetNextState(new Combat());
		}
		base.ai.SetGoalPosition(targetPosition);
		_ = base.bodyTransform.position;
		BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
		if (aiUpdateTimer <= 0f)
		{
			aiUpdateTimer = BaseAIState.cvAIUpdateInterval.value;
			base.ai.localNavigator.targetPosition = (Vector3)(((_003F?)broadNavigationAgent.output.nextPosition) ?? base.ai.localNavigator.targetPosition);
			base.ai.localNavigator.Update(BaseAIState.cvAIUpdateInterval.value);
			Vector3 val;
			if (Object.op_Implicit((Object)(object)base.bodyInputBank))
			{
				bodyInputs.moveVector = base.ai.localNavigator.moveVector * 0.25f;
				ref BaseAI.BodyInputs reference = ref bodyInputs;
				val = targetLookPosition - base.bodyInputBank.aimOrigin;
				reference.desiredAimDirection = ((Vector3)(ref val)).normalized;
			}
			lookTimer -= Time.fixedDeltaTime;
			if (lookTimer <= 0f)
			{
				PickNewTargetLookPosition();
			}
			bool flag = false;
			if (targetPosition.HasValue)
			{
				val = base.body.footPosition - targetPosition.Value;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				float num = base.body.radius * base.body.radius * 4f;
				flag = sqrMagnitude > num;
			}
			if (!flag)
			{
				outer.SetNextState(new LookBusy());
			}
		}
	}

	public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
	{
		ModifyInputsForJumpIfNeccessary(ref bodyInputs);
		return bodyInputs;
	}
}
