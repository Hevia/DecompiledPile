using System.Collections.Generic;
using System.Globalization;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Bell.BellWeapon;

public class ChargeTrioBomb : BaseState
{
	public static float basePrepDuration;

	public static float baseTimeBetweenPreps;

	public static GameObject preppedBombPrefab;

	public static float baseBarrageDuration;

	public static float baseTimeBetweenBarrages;

	public static GameObject bombProjectilePrefab;

	public static GameObject muzzleflashPrefab;

	public static float damageCoefficient;

	public static float force;

	public static float selfForce;

	private float prepDuration;

	private float timeBetweenPreps;

	private float barrageDuration;

	private float timeBetweenBarrages;

	private ChildLocator childLocator;

	private List<GameObject> preppedBombInstances = new List<GameObject>();

	private int currentBombIndex;

	private float perProjectileStopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		prepDuration = basePrepDuration / attackSpeedStat;
		timeBetweenPreps = baseTimeBetweenPreps / attackSpeedStat;
		barrageDuration = baseBarrageDuration / attackSpeedStat;
		timeBetweenBarrages = baseTimeBetweenBarrages / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
	}

	private string FindTargetChildStringFromBombIndex()
	{
		return string.Format(CultureInfo.InvariantCulture, "ProjectilePosition{0}", currentBombIndex);
	}

	private Transform FindTargetChildTransformFromBombIndex()
	{
		string childName = FindTargetChildStringFromBombIndex();
		return childLocator.FindChild(childName);
	}

	public override void FixedUpdate()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		perProjectileStopwatch += Time.fixedDeltaTime;
		if (base.fixedAge < prepDuration)
		{
			if (perProjectileStopwatch > timeBetweenPreps && currentBombIndex < 3)
			{
				currentBombIndex++;
				perProjectileStopwatch = 0f;
				Transform val = FindTargetChildTransformFromBombIndex();
				if (Object.op_Implicit((Object)(object)val))
				{
					GameObject item = Object.Instantiate<GameObject>(preppedBombPrefab, val);
					preppedBombInstances.Add(item);
				}
			}
		}
		else if (base.fixedAge < prepDuration + barrageDuration)
		{
			if (!(perProjectileStopwatch > timeBetweenBarrages) || currentBombIndex <= 0)
			{
				return;
			}
			perProjectileStopwatch = 0f;
			Ray aimRay = GetAimRay();
			Transform val2 = FindTargetChildTransformFromBombIndex();
			if (Object.op_Implicit((Object)(object)val2))
			{
				if (base.isAuthority)
				{
					ProjectileManager.instance.FireProjectile(bombProjectilePrefab, val2.position, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
					Rigidbody component = GetComponent<Rigidbody>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.AddForceAtPosition((0f - selfForce) * val2.forward, val2.position);
					}
				}
				EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, FindTargetChildStringFromBombIndex(), transmit: false);
			}
			currentBombIndex--;
			EntityState.Destroy((Object)(object)preppedBombInstances[currentBombIndex]);
		}
		else if (base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		foreach (GameObject preppedBombInstance in preppedBombInstances)
		{
			EntityState.Destroy((Object)(object)preppedBombInstance);
		}
	}
}
