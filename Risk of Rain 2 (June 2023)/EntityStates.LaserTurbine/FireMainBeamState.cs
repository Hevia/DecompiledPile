using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LaserTurbine;

public class FireMainBeamState : LaserTurbineBaseState
{
	public static float baseDuration;

	public static float mainBeamDamageCoefficient;

	public static float mainBeamProcCoefficient;

	public static float mainBeamForce;

	public static float mainBeamRadius;

	public static float mainBeamMaxDistance;

	public static GameObject forwardBeamTracerEffect;

	public static GameObject backwardBeamTracerEffect;

	public static GameObject mainBeamImpactEffect;

	public static GameObject secondBombPrefab;

	public static float secondBombDamageCoefficient;

	private Ray initialAimRay;

	private Vector3 beamHitPosition;

	private bool isCrit;

	protected override bool shouldFollow => true;

	public override void OnEnter()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			initialAimRay = GetAimRay();
		}
		if (NetworkServer.active)
		{
			isCrit = base.ownerBody.RollCrit();
			FireBeamServer(initialAimRay, forwardBeamTracerEffect, mainBeamMaxDistance, isInitialBeam: true);
		}
		base.laserTurbineController.showTurbineDisplay = false;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= baseDuration)
		{
			outer.SetNextState(new RechargeState());
		}
	}

	public override void OnExit()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && !outer.destroying)
		{
			Vector3 val = ((Ray)(ref initialAimRay)).origin - beamHitPosition;
			Ray aimRay = default(Ray);
			((Ray)(ref aimRay))._002Ector(beamHitPosition, val);
			FireBeamServer(aimRay, backwardBeamTracerEffect, ((Vector3)(ref val)).magnitude, isInitialBeam: false);
		}
		base.laserTurbineController.showTurbineDisplay = true;
		base.OnExit();
	}

	private void FireBeamServer(Ray aimRay, GameObject tracerEffectPrefab, float maxDistance, bool isInitialBeam)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		bool didHit = false;
		BulletAttack obj = new BulletAttack
		{
			origin = ((Ray)(ref aimRay)).origin,
			aimVector = ((Ray)(ref aimRay)).direction,
			bulletCount = 1u,
			damage = GetDamage() * mainBeamDamageCoefficient,
			damageColorIndex = DamageColorIndex.Item,
			damageType = DamageType.Generic,
			falloffModel = BulletAttack.FalloffModel.None,
			force = mainBeamForce,
			hitEffectPrefab = mainBeamImpactEffect,
			HitEffectNormal = false,
			hitMask = LayerIndex.entityPrecise.mask,
			isCrit = isCrit,
			maxDistance = maxDistance,
			minSpread = 0f,
			maxSpread = 0f,
			muzzleName = "",
			owner = ((Component)base.ownerBody).gameObject,
			procChainMask = default(ProcChainMask),
			procCoefficient = mainBeamProcCoefficient,
			queryTriggerInteraction = (QueryTriggerInteraction)0,
			radius = mainBeamRadius,
			smartCollision = true,
			sniper = false,
			spreadPitchScale = 1f,
			spreadYawScale = 1f,
			stopperMask = LayerIndex.world.mask,
			tracerEffectPrefab = (isInitialBeam ? tracerEffectPrefab : null),
			weapon = base.gameObject
		};
		TeamIndex teamIndex = base.ownerBody.teamComponent.teamIndex;
		obj.hitCallback = delegate(BulletAttack _bulletAttack, ref BulletAttack.BulletHit info)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			bool flag = BulletAttack.defaultHitCallback(_bulletAttack, ref info);
			if (!isInitialBeam)
			{
				return true;
			}
			if (flag)
			{
				HealthComponent healthComponent = (Object.op_Implicit((Object)(object)info.hitHurtBox) ? info.hitHurtBox.healthComponent : null);
				if (Object.op_Implicit((Object)(object)healthComponent) && healthComponent.alive && info.hitHurtBox.teamIndex != teamIndex)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				didHit = true;
				beamHitPosition = info.point;
			}
			return flag;
		};
		obj.filterCallback = delegate(BulletAttack _bulletAttack, ref BulletAttack.BulletHit info)
		{
			return (!Object.op_Implicit((Object)(object)info.entityObject) || info.entityObject != _bulletAttack.owner) && BulletAttack.defaultFilterCallback(_bulletAttack, ref info);
		};
		obj.Fire();
		if (!didHit)
		{
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val, mainBeamMaxDistance, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				didHit = true;
				beamHitPosition = ((RaycastHit)(ref val)).point;
			}
			else
			{
				beamHitPosition = ((Ray)(ref aimRay)).GetPoint(mainBeamMaxDistance);
			}
		}
		if (didHit && isInitialBeam)
		{
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = secondBombPrefab;
			fireProjectileInfo.owner = ((Component)base.ownerBody).gameObject;
			fireProjectileInfo.position = beamHitPosition - ((Ray)(ref aimRay)).direction * 0.5f;
			fireProjectileInfo.rotation = Quaternion.identity;
			fireProjectileInfo.damage = GetDamage() * secondBombDamageCoefficient;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
			fireProjectileInfo.crit = isCrit;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}
		if (!isInitialBeam)
		{
			EffectData effectData = new EffectData
			{
				origin = ((Ray)(ref aimRay)).origin,
				start = base.transform.position
			};
			effectData.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		Vector3 origin = ((Ray)(ref initialAimRay)).origin;
		Vector3 direction = ((Ray)(ref initialAimRay)).direction;
		writer.Write(origin);
		writer.Write(direction);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		Vector3 val = reader.ReadVector3();
		Vector3 val2 = reader.ReadVector3();
		initialAimRay = new Ray(val, val2);
	}
}
