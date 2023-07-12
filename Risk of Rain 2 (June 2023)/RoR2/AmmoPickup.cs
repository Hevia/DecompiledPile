using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class AmmoPickup : MonoBehaviour
{
	[Tooltip("The base object to destroy when this pickup is consumed.")]
	public GameObject baseObject;

	[Tooltip("The team filter object which determines who can pick up this pack.")]
	public TeamFilter teamFilter;

	public GameObject pickupEffect;

	private bool alive = true;

	private void OnTriggerStay(Collider other)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(((Component)other).gameObject) == teamFilter.teamIndex)
		{
			SkillLocator component = ((Component)other).GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				alive = false;
				component.ApplyAmmoPack();
				EffectManager.SimpleEffect(pickupEffect, ((Component)this).transform.position, Quaternion.identity, transmit: true);
				Object.Destroy((Object)(object)baseObject);
			}
		}
	}
}
