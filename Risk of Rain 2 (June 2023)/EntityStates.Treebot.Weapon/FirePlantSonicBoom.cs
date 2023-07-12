using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Treebot.Weapon;

public class FirePlantSonicBoom : FireSonicBoom
{
	public static float damageCoefficient;

	public static float procCoefficient;

	public static GameObject hitEffectPrefab;

	public static float healthFractionPerHit;

	public static float healthCostFraction;

	public static string impactSoundString;

	public override void OnEnter()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.healthComponent) && healthCostFraction >= Mathf.Epsilon)
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
			damageInfo.position = base.characterBody.corePosition;
			damageInfo.force = Vector3.zero;
			damageInfo.damageColorIndex = DamageColorIndex.Default;
			damageInfo.crit = false;
			damageInfo.attacker = null;
			damageInfo.inflictor = null;
			damageInfo.damageType = DamageType.NonLethal;
			damageInfo.procCoefficient = 0f;
			damageInfo.procChainMask = default(ProcChainMask);
			base.healthComponent.TakeDamage(damageInfo);
		}
	}

	protected override void AddDebuff(CharacterBody body)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		((Component)body.healthComponent).GetComponent<SetStateOnHurt>()?.SetStun(-1f);
		if (Object.op_Implicit((Object)(object)hitEffectPrefab))
		{
			EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
			{
				origin = body.corePosition
			}, transmit: true);
		}
		if (Object.op_Implicit((Object)(object)base.healthComponent))
		{
			HealOrb healOrb = new HealOrb();
			healOrb.origin = body.corePosition;
			healOrb.target = base.healthComponent.body.mainHurtBox;
			healOrb.healValue = healthFractionPerHit * base.healthComponent.fullHealth;
			healOrb.overrideDuration = Random.Range(0.3f, 0.6f);
			OrbManager.instance.AddOrb(healOrb);
		}
		Util.PlaySound(impactSoundString, base.gameObject);
	}

	protected override float CalculateDamage()
	{
		return damageCoefficient * damageStat;
	}

	protected override float CalculateProcCoefficient()
	{
		return procCoefficient;
	}
}
