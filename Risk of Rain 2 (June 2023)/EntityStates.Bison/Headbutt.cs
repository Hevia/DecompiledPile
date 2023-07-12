using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Bison;

public class Headbutt : BaseState
{
	public static float baseHeadbuttDuration;

	public static float damageCoefficient;

	public static string attackSoundString;

	public static GameObject hitEffectPrefab;

	public static float upwardForceMagnitude;

	public static float awayForceMagnitude;

	private float headbuttDuration;

	private float stopwatch;

	private OverlapAttack attack;

	private Animator animator;

	private bool hasAttacked;

	public override void OnEnter()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		animator = ((Component)modelTransform).GetComponent<Animator>();
		headbuttDuration = baseHeadbuttDuration / attackSpeedStat;
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayCrossfade("Body", "Headbutt", "Headbutt.playbackRate", headbuttDuration, 0.2f);
		base.characterMotor.moveDirection = Vector3.zero;
		base.characterDirection.moveVector = base.characterDirection.forward;
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = Vector3.up * upwardForceMagnitude;
		attack.pushAwayForce = awayForceMagnitude;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Headbutt");
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator) && animator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
		{
			if (NetworkServer.active)
			{
				attack.Fire();
			}
			if (base.isAuthority && !hasAttacked)
			{
				hasAttacked = true;
			}
		}
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > headbuttDuration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
