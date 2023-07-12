using System;
using System.Collections.Generic;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BulletAttack
{
	public enum FalloffModel
	{
		None,
		DefaultBullet,
		Buckshot
	}

	public delegate bool HitCallback(BulletAttack bulletAttack, ref BulletHit hitInfo);

	public delegate void ModifyOutgoingDamageCallback(BulletAttack bulletAttack, ref BulletHit hitInfo, DamageInfo damageInfo);

	public delegate bool FilterCallback(BulletAttack bulletAttack, ref BulletHit hitInfo);

	public struct BulletHit
	{
		public Vector3 direction;

		public Vector3 point;

		public Vector3 surfaceNormal;

		public float distance;

		public Collider collider;

		public HurtBox hitHurtBox;

		public GameObject entityObject;

		public HurtBox.DamageModifier damageModifier;

		public bool isSniperHit;
	}

	private static GameObject sniperTargetHitEffect;

	public GameObject owner;

	public GameObject weapon;

	public float damage = 1f;

	public bool isCrit;

	public float force = 1f;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	public DamageType damageType;

	public DamageColorIndex damageColorIndex;

	public bool sniper;

	public FalloffModel falloffModel = FalloffModel.DefaultBullet;

	public GameObject tracerEffectPrefab;

	public GameObject hitEffectPrefab;

	public string muzzleName = "";

	public bool HitEffectNormal = true;

	public Vector3 origin;

	private Vector3 _aimVector;

	private float _maxDistance = 200f;

	public float radius;

	public uint bulletCount = 1u;

	public float minSpread;

	public float maxSpread;

	public float spreadPitchScale = 1f;

	public float spreadYawScale = 1f;

	public QueryTriggerInteraction queryTriggerInteraction = (QueryTriggerInteraction)1;

	private static readonly LayerMask defaultHitMask = LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));

	public LayerMask hitMask = defaultHitMask;

	private static readonly LayerMask defaultStopperMask = defaultHitMask;

	public LayerMask stopperMask = defaultStopperMask;

	public bool smartCollision;

	public HitCallback hitCallback;

	public static readonly HitCallback defaultHitCallback = DefaultHitCallbackImplementation;

	public ModifyOutgoingDamageCallback modifyOutgoingDamageCallback;

	private static NetworkWriter messageWriter = new NetworkWriter();

	public FilterCallback filterCallback;

	public static readonly FilterCallback defaultFilterCallback = DefaultFilterCallbackImplementation;

	public Vector3 aimVector
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _aimVector;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_aimVector = value;
			((Vector3)(ref _aimVector)).Normalize();
		}
	}

	public float maxDistance
	{
		get
		{
			return _maxDistance;
		}
		set
		{
			if (!float.IsInfinity(value) && !float.IsNaN(value))
			{
				_maxDistance = value;
				return;
			}
			Debug.LogFormat("BulletAttack.maxDistance was assigned a value other than a finite number. value={0}", new object[1] { value });
		}
	}

	[Obsolete("Use .defaultHitCallback instead.", false)]
	public static HitCallback DefaultHitCallback => defaultHitCallback;

	[Obsolete("Use .defaultFilterCallback instead.", false)]
	public static FilterCallback DefaultFilterCallback => defaultFilterCallback;

	public BulletAttack()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		filterCallback = defaultFilterCallback;
		hitCallback = defaultHitCallback;
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		sniperTargetHitEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/SniperTargetHitEffect");
	}

	private static bool DefaultHitCallbackImplementation(BulletAttack bulletAttack, ref BulletHit hitInfo)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (Object.op_Implicit((Object)(object)hitInfo.collider))
		{
			result = ((1 << ((Component)hitInfo.collider).gameObject.layer) & LayerMask.op_Implicit(bulletAttack.stopperMask)) == 0;
		}
		if (Object.op_Implicit((Object)(object)bulletAttack.hitEffectPrefab))
		{
			EffectManager.SimpleImpactEffect(bulletAttack.hitEffectPrefab, hitInfo.point, bulletAttack.HitEffectNormal ? hitInfo.surfaceNormal : (-hitInfo.direction), transmit: true);
		}
		if (hitInfo.isSniperHit && sniperTargetHitEffect != null)
		{
			EffectData effectData = new EffectData
			{
				origin = hitInfo.point,
				rotation = Quaternion.LookRotation(-hitInfo.direction)
			};
			effectData.SetHurtBoxReference(hitInfo.hitHurtBox);
			EffectManager.SpawnEffect(sniperTargetHitEffect, effectData, transmit: true);
		}
		if (Object.op_Implicit((Object)(object)hitInfo.collider))
		{
			SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(hitInfo.collider, hitInfo.point);
			if (Object.op_Implicit((Object)(object)objectSurfaceDef) && Object.op_Implicit((Object)(object)objectSurfaceDef.impactEffectPrefab))
			{
				EffectData effectData2 = new EffectData
				{
					origin = hitInfo.point,
					rotation = Quaternion.LookRotation(hitInfo.surfaceNormal),
					color = Color32.op_Implicit(objectSurfaceDef.approximateColor),
					surfaceDefIndex = objectSurfaceDef.surfaceDefIndex
				};
				EffectManager.SpawnEffect(objectSurfaceDef.impactEffectPrefab, effectData2, transmit: true);
			}
		}
		GameObject entityObject = hitInfo.entityObject;
		if (Object.op_Implicit((Object)(object)entityObject))
		{
			float num = CalcFalloffFactor(bulletAttack.falloffModel, hitInfo.distance);
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = bulletAttack.damage * num;
			damageInfo.crit = bulletAttack.isCrit;
			damageInfo.attacker = bulletAttack.owner;
			damageInfo.inflictor = bulletAttack.weapon;
			damageInfo.position = hitInfo.point;
			damageInfo.force = hitInfo.direction * (bulletAttack.force * num);
			damageInfo.procChainMask = bulletAttack.procChainMask;
			damageInfo.procCoefficient = bulletAttack.procCoefficient;
			damageInfo.damageType = bulletAttack.damageType;
			damageInfo.damageColorIndex = bulletAttack.damageColorIndex;
			damageInfo.ModifyDamageInfo(hitInfo.damageModifier);
			if (hitInfo.isSniperHit)
			{
				damageInfo.crit = true;
				damageInfo.damageColorIndex = DamageColorIndex.Sniper;
			}
			bulletAttack.modifyOutgoingDamageCallback?.Invoke(bulletAttack, ref hitInfo, damageInfo);
			TeamIndex attackerTeamIndex = TeamIndex.None;
			if (Object.op_Implicit((Object)(object)bulletAttack.owner))
			{
				TeamComponent component = bulletAttack.owner.GetComponent<TeamComponent>();
				if (Object.op_Implicit((Object)(object)component))
				{
					attackerTeamIndex = component.teamIndex;
				}
			}
			HealthComponent healthComponent = null;
			if (Object.op_Implicit((Object)(object)hitInfo.hitHurtBox))
			{
				healthComponent = hitInfo.hitHurtBox.healthComponent;
			}
			bool flag = Object.op_Implicit((Object)(object)healthComponent) && FriendlyFireManager.ShouldDirectHitProceed(healthComponent, attackerTeamIndex);
			if (NetworkServer.active)
			{
				if (flag)
				{
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, hitInfo.entityObject);
				}
				GlobalEventManager.instance.OnHitAll(damageInfo, hitInfo.entityObject);
			}
			else if (ClientScene.ready)
			{
				messageWriter.StartMessage((short)53);
				int currentLogLevel = LogFilter.currentLogLevel;
				LogFilter.currentLogLevel = 4;
				messageWriter.Write(entityObject);
				LogFilter.currentLogLevel = currentLogLevel;
				messageWriter.Write(damageInfo);
				messageWriter.Write(flag);
				messageWriter.FinishMessage();
				ClientScene.readyConnection.SendWriter(messageWriter, QosChannelIndex.defaultReliable.intVal);
			}
		}
		return result;
	}

	private static float CalcFalloffFactor(FalloffModel falloffModel, float distance)
	{
		return falloffModel switch
		{
			FalloffModel.DefaultBullet => 0.5f + Mathf.Clamp01(Mathf.InverseLerp(60f, 25f, distance)) * 0.5f, 
			FalloffModel.Buckshot => 0.25f + Mathf.Clamp01(Mathf.InverseLerp(25f, 7f, distance)) * 0.75f, 
			_ => 1f, 
		};
	}

	private static bool DefaultFilterCallbackImplementation(BulletAttack bulletAttack, ref BulletHit hitInfo)
	{
		HurtBox component = ((Component)hitInfo.collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.healthComponent) && (Object)(object)((Component)component.healthComponent).gameObject == (Object)(object)bulletAttack.weapon)
		{
			return false;
		}
		return (Object)(object)hitInfo.entityObject != (Object)(object)bulletAttack.weapon;
	}

	private void InitBulletHitFromOriginHit(ref BulletHit bulletHit, Vector3 direction, Collider hitCollider)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		bulletHit.direction = direction;
		bulletHit.point = origin;
		bulletHit.surfaceNormal = -direction;
		bulletHit.distance = 0f;
		bulletHit.collider = hitCollider;
		HurtBox hurtBox = (bulletHit.hitHurtBox = ((Component)bulletHit.collider).GetComponent<HurtBox>());
		bulletHit.entityObject = ((Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent)) ? ((Component)hurtBox.healthComponent).gameObject : ((Component)bulletHit.collider).gameObject);
		bulletHit.damageModifier = (Object.op_Implicit((Object)(object)hurtBox) ? hurtBox.damageModifier : HurtBox.DamageModifier.Normal);
	}

	private void InitBulletHitFromRaycastHit(ref BulletHit bulletHit, Vector3 origin, Vector3 direction, ref RaycastHit raycastHit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		bulletHit.direction = direction;
		bulletHit.surfaceNormal = ((RaycastHit)(ref raycastHit)).normal;
		bulletHit.distance = ((RaycastHit)(ref raycastHit)).distance;
		bulletHit.collider = ((RaycastHit)(ref raycastHit)).collider;
		bulletHit.point = ((bulletHit.distance == 0f) ? origin : ((RaycastHit)(ref raycastHit)).point);
		HurtBox hurtBox = (bulletHit.hitHurtBox = ((Component)bulletHit.collider).GetComponent<HurtBox>());
		bulletHit.entityObject = ((Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent)) ? ((Component)hurtBox.healthComponent).gameObject : ((Component)bulletHit.collider).gameObject);
		bulletHit.damageModifier = (Object.op_Implicit((Object)(object)hurtBox) ? hurtBox.damageModifier : HurtBox.DamageModifier.Normal);
	}

	private bool ProcessHit(ref BulletHit hitInfo)
	{
		if (!filterCallback(this, ref hitInfo))
		{
			return true;
		}
		if (sniper)
		{
			hitInfo.isSniperHit = IsSniperTargetHit(in hitInfo);
		}
		return hitCallback(this, ref hitInfo);
	}

	private static bool IsSniperTargetHit(in BulletHit hitInfo)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)hitInfo.hitHurtBox) && Object.op_Implicit((Object)(object)hitInfo.hitHurtBox.hurtBoxGroup))
		{
			HurtBox[] hurtBoxes = hitInfo.hitHurtBox.hurtBoxGroup.hurtBoxes;
			foreach (HurtBox hurtBox in hurtBoxes)
			{
				if (Object.op_Implicit((Object)(object)hurtBox) && hurtBox.isSniperTarget)
				{
					Vector3 val = Vector3.ProjectOnPlane(hitInfo.point - ((Component)hurtBox).transform.position, hitInfo.direction);
					if (((Vector3)(ref val)).sqrMagnitude <= HurtBox.sniperTargetRadiusSqr)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private GameObject ProcessHitList(List<BulletHit> hits, ref Vector3 endPosition, List<GameObject> ignoreList)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		int count = hits.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		for (int j = 0; j < count; j++)
		{
			float distance = maxDistance;
			int num = j;
			for (int k = j; k < count; k++)
			{
				int index = array[k];
				if (hits[index].distance < distance)
				{
					distance = hits[index].distance;
					num = k;
				}
			}
			GameObject entityObject = hits[array[num]].entityObject;
			if (!ignoreList.Contains(entityObject))
			{
				ignoreList.Add(entityObject);
				BulletHit hitInfo = hits[array[num]];
				if (!ProcessHit(ref hitInfo))
				{
					endPosition = hits[array[num]].point;
					return entityObject;
				}
			}
			array[num] = array[j];
		}
		return null;
	}

	private static GameObject LookUpColliderEntityObject(Collider collider)
	{
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.healthComponent))
		{
			return ((Component)collider).gameObject;
		}
		return ((Component)component.healthComponent).gameObject;
	}

	private static Collider[] PhysicsOverlapPoint(Vector3 point, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = 1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Physics.OverlapBox(point, Vector3.zero, Quaternion.identity, layerMask, queryTriggerInteraction);
	}

	public void Fire()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[bulletCount];
		Vector3 up = Vector3.up;
		Vector3 val = Vector3.Cross(up, aimVector);
		for (int i = 0; i < bulletCount; i++)
		{
			float num = Random.Range(minSpread, maxSpread);
			float num2 = Random.Range(0f, 360f);
			Vector3 val2 = Quaternion.Euler(0f, 0f, num2) * (Quaternion.Euler(num, 0f, 0f) * Vector3.forward);
			float y = val2.y;
			val2.y = 0f;
			float num3 = (Mathf.Atan2(val2.z, val2.x) * 57.29578f - 90f) * spreadYawScale;
			float num4 = Mathf.Atan2(y, ((Vector3)(ref val2)).magnitude) * 57.29578f * spreadPitchScale;
			array[i] = Quaternion.AngleAxis(num3, up) * (Quaternion.AngleAxis(num4, val) * aimVector);
		}
		int muzzleIndex = -1;
		_ = origin;
		if (!Object.op_Implicit((Object)(object)weapon))
		{
			weapon = owner;
		}
		if (Object.op_Implicit((Object)(object)weapon))
		{
			ModelLocator component = weapon.GetComponent<ModelLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.modelTransform))
			{
				ChildLocator component2 = ((Component)component.modelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					muzzleIndex = component2.FindChildIndex(muzzleName);
				}
			}
		}
		for (int j = 0; j < bulletCount; j++)
		{
			FireSingle(array[j], muzzleIndex);
		}
	}

	private void FireSingle(Vector3 normal, int muzzleIndex)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		float num = maxDistance;
		Vector3 endPosition = origin + normal * maxDistance;
		List<BulletHit> list = new List<BulletHit>();
		bool num2 = radius == 0f || smartCollision;
		bool flag = radius != 0f;
		HashSet<GameObject> hashSet = null;
		if (smartCollision)
		{
			hashSet = new HashSet<GameObject>();
		}
		if (num2)
		{
			RaycastHit[] array = Physics.RaycastAll(origin, normal, num, LayerMask.op_Implicit(hitMask), queryTriggerInteraction);
			float num3 = float.NegativeInfinity;
			for (int i = 0; i < array.Length; i++)
			{
				BulletHit bulletHit = default(BulletHit);
				InitBulletHitFromRaycastHit(ref bulletHit, origin, normal, ref array[i]);
				list.Add(bulletHit);
				if (smartCollision)
				{
					hashSet.Add(bulletHit.entityObject);
				}
				num3 = ((num3 < bulletHit.distance) ? bulletHit.distance : num3);
			}
			if (num3 != float.NegativeInfinity)
			{
				num = num3;
			}
		}
		if (flag)
		{
			LayerMask val = hitMask;
			if (smartCollision)
			{
				val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) & ~LayerMask.op_Implicit(LayerIndex.world.mask));
			}
			RaycastHit[] array2 = Physics.SphereCastAll(origin, radius, normal, num, LayerMask.op_Implicit(val), queryTriggerInteraction);
			for (int j = 0; j < array2.Length; j++)
			{
				BulletHit bulletHit2 = default(BulletHit);
				InitBulletHitFromRaycastHit(ref bulletHit2, origin, normal, ref array2[j]);
				if (!smartCollision || !hashSet.Contains(bulletHit2.entityObject))
				{
					list.Add(bulletHit2);
				}
			}
		}
		ProcessHitList(list, ref endPosition, new List<GameObject>());
		if (Object.op_Implicit((Object)(object)tracerEffectPrefab))
		{
			EffectData effectData = new EffectData
			{
				origin = endPosition,
				start = origin
			};
			effectData.SetChildLocatorTransformReference(weapon, muzzleIndex);
			EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
		}
	}

	[NetworkMessageHandler(msgType = 53, server = true)]
	private static void HandleBulletDamage(NetworkMessage netMsg)
	{
		NetworkReader reader = netMsg.reader;
		GameObject val = reader.ReadGameObject();
		DamageInfo damageInfo = reader.ReadDamageInfo();
		if (reader.ReadBoolean() && Object.op_Implicit((Object)(object)val))
		{
			HealthComponent component = val.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.TakeDamage(damageInfo);
			}
			GlobalEventManager.instance.OnHitEnemy(damageInfo, val);
		}
		GlobalEventManager.instance.OnHitAll(damageInfo, val);
	}
}
