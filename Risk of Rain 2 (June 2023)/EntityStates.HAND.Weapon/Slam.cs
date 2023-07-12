using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.HAND.Weapon;

public class Slam : BaseState
{
	public static float baseDuration = 3.5f;

	public static float returnToIdlePercentage;

	public static float impactDamageCoefficient = 2f;

	public static float earthquakeDamageCoefficient = 2f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	public static GameObject hitEffectPrefab;

	public static GameObject swingEffectPrefab;

	public static GameObject projectilePrefab;

	private Transform hammerChildTransform;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private bool hasSwung;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = impactDamageCoefficient * damageStat;
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
			PlayAnimation("Gesture", "Slam", "Slam.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Hammer.hitBoxActive") > 0.5f)
		{
			if (!hasSwung)
			{
				Ray aimRay = GetAimRay();
				EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);
				ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageStat * earthquakeDamageCoefficient, forceMagnitude, Util.CheckRoll(critStat, base.characterBody.master));
				hasSwung = true;
			}
			attack.forceVector = hammerChildTransform.right;
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
}
