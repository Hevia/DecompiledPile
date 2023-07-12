using UnityEngine;

namespace RoR2.Orbs;

public class DamageOrb : Orb
{
	public enum DamageOrbType
	{
		ClayGooOrb
	}

	private float speed = 60f;

	public float damageValue;

	public GameObject attacker;

	public TeamIndex teamIndex;

	public bool isCrit;

	public ProcChainMask procChainMask;

	public float procCoefficient = 0.2f;

	public DamageColorIndex damageColorIndex;

	public DamageOrbType damageOrbType;

	private DamageType orbDamageType;

	public override void Begin()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		GameObject effectPrefab = null;
		if (damageOrbType == DamageOrbType.ClayGooOrb)
		{
			speed = 5f;
			effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ClayGooOrbEffect");
			orbDamageType = DamageType.ClayGoo;
		}
		base.duration = base.distanceToTarget / speed;
		EffectData effectData = new EffectData
		{
			origin = origin,
			genericFloat = base.duration
		};
		effectData.SetHurtBoxReference(target);
		EffectManager.SpawnEffect(effectPrefab, effectData, transmit: true);
	}

	public override void OnArrival()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		HealthComponent healthComponent = target.healthComponent;
		if (!Object.op_Implicit((Object)(object)healthComponent))
		{
			return;
		}
		if (damageOrbType == DamageOrbType.ClayGooOrb)
		{
			CharacterBody component = ((Component)healthComponent).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component) && (component.bodyFlags & CharacterBody.BodyFlags.ImmuneToGoo) != 0)
			{
				healthComponent.Heal(damageValue, default(ProcChainMask));
				return;
			}
		}
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
		damageInfo.damageType = orbDamageType;
		healthComponent.TakeDamage(damageInfo);
		GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
		GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
	}
}
