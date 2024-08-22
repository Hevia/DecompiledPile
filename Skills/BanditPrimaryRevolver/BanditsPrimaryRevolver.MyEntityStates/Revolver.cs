using EntityStates;
using EntityStates.Bandit2.Weapon;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BanditsPrimaryRevolver.MyEntityStates;

public class Revolver : BaseSkillState
{
	public float baseDuration = 0.5f;

	private float duration;

	public float revolverDuration;

	public GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2CrosshairPrepRevolverFire.prefab").WaitForCompletion();

	private Animator animator;

	private int bodySideWeaponLayerIndex;

	private OverrideRequest crosshairOverrideRequest;

	public GameObject hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit2Pistol.prefab").WaitForCompletion();

	public GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBanditPistol.prefab").WaitForCompletion();

	public GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();

	public virtual string exitAnimationStateName => "BufferEmpty";

	public override void OnEnter()
	{
		base.OnEnter();
		this.skillLocator.primary.finalRechargeInterval = this.skillLocator.primary.baseRechargeInterval / base.attackSpeedStat;
		animator = this.GetModelAnimator();
		if (animator)
		{
			bodySideWeaponLayerIndex = animator.GetLayerIndex("Body, SideWeapon");
			animator.SetLayerWeight(bodySideWeaponLayerIndex, 1f);
		}

		if (crosshairOverridePrefab)
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(this.characterBody, crosshairOverridePrefab, (OverridePriority)1);
		}

		this.characterBody.SetAimTimer(3f);
		duration = baseDuration / base.attackSpeedStat;
		revolverDuration = duration;
		Ray aimRay = base.GetAimRay();
		base.StartAimMode(aimRay, 2f, false);
		this.PlayAnimation("Gesture, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", duration);
		Util.PlaySound("Play_bandit2_R_fire", this.gameObject);
		base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, this.gameObject, "MuzzlePistol", false);
		}
		if (this.isAuthority)
		{
			new BulletAttack
			{
				owner = this.gameObject,
				weapon = this.gameObject,
				origin = aimRay.origin,
				aimVector = aimRay.direction,
				minSpread = 0f,
				maxSpread = this.characterBody.spreadBloomAngle,
				bulletCount = 1u,
				procCoefficient = 1f,
				damage = this.characterBody.damage * 6f,
				force = 3f,
				falloffModel = (FalloffModel)1,
				tracerEffectPrefab = tracerEffectPrefab,
				muzzleName = "MuzzlePistol",
				hitEffectPrefab = hitEffectPrefab,
				isCrit = base.RollCrit(),
				HitEffectNormal = false,
				stopperMask = ((LayerIndex)(ref LayerIndex.world)).mask,
				smartCollision = true,
				maxDistance = 300f
			}.Fire();
		}
	}

	public override void OnExit()
	{
		this.OnExit();
		if (animator)
		{
			animator.SetLayerWeight(bodySideWeaponLayerIndex, 0f);
		}
		Transform val = base.FindModelChild("SpinningPistolFX");
	}

	public override void FixedUpdate()
	{
		this.FixedUpdate();
		if (this.fixedAge >= duration && this.isAuthority)
		{
			this.outer.SetNextState(new ExitSidearmRevolver());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return (InterruptPriority)1;
	}
}
