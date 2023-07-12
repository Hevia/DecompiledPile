using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public static class MissileUtils
{
	public static void FireMissile(Vector3 position, CharacterBody attackerBody, ProcChainMask procChainMask, GameObject victim, float missileDamage, bool isCrit, GameObject projectilePrefab, DamageColorIndex damageColorIndex, bool addMissileProc)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		FireMissile(position, attackerBody, procChainMask, victim, missileDamage, isCrit, projectilePrefab, damageColorIndex, Vector3.up + Random.insideUnitSphere * 0.1f, 200f, addMissileProc);
	}

	public static void FireMissile(Vector3 position, CharacterBody attackerBody, ProcChainMask procChainMask, GameObject victim, float missileDamage, bool isCrit, GameObject projectilePrefab, DamageColorIndex damageColorIndex, Vector3 initialDirection, float force, bool addMissileProc)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		int num = attackerBody?.inventory?.GetItemCount(DLC1Content.Items.MoreMissile) ?? 0;
		float num2 = Mathf.Max(1f, 1f + 0.5f * (float)(num - 1));
		InputBankTest component = ((Component)attackerBody).GetComponent<InputBankTest>();
		ProcChainMask procChainMask2 = procChainMask;
		if (addMissileProc)
		{
			procChainMask2.AddProc(ProcType.Missile);
		}
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.position = position;
		fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(initialDirection);
		fireProjectileInfo.procChainMask = procChainMask2;
		fireProjectileInfo.target = victim;
		fireProjectileInfo.owner = ((Component)attackerBody).gameObject;
		fireProjectileInfo.damage = missileDamage * num2;
		fireProjectileInfo.crit = isCrit;
		fireProjectileInfo.force = force;
		fireProjectileInfo.damageColorIndex = damageColorIndex;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		if (num > 0)
		{
			Vector3 val = (Object.op_Implicit((Object)(object)component) ? component.aimDirection : ((Component)attackerBody).transform.position);
			FireProjectileInfo fireProjectileInfo3 = fireProjectileInfo2;
			fireProjectileInfo3.rotation = Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(45f, val) * initialDirection);
			ProjectileManager.instance.FireProjectile(fireProjectileInfo3);
			FireProjectileInfo fireProjectileInfo4 = fireProjectileInfo2;
			fireProjectileInfo4.rotation = Util.QuaternionSafeLookRotation(Quaternion.AngleAxis(-45f, val) * initialDirection);
			ProjectileManager.instance.FireProjectile(fireProjectileInfo4);
		}
	}
}
