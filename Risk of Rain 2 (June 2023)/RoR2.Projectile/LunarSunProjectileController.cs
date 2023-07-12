using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileImpactExplosion))]
[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileOwnerOrbiter))]
[DisallowMultipleComponent]
public class LunarSunProjectileController : MonoBehaviour
{
	private ProjectileImpactExplosion explosion;

	public void OnEnable()
	{
		explosion = ((Component)this).GetComponent<ProjectileImpactExplosion>();
		if (NetworkServer.active)
		{
			ProjectileController component = ((Component)this).GetComponent<ProjectileController>();
			if (Object.op_Implicit((Object)(object)component.owner))
			{
				AcquireOwner(component);
			}
			else
			{
				component.onInitialized += AcquireOwner;
			}
		}
	}

	private void AcquireOwner(ProjectileController controller)
	{
		controller.onInitialized -= AcquireOwner;
		CharacterBody component = controller.owner.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			ProjectileOwnerOrbiter component2 = ((Component)this).GetComponent<ProjectileOwnerOrbiter>();
			((Component)component).GetComponent<LunarSunBehavior>().InitializeOrbiter(component2, this);
		}
	}

	public void Detonate()
	{
		if (Object.op_Implicit((Object)(object)explosion))
		{
			explosion.Detonate();
		}
	}
}
