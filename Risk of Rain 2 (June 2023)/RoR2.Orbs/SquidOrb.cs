using UnityEngine;

namespace RoR2.Orbs;

public class SquidOrb : GenericDamageOrb
{
	public float forceScalar;

	public override void Begin()
	{
		speed = 120f;
		base.Begin();
	}

	protected override GameObject GetOrbEffect()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/SquidOrbEffect");
	}

	public override void OnArrival()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		HealthComponent healthComponent = target.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			Vector3 val = ((Component)target).transform.position - origin;
			if (((Vector3)(ref val)).sqrMagnitude > 0f)
			{
				((Vector3)(ref val)).Normalize();
				val *= forceScalar;
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
			damageInfo.damageType = damageType;
			damageInfo.force = val;
			healthComponent.TakeDamage(damageInfo);
			GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
			GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
		}
	}
}
