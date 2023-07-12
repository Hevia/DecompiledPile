using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Destructible;

public class SulfurPodDeath : GenericCharacterDeath
{
	public static GameObject chargePrefab;

	public static float chargeDuration;

	public static GameObject explosionEffectPrefab;

	public static float explosionRadius;

	public static float explosionDamageCoefficient;

	public static float explosionProcCoefficient;

	public static float explosionForce;

	private bool hasExploded;

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
		if (base.fixedAge >= chargeDuration)
		{
			Explode();
		}
	}

	private void Explode()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!hasExploded)
		{
			hasExploded = true;
			if (Object.op_Implicit((Object)(object)explosionEffectPrefab))
			{
				EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
				{
					origin = base.transform.position,
					scale = explosionRadius,
					rotation = Quaternion.identity
				}, transmit: true);
			}
			DestroyModel();
			if (NetworkServer.active)
			{
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = base.gameObject;
				blastAttack.damageColorIndex = DamageColorIndex.Poison;
				blastAttack.baseDamage = damageStat * explosionDamageCoefficient * Run.instance.teamlessDamageCoefficient;
				blastAttack.radius = explosionRadius;
				blastAttack.falloffModel = BlastAttack.FalloffModel.None;
				blastAttack.procCoefficient = explosionProcCoefficient;
				blastAttack.teamIndex = TeamIndex.None;
				blastAttack.damageType = DamageType.PoisonOnHit;
				blastAttack.position = base.transform.position;
				blastAttack.baseForce = explosionForce;
				blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
				blastAttack.Fire();
				DestroyBodyAsapServer();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
