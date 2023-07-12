using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class EndRain : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject effectPrefab;

	public static GameObject delayPrefab;

	private float duration;

	public override void OnEnter()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
		PlayAnimation("Body", "EndRain", "EndRain.playbackRate", duration);
		NodeGraph airNodes = SceneInfo.instance.airNodes;
		NodeGraph.NodeIndex nodeIndex = airNodes.FindClosestNode(base.transform.position, base.characterBody.hullClassification);
		airNodes.GetNodePosition(nodeIndex, out var position);
		base.transform.position = position;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
