using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidMegaCrab;

public class DeathState : GenericCharacterDeath
{
	public static GameObject deathBombProjectile;

	public static float duration;

	public static string muzzleName;

	private Transform muzzleTransform;

	protected override bool shouldAutoDestroy => false;

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		PlayCrossfade("Body", "Death", "Death.playbackRate", duration, 0.1f);
		PlayAnimation("Left Gun Override (Arm)", "Empty");
		PlayAnimation("Right Gun Override (Arm)", "Empty");
	}

	public override void OnEnter()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		muzzleTransform = FindModelChild(muzzleName);
		if (Object.op_Implicit((Object)(object)muzzleTransform) && base.isAuthority)
		{
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = deathBombProjectile;
			fireProjectileInfo.position = muzzleTransform.position;
			fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(Vector3.up);
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat;
			fireProjectileInfo.crit = base.characterBody.RollCrit();
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			DestroyModel();
			if (NetworkServer.active)
			{
				DestroyBodyAsapServer();
			}
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
