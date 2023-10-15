using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireCorruptHandBeam : BaseSkillState
{
	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public GameObject hitEffectPrefab;

	[SerializeField]
	public GameObject beamVfxPrefab;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string exitSoundString;

	[SerializeField]
	public float tickRate;

	[SerializeField]
	public float damageCoefficientPerSecond;

	[SerializeField]
	public float procCoefficientPerSecond;

	[SerializeField]
	public float forcePerSecond;

	[SerializeField]
	public float maxDistance;

	[SerializeField]
	public float minDistance;

	[SerializeField]
	public float bulletRadius;

	[SerializeField]
	public float baseMinimumDuration = 2f;

	[SerializeField]
	public float recoilAmplitude;

	[SerializeField]
	public float spreadBloomValue = 0.3f;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationEnterStateName;

	[SerializeField]
	public string animationExitStateName;

	private GameObject blinkVfxInstance;

	private float minimumDuration;

	private float fireCountdown;

	public override void OnEnter()
	{
		base.OnEnter();
		minimumDuration = baseMinimumDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationEnterStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		blinkVfxInstance = Object.Instantiate<GameObject>(beamVfxPrefab);
		blinkVfxInstance.transform.SetParent(base.characterBody.aimOriginTransform, false);
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		fireCountdown -= Time.fixedDeltaTime;
		if (fireCountdown <= 0f)
		{
			fireCountdown = 1f / tickRate / attackSpeedStat;
			FireBullet();
		}
		base.characterBody.SetAimTimer(3f);
		if (Object.op_Implicit((Object)(object)blinkVfxInstance))
		{
			Ray aimRay = GetAimRay();
			Vector3 point = aimRay.GetPoint(maxDistance);
			if (Util.CharacterRaycast(base.gameObject, GetAimRay(), out var hitInfo, maxDistance, LayerIndex.world.mask, (QueryTriggerInteraction)0))
			{
				point = ((RaycastHit)(ref hitInfo)).point;
			}
			blinkVfxInstance.transform.forward = point - blinkVfxInstance.transform.position;
		}
		if (((base.fixedAge >= minimumDuration && !IsKeyDownAuthority()) || base.characterBody.isSprinting) && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)blinkVfxInstance))
		{
			VfxKillBehavior.KillVfxObject(blinkVfxInstance);
		}
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
		}
		PlayAnimation(animationLayerName, animationExitStateName);
		Util.PlaySound(exitSoundString, base.gameObject);
		base.OnExit();
	}

	private void FireBullet()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.muzzleName = muzzle;
			bulletAttack.maxDistance = Mathf.Lerp(minDistance, maxDistance, Random.value);
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.radius = bulletRadius;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.smartCollision = false;
			bulletAttack.stopperMask = default(LayerMask);
			bulletAttack.hitMask = LayerIndex.entityPrecise.mask;
			bulletAttack.damage = damageCoefficientPerSecond * damageStat / tickRate;
			bulletAttack.procCoefficient = procCoefficientPerSecond / tickRate;
			bulletAttack.force = forcePerSecond / tickRate;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.Fire();
		}
		base.characterBody.AddSpreadBloom(spreadBloomValue);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
