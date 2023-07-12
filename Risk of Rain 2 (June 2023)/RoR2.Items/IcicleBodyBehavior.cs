using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public class IcicleBodyBehavior : BaseItemBodyBehavior
{
	private static GameObject icicleAuraPrefab;

	private IcicleAuraController icicleAura;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.Icicle;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		icicleAuraPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/IcicleAura");
	}

	private void OnEnable()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
		GameObject val = Object.Instantiate<GameObject>(icicleAuraPrefab, ((Component)this).transform.position, Quaternion.identity);
		icicleAura = val.GetComponent<IcicleAuraController>();
		icicleAura.Networkowner = ((Component)this).gameObject;
		NetworkServer.Spawn(val);
	}

	private void OnDisable()
	{
		GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
		if (Object.op_Implicit((Object)(object)icicleAura))
		{
			Object.Destroy((Object)(object)icicleAura);
			icicleAura = null;
		}
	}

	private void OnCharacterDeathGlobal(DamageReport damageReport)
	{
		if (damageReport.attackerBody == base.body && Object.op_Implicit((Object)(object)icicleAura))
		{
			icicleAura.OnOwnerKillOther();
		}
	}
}
