using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class DroneProjectileHoverStun : DroneProjectileHover
{
	protected override void Pulse()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		BlastAttack blastAttack = new BlastAttack
		{
			baseDamage = 0f,
			baseForce = 0f,
			attacker = (Object.op_Implicit((Object)(object)base.projectileController) ? base.projectileController.owner : null),
			inflictor = base.gameObject,
			bonusForce = Vector3.zero,
			attackerFiltering = AttackerFiltering.NeverHitSelf,
			crit = false,
			damageColorIndex = DamageColorIndex.Default,
			damageType = DamageType.Stun1s,
			falloffModel = BlastAttack.FalloffModel.None,
			procChainMask = default(ProcChainMask),
			position = base.transform.position,
			procCoefficient = 0f,
			teamIndex = (Object.op_Implicit((Object)(object)base.projectileController) ? base.projectileController.teamFilter.teamIndex : TeamIndex.None),
			radius = pulseRadius
		};
		blastAttack.Fire();
		EffectData effectData = new EffectData();
		effectData.origin = blastAttack.position;
		effectData.scale = blastAttack.radius;
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionVFX"), effectData, transmit: true);
	}
}
