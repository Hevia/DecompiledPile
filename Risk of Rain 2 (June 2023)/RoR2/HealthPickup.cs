using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class HealthPickup : MonoBehaviour
{
	[Tooltip("The base object to destroy when this pickup is consumed.")]
	public GameObject baseObject;

	[Tooltip("The team filter object which determines who can pick up this pack.")]
	public TeamFilter teamFilter;

	public GameObject pickupEffect;

	public float flatHealing;

	public float fractionalHealing;

	private bool alive = true;

	private void OnTriggerStay(Collider other)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || !alive || TeamComponent.GetObjectTeam(((Component)other).gameObject) != teamFilter.teamIndex)
		{
			return;
		}
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			HealthComponent healthComponent = component.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				component.healthComponent.Heal(flatHealing + healthComponent.fullHealth * fractionalHealing, default(ProcChainMask));
				EffectManager.SpawnEffect(pickupEffect, new EffectData
				{
					origin = ((Component)this).transform.position
				}, transmit: true);
			}
			Object.Destroy((Object)(object)baseObject);
		}
	}
}
