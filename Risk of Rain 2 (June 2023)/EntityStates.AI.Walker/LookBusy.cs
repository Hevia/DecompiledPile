using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace EntityStates.AI.Walker;

public class LookBusy : BaseAIState
{
	private const float minDuration = 2f;

	private const float maxDuration = 7f;

	private Vector3 targetPosition;

	private float duration;

	private float lookTimer;

	private const float minLookDuration = 0.5f;

	private const float maxLookDuration = 4f;

	private const int lookTries = 4;

	private const float lookRaycastLength = 25f;

	protected virtual void PickNewTargetLookDirection()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.bodyInputBank))
		{
			float num = 0f;
			Vector3 aimOrigin = base.bodyInputBank.aimOrigin;
			RaycastHit val = default(RaycastHit);
			for (int i = 0; i < 4; i++)
			{
				Vector3 onUnitSphere = Random.onUnitSphere;
				float num2 = 25f;
				if (Physics.Raycast(new Ray(aimOrigin, onUnitSphere), ref val, 25f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					num2 = ((RaycastHit)(ref val)).distance;
				}
				if (num2 > num)
				{
					num = num2;
					bodyInputs.desiredAimDirection = onUnitSphere;
				}
			}
		}
		lookTimer = Random.Range(0.5f, 4f);
	}

	public override void OnEnter()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = Random.Range(2f, 7f);
		base.bodyInputBank.moveVector = Vector3.zero;
		base.bodyInputBank.jump.PushState(newState: false);
		PickNewTargetLookDirection();
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
		if (Object.op_Implicit((Object)(object)base.ai.skillDriverEvaluation.dominantSkillDriver))
		{
			outer.SetNextState(new Combat());
		}
		if (base.ai.hasAimConfirmation)
		{
			lookTimer -= Time.fixedDeltaTime;
			if (lookTimer <= 0f)
			{
				PickNewTargetLookDirection();
			}
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new Wander());
		}
	}

	public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
	{
		return bodyInputs;
	}
}
