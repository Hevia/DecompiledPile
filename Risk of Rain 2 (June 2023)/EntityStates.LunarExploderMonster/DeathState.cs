using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LunarExploderMonster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject deathPreExplosionVFX;

	public static GameObject deathExplosionEffect;

	public static GameObject projectilePrefab;

	public static float projectileVerticalSpawnOffset;

	public static float projectileDamageCoefficient;

	public static int projectileCount;

	public static float duration;

	public static float deathExplosionRadius;

	public static string muzzleName;

	private GameObject deathPreExplosionInstance;

	private Transform muzzleTransform;

	private bool hasExploded;

	private Vector3? explosionPosition;

	protected override bool shouldAutoDestroy => false;

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		PlayCrossfade("Body", "Death", "Death.playbackRate", duration, 0.1f);
	}

	public override void OnEnter()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		muzzleTransform = FindModelChild(muzzleName);
		if (!Object.op_Implicit((Object)(object)muzzleTransform))
		{
			return;
		}
		explosionPosition = muzzleTransform.position;
		if (Object.op_Implicit((Object)(object)deathPreExplosionVFX))
		{
			deathPreExplosionInstance = Object.Instantiate<GameObject>(deathPreExplosionVFX, muzzleTransform.position, muzzleTransform.rotation);
			deathPreExplosionInstance.transform.parent = muzzleTransform;
			deathPreExplosionInstance.transform.localScale = Vector3.one * deathExplosionRadius;
			ScaleParticleSystemDuration component = deathPreExplosionInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)muzzleTransform))
		{
			explosionPosition = muzzleTransform.position;
		}
		if (base.fixedAge >= duration && !hasExploded)
		{
			if (base.isAuthority)
			{
				FireExplosion();
			}
			DestroyModel();
			if (NetworkServer.active)
			{
				DestroyBodyAsapServer();
			}
			hasExploded = true;
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}

	private void FireExplosion()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (!hasExploded && Object.op_Implicit((Object)(object)base.cachedModelTransform) && base.isAuthority)
		{
			for (int i = 0; i < projectileCount; i++)
			{
				float num = 360f / (float)projectileCount;
				Vector3 forward = Util.QuaternionSafeLookRotation(base.cachedModelTransform.forward, base.cachedModelTransform.up) * Util.ApplySpread(Vector3.forward, 0f, 0f, 1f, 1f, num * (float)i);
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = base.characterBody.corePosition + Vector3.up * projectileVerticalSpawnOffset;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(forward);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * projectileDamageCoefficient;
				fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
			if (Object.op_Implicit((Object)(object)deathExplosionEffect))
			{
				EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
				{
					origin = base.characterBody.corePosition,
					scale = deathExplosionRadius
				}, transmit: true);
			}
		}
	}
}
