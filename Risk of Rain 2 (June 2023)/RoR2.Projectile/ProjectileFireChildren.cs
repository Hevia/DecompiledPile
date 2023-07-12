using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

public class ProjectileFireChildren : MonoBehaviour
{
	public float duration = 5f;

	public int count = 5;

	public GameObject childProjectilePrefab;

	private float timer;

	private float nextSpawnTimer;

	public float childDamageCoefficient = 1f;

	public float childProcCoefficient = 1f;

	private ProjectileDamage projectileDamage;

	private ProjectileController projectileController;

	public bool ignoreParentForChainController;

	private void Start()
	{
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
	}

	private void Update()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		nextSpawnTimer += Time.deltaTime;
		if (timer >= duration)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (nextSpawnTimer >= duration / (float)count)
		{
			nextSpawnTimer -= duration / (float)count;
			GameObject obj = Object.Instantiate<GameObject>(childProjectilePrefab, ((Component)this).transform.position, Util.QuaternionSafeLookRotation(((Component)this).transform.forward));
			ProjectileController component = obj.GetComponent<ProjectileController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.procChainMask = projectileController.procChainMask;
				component.procCoefficient = projectileController.procCoefficient * childProcCoefficient;
				component.Networkowner = projectileController.owner;
			}
			obj.GetComponent<TeamFilter>().teamIndex = ((Component)this).GetComponent<TeamFilter>().teamIndex;
			ProjectileDamage component2 = obj.GetComponent<ProjectileDamage>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.damage = projectileDamage.damage * childDamageCoefficient;
				component2.crit = projectileDamage.crit;
				component2.force = projectileDamage.force;
				component2.damageColorIndex = projectileDamage.damageColorIndex;
			}
			NetworkServer.Spawn(obj);
		}
	}
}
