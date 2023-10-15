using EntityStates.Sniper.Scope;
using RoR2;
using UnityEngine;

namespace EntityStates.Sniper.SniperWeapon;

public class FireRifle : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float minChargeDamageCoefficient;

	public static float maxChargeDamageCoefficient;

	public static float minChargeForce;

	public static float maxChargeForce;

	public static int bulletCount;

	public static float baseDuration = 2f;

	public static string attackSoundString;

	public static float recoilAmplitude;

	public static float spreadBloomValue = 0.3f;

	public static float interruptInterval = 0.2f;

	private float duration;

	private bool inputReleased;

	public override void OnEnter()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		float num = 0f;
		if (Object.op_Implicit((Object)(object)base.skillLocator))
		{
			GenericSkill secondary = base.skillLocator.secondary;
			if (Object.op_Implicit((Object)(object)secondary))
			{
				EntityStateMachine stateMachine = secondary.stateMachine;
				if (Object.op_Implicit((Object)(object)stateMachine) && stateMachine.state is ScopeSniper scopeSniper)
				{
					num = scopeSniper.charge;
					scopeSniper.charge = 0f;
				}
			}
		}
		AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
		duration = baseDuration / attackSpeedStat;
		Ray aimRay = GetAimRay();
		StartAimMode(aimRay);
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayAnimation("Gesture", "FireShotgun", "FireShotgun.playbackRate", duration * 1.1f);
		string muzzleName = "MuzzleShotgun";
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
			bulletAttack.bulletCount = ((bulletCount > 0) ? ((uint)bulletCount) : 0u);
			bulletAttack.procCoefficient = 1f / (float)bulletCount;
			bulletAttack.damage = Mathf.LerpUnclamped(minChargeDamageCoefficient, maxChargeDamageCoefficient, num) * damageStat / (float)bulletCount;
			bulletAttack.force = Mathf.LerpUnclamped(minChargeForce, maxChargeForce, num);
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleName;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			if (num == 1f)
			{
				bulletAttack.stopperMask = LayerIndex.world.mask;
			}
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = 0f;
			bulletAttack.sniper = true;
			bulletAttack.Fire();
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			inputReleased |= !base.inputBank.skill1.down;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (inputReleased && base.fixedAge >= interruptInterval / attackSpeedStat)
		{
			return InterruptPriority.Any;
		}
		return InterruptPriority.Skill;
	}
}
