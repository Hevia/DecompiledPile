using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class FireSecondaryProjectile : BaseState
{
	[SerializeField]
	public float baseDuration = 3f;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject muzzleEffectPrefab;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public string muzzleName = "SecondaryProjectileMuzzle";

	[SerializeField]
	public string animationStateName = "FireSecondaryProjectile";

	[SerializeField]
	public string playbackRateParam = "FireSecondaryProjectile.playbackRate";

	[SerializeField]
	public string animationLayerName = "Body";

	[SerializeField]
	public float baseFireDelay;

	private float duration;

	private float fireDelay;

	private bool hasFired;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		fireDelay = baseFireDelay / attackSpeedStat;
		if (fireDelay <= Time.fixedDeltaTime * 2f)
		{
			Fire();
		}
		PlayCrossfade(animationLayerName, animationStateName, playbackRateParam, duration, 0.2f);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!hasFired && base.fixedAge >= fireDelay)
		{
			Fire();
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void Fire()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		hasFired = true;
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (!base.isAuthority || !Object.op_Implicit((Object)(object)projectilePrefab))
		{
			return;
		}
		Ray aimRay = GetAimRay();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Ray)(ref aimRay)).origin = ((Component)component.FindChild(muzzleName)).transform.position;
			}
		}
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
	}
}
