using RoR2;
using UnityEngine;

namespace EntityStates.SurvivorPod;

public class PreRelease : SurvivorPodBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Base", "IdleToRelease");
		Util.PlaySound("Play_UI_podBlastDoorOpen", base.gameObject);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			((Component)((Component)modelTransform).GetComponent<ChildLocator>().FindChild("InitialExhaustFX")).gameObject.SetActive(true);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		Animator modelAnimator = GetModelAnimator();
		if (!Object.op_Implicit((Object)(object)modelAnimator))
		{
			return;
		}
		int layerIndex = modelAnimator.GetLayerIndex("Base");
		if (layerIndex != -1)
		{
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("IdleToReleaseFinished"))
			{
				outer.SetNextState(new Release());
			}
		}
	}
}
