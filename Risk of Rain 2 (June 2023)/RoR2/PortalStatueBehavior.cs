using UnityEngine;

namespace RoR2;

public class PortalStatueBehavior : MonoBehaviour
{
	public enum PortalType
	{
		Shop,
		Goldshores,
		Count
	}

	public PortalType portalType;

	public void GrantPortalEntry()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		switch (portalType)
		{
		case PortalType.Shop:
			if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
			{
				TeleporterInteraction.instance.shouldAttemptToSpawnShopPortal = true;
			}
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
			{
				origin = ((Component)this).transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem)
			}, transmit: true);
			break;
		case PortalType.Goldshores:
			if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance))
			{
				TeleporterInteraction.instance.shouldAttemptToSpawnGoldshoresPortal = true;
			}
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
			{
				origin = ((Component)this).transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Money)
			}, transmit: true);
			break;
		}
		PortalStatueBehavior[] array = Object.FindObjectsOfType<PortalStatueBehavior>();
		foreach (PortalStatueBehavior portalStatueBehavior in array)
		{
			if (portalStatueBehavior.portalType == portalType)
			{
				PurchaseInteraction component = ((Component)portalStatueBehavior).GetComponent<PurchaseInteraction>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.Networkavailable = false;
				}
			}
		}
	}
}
