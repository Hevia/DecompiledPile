using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(Deployable))]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileDeployToOwner : MonoBehaviour
{
	public DeployableSlot deployableSlot;

	private void Start()
	{
		if (NetworkServer.active)
		{
			DeployToOwner();
		}
	}

	private void DeployToOwner()
	{
		GameObject owner = ((Component)this).GetComponent<ProjectileController>().owner;
		if (!Object.op_Implicit((Object)(object)owner))
		{
			return;
		}
		CharacterBody component = owner.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			CharacterMaster master = component.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				master.AddDeployable(((Component)this).GetComponent<Deployable>(), deployableSlot);
			}
		}
	}
}
