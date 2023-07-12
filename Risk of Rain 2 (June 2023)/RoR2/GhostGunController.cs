using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GhostGunController : MonoBehaviour
{
	public GameObject owner;

	public float interval;

	public float maxRange = 20f;

	public float turnSpeed = 180f;

	public Vector3 localOffset = Vector3.zero;

	public float positionSmoothTime = 0.05f;

	public float timeout = 2f;

	private float fireTimer;

	private float timeoutTimer;

	private int ammo;

	private int kills;

	private GameObject target;

	private Vector3 velocity;

	private void Start()
	{
		fireTimer = 0f;
		ammo = 6;
		kills = 0;
		timeoutTimer = timeout;
	}

	private void Fire(Vector3 origin, Vector3 aimDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		BulletAttack obj = new BulletAttack
		{
			aimVector = aimDirection,
			bulletCount = 1u,
			damage = CalcDamage(),
			force = 2400f,
			maxSpread = 0f,
			minSpread = 0f,
			muzzleName = "muzzle",
			origin = origin,
			owner = owner,
			procCoefficient = 0f,
			tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerSmokeChase"),
			hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Hitspark1"),
			damageColorIndex = DamageColorIndex.Item
		};
		GlobalEventManager.onCharacterDeathGlobal += CheckForKill;
		obj.Fire();
		GlobalEventManager.onCharacterDeathGlobal -= CheckForKill;
	}

	private void CheckForKill(DamageReport damageReport)
	{
		if ((Object)(object)damageReport.damageInfo.inflictor == (Object)(object)((Component)this).gameObject)
		{
			kills++;
		}
	}

	private float CalcDamage()
	{
		float damage = owner.GetComponent<CharacterBody>().damage;
		return 5f * Mathf.Pow(2f, (float)kills) * damage;
	}

	private bool HasLoS(GameObject target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Ray val = new Ray(((Component)this).transform.position, target.transform.position - ((Component)this).transform.position);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, ref val2, maxRange, LayerMask.op_Implicit(LayerIndex.defaultLayer.mask) | LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			return (Object)(object)((Component)((RaycastHit)(ref val2)).collider).gameObject == (Object)(object)target;
		}
		return true;
	}

	private bool WillHit(GameObject target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Ray val = new Ray(((Component)this).transform.position, ((Component)this).transform.forward);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, ref val2, maxRange, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask) | LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			HurtBox component = ((Component)((RaycastHit)(ref val2)).collider).GetComponent<HurtBox>();
			if (Object.op_Implicit((Object)(object)component))
			{
				HealthComponent healthComponent = component.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					return (Object)(object)((Component)healthComponent).gameObject == (Object)(object)target;
				}
			}
		}
		return false;
	}

	private GameObject FindTarget()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		TeamIndex teamA = TeamIndex.Neutral;
		TeamComponent component = owner.GetComponent<TeamComponent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			teamA = component.teamIndex;
		}
		Vector3 position = ((Component)this).transform.position;
		float num = CalcDamage();
		float num2 = maxRange * maxRange;
		GameObject val = null;
		GameObject result = null;
		float num3 = 0f;
		float num4 = float.PositiveInfinity;
		for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
		{
			if (TeamManager.IsTeamEnemy(teamA, teamIndex))
			{
				ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
				for (int i = 0; i < teamMembers.Count; i++)
				{
					GameObject gameObject = ((Component)teamMembers[i]).gameObject;
					Vector3 val2 = gameObject.transform.position - position;
					if (!(((Vector3)(ref val2)).sqrMagnitude <= num2))
					{
						continue;
					}
					HealthComponent component2 = ((Component)teamMembers[i]).GetComponent<HealthComponent>();
					if (!Object.op_Implicit((Object)(object)component2))
					{
						continue;
					}
					if (component2.health <= num)
					{
						if (component2.health > num3 && HasLoS(gameObject))
						{
							val = gameObject;
							num3 = component2.health;
						}
					}
					else if (component2.health < num4 && HasLoS(gameObject))
					{
						result = gameObject;
						num4 = component2.health;
					}
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)val))
		{
			return result;
		}
		return val;
	}

	private void FixedUpdate()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)owner))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		InputBankTest component = owner.GetComponent<InputBankTest>();
		Vector3 val = (Object.op_Implicit((Object)(object)component) ? component.aimDirection : ((Component)this).transform.forward);
		Vector3 val2;
		if (Object.op_Implicit((Object)(object)target))
		{
			val2 = target.transform.position - ((Component)this).transform.position;
			val = ((Vector3)(ref val2)).normalized;
		}
		((Component)this).transform.forward = Vector3.RotateTowards(((Component)this).transform.forward, val, MathF.PI / 180f * turnSpeed * Time.fixedDeltaTime, 0f);
		Vector3 val3 = owner.transform.position + ((Component)this).transform.rotation * localOffset;
		((Component)this).transform.position = Vector3.SmoothDamp(((Component)this).transform.position, val3, ref velocity, positionSmoothTime, float.PositiveInfinity, Time.fixedDeltaTime);
		fireTimer -= Time.fixedDeltaTime;
		timeoutTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			target = FindTarget();
			fireTimer = interval;
		}
		if (Object.op_Implicit((Object)(object)target) && WillHit(target))
		{
			val2 = target.transform.position - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			Fire(((Component)this).transform.position, normalized);
			ammo--;
			target = null;
			timeoutTimer = timeout;
		}
		if (ammo <= 0 || timeoutTimer <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
