using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileDamage))]
[RequireComponent(typeof(HitBoxGroup))]
public class ProjectileOverlapAttack : MonoBehaviour, IProjectileImpactBehavior
{
	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	public float damageCoefficient;

	public GameObject impactEffect;

	public Vector3 forceVector;

	public float overlapProcCoefficient = 1f;

	public int maximumOverlapTargets = 100;

	public UnityEvent onServerHit;

	private OverlapAttack attack;

	public float fireFrequency = 60f;

	[Tooltip("If non-negative, the attack clears its hit memory at the specified interval.")]
	public float resetInterval = -1f;

	private float resetTimer;

	private float fireTimer;

	private void Start()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		attack = new OverlapAttack();
		attack.procChainMask = projectileController.procChainMask;
		attack.procCoefficient = projectileController.procCoefficient * overlapProcCoefficient;
		attack.attacker = projectileController.owner;
		attack.inflictor = ((Component)this).gameObject;
		attack.teamIndex = projectileController.teamFilter.teamIndex;
		attack.damage = damageCoefficient * projectileDamage.damage;
		attack.forceVector = forceVector + projectileDamage.force * ((Component)this).transform.forward;
		attack.hitEffectPrefab = impactEffect;
		attack.isCrit = projectileDamage.crit;
		attack.damageColorIndex = projectileDamage.damageColorIndex;
		attack.damageType = projectileDamage.damageType;
		attack.procChainMask = projectileController.procChainMask;
		attack.maximumOverlapTargets = maximumOverlapTargets;
		attack.hitBoxGroup = ((Component)this).GetComponent<HitBoxGroup>();
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
	}

	public void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (resetInterval >= 0f)
		{
			resetTimer -= Time.fixedDeltaTime;
			if (resetTimer <= 0f)
			{
				resetTimer = resetInterval;
				ResetOverlapAttack();
			}
		}
		fireTimer -= Time.fixedDeltaTime;
		if (!(fireTimer <= 0f))
		{
			return;
		}
		fireTimer = 1f / fireFrequency;
		attack.damage = damageCoefficient * projectileDamage.damage;
		if (attack.Fire())
		{
			UnityEvent obj = onServerHit;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	public void ResetOverlapAttack()
	{
		attack.damageType = projectileDamage.damageType;
		attack.ResetIgnoredHealthComponents();
	}

	public void SetDamageCoefficient(float newDamageCoefficient)
	{
		damageCoefficient = newDamageCoefficient;
	}

	public void AddToDamageCoefficient(float bonusDamageCoefficient)
	{
		damageCoefficient += bonusDamageCoefficient;
	}
}
