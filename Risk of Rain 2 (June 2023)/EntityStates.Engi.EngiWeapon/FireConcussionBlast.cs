using RoR2;
using UnityEngine;

namespace EntityStates.Engi.EngiWeapon;

public class FireConcussionBlast : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static int grenadeCountMax = 3;

	public static float damageCoefficient;

	public static float fireDuration = 1f;

	public static float baseDuration = 2f;

	public static float minSpread = 0f;

	public static float maxSpread = 5f;

	public static float recoilAmplitude = 1f;

	public static string attackSoundString;

	public static float force;

	public static float maxDistance;

	public static float radius;

	public static GameObject tracerEffectPrefab;

	private Ray aimRay;

	private Transform modelTransform;

	private float duration;

	private float fireTimer;

	private int grenadeCount;

	private void FireGrenade(string targetMuzzle)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(attackSoundString, base.gameObject);
		aimRay = GetAimRay();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(targetMuzzle);
				if (Object.op_Implicit((Object)(object)val))
				{
					aimRay.origin = val.position;
				}
			}
		}
		AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -1f * recoilAmplitude, 1f * recoilAmplitude);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.maxDistance = maxDistance;
			bulletAttack.radius = radius;
			bulletAttack.stopperMask = LayerMask.op_Implicit(0);
			bulletAttack.Fire();
		}
	}

	public override void OnEnter()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelTransform = GetModelTransform();
		aimRay = GetAimRay();
		StartAimMode(aimRay);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		fireTimer -= Time.fixedDeltaTime;
		float num = fireDuration / attackSpeedStat / (float)grenadeCountMax;
		if (fireTimer <= 0f && grenadeCount < grenadeCountMax)
		{
			fireTimer += num;
			if (grenadeCount % 2 == 0)
			{
				FireGrenade("MuzzleLeft");
				PlayCrossfade("Gesture, Left Cannon", "FireGrenadeLeft", 0.1f);
			}
			else
			{
				FireGrenade("MuzzleRight");
				PlayCrossfade("Gesture, Right Cannon", "FireGrenadeRight", 0.1f);
			}
			grenadeCount++;
		}
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
