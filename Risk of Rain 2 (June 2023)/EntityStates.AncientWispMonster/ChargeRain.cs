using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class ChargeRain : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject effectPrefab;

	public static GameObject delayPrefab;

	private float duration;

	public override void OnEnter()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
		PlayAnimation("Body", "ChargeRain", "ChargeRain.playbackRate", duration);
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(base.transform.position, Vector3.down, ref val, 999f, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(((RaycastHit)(ref val)).point, HullClassification.BeetleQueen);
			groundNodes.GetNodePosition(nodeIndex, out var position);
			base.transform.position = position + Vector3.up * 2f;
		}
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
			outer.SetNextState(new ChannelRain());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
