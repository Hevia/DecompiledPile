using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileDamage))]
public class ProjectileParentTether : MonoBehaviour
{
	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private TeamIndex myTeamIndex;

	public float attackInterval = 1f;

	public float maxTetherRange = 20f;

	public float procCoefficient = 0.1f;

	public float damageCoefficient = 1f;

	public float raycastRadius;

	public float lifetime;

	public GameObject impactEffect;

	public GameObject tetherEffectPrefab;

	public ProjectileStickOnImpact stickOnImpact;

	private GameObject tetherEffectInstance;

	private GameObject tetherEffectInstanceEnd;

	private float attackTimer;

	private float lifetimeStopwatch;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		attackTimer = 0f;
		UpdateTetherGraphic();
	}

	private void UpdateTetherGraphic()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldIFire())
		{
			if (Object.op_Implicit((Object)(object)tetherEffectPrefab) && !Object.op_Implicit((Object)(object)tetherEffectInstance))
			{
				tetherEffectInstance = Object.Instantiate<GameObject>(tetherEffectPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
				tetherEffectInstance.transform.parent = ((Component)this).transform;
				ChildLocator component = tetherEffectInstance.GetComponent<ChildLocator>();
				tetherEffectInstanceEnd = ((Component)component.FindChild("LaserEnd")).gameObject;
			}
			if (Object.op_Implicit((Object)(object)tetherEffectInstance))
			{
				Ray aimRay = GetAimRay();
				tetherEffectInstance.transform.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
				tetherEffectInstanceEnd.transform.position = aimRay.origin + aimRay.direction * GetRayDistance();
			}
		}
	}

	private float GetRayDistance()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)projectileController.owner))
		{
			Vector3 val = projectileController.owner.transform.position - ((Component)this).transform.position;
			return ((Vector3)(ref val)).magnitude;
		}
		return 0f;
	}

	private Ray GetAimRay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Ray result = default(Ray);
		((Ray)(ref result)).origin = ((Component)this).transform.position;
		((Ray)(ref result)).direction = projectileController.owner.transform.position - ((Ray)(ref result)).origin;
		return result;
	}

	private bool ShouldIFire()
	{
		if (Object.op_Implicit((Object)(object)stickOnImpact))
		{
			return stickOnImpact.stuck;
		}
		return true;
	}

	private void Update()
	{
		UpdateTetherGraphic();
	}

	private void FixedUpdate()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldIFire())
		{
			lifetimeStopwatch += Time.fixedDeltaTime;
		}
		if (lifetimeStopwatch > lifetime)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (!Object.op_Implicit((Object)(object)projectileController.owner.transform) || !ShouldIFire())
		{
			return;
		}
		myTeamIndex = (Object.op_Implicit((Object)(object)projectileController.teamFilter) ? projectileController.teamFilter.teamIndex : TeamIndex.Neutral);
		attackTimer -= Time.fixedDeltaTime;
		if (attackTimer <= 0f)
		{
			Ray aimRay = GetAimRay();
			attackTimer = attackInterval;
			Vector3 direction = aimRay.direction;
			if (((Vector3)(ref direction)).magnitude < maxTetherRange && NetworkServer.active)
			{
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = projectileController.owner;
				bulletAttack.origin = aimRay.origin;
				bulletAttack.aimVector = aimRay.direction;
				bulletAttack.minSpread = 0f;
				bulletAttack.damage = damageCoefficient * projectileDamage.damage;
				bulletAttack.force = 0f;
				bulletAttack.hitEffectPrefab = impactEffect;
				bulletAttack.isCrit = projectileDamage.crit;
				bulletAttack.radius = raycastRadius;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.stopperMask = LayerMask.op_Implicit(0);
				bulletAttack.hitMask = LayerIndex.entityPrecise.mask;
				bulletAttack.procCoefficient = procCoefficient;
				bulletAttack.maxDistance = GetRayDistance();
				bulletAttack.Fire();
			}
		}
	}
}
