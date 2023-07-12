using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GravekeeperMonster.Weapon;

public class GravekeeperBarrage : BaseState
{
	private float stopwatch;

	private float missileStopwatch;

	public static float baseDuration;

	public static string muzzleString;

	public static float missileSpawnFrequency;

	public static float missileSpawnDelay;

	public static float missileForce;

	public static float damageCoefficient;

	public static float maxSpread;

	public static GameObject projectilePrefab;

	public static GameObject muzzleflashPrefab;

	public static string jarEffectChildLocatorString;

	public static string jarOpenSoundString;

	public static string jarCloseSoundString;

	public static GameObject jarOpenEffectPrefab;

	public static GameObject jarCloseEffectPrefab;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		base.OnEnter();
		missileStopwatch -= missileSpawnDelay;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				((Component)childLocator.FindChild("JarEffectLoop")).gameObject.SetActive(true);
			}
		}
		PlayAnimation("Jar, Override", "BeginGravekeeperBarrage");
		EffectManager.SimpleMuzzleFlash(jarOpenEffectPrefab, base.gameObject, jarEffectChildLocatorString, transmit: false);
		Util.PlaySound(jarOpenSoundString, base.gameObject);
		base.characterBody.SetAimTimer(baseDuration + 2f);
	}

	private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((Ray)(ref projectileRay)).direction = Util.ApplySpread(((Ray)(ref projectileRay)).direction, 0f, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
		EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, muzzleString, transmit: false);
		if (NetworkServer.active)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref projectileRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref projectileRay)).direction), base.gameObject, damageStat * damageCoefficient, missileForce, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void OnExit()
	{
		PlayCrossfade("Jar, Override", "EndGravekeeperBarrage", 0.06f);
		EffectManager.SimpleMuzzleFlash(jarCloseEffectPrefab, base.gameObject, jarEffectChildLocatorString, transmit: false);
		Util.PlaySound(jarCloseSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			((Component)childLocator.FindChild("JarEffectLoop")).gameObject.SetActive(false);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		missileStopwatch += Time.fixedDeltaTime;
		if (missileStopwatch >= 1f / missileSpawnFrequency)
		{
			missileStopwatch -= 1f / missileSpawnFrequency;
			Transform val = childLocator.FindChild(muzzleString);
			if (Object.op_Implicit((Object)(object)val))
			{
				Ray projectileRay = default(Ray);
				((Ray)(ref projectileRay)).origin = val.position;
				Ray aimRay = GetAimRay();
				((Ray)(ref projectileRay)).direction = ((Ray)(ref aimRay)).direction;
				float num = 1000f;
				RaycastHit val2 = default(RaycastHit);
				if (Physics.Raycast(GetAimRay(), ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					((Ray)(ref projectileRay)).direction = ((RaycastHit)(ref val2)).point - val.position;
				}
				FireBlob(projectileRay, 0f, 0f);
			}
		}
		if (stopwatch >= baseDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
