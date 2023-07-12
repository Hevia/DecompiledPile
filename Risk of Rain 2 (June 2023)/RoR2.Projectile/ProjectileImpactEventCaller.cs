using UnityEngine;
using UnityEngine.Events;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileImpactEventCaller : MonoBehaviour, IProjectileImpactBehavior
{
	public ProjectileImpactEvent impactEvent;

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		((UnityEvent<ProjectileImpactInfo>)impactEvent)?.Invoke(impactInfo);
	}
}
