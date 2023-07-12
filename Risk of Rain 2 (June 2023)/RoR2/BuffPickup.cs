using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BuffPickup : MonoBehaviour
{
	[Tooltip("The base object to destroy when this pickup is consumed.")]
	public GameObject baseObject;

	[Tooltip("The team filter object which determines who can pick up this pack.")]
	public TeamFilter teamFilter;

	public GameObject pickupEffect;

	public BuffDef buffDef;

	public float buffDuration;

	private bool alive = true;

	private void OnTriggerStay(Collider other)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(((Component)other).gameObject) == teamFilter.teamIndex)
		{
			CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.AddTimedBuff(buffDef.buffIndex, buffDuration);
				Object.Instantiate<GameObject>(pickupEffect, ((Component)other).transform.position, Quaternion.identity);
				Object.Destroy((Object)(object)baseObject);
			}
		}
	}
}
