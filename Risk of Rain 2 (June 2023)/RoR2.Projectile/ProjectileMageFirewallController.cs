using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileDamage))]
public class ProjectileMageFirewallController : MonoBehaviour, IProjectileImpactBehavior
{
	public GameObject walkerPrefab;

	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private bool consumed;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
	}

	void IProjectileImpactBehavior.OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		if (!consumed)
		{
			consumed = true;
			CreateWalkers();
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void CreateWalkers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		Vector3 val = Vector3.Cross(Vector3.up, forward);
		ProjectileManager.instance.FireProjectile(walkerPrefab, ((Component)this).transform.position + Vector3.up, Util.QuaternionSafeLookRotation(val), projectileController.owner, projectileDamage.damage, projectileDamage.force, projectileDamage.crit, projectileDamage.damageColorIndex);
		ProjectileManager.instance.FireProjectile(walkerPrefab, ((Component)this).transform.position + Vector3.up, Util.QuaternionSafeLookRotation(-val), projectileController.owner, projectileDamage.damage, projectileDamage.force, projectileDamage.crit, projectileDamage.damageColorIndex);
	}
}
