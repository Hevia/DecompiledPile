using UnityEngine;

namespace RoR2.Projectile;

public class ProjectileTargetComponent : MonoBehaviour
{
	public Transform target { get; set; }

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)target) && !((Component)target).gameObject.activeSelf)
		{
			target = null;
		}
	}
}
