using UnityEngine;

namespace RoR2.Items;

public class LaserTurbineBodyBehavior : BaseItemBodyBehavior
{
	private GameObject laserTurbineControllerInstance;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.LaserTurbine;
	}

	private void OnEnable()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		laserTurbineControllerInstance = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/LaserTurbineController"), base.body.corePosition, Quaternion.identity);
		laserTurbineControllerInstance.GetComponent<GenericOwnership>().ownerObject = ((Component)this).gameObject;
		laserTurbineControllerInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)laserTurbineControllerInstance);
		laserTurbineControllerInstance = null;
	}
}
