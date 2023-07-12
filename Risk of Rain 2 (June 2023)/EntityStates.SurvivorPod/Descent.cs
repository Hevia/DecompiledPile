using RoR2;
using UnityEngine;

namespace EntityStates.SurvivorPod;

public class Descent : SurvivorPodBaseState
{
	private ShakeEmitter shakeEmitter;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "InitialSpawn");
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("Travel");
			if (Object.op_Implicit((Object)(object)val))
			{
				shakeEmitter = ((Component)val).gameObject.AddComponent<ShakeEmitter>();
				shakeEmitter.wave = new Wave
				{
					amplitude = 1f,
					frequency = 180f,
					cycleOffset = 0f
				};
				shakeEmitter.duration = 10000f;
				shakeEmitter.radius = 400f;
				shakeEmitter.amplitudeTimeDecay = false;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			AuthorityFixedUpdate();
		}
	}

	protected void AuthorityFixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Animator modelAnimator = GetModelAnimator();
		if (!Object.op_Implicit((Object)(object)modelAnimator))
		{
			return;
		}
		int layerIndex = modelAnimator.GetLayerIndex("Base");
		if (layerIndex != -1)
		{
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Idle"))
			{
				TransitionIntoNextState();
			}
		}
	}

	protected virtual void TransitionIntoNextState()
	{
		outer.SetNextState(new Landed());
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)shakeEmitter);
		base.OnExit();
	}
}
