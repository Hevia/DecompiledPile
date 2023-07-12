using UnityEngine;

namespace RoR2.Orbs;

public class LunarDetonatorOrb : Orb
{
	public float travelSpeed = 60f;

	public float baseDamage;

	public float damagePerStack;

	public GameObject attacker;

	public bool isCrit;

	public ProcChainMask procChainMask;

	public float procCoefficient;

	public DamageColorIndex damageColorIndex;

	public GameObject detonationEffectPrefab;

	public GameObject orbEffectPrefab;

	public override void Begin()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.duration = base.distanceToTarget / travelSpeed;
		EffectData effectData = new EffectData
		{
			scale = 1f,
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		if (Object.op_Implicit((Object)(object)orbEffectPrefab))
		{
			EffectManager.SpawnEffect(orbEffectPrefab, effectData, transmit: true);
		}
	}

	public override void OnArrival()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		base.OnArrival();
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		HealthComponent healthComponent = target.healthComponent;
		if (!Object.op_Implicit((Object)(object)healthComponent))
		{
			return;
		}
		CharacterBody body = healthComponent.body;
		if (Object.op_Implicit((Object)(object)body))
		{
			int buffCount = body.GetBuffCount(RoR2Content.Buffs.LunarDetonationCharge);
			if (buffCount > 0)
			{
				body.ClearTimedBuffs(RoR2Content.Buffs.LunarDetonationCharge);
				Vector3 position = ((Component)target).transform.position;
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = baseDamage + damagePerStack * (float)buffCount;
				damageInfo.attacker = attacker;
				damageInfo.inflictor = null;
				damageInfo.force = Vector3.zero;
				damageInfo.crit = isCrit;
				damageInfo.procChainMask = procChainMask;
				damageInfo.procCoefficient = procCoefficient;
				damageInfo.position = position;
				damageInfo.damageColorIndex = damageColorIndex;
				healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
				EffectManager.SpawnEffect(detonationEffectPrefab, new EffectData
				{
					origin = position,
					rotation = Quaternion.identity,
					scale = Mathf.Log((float)buffCount, 5f)
				}, transmit: true);
			}
		}
	}
}
