using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2;

public class AffixEarthBehavior : CharacterBody.ItemBehavior
{
	private const float baseOrbitDegreesPerSecond = 90f;

	private const float baseOrbitRadius = 3f;

	private const string projectilePath = "Prefabs/Projectiles/AffixEarthProjectile";

	private GameObject affixEarthAttachment;

	private GameObject projectilePrefab;

	private int maxProjectiles;

	private static GameObject attachmentPrefab;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		attachmentPrefab = Addressables.LoadAssetAsync<GameObject>((object)"112bf5913df2135478a9785e0bc18477").WaitForCompletion();
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		bool flag = stack > 0;
		if (Object.op_Implicit((Object)(object)affixEarthAttachment) != flag)
		{
			if (flag)
			{
				affixEarthAttachment = Object.Instantiate<GameObject>(attachmentPrefab);
				affixEarthAttachment.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)body).gameObject);
			}
			else
			{
				Object.Destroy((Object)(object)affixEarthAttachment);
				affixEarthAttachment = null;
			}
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)affixEarthAttachment))
		{
			Object.Destroy((Object)(object)affixEarthAttachment);
		}
	}
}
