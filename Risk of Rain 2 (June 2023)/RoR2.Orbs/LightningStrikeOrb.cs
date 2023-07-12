using UnityEngine;

namespace RoR2.Orbs;

public class LightningStrikeOrb : GenericDamageOrb, IOrbFixedUpdateBehavior
{
	private Vector3 lastKnownTargetPosition;

	private static readonly float positionLockDuration = 0.3f;

	public override void Begin()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.Begin();
		base.duration = 0.5f;
		if (Object.op_Implicit((Object)(object)target))
		{
			lastKnownTargetPosition = ((Component)target).transform.position;
		}
	}

	protected override GameObject GetOrbEffect()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect");
	}

	public override void OnArrival()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact"), new EffectData
		{
			origin = lastKnownTargetPosition
		}, transmit: true);
		if (Object.op_Implicit((Object)(object)attacker))
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = attacker;
			blastAttack.baseDamage = damageValue;
			blastAttack.baseForce = 0f;
			blastAttack.bonusForce = Vector3.down * 3000f;
			blastAttack.crit = isCrit;
			blastAttack.damageColorIndex = DamageColorIndex.Item;
			blastAttack.damageType = DamageType.Stun1s;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.inflictor = null;
			blastAttack.position = lastKnownTargetPosition;
			blastAttack.procChainMask = procChainMask;
			blastAttack.procCoefficient = 1f;
			blastAttack.radius = 3f;
			blastAttack.teamIndex = TeamComponent.GetObjectTeam(attacker);
			blastAttack.Fire();
		}
	}

	public void FixedUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target) && base.timeUntilArrival >= positionLockDuration)
		{
			lastKnownTargetPosition = ((Component)target).transform.position;
		}
	}
}
