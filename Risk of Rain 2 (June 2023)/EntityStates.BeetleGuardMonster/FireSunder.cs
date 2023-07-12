using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.BeetleGuardMonster;

public class FireSunder : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static string initialAttackSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject projectilePrefab;

	public static GameObject hitEffectPrefab;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private float duration;

	private GameObject rightHandChargeEffect;

	private ChildLocator modelChildLocator;

	private Transform handRTransform;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlaySound(initialAttackSoundString, base.gameObject);
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Body", "FireSunder", "FireSunder.playbackRate", duration, 0.2f);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration + 2f);
		}
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = true;
		}
		modelChildLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			GameObject val = chargeEffectPrefab;
			handRTransform = modelChildLocator.FindChild("HandR");
			if (Object.op_Implicit((Object)(object)handRTransform))
			{
				rightHandChargeEffect = Object.Instantiate<GameObject>(val, handRTransform);
			}
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)rightHandChargeEffect);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("FireSunder.activate") > 0.5f && !hasAttacked)
		{
			if (base.isAuthority && Object.op_Implicit((Object)(object)modelTransform))
			{
				Ray aimRay = GetAimRay();
				((Ray)(ref aimRay)).origin = handRTransform.position;
				ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageStat * damageCoefficient, forceMagnitude, Util.CheckRoll(critStat, base.characterBody.master));
			}
			hasAttacked = true;
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
