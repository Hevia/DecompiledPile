using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Loader;

public class FireHook : BaseSkillState
{
	[SerializeField]
	public GameObject projectilePrefab;

	public static float damageCoefficient;

	public static GameObject muzzleflashEffectPrefab;

	public static string fireSoundString;

	public GameObject hookInstance;

	protected ProjectileStickOnImpact hookStickOnImpact;

	private bool isStuck;

	private bool hadHookInstance;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.position = ((Ray)(ref aimRay)).origin;
			fireProjectileInfo.rotation = Quaternion.LookRotation(((Ray)(ref aimRay)).direction);
			fireProjectileInfo.crit = base.characterBody.RollCrit();
			fireProjectileInfo.damage = damageStat * damageCoefficient;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
			fireProjectileInfo.procChainMask = default(ProcChainMask);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.owner = base.gameObject;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
		EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", transmit: false);
		Util.PlaySound(fireSoundString, base.gameObject);
		PlayAnimation("Grapple", "FireHookIntro");
	}

	public void SetHookReference(GameObject hook)
	{
		hookInstance = hook;
		hookStickOnImpact = hook.GetComponent<ProjectileStickOnImpact>();
		hadHookInstance = true;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)hookStickOnImpact))
		{
			if (hookStickOnImpact.stuck && !isStuck)
			{
				PlayAnimation("Grapple", "FireHookLoop");
			}
			isStuck = hookStickOnImpact.stuck;
		}
		if (base.isAuthority && !Object.op_Implicit((Object)(object)hookInstance) && hadHookInstance)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		PlayAnimation("Grapple", "FireHookExit");
		EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", transmit: false);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
