using UnityEngine;

namespace RoR2.Projectile;

public class ProjectileFireEffects : MonoBehaviour
{
	public float duration = 5f;

	public int count = 5;

	public GameObject effectPrefab;

	public Vector3 randomOffset;

	private float timer;

	private float nextSpawnTimer;

	private void Update()
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		nextSpawnTimer += Time.deltaTime;
		if (timer >= duration)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (nextSpawnTimer >= duration / (float)count)
		{
			nextSpawnTimer -= duration / (float)count;
			if (Object.op_Implicit((Object)(object)effectPrefab))
			{
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(Random.Range(0f - randomOffset.x, randomOffset.x), Random.Range(0f - randomOffset.y, randomOffset.y), Random.Range(0f - randomOffset.z, randomOffset.z));
				EffectManager.SimpleImpactEffect(effectPrefab, ((Component)this).transform.position + val, Vector3.forward, transmit: true);
			}
		}
	}
}
