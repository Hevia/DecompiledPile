using UnityEngine;

namespace RoR2.Items;

public class WarCryOnCombatBodyBehavior : BaseItemBodyBehavior
{
	private static readonly float warCryChargeDuration = 30f;

	private float warCryTimer;

	private GameObject warCryAuraController;

	private bool wasOutOfCombat;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return JunkContent.Items.WarCryOnCombat;
	}

	private void OnEnable()
	{
		warCryTimer = warCryChargeDuration;
	}

	private void FixedUpdate()
	{
		warCryTimer -= Time.fixedDeltaTime;
		if (warCryTimer <= 0f && !base.body.outOfCombat && wasOutOfCombat)
		{
			warCryTimer = warCryChargeDuration;
			ActivateWarCryAura(stack);
		}
		wasOutOfCombat = base.body.outOfCombat;
	}

	private void ActivateWarCryAura(int stacks)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)warCryAuraController))
		{
			Object.Destroy((Object)(object)warCryAuraController);
		}
		warCryAuraController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/WarCryAura"), ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		warCryAuraController.GetComponent<TeamFilter>().teamIndex = base.body.teamComponent.teamIndex;
		BuffWard component = warCryAuraController.GetComponent<BuffWard>();
		component.expireDuration = 2f + 4f * (float)stacks;
		component.Networkradius = 8f + 4f * (float)stacks;
		warCryAuraController.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
	}
}
