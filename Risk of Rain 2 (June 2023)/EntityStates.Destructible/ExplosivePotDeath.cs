using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Destructible;

public class ExplosivePotDeath : BaseState
{
	public static GameObject chargePrefab;

	public static float chargeDuration;

	public static GameObject explosionEffectPrefab;

	public static float explosionRadius;

	public static float explosionDamageCoefficient;

	public static float explosionProcCoefficient;

	public static float explosionForce;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)chargePrefab))
		{
			Object.Instantiate<GameObject>(chargePrefab, base.transform);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= chargeDuration)
		{
			Explode();
		}
	}

	private void Explode()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)explosionEffectPrefab))
		{
			EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
			{
				origin = base.transform.position,
				scale = explosionRadius,
				rotation = Quaternion.identity
			}, transmit: true);
		}
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.attacker = base.gameObject;
		blastAttack.damageColorIndex = DamageColorIndex.Item;
		blastAttack.baseDamage = damageStat * explosionDamageCoefficient * Run.instance.teamlessDamageCoefficient;
		blastAttack.radius = explosionRadius;
		blastAttack.falloffModel = BlastAttack.FalloffModel.None;
		blastAttack.procCoefficient = explosionProcCoefficient;
		blastAttack.teamIndex = TeamIndex.None;
		blastAttack.damageType = DamageType.ClayGoo;
		blastAttack.position = base.transform.position;
		blastAttack.baseForce = explosionForce;
		blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
		blastAttack.Fire();
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
