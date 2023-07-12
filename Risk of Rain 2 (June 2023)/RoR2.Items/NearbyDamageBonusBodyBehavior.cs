using UnityEngine;

namespace RoR2.Items;

public class NearbyDamageBonusBodyBehavior : BaseItemBodyBehavior
{
	private GameObject nearbyDamageBonusIndicator;

	private bool indicatorEnabled
	{
		get
		{
			return Object.op_Implicit((Object)(object)nearbyDamageBonusIndicator);
		}
		set
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			if (indicatorEnabled != value)
			{
				if (value)
				{
					GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
					nearbyDamageBonusIndicator = Object.Instantiate<GameObject>(val, base.body.corePosition, Quaternion.identity);
					nearbyDamageBonusIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
				}
				else
				{
					Object.Destroy((Object)(object)nearbyDamageBonusIndicator);
					nearbyDamageBonusIndicator = null;
				}
			}
		}
	}

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.NearbyDamageBonus;
	}

	private void OnEnable()
	{
		indicatorEnabled = true;
	}

	private void OnDisable()
	{
		indicatorEnabled = false;
	}
}
