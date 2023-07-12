using UnityEngine;

namespace RoR2.Items;

public class PhasingBodyBehavior : BaseItemBodyBehavior
{
	private readonly float baseRechargeSeconds = 30f;

	private readonly float rechargeReductionMultiplierPerStack = 0.5f;

	private readonly float buffDuration = 5f;

	private float rechargeStopwatch;

	private GameObject effectPrefab;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.Phasing;
	}

	private void Start()
	{
		rechargeStopwatch = baseRechargeSeconds;
		effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ProcStealthkit");
	}

	private void FixedUpdate()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (base.body.healthComponent.alive)
		{
			rechargeStopwatch += Time.fixedDeltaTime;
			if (base.body.healthComponent.isHealthLow && !base.body.hasCloakBuff && rechargeStopwatch >= buffDuration + baseRechargeSeconds * Mathf.Pow(rechargeReductionMultiplierPerStack, (float)(stack - 1)))
			{
				base.body.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
				base.body.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, buffDuration);
				EffectManager.SpawnEffect(effectPrefab, new EffectData
				{
					origin = ((Component)this).transform.position,
					rotation = Quaternion.identity
				}, transmit: true);
				rechargeStopwatch = 0f;
			}
		}
	}
}
