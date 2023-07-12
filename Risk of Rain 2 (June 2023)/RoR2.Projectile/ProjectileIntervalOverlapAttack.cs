using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileDamage))]
public class ProjectileIntervalOverlapAttack : MonoBehaviour
{
	public HitBoxGroup hitBoxGroup;

	public float interval;

	public float damageCoefficient = 1f;

	private float countdown;

	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
	}

	private void Start()
	{
		countdown = 0f;
	}

	private void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		countdown -= Time.fixedDeltaTime;
		if (countdown <= 0f)
		{
			countdown += interval;
			if (Object.op_Implicit((Object)(object)hitBoxGroup))
			{
				OverlapAttack overlapAttack = new OverlapAttack();
				overlapAttack.attacker = projectileController.owner;
				overlapAttack.inflictor = ((Component)this).gameObject;
				overlapAttack.teamIndex = projectileController.teamFilter.teamIndex;
				overlapAttack.damage = projectileDamage.damage * damageCoefficient;
				overlapAttack.hitBoxGroup = hitBoxGroup;
				overlapAttack.isCrit = projectileDamage.crit;
				overlapAttack.procCoefficient = 0f;
				overlapAttack.damageType = projectileDamage.damageType;
				overlapAttack.Fire();
			}
		}
	}
}
