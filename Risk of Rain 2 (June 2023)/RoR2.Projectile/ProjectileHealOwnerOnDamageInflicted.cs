using RoR2.Orbs;
using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileHealOwnerOnDamageInflicted : MonoBehaviour, IOnDamageInflictedServerReceiver
{
	public float fractionOfDamage;

	private ProjectileController projectileController;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
	}

	public void OnDamageInflictedServer(DamageReport damageReport)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)projectileController.owner))
		{
			HealthComponent component = projectileController.owner.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				HealOrb healOrb = new HealOrb();
				healOrb.origin = ((Component)this).transform.position;
				healOrb.target = component.body.mainHurtBox;
				healOrb.healValue = damageReport.damageDealt * fractionOfDamage;
				healOrb.overrideDuration = 0.3f;
				OrbManager.instance.AddOrb(healOrb);
			}
		}
	}
}
