using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

public class MineProximityDetonator : MonoBehaviour
{
	public TeamFilter myTeamFilter;

	public UnityEvent triggerEvents;

	public void OnTriggerEnter(Collider collider)
	{
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)collider))
		{
			return;
		}
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		HealthComponent healthComponent = component.healthComponent;
		if (!Object.op_Implicit((Object)(object)healthComponent))
		{
			return;
		}
		TeamComponent component2 = ((Component)healthComponent).GetComponent<TeamComponent>();
		if (!Object.op_Implicit((Object)(object)component2) || component2.teamIndex != myTeamFilter.teamIndex)
		{
			UnityEvent obj = triggerEvents;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}
}
