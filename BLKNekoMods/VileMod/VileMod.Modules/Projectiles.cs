using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace VileMod.Modules;

internal static class Projectiles
{
	internal static GameObject bombPrefab;

	internal static GameObject EletricSpark;

	internal static GameObject BumpityBombProjectile;

	internal static GameObject FrontRunnerFireBallProjectile;

	internal static GameObject CerberusPhantonFMJProjectile;

	internal static GameObject ShotgunIceProjectile;

	internal static GameObject NapalmBombProjectile;

	internal static void RegisterProjectiles()
	{
		CreateEletricSparkProjectile();
		CreateBumpityBombProjectile();
		CreateFrontRunnerProjectile();
		CreateCerberusPhantonProjectile();
		CreateShotgunIceProjectile();
		CreateNapalmBombProjectile();
		AddProjectile(EletricSpark);
		AddProjectile(BumpityBombProjectile);
		AddProjectile(FrontRunnerFireBallProjectile);
		AddProjectile(CerberusPhantonFMJProjectile);
		AddProjectile(ShotgunIceProjectile);
		AddProjectile(NapalmBombProjectile);
	}

	internal static void AddProjectile(GameObject projectileToAdd)
	{
		Content.AddProjectilePrefab(projectileToAdd);
	}

	private static void CreateBomb()
	{
		bombPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");
		ProjectileImpactExplosion component = bombPrefab.GetComponent<ProjectileImpactExplosion>();
		InitializeImpactExplosion(component);
		((ProjectileExplosion)component).blastRadius = 16f;
		component.destroyOnEnemy = true;
		component.lifetime = 12f;
		component.impactEffect = Assets.bombExplosionEffect;
		component.timerAfterImpact = true;
		component.lifetimeAfterImpact = 0.1f;
		ProjectileController component2 = bombPrefab.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("HenryBombGhost") != (Object)null)
		{
			component2.ghostPrefab = CreateGhostPrefab("HenryBombGhost");
		}
		component2.startSound = "";
	}

	private static void CreateEletricSparkProjectile()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		EletricSpark = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageLightningBombProjectile"), "Prefabs/Projectiles/ESparkProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		EletricSpark.GetComponent<ProjectileController>().procCoefficient = 1f;
		EletricSpark.GetComponent<ProjectileDamage>().damage = 1f;
		EletricSpark.GetComponent<ProjectileDamage>().damageType = (DamageType)16777216;
		if (Object.op_Implicit((Object)(object)EletricSpark))
		{
			PrefabAPI.RegisterNetworkPrefab(EletricSpark);
		}
		ProjectileController component = EletricSpark.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("ESGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("ESGhost");
		}
		component.startSound = "";
	}

	private static void CreateBumpityBombProjectile()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		BumpityBombProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		BumpityBombProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
		BumpityBombProjectile.GetComponent<ProjectileDamage>().damage = 1f;
		BumpityBombProjectile.GetComponent<ProjectileDamage>().damageType = (DamageType)0;
		if (Object.op_Implicit((Object)(object)BumpityBombProjectile))
		{
			PrefabAPI.RegisterNetworkPrefab(BumpityBombProjectile);
		}
		ProjectileController component = BumpityBombProjectile.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("BBGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("BBGhost");
		}
		component.startSound = "";
	}

	private static void CreateFrontRunnerProjectile()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		FrontRunnerFireBallProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFireBombProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		FrontRunnerFireBallProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
		FrontRunnerFireBallProjectile.GetComponent<ProjectileDamage>().damage = 1f;
		FrontRunnerFireBallProjectile.GetComponent<ProjectileDamage>().damageType = (DamageType)0;
		if (Object.op_Implicit((Object)(object)FrontRunnerFireBallProjectile))
		{
			PrefabAPI.RegisterNetworkPrefab(FrontRunnerFireBallProjectile);
		}
		ProjectileController component = FrontRunnerFireBallProjectile.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("FRGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("FRGhost");
		}
		component.startSound = "";
	}

	private static void CreateCerberusPhantonProjectile()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		CerberusPhantonFMJProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/FMJ"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		CerberusPhantonFMJProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
		CerberusPhantonFMJProjectile.GetComponent<ProjectileDamage>().damage = 1f;
		CerberusPhantonFMJProjectile.GetComponent<ProjectileDamage>().damageType = (DamageType)0;
		if (Object.op_Implicit((Object)(object)CerberusPhantonFMJProjectile))
		{
			PrefabAPI.RegisterNetworkPrefab(CerberusPhantonFMJProjectile);
		}
		ProjectileController component = CerberusPhantonFMJProjectile.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("CPGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("CPGhost");
		}
		component.startSound = "";
	}

	private static void CreateShotgunIceProjectile()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		ShotgunIceProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageIceBombProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		ShotgunIceProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
		ShotgunIceProjectile.GetComponent<ProjectileDamage>().damage = 1f;
		ShotgunIceProjectile.GetComponent<ProjectileDamage>().damageType = (DamageType)256;
		if (Object.op_Implicit((Object)(object)ShotgunIceProjectile))
		{
			PrefabAPI.RegisterNetworkPrefab(ShotgunIceProjectile);
		}
		ProjectileController component = ShotgunIceProjectile.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("SIGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("SIGhost");
		}
		component.startSound = "";
	}

	private static void CreateNapalmBombProjectile()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		NapalmBombProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/CryoCanisterProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);
		NapalmBombProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
		NapalmBombProjectile.GetComponent<ProjectileDamage>().damage = 1f;
		NapalmBombProjectile.GetComponent<ProjectileDamage>().damageType = (DamageType)128;
		if (Object.op_Implicit((Object)(object)NapalmBombProjectile))
		{
			PrefabAPI.RegisterNetworkPrefab(NapalmBombProjectile);
		}
		ProjectileController component = NapalmBombProjectile.GetComponent<ProjectileController>();
		if ((Object)(object)Assets.mainAssetBundle.LoadAsset<GameObject>("NBGhost") != (Object)null)
		{
			component.ghostPrefab = CreateGhostPrefab("NBGhost");
		}
		component.startSound = "";
	}

	private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		((ProjectileExplosion)projectileImpactExplosion).blastDamageCoefficient = 1f;
		((ProjectileExplosion)projectileImpactExplosion).blastProcCoefficient = 1f;
		((ProjectileExplosion)projectileImpactExplosion).blastRadius = 1f;
		((ProjectileExplosion)projectileImpactExplosion).bonusBlastForce = Vector3.zero;
		((ProjectileExplosion)projectileImpactExplosion).childrenCount = 0;
		((ProjectileExplosion)projectileImpactExplosion).childrenDamageCoefficient = 0f;
		((ProjectileExplosion)projectileImpactExplosion).childrenProjectilePrefab = null;
		projectileImpactExplosion.destroyOnEnemy = false;
		projectileImpactExplosion.destroyOnWorld = false;
		((ProjectileExplosion)projectileImpactExplosion).falloffModel = (FalloffModel)0;
		((ProjectileExplosion)projectileImpactExplosion).fireChildren = false;
		projectileImpactExplosion.impactEffect = null;
		projectileImpactExplosion.lifetime = 0f;
		projectileImpactExplosion.lifetimeAfterImpact = 0f;
		projectileImpactExplosion.lifetimeRandomOffset = 0f;
		projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
		projectileImpactExplosion.timerAfterImpact = false;
		((Component)projectileImpactExplosion).GetComponent<ProjectileDamage>().damageType = (DamageType)0;
	}

	private static GameObject CreateGhostPrefab(string ghostName)
	{
		GameObject val = Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName);
		if (!Object.op_Implicit((Object)(object)val.GetComponent<NetworkIdentity>()))
		{
			val.AddComponent<NetworkIdentity>();
		}
		if (!Object.op_Implicit((Object)(object)val.GetComponent<ProjectileGhostController>()))
		{
			val.AddComponent<ProjectileGhostController>();
		}
		Assets.ConvertAllRenderersToHopooShader(val);
		return val;
	}

	private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
	{
		return PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
	}
}
