using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class FireworkLauncher : MonoBehaviour
{
	public GameObject projectilePrefab;

	public float launchInterval = 0.1f;

	public float damageCoefficient = 3f;

	public float coneAngle = 10f;

	public float randomCircleRange;

	[HideInInspector]
	public GameObject owner;

	[HideInInspector]
	public TeamIndex team;

	[HideInInspector]
	public int remaining;

	[HideInInspector]
	public bool crit;

	private float nextFireTimer;

	private void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (remaining <= 0 || !Object.op_Implicit((Object)(object)owner))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		nextFireTimer -= Time.fixedDeltaTime;
		if (nextFireTimer <= 0f)
		{
			remaining--;
			nextFireTimer += launchInterval;
			FireMissile();
		}
	}

	private void FireMissile()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody component = owner.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			ProcChainMask procChainMask = default(ProcChainMask);
			Vector2 val = Random.insideUnitCircle * randomCircleRange;
			MissileUtils.FireMissile(((Component)this).transform.position + new Vector3(val.x, 0f, val.y), component, procChainMask, null, component.damage * damageCoefficient, crit, projectilePrefab, DamageColorIndex.Item, addMissileProc: false);
		}
	}
}
