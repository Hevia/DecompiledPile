using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public class MushroomBodyBehavior : BaseItemBodyBehavior
{
	private static GameObject mushroomWardPrefab;

	private const float baseHealFractionPerSecond = 0.045f;

	private const float healFractionPerSecondPerStack = 0.0225f;

	private GameObject mushroomWardGameObject;

	private HealingWard mushroomHealingWard;

	private TeamFilter mushroomWardTeamFilter;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.Mushroom;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		mushroomWardPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MushroomWard");
	}

	private void FixedUpdate()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		int num = stack;
		bool flag = num > 0 && base.body.GetNotMoving();
		float networkradius = base.body.radius + 1.5f + 1.5f * (float)num;
		if (Object.op_Implicit((Object)(object)mushroomWardGameObject) != flag)
		{
			if (flag)
			{
				mushroomWardGameObject = Object.Instantiate<GameObject>(mushroomWardPrefab, base.body.footPosition, Quaternion.identity);
				mushroomWardTeamFilter = mushroomWardGameObject.GetComponent<TeamFilter>();
				mushroomHealingWard = mushroomWardGameObject.GetComponent<HealingWard>();
				NetworkServer.Spawn(mushroomWardGameObject);
			}
			else
			{
				Object.Destroy((Object)(object)mushroomWardGameObject);
				mushroomWardGameObject = null;
			}
		}
		if (Object.op_Implicit((Object)(object)mushroomHealingWard))
		{
			mushroomHealingWard.interval = 0.25f;
			mushroomHealingWard.healFraction = (0.045f + 0.0225f * (float)(num - 1)) * mushroomHealingWard.interval;
			mushroomHealingWard.healPoints = 0f;
			mushroomHealingWard.Networkradius = networkradius;
		}
		if (Object.op_Implicit((Object)(object)mushroomWardTeamFilter))
		{
			mushroomWardTeamFilter.teamIndex = base.body.teamComponent.teamIndex;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)mushroomWardGameObject))
		{
			Object.Destroy((Object)(object)mushroomWardGameObject);
		}
	}
}
