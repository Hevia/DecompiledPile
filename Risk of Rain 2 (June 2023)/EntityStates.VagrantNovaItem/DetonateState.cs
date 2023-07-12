using EntityStates.VagrantMonster;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VagrantNovaItem;

public class DetonateState : BaseVagrantNovaItemState
{
	public static float blastRadius;

	public static float blastDamageCoefficient;

	public static float blastProcCoefficient;

	public static float blastForce;

	public static float baseDuration;

	public static string explosionSound;

	private float duration;

	public override void OnEnter()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.attachedBody))
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = ((Component)base.attachedBody).gameObject;
			blastAttack.baseDamage = base.attachedBody.damage * blastDamageCoefficient;
			blastAttack.baseForce = blastForce;
			blastAttack.bonusForce = Vector3.zero;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.crit = base.attachedBody.RollCrit();
			blastAttack.damageColorIndex = DamageColorIndex.Item;
			blastAttack.damageType = DamageType.Generic;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.inflictor = base.gameObject;
			blastAttack.position = base.attachedBody.corePosition;
			blastAttack.procChainMask = default(ProcChainMask);
			blastAttack.procCoefficient = blastProcCoefficient;
			blastAttack.radius = blastRadius;
			blastAttack.losType = BlastAttack.LoSType.NearestHit;
			blastAttack.teamIndex = base.attachedBody.teamComponent.teamIndex;
			blastAttack.Fire();
			EffectData effectData = new EffectData();
			effectData.origin = base.attachedBody.corePosition;
			effectData.SetHurtBoxReference(base.attachedBody.mainHurtBox);
			EffectManager.SpawnEffect(FireMegaNova.novaEffectPrefab, effectData, transmit: true);
		}
		SetChargeSparkEmissionRateMultiplier(0f);
		Util.PlaySound(explosionSound, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new RechargeState());
		}
	}
}
