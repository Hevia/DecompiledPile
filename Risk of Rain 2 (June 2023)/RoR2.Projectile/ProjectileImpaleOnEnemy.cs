using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileImpaleOnEnemy : MonoBehaviour, IProjectileImpactBehavior
{
	private bool alive = true;

	public GameObject impalePrefab;

	private Rigidbody rigidbody;

	private void Awake()
	{
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!alive)
		{
			return;
		}
		Collider collider = impactInfo.collider;
		if (!Object.op_Implicit((Object)(object)collider))
		{
			return;
		}
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component))
		{
			_ = ((Component)this).transform.position;
			Vector3 estimatedPointOfImpact = impactInfo.estimatedPointOfImpact;
			_ = Quaternion.identity;
			if (Object.op_Implicit((Object)(object)rigidbody))
			{
				Util.QuaternionSafeLookRotation(rigidbody.velocity);
			}
			GameObject obj = Object.Instantiate<GameObject>(impalePrefab, ((Component)component).transform);
			obj.transform.position = estimatedPointOfImpact;
			obj.transform.rotation = ((Component)this).transform.rotation;
		}
	}
}
