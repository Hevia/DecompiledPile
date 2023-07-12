using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.TitanMonster;

public class FireGoldMegaLaser : FireMegaLaser
{
	public static GameObject projectilePrefab;

	public static float projectileFireFrequency;

	public static float projectileDamageCoefficient;

	public static float projectileMinSpread;

	public static float projectileMaxSpread;

	private float projectileStopwatch;

	public override void FixedUpdate()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		projectileStopwatch += Time.fixedDeltaTime * attackSpeedStat;
		if (projectileStopwatch >= 1f / projectileFireFrequency)
		{
			Ray val = GetAimRay();
			if (Object.op_Implicit((Object)(object)muzzleTransform))
			{
				((Ray)(ref val)).origin = ((Component)muzzleTransform).transform.position;
			}
			((Ray)(ref val)).direction = Util.ApplySpread(((Ray)(ref val)).direction, projectileMinSpread, projectileMaxSpread, 1f, 1f);
			projectileStopwatch -= 1f / projectileFireFrequency;
			ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref val)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref val)).direction), base.gameObject, damageStat * FireMegaLaser.damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}
}
