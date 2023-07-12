using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Fauna;

public class VultureEggDeathState : BirdsharkDeathState
{
	public static int healPackCount;

	public static float healPackMaxVelocity;

	public static float fractionalHealing;

	public static float scale;

	public override void OnEnter()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			for (int i = 0; i < healPackCount; i++)
			{
				GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), base.transform.position, Random.rotation);
				obj.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
				obj.GetComponentInChildren<HealthPickup>().fractionalHealing = fractionalHealing;
				obj.transform.localScale = new Vector3(scale, scale, scale);
				obj.GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere * healPackMaxVelocity, (ForceMode)2);
				NetworkServer.Spawn(obj);
			}
		}
	}
}
