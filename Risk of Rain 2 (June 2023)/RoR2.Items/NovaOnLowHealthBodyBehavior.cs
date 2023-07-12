using UnityEngine;

namespace RoR2.Items;

public class NovaOnLowHealthBodyBehavior : BaseItemBodyBehavior
{
	private NetworkedBodyAttachment attachment;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.NovaOnLowHealth;
	}

	private void Start()
	{
		attachment = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/VagrantNovaItemBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
		attachment.AttachToGameObjectAndSpawn(((Component)base.body).gameObject);
	}

	private void FixedUpdate()
	{
		if (!base.body.healthComponent.alive)
		{
			Object.Destroy((Object)(object)this);
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)attachment))
		{
			Object.Destroy((Object)(object)((Component)attachment).gameObject);
			attachment = null;
		}
	}
}
