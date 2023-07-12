using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class MoneyPickup : MonoBehaviour
{
	[Tooltip("The base object to destroy when this pickup is consumed.")]
	public GameObject baseObject;

	[Tooltip("The team filter object which determines who can pick up this pack.")]
	public TeamFilter teamFilter;

	public GameObject pickupEffectPrefab;

	public int baseGoldReward;

	public bool shouldScale;

	private bool alive = true;

	private int goldReward;

	private void Start()
	{
		if (NetworkServer.active)
		{
			goldReward = (shouldScale ? Run.instance.GetDifficultyScaledCost(baseGoldReward) : baseGoldReward);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || !alive)
		{
			return;
		}
		TeamIndex objectTeam = TeamComponent.GetObjectTeam(((Component)other).gameObject);
		if (objectTeam == teamFilter.teamIndex)
		{
			alive = false;
			Vector3 position = ((Component)this).transform.position;
			TeamManager.instance.GiveTeamMoney(objectTeam, (uint)goldReward);
			if (Object.op_Implicit((Object)(object)pickupEffectPrefab))
			{
				EffectManager.SimpleEffect(pickupEffectPrefab, position, Quaternion.identity, transmit: true);
			}
			Object.Destroy((Object)(object)baseObject);
		}
	}
}
