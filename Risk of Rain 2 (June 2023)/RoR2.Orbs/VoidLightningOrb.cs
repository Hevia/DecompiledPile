using UnityEngine;

namespace RoR2.Orbs;

public class VoidLightningOrb : Orb, IOrbFixedUpdateBehavior
{
	public float damageValue;

	public GameObject attacker;

	public GameObject inflictor;

	public int totalStrikes;

	public float secondsPerStrike = 0.5f;

	public TeamIndex teamIndex;

	public bool isCrit;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	public DamageColorIndex damageColorIndex;

	public DamageType damageType;

	private GameObject effectPrefab;

	private float accumulatedTime;

	public override void Begin()
	{
		accumulatedTime = 0f;
		base.duration = (float)(totalStrikes - 1) * secondsPerStrike;
		effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/VoidLightningOrbEffect");
		Strike();
	}

	public override void OnArrival()
	{
	}

	public void FixedUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		accumulatedTime += Time.fixedDeltaTime;
		while (accumulatedTime > secondsPerStrike)
		{
			accumulatedTime -= secondsPerStrike;
			if (Object.op_Implicit((Object)(object)target))
			{
				origin = ((Component)target).transform.position;
				Strike();
			}
		}
	}

	private void Strike()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		HealthComponent healthComponent = target.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = damageValue;
			damageInfo.attacker = attacker;
			damageInfo.inflictor = inflictor;
			damageInfo.force = Vector3.zero;
			damageInfo.crit = isCrit;
			damageInfo.procChainMask = procChainMask;
			damageInfo.procCoefficient = procCoefficient;
			damageInfo.position = ((Component)target).transform.position;
			damageInfo.damageColorIndex = damageColorIndex;
			damageInfo.damageType = damageType;
			healthComponent.TakeDamage(damageInfo);
			GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
			GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
			if (Object.op_Implicit((Object)(object)target.hurtBoxGroup))
			{
				target = target.hurtBoxGroup.hurtBoxes[Random.Range(0, target.hurtBoxGroup.hurtBoxes.Length)];
			}
			EffectData effectData = new EffectData
			{
				origin = origin,
				genericFloat = 0.1f
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(effectPrefab, effectData, transmit: true);
		}
	}
}
