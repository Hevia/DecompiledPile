using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AncientWispMonster;

public class Enrage : BaseState
{
	public static float baseDuration = 3.5f;

	public static GameObject enragePrefab;

	private Animator modelAnimator;

	private float duration;

	private bool hasCastBuff;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayCrossfade("Gesture", "Enrage", "Enrage.playbackRate", duration, 0.2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Enrage.activate") > 0.5f && !hasCastBuff)
		{
			EffectData effectData = new EffectData();
			effectData.origin = base.transform.position;
			effectData.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(enragePrefab, effectData, transmit: true);
			hasCastBuff = true;
			base.characterBody.AddBuff(JunkContent.Buffs.EnrageAncientWisp);
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private static void PullEnemies(Vector3 position, Vector3 direction, float coneAngle, float maxDistance, float force, TeamIndex excludedTeam)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos(coneAngle * 0.5f * (MathF.PI / 180f));
		Collider[] array = Physics.OverlapSphere(position, maxDistance);
		foreach (Collider val in array)
		{
			Vector3 position2 = ((Component)val).transform.position;
			Vector3 val2 = position - position2;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			if (!(Vector3.Dot(-normalized, direction) >= num))
			{
				continue;
			}
			TeamIndex teamIndex = TeamIndex.Neutral;
			TeamComponent component = ((Component)val).GetComponent<TeamComponent>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				continue;
			}
			teamIndex = component.teamIndex;
			if (teamIndex != excludedTeam)
			{
				CharacterMotor component2 = ((Component)val).GetComponent<CharacterMotor>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.ApplyForce(normalized * force);
				}
				Rigidbody component3 = ((Component)val).GetComponent<Rigidbody>();
				if (Object.op_Implicit((Object)(object)component3))
				{
					component3.AddForce(normalized * force, (ForceMode)1);
				}
			}
		}
	}
}
