using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileDamage))]
public class ProjectileDamageTrail : MonoBehaviour
{
	public GameObject trailPrefab;

	public float damageToTrailDpsFactor = 1f;

	public float trailLifetimeAfterExpiration = 1f;

	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private GameObject currentTrailObject;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
	}

	private void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)currentTrailObject))
		{
			currentTrailObject = Object.Instantiate<GameObject>(trailPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
			DamageTrail component = currentTrailObject.GetComponent<DamageTrail>();
			component.damagePerSecond = projectileDamage.damage * damageToTrailDpsFactor;
			component.owner = projectileController.owner;
		}
		else
		{
			currentTrailObject.transform.position = ((Component)this).transform.position;
		}
	}

	private void OnDestroy()
	{
		DiscontinueTrail();
	}

	private void DiscontinueTrail()
	{
		if (Object.op_Implicit((Object)(object)currentTrailObject))
		{
			currentTrailObject.AddComponent<DestroyOnTimer>().duration = trailLifetimeAfterExpiration;
			currentTrailObject.GetComponent<DamageTrail>().active = false;
			currentTrailObject = null;
		}
	}
}
