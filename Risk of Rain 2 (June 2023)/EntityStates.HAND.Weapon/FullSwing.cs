using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.HAND.Weapon;

public class FullSwing : BaseState
{
	public static float baseDuration = 3.5f;

	public static float returnToIdlePercentage;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	public static GameObject hitEffectPrefab;

	public static GameObject swingEffectPrefab;

	private Transform hammerChildTransform;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private bool hasSwung;

	public override void OnEnter()
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Hammer");
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				hammerChildTransform = component.FindChild("SwingCenter");
			}
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Gesture");
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("FullSwing3"))
			{
				currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
				if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("FullSwing1"))
				{
					currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
					if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("FullSwing2"))
					{
						PlayCrossfade("Gesture", "FullSwing3", "FullSwing.playbackRate", duration / (1f - returnToIdlePercentage), 0.2f);
					}
					else
					{
						PlayCrossfade("Gesture", "FullSwing1", "FullSwing.playbackRate", duration / (1f - returnToIdlePercentage), 0.2f);
					}
					goto IL_0212;
				}
			}
			PlayCrossfade("Gesture", "FullSwing2", "FullSwing.playbackRate", duration / (1f - returnToIdlePercentage), 0.2f);
		}
		goto IL_0212;
		IL_0212:
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Hammer.hitBoxActive") > 0.5f)
		{
			if (!hasSwung)
			{
				EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);
				hasSwung = true;
			}
			attack.forceVector = hammerChildTransform.right * (0f - forceMagnitude);
			attack.Fire();
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
