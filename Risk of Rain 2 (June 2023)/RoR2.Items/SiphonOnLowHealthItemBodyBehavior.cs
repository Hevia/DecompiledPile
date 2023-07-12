using UnityEngine;

namespace RoR2.Items;

public class SiphonOnLowHealthItemBodyBehavior : BaseItemBodyBehavior
{
	private NetworkedBodyAttachment attachment;

	private SiphonNearbyController siphonNearbyController;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.SiphonOnLowHealth;
	}

	private void OnEnable()
	{
		attachment = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/SiphonNearbyBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
		attachment.AttachToGameObjectAndSpawn(((Component)base.body).gameObject);
		siphonNearbyController = ((Component)attachment).GetComponent<SiphonNearbyController>();
	}

	private void OnDisable()
	{
		DestroyAttachment();
	}

	private void FixedUpdate()
	{
		siphonNearbyController.NetworkmaxTargets = (base.body.healthComponent.alive ? stack : 0);
	}

	private void DestroyAttachment()
	{
		if (Object.op_Implicit((Object)(object)attachment))
		{
			Object.Destroy((Object)(object)((Component)attachment).gameObject);
		}
		attachment = null;
		siphonNearbyController = null;
	}
}
