using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireCrabCannon : BaseState
{
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public int grenadeCountMax = 3;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public float fireDuration = 1f;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	private static float recoilAmplitude = 1f;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string perGrenadeSoundString;

	[SerializeField]
	public float spreadBloomValue = 0.3f;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private int grenadeCount;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		StartAimMode();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void FireProjectile()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(perGrenadeSoundString, base.gameObject);
		AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
		base.characterBody.AddSpreadBloom(spreadBloomValue);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			((Ray)(ref aimRay)).direction = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, maxSpread, 1f, 1f);
			Vector3 onUnitSphere = Random.onUnitSphere;
			Vector3.ProjectOnPlane(onUnitSphere, ((Ray)(ref aimRay)).direction);
			Quaternion rotation = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction, onUnitSphere);
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, rotation, base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		fireTimer -= Time.fixedDeltaTime;
		float num = fireDuration / attackSpeedStat / (float)grenadeCountMax;
		if (fireTimer <= 0f && grenadeCount < grenadeCountMax)
		{
			FireProjectile();
			fireTimer += num;
			grenadeCount++;
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
