using UnityEngine;

namespace RoR2.Orbs;

public class GenericDamageOrb : Orb
{
	public float speed = 60f;

	public float damageValue;

	public GameObject attacker;

	public TeamIndex teamIndex;

	public bool isCrit;

	public float scale;

	public ProcChainMask procChainMask;

	public float procCoefficient = 0.2f;

	public DamageColorIndex damageColorIndex;

	public DamageType damageType;

	public override void Begin()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.duration = base.distanceToTarget / speed;
		if (Object.op_Implicit((Object)(object)GetOrbEffect()))
		{
			EffectData effectData = new EffectData
			{
				scale = scale,
				origin = origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(target);
			EffectManager.SpawnEffect(GetOrbEffect(), effectData, transmit: true);
		}
	}

	protected virtual GameObject GetOrbEffect()
	{
		return null;
	}

	public override void OnArrival()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			HealthComponent healthComponent = target.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = damageValue;
				damageInfo.attacker = attacker;
				damageInfo.inflictor = null;
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
			}
		}
	}
}
