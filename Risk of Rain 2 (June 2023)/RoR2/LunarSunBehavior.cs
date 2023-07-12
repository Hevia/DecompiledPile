using System;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public class LunarSunBehavior : CharacterBody.ItemBehavior
{
	private const float secondsPerTransform = 60f;

	private const float secondsPerProjectile = 3f;

	private const string projectilePath = "Prefabs/Projectiles/LunarSunProjectile";

	private const int baseMaxProjectiles = 2;

	private const int maxProjectilesPerStack = 1;

	private const float baseOrbitDegreesPerSecond = 180f;

	private const float orbitDegreesPerSecondFalloff = 0.9f;

	private const float baseOrbitRadius = 2f;

	private const float orbitRadiusPerStack = 0.25f;

	private const float maxInclinationDegrees = 0f;

	private const float baseDamageCoefficient = 3.6f;

	private float projectileTimer;

	private float transformTimer;

	private GameObject projectilePrefab;

	private Xoroshiro128Plus transformRng;

	public event Action<LunarSunBehavior> onDisabled;

	public static int GetMaxProjectiles(Inventory inventory)
	{
		return 2 + inventory.GetItemCount(DLC1Content.Items.LunarSun);
	}

	public void InitializeOrbiter(ProjectileOwnerOrbiter orbiter, LunarSunProjectileController controller)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		float num = body.radius + 2f + Random.Range(0.25f, 0.25f * (float)stack);
		float num2 = num / 2f;
		num2 *= num2;
		float degreesPerSecond = 180f * Mathf.Pow(0.9f, num2);
		Quaternion val = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
		Quaternion val2 = Quaternion.AngleAxis(Random.Range(0f, 0f), Vector3.forward);
		Vector3 planeNormal = val * val2 * Vector3.up;
		float initialDegreesFromOwnerForward = Random.Range(0f, 360f);
		orbiter.Initialize(planeNormal, num, degreesPerSecond, initialDegreesFromOwnerForward);
		onDisabled += DestroyOrbiter;
		void DestroyOrbiter(LunarSunBehavior lunarSunBehavior)
		{
			if (Object.op_Implicit((Object)(object)controller))
			{
				controller.Detonate();
			}
		}
	}

	private void Awake()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		((Behaviour)this).enabled = false;
		projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LunarSunProjectile");
		ulong num = Run.instance.seed ^ (ulong)Run.instance.stageClearCount;
		transformRng = new Xoroshiro128Plus(num);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		this.onDisabled?.Invoke(this);
		this.onDisabled = null;
	}

	private void FixedUpdate()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		projectileTimer += Time.fixedDeltaTime;
		if (!body.master.IsDeployableLimited(DeployableSlot.LunarSunBomb) && projectileTimer > 3f / (float)stack)
		{
			projectileTimer = 0f;
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = projectilePrefab;
			fireProjectileInfo.crit = body.RollCrit();
			fireProjectileInfo.damage = body.damage * 3.6f;
			fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
			fireProjectileInfo.force = 0f;
			fireProjectileInfo.owner = ((Component)this).gameObject;
			fireProjectileInfo.position = ((Component)body).transform.position;
			fireProjectileInfo.rotation = Quaternion.identity;
			FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		}
		transformTimer += Time.fixedDeltaTime;
		if (!(transformTimer > 60f))
		{
			return;
		}
		transformTimer = 0f;
		if (!Object.op_Implicit((Object)(object)body.master) || !Object.op_Implicit((Object)(object)body.inventory))
		{
			return;
		}
		List<ItemIndex> list = new List<ItemIndex>(body.inventory.itemAcquisitionOrder);
		ItemIndex itemIndex = ItemIndex.None;
		Util.ShuffleList(list, transformRng);
		foreach (ItemIndex item in list)
		{
			if (item != DLC1Content.Items.LunarSun.itemIndex)
			{
				ItemDef itemDef = ItemCatalog.GetItemDef(item);
				if (Object.op_Implicit((Object)(object)itemDef) && itemDef.tier != ItemTier.NoTier)
				{
					itemIndex = item;
					break;
				}
			}
		}
		if (itemIndex != ItemIndex.None)
		{
			body.inventory.RemoveItem(itemIndex);
			body.inventory.GiveItem(DLC1Content.Items.LunarSun);
			CharacterMasterNotificationQueue.SendTransformNotification(body.master, itemIndex, DLC1Content.Items.LunarSun.itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
		}
	}
}
