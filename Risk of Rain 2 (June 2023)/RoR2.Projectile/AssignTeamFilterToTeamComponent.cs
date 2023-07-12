using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(HealthComponent))]
public class AssignTeamFilterToTeamComponent : MonoBehaviour
{
	private void Start()
	{
		if (NetworkServer.active)
		{
			TeamComponent component = ((Component)this).GetComponent<TeamComponent>();
			TeamFilter component2 = ((Component)this).GetComponent<TeamFilter>();
			if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component))
			{
				component.teamIndex = component2.teamIndex;
			}
		}
	}
}
