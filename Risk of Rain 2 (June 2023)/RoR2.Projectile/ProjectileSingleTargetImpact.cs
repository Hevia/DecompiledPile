using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileSingleTargetImpact : MonoBehaviour, IProjectileImpactBehavior
{
	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private bool alive = true;

	public bool destroyWhenNotAlive = true;

	public bool destroyOnWorld;

	public GameObject impactEffect;

	public string hitSoundString;

	public string enemyHitSoundString;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		if (!alive)
		{
			return;
		}
		Collider collider = impactInfo.collider;
		if (Object.op_Implicit((Object)(object)collider))
		{
			DamageInfo damageInfo = new DamageInfo();
			if (Object.op_Implicit((Object)(object)projectileDamage))
			{
				damageInfo.damage = projectileDamage.damage;
				damageInfo.crit = projectileDamage.crit;
				damageInfo.attacker = projectileController.owner;
				damageInfo.inflictor = ((Component)this).gameObject;
				damageInfo.position = impactInfo.estimatedPointOfImpact;
				damageInfo.force = projectileDamage.force * ((Component)this).transform.forward;
				damageInfo.procChainMask = projectileController.procChainMask;
				damageInfo.procCoefficient = projectileController.procCoefficient;
				damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
				damageInfo.damageType = projectileDamage.damageType;
			}
			else
			{
				Debug.Log((object)"No projectile damage component!");
			}
			HurtBox component = ((Component)collider).GetComponent<HurtBox>();
			if (Object.op_Implicit((Object)(object)component))
			{
				HealthComponent healthComponent = component.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					if ((Object)(object)((Component)healthComponent).gameObject == (Object)(object)projectileController.owner)
					{
						return;
					}
					if (FriendlyFireManager.ShouldDirectHitProceed(healthComponent, projectileController.teamFilter.teamIndex))
					{
						Util.PlaySound(enemyHitSoundString, ((Component)this).gameObject);
						if (NetworkServer.active)
						{
							damageInfo.ModifyDamageInfo(component.damageModifier);
							healthComponent.TakeDamage(damageInfo);
							GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)component.healthComponent).gameObject);
						}
					}
					alive = false;
				}
			}
			else if (destroyOnWorld)
			{
				alive = false;
			}
			damageInfo.position = ((Component)this).transform.position;
			if (NetworkServer.active)
			{
				GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)collider).gameObject);
			}
		}
		if (!alive)
		{
			if (NetworkServer.active && Object.op_Implicit((Object)(object)impactEffect))
			{
				EffectManager.SimpleImpactEffect(impactEffect, impactInfo.estimatedPointOfImpact, -((Component)this).transform.forward, !projectileController.isPrediction);
			}
			Util.PlaySound(hitSoundString, ((Component)this).gameObject);
			if (destroyWhenNotAlive)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}
}
