using UnityEngine;

namespace RoR2.Items;

public class HeadstomperBodyBehavior : BaseItemBodyBehavior
{
	private GameObject headstompersControllerObject;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.FallBoots;
	}

	private void OnEnable()
	{
		headstompersControllerObject = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HeadstompersController"));
		headstompersControllerObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)base.body).gameObject);
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)headstompersControllerObject))
		{
			Object.Destroy((Object)(object)headstompersControllerObject);
			headstompersControllerObject = null;
		}
	}
}
