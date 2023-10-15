using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpBossMonster;

public class FireVoidspikes : BaseState
{
	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float procCoefficient;

	public static float selfForce;

	public static float forceMagnitude = 16f;

	public static GameObject hitEffectPrefab;

	public static GameObject swipeEffectPrefab;

	public static string enterSoundString;

	public static string attackSoundString;

	public static float walkSpeedPenaltyCoefficient;

	public static int projectileCount;

	public static float projectileYawSpread;

	public static float projectileDamageCoefficient;

	public static float projectileSpeed;

	public static float projectileSpeedPerProjectile;

	public static GameObject projectilePrefab;

	private OverlapAttack attack;

	private Animator modelAnimator;

	private float duration;

	private int slashCount;

	private Transform modelTransform;

	private int chosenAnim = -1;

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
		attack.teamIndex = GetTeam();
		attack.damage = damageCoefficient * damageStat;
		attack.hitEffectPrefab = hitEffectPrefab;
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
		attack.procCoefficient = procCoefficient;
		attack.damageType = DamageType.BleedOnHit;
		Util.PlaySound(enterSoundString, base.gameObject);
		if (base.isAuthority)
		{
			chosenAnim = ((!Util.CheckRoll(50f)) ? 1 : 0);
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			string animationStateName = ((chosenAnim == 1) ? "FireVoidspikesL" : "FireVoidspikesR");
			PlayAnimation("Gesture, Additive", animationStateName, "FireVoidspikes.playbackRate", duration);
			PlayAnimation("Gesture, Override", animationStateName, "FireVoidspikes.playbackRate", duration);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration + 3f);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && slashCount <= 0)
		{
			if (modelAnimator.GetFloat("HandR.hitBoxActive") > 0.1f)
			{
				FireSpikeFan(GetAimRay(), "FireVoidspikesR", "HandR");
			}
			if (modelAnimator.GetFloat("HandL.hitBoxActive") > 0.1f)
			{
				FireSpikeFan(GetAimRay(), "FireVoidspikesL", "HandL");
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireSpikeFan(Ray aimRay, string muzzleName, string hitBoxGroupName)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(attackSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, muzzleName, transmit: false);
		slashCount++;
		if (base.isAuthority)
		{
			Vector3 forward = base.characterDirection.forward;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				attack.hitBoxGroup = FindHitBoxGroup(hitBoxGroupName);
				attack.forceVector = forward * forceMagnitude;
				attack.Fire();
			}
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				base.characterMotor.ApplyForce(forward * selfForce, alwaysApply: true);
			}
			for (int i = 0; i < projectileCount; i++)
			{
				FireSpikeAuthority(aimRay, 0f, ((float)projectileCount / 2f - (float)i) * projectileYawSpread, projectileSpeed + projectileSpeedPerProjectile * (float)i);
			}
		}
	}

	private void FireSpikeAuthority(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, bonusYaw, bonusPitch);
		ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * projectileDamageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speed);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((char)chosenAnim);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		chosenAnim = reader.ReadChar();
	}
}
