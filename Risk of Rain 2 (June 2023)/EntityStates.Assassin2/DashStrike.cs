using System;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Assassin2;

public class DashStrike : BaseState
{
	public static float dashDuration = 1f;

	public static float slashDuration = 1.667f;

	public static float damageCoefficient = 4f;

	public static float procCoefficient;

	public static float selfForce;

	public static float forceMagnitude = 16f;

	public static float maxDashSpeedCoefficient = 4f;

	public static float maxSlashDistance = 20f;

	public static GameObject hitEffectPrefab;

	public static GameObject swipeEffectPrefab;

	public static string enterSoundString;

	public static string slashSoundString;

	private Vector3 targetMoveVector;

	private Vector3 targetMoveVectorVelocity;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private int slashCount;

	private Transform modelTransform;

	private bool dashComplete;

	private int handParamHash;

	private int swordParamHash;

	private float calculatedDashSpeed;

	private bool startedSlash;

	public override void OnEnter()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = (dashDuration + slashDuration) / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		base.characterMotor.velocity = Vector3.zero;
		Vector3 val = ((Component)base.characterBody.master).GetComponent<BaseAI>().currentEnemy.characterBody.corePosition - base.characterBody.corePosition;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		calculatedDashSpeed = Util.Remap(sqrMagnitude, 0f, maxSlashDistance * maxSlashDistance, 0f, maxDashSpeedCoefficient);
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		attack.procCoefficient = procCoefficient;
		attack.damageType = DamageType.Generic;
		Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture", "Dash");
			handParamHash = Animator.StringToHash("HandStrike");
			swordParamHash = Animator.StringToHash("SwordStrike");
		}
	}

	public override void OnExit()
	{
		base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
		base.OnExit();
	}

	private void HandleSlash(int animatorParamHash, string muzzleName, string hitBoxGroupName)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (modelAnimator.GetFloat(animatorParamHash) > 0.1f)
		{
			if (!startedSlash)
			{
				Util.PlaySound(slashSoundString, base.gameObject);
				EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, muzzleName, transmit: true);
				startedSlash = true;
			}
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitBoxGroupName);
			}
			if (Object.op_Implicit((Object)(object)base.healthComponent))
			{
				base.healthComponent.TakeDamageForce(base.characterDirection.forward * selfForce, alwaysApply: true);
			}
			attack.ResetIgnoredHealthComponents();
			if (Object.op_Implicit((Object)(object)base.characterDirection))
			{
				attack.forceVector = base.characterDirection.forward * forceMagnitude;
			}
			flag = attack.Fire();
		}
		else if (startedSlash || flag)
		{
			slashCount++;
			startedSlash = false;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!dashComplete)
		{
			Vector3 val = Vector3.ProjectOnPlane(Vector3.SmoothDamp(targetMoveVector, base.inputBank.aimDirection, ref targetMoveVectorVelocity, 0f, 0f), Vector3.up);
			targetMoveVector = ((Vector3)(ref val)).normalized;
			base.characterDirection.moveVector = targetMoveVector;
			Vector3 forward = base.characterDirection.forward;
			float num = moveSpeedStat * calculatedDashSpeed;
			base.characterMotor.moveDirection = forward * calculatedDashSpeed;
			modelAnimator.SetFloat("forwardSpeed", num);
		}
		if (NetworkServer.active)
		{
			if (base.fixedAge > dashDuration && !dashComplete)
			{
				PlayAnimation("Gesture", "SwordStrike");
				dashComplete = true;
			}
			if (Object.op_Implicit((Object)(object)modelAnimator))
			{
				switch (slashCount)
				{
				case 0:
					HandleSlash(handParamHash, "ShurikenTag", "ShurikenHitbox");
					break;
				case 1:
					HandleSlash(swordParamHash, "Sword", "SwordHitbox");
					break;
				}
			}
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
}
