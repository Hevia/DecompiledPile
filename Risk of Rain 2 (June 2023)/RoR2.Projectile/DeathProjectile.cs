using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(ProjectileDamage))]
[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileStickOnImpact))]
public class DeathProjectile : MonoBehaviour
{
	private ProjectileStickOnImpact projectileStickOnImpactController;

	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private HealthComponent healthComponent;

	public GameObject OnKillTickEffect;

	public TeamIndex teamIndex;

	public string activeSoundLoopString;

	public string exitSoundString;

	private float duration;

	private float fixedAge;

	public float baseDuration = 8f;

	public float radius = 500f;

	public GameObject rotateObject;

	private bool doneWithRemovalEvents;

	public float removalTime = 1f;

	private bool shouldStopSound;

	private void Awake()
	{
		projectileStickOnImpactController = ((Component)this).GetComponent<ProjectileStickOnImpact>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		healthComponent = ((Component)this).GetComponent<HealthComponent>();
		duration = baseDuration;
		fixedAge = 0f;
	}

	private void FixedUpdate()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		fixedAge += Time.deltaTime;
		if (duration > 0f)
		{
			if (!(fixedAge >= 1f))
			{
				return;
			}
			if (projectileStickOnImpactController.stuck)
			{
				if (Object.op_Implicit((Object)(object)projectileController.owner))
				{
					RotateDoll(Random.Range(90f, 180f));
					SpawnTickEffect();
					if (NetworkServer.active)
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
				duration -= 1f;
			}
			fixedAge = 0f;
		}
		else
		{
			if (!doneWithRemovalEvents)
			{
				doneWithRemovalEvents = true;
				((Behaviour)rotateObject.GetComponent<ObjectScaleCurve>()).enabled = true;
			}
			if (fixedAge >= removalTime)
			{
				Util.PlaySound(exitSoundString, ((Component)this).gameObject);
				shouldStopSound = false;
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	private void OnDisable()
	{
		if (shouldStopSound)
		{
			Util.PlaySound(exitSoundString, ((Component)this).gameObject);
			shouldStopSound = false;
		}
	}

	public void SpawnTickEffect()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Quaternion.identity
		};
		EffectManager.SpawnEffect(OnKillTickEffect, effectData, transmit: false);
	}

	public void PlayStickSoundLoop()
	{
		Util.PlaySound(activeSoundLoopString, ((Component)this).gameObject);
		shouldStopSound = true;
	}

	public void RotateDoll(float rotationAmount)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		rotateObject.transform.Rotate(new Vector3(0f, 0f, rotationAmount));
	}
}
