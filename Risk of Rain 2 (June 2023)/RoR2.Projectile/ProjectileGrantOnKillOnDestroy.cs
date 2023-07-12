using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileDamage))]
[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(HealthComponent))]
public class ProjectileGrantOnKillOnDestroy : MonoBehaviour
{
	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private HealthComponent healthComponent;

	private void OnDestroy()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		healthComponent = ((Component)this).GetComponent<HealthComponent>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)projectileController.owner))
		{
			DamageInfo damageInfo = new DamageInfo
			{
				attacker = projectileController.owner,
				crit = projectileDamage.crit,
				damage = projectileDamage.damage,
				position = ((Component)this).transform.position,
				procCoefficient = projectileController.procCoefficient,
				damageType = projectileDamage.damageType,
				damageColorIndex = projectileDamage.damageColorIndex
			};
			HealthComponent victim = healthComponent;
			DamageReport damageReport = new DamageReport(damageInfo, victim, damageInfo.damage, healthComponent.combinedHealth);
			GlobalEventManager.instance.OnCharacterDeath(damageReport);
		}
	}
}
