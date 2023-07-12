using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Destructible;

public class FusionCellDeath : BaseState
{
	public static string chargeChildEffectName;

	public static float chargeDuration;

	public static GameObject explosionEffectPrefab;

	public static float explosionRadius;

	public static float explosionDamageCoefficient;

	public static float explosionProcCoefficient;

	public static float explosionForce;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		ChildLocator component = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild(chargeChildEffectName);
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(true);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > chargeDuration)
		{
			Explode();
		}
	}

	private void Explode()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelTransform).gameObject);
			}
		}
		if (Object.op_Implicit((Object)(object)explosionEffectPrefab) && NetworkServer.active)
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
		blastAttack.position = base.transform.position;
		blastAttack.baseForce = explosionForce;
		blastAttack.bonusForce = explosionForce * 0.5f * Vector3.up;
		blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
		blastAttack.Fire();
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
