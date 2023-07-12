using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleGuardMonster;

public class GroundSlam : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	private OverlapAttack attack;

	public static string initialAttackSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject slamEffectPrefab;

	public static GameObject hitEffectPrefab;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private float duration;

	private GameObject leftHandChargeEffect;

	private GameObject rightHandChargeEffect;

	private ChildLocator modelChildLocator;

	private Transform groundSlamIndicatorInstance;

	private void EnableIndicator(Transform indicator)
	{
		if (Object.op_Implicit((Object)(object)indicator))
		{
			((Component)indicator).gameObject.SetActive(true);
			ObjectScaleCurve component = ((Component)indicator).gameObject.GetComponent<ObjectScaleCurve>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.time = 0f;
			}
		}
	}

	private void DisableIndicator(Transform indicator)
	{
		if (Object.op_Implicit((Object)(object)indicator))
		{
			((Component)indicator).gameObject.SetActive(false);
		}
	}

	public override void OnEnter()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlaySound(initialAttackSoundString, base.gameObject);
		Object.op_Implicit((Object)(object)base.characterDirection);
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.forceVector = Vector3.up * forceMagnitude;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			attack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "GroundSlam");
		}
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Body", "GroundSlam", "GroundSlam.playbackRate", duration, 0.2f);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		modelChildLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			GameObject val = chargeEffectPrefab;
			Transform val2 = modelChildLocator.FindChild("HandL");
			Transform val3 = modelChildLocator.FindChild("HandR");
			if (Object.op_Implicit((Object)(object)val2))
			{
				leftHandChargeEffect = Object.Instantiate<GameObject>(val, val2);
			}
			if (Object.op_Implicit((Object)(object)val3))
			{
				rightHandChargeEffect = Object.Instantiate<GameObject>(val, val3);
			}
			groundSlamIndicatorInstance = modelChildLocator.FindChild("GroundSlamIndicator");
			EnableIndicator(groundSlamIndicatorInstance);
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)leftHandChargeEffect);
		EntityState.Destroy((Object)(object)rightHandChargeEffect);
		DisableIndicator(groundSlamIndicatorInstance);
		Object.op_Implicit((Object)(object)base.characterDirection);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("GroundSlam.hitBoxActive") > 0.5f && !hasAttacked)
		{
			if (NetworkServer.active)
			{
				attack.Fire();
			}
			if (base.isAuthority && Object.op_Implicit((Object)(object)modelTransform))
			{
				DisableIndicator(groundSlamIndicatorInstance);
				EffectManager.SimpleMuzzleFlash(slamEffectPrefab, base.gameObject, "SlamZone", transmit: true);
			}
			hasAttacked = true;
			EntityState.Destroy((Object)(object)leftHandChargeEffect);
			EntityState.Destroy((Object)(object)rightHandChargeEffect);
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
