using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpMonster;

public class DoubleSlash : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float procCoefficient;

	public static float selfForce;

	public static float forceMagnitude = 16f;

	public static GameObject hitEffectPrefab;

	public static GameObject swipeEffectPrefab;

	public static string enterSoundString;

	public static string slashSoundString;

	public static float walkSpeedPenaltyCoefficient;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private int slashCount;

	private Transform modelTransform;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		attack.procCoefficient = procCoefficient;
		attack.damageType = DamageType.BleedOnHit;
		Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayAnimation("Gesture, Additive", "DoubleSlash", "DoubleSlash.playbackRate", duration);
			PlayAnimation("Gesture, Override", "DoubleSlash", "DoubleSlash.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration + 2f);
		}
	}

	public override void OnExit()
	{
		base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
		base.OnExit();
	}

	private void HandleSlash(string animatorParamName, string muzzleName, string hitBoxGroupName)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!(modelAnimator.GetFloat(animatorParamName) > 0.1f))
		{
			return;
		}
		Util.PlaySound(slashSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, muzzleName, transmit: true);
		slashCount++;
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
		attack.Fire();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator))
		{
			switch (slashCount)
			{
			case 0:
				HandleSlash("HandR.hitBoxActive", "SwipeRight", "HandR");
				break;
			case 1:
				HandleSlash("HandL.hitBoxActive", "SwipeLeft", "HandL");
				break;
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
