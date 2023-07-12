using RoR2;
using UnityEngine;

namespace EntityStates.EngiTurret.EngiTurretWeapon;

public class FireBeam : BaseState
{
	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public GameObject hitEffectPrefab;

	[SerializeField]
	public GameObject laserPrefab;

	[SerializeField]
	public string muzzleString;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float procCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float minSpread;

	[SerializeField]
	public float maxSpread;

	[SerializeField]
	public int bulletCount;

	[SerializeField]
	public float fireFrequency;

	[SerializeField]
	public float maxDistance;

	private float fireTimer;

	private Ray laserRay;

	private Transform modelTransform;

	private GameObject laserEffectInstance;

	private Transform laserEffectInstanceEndTransform;

	public int bulletCountCurrent = 1;

	public override void OnEnter()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(attackSoundString, base.gameObject);
		fireTimer = 0f;
		modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild(muzzleString);
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)laserPrefab))
			{
				laserEffectInstance = Object.Instantiate<GameObject>(laserPrefab, val.position, val.rotation);
				laserEffectInstance.transform.parent = val;
				laserEffectInstanceEndTransform = laserEffectInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)laserEffectInstance))
		{
			EntityState.Destroy((Object)(object)laserEffectInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		laserRay = GetLaserRay();
		fireTimer += Time.fixedDeltaTime;
		float num = fireFrequency * base.characterBody.attackSpeed;
		float num2 = 1f / num;
		if (fireTimer > num2)
		{
			FireBullet(modelTransform, laserRay, muzzleString);
			fireTimer = 0f;
		}
		if (Object.op_Implicit((Object)(object)laserEffectInstance) && Object.op_Implicit((Object)(object)laserEffectInstanceEndTransform))
		{
			laserEffectInstanceEndTransform.position = GetBeamEndPoint();
		}
		if (base.isAuthority && !ShouldFireLaser())
		{
			outer.SetNextState(GetNextState());
		}
	}

	protected Vector3 GetBeamEndPoint()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3 point = ((Ray)(ref laserRay)).GetPoint(maxDistance);
		if (Util.CharacterRaycast(base.gameObject, laserRay, out var hitInfo, maxDistance, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)0))
		{
			point = ((RaycastHit)(ref hitInfo)).point;
		}
		return point;
	}

	protected virtual EntityState GetNextState()
	{
		return EntityStateCatalog.InstantiateState(outer.mainStateType);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public virtual void ModifyBullet(BulletAttack bulletAttack)
	{
		bulletAttack.damageType |= DamageType.SlowOnHit;
	}

	public virtual bool ShouldFireLaser()
	{
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			return base.inputBank.skill1.down;
		}
		return false;
	}

	public virtual Ray GetLaserRay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetAimRay();
	}

	private void FireBullet(Transform modelTransform, Ray laserRay, string targetMuzzle)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = ((Ray)(ref laserRay)).origin;
			bulletAttack.aimVector = ((Ray)(ref laserRay)).direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.bulletCount = 1u;
			bulletAttack.damage = damageCoefficient * damageStat / fireFrequency;
			bulletAttack.procCoefficient = procCoefficient / fireFrequency;
			bulletAttack.force = force;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = 0f;
			bulletAttack.maxDistance = maxDistance;
			ModifyBullet(bulletAttack);
			bulletAttack.Fire();
		}
	}
}
