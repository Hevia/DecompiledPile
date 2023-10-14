using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Rorschach;

internal class Prefabs
{
	internal static GameObject utilityProjectile;

	internal static GameObject sprayFire;

	internal static GameObject punchHitFX;

	internal static GameObject cleaveHitFX;

	internal static GameObject cleaveSwingFX;

	internal static GameObject cleaveSwingFX2;

	internal static GameObject cleaveSwingFX3;

	internal static GameObject dashEffect;

	internal static void CreatePrefabs()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PrefabAPI.InstantiateClone(Assets.MainAssetBundle.LoadAsset<GameObject>("hook"), "RorschachHookProjectileGhost", false);
		val.AddComponent<ProjectileGhostController>();
		utilityProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Gravekeeper/GravekeeperHookProjectileSimple.prefab").WaitForCompletion(), "RorschachHookProjectile", true);
		Object.Destroy((Object)(object)utilityProjectile.GetComponent<ProjectileSingleTargetImpact>());
		utilityProjectile.AddComponent<RorschachHookBehaviour>();
		ProjectileController component = utilityProjectile.GetComponent<ProjectileController>();
		component.allowPrediction = false;
		component.ghostPrefab = val;
		utilityProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 120f;
		utilityProjectile.GetComponent<ProjectileSimple>().lifetime = 0.6f;
		utilityProjectile.GetComponent<ProjectileStickOnImpact>().ignoreCharacters = false;
		ContentAddition.AddProjectile(utilityProjectile);
		sprayFire = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Mage/MageFlamethrowerEffect.prefab").WaitForCompletion(), "RorschachSprayFireEffect", false);
		Object.Destroy((Object)(object)((Component)sprayFire.transform.Find("Matrix, Dynamic")).gameObject);
		Object.Destroy((Object)(object)((Component)sprayFire.transform.Find("IcoCharge")).gameObject);
		Object.Destroy((Object)(object)((Component)sprayFire.transform.Find("Billboard")).gameObject);
		punchHitFX = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Common/VFX/OmniImpactVFX.prefab").WaitForCompletion(), "RorschachPunchHitEffect", false);
		ParticleSystemRenderer[] componentsInChildren = punchHitFX.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer val2 in componentsInChildren)
		{
		}
		Utils.RegisterEffect(punchHitFX, 1f, "Play_Rorschach_Punch_Hit");
		cleaveHitFX = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>((object)"RoR2/Base/Common/VFX/BleedEffect.prefab").WaitForCompletion(), "RorschachCleaveHitEffect", false);
		Object.Destroy((Object)(object)cleaveHitFX.GetComponent<LoopSound>());
		Object.Destroy((Object)(object)((Component)cleaveHitFX.transform.GetChild(1)).gameObject);
		Utils.RegisterEffect(cleaveHitFX, 1f, "Play_item_proc_dagger_impact");
		cleaveSwingFX = PrefabAPI.InstantiateClone(Assets.MainAssetBundle.LoadAsset<GameObject>("SlashFX1"), "RorschachCleaveSwingEffect", false);
		Utils.RegisterEffect(cleaveSwingFX, 1f);
		cleaveSwingFX2 = PrefabAPI.InstantiateClone(Assets.MainAssetBundle.LoadAsset<GameObject>("SlashFX2"), "RorschachCleaveSwingEffect", false);
		Utils.RegisterEffect(cleaveSwingFX2, 1f);
		cleaveSwingFX3 = PrefabAPI.InstantiateClone(Assets.MainAssetBundle.LoadAsset<GameObject>("SlashFX3"), "RorschachCleaveSwingEffect", false);
		Utils.RegisterEffect(cleaveSwingFX3, 1f);
		dashEffect = PrefabAPI.InstantiateClone(Assets.MainAssetBundle.LoadAsset<GameObject>("dashEffect"), "RorschachDashEffect", false);
	}
}
