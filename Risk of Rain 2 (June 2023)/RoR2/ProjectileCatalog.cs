using System;
using System.Collections.Generic;
using HG;
using RoR2.ContentManagement;
using RoR2.Modding;
using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public static class ProjectileCatalog
{
	private static GameObject[] projectilePrefabs = Array.Empty<GameObject>();

	private static ProjectileController[] projectilePrefabProjectileControllerComponents = Array.Empty<ProjectileController>();

	private static string[] projectileNames = Array.Empty<string>();

	public static int projectilePrefabCount => projectilePrefabs.Length;

	[Obsolete("Use IContentPackProvider instead.")]
	public static event Action<List<GameObject>> getAdditionalEntries
	{
		add
		{
			LegacyModContentPackProvider.instance.HandleLegacyGetAdditionalEntries("RoR2.ProjectileCatalog.getAdditionalEntries", value, LegacyModContentPackProvider.instance.registrationContentPack.projectilePrefabs);
		}
		remove
		{
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SetProjectilePrefabs(ContentManager.projectilePrefabs);
	}

	private static void SetProjectilePrefabs(GameObject[] newProjectilePrefabs)
	{
		ProjectileController[] array = projectilePrefabProjectileControllerComponents;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].catalogIndex = -1;
		}
		ArrayUtils.CloneTo<GameObject>(newProjectilePrefabs, ref projectilePrefabs);
		Array.Sort(projectilePrefabs, (GameObject a, GameObject b) => string.CompareOrdinal(((Object)a).name, ((Object)b).name));
		int num = 256;
		if (projectilePrefabs.Length > num)
		{
			Debug.LogErrorFormat("Cannot have more than {0} projectile prefabs defined, which is over the limit for {1}. Check comments at error source for details.", new object[2]
			{
				num,
				typeof(byte).Name
			});
			for (int j = num; j < projectilePrefabs.Length; j++)
			{
				Debug.LogErrorFormat("Could not register projectile [{0}/{1}]=\"{2}\"", new object[3]
				{
					j,
					num - 1,
					((Object)projectilePrefabs[j]).name
				});
			}
		}
		Array.Resize(ref projectilePrefabProjectileControllerComponents, projectilePrefabs.Length);
		Array.Resize(ref projectileNames, projectilePrefabs.Length);
		for (int k = 0; k < projectilePrefabs.Length; k++)
		{
			GameObject val = projectilePrefabs[k];
			ProjectileController component = val.GetComponent<ProjectileController>();
			component.catalogIndex = k;
			projectilePrefabProjectileControllerComponents[k] = component;
			projectileNames[k] = ((Object)val).name;
		}
	}

	public static int GetProjectileIndex(GameObject projectileObject)
	{
		if (Object.op_Implicit((Object)(object)projectileObject))
		{
			return GetProjectileIndex(projectileObject.GetComponent<ProjectileController>());
		}
		return -1;
	}

	public static int GetProjectileIndex(ProjectileController projectileController)
	{
		if (!Object.op_Implicit((Object)(object)projectileController))
		{
			return -1;
		}
		return projectileController.catalogIndex;
	}

	public static GameObject GetProjectilePrefab(int projectileIndex)
	{
		return ArrayUtils.GetSafe<GameObject>(projectilePrefabs, projectileIndex);
	}

	public static ProjectileController GetProjectilePrefabProjectileControllerComponent(int projectileIndex)
	{
		return ArrayUtils.GetSafe<ProjectileController>(projectilePrefabProjectileControllerComponents, projectileIndex);
	}

	public static int FindProjectileIndex(string projectileName)
	{
		return Array.IndexOf(projectileNames, projectileName);
	}

	[ConCommand(commandName = "dump_projectile_map", flags = ConVarFlags.None, helpText = "Dumps the map between indices and projectile prefabs.")]
	private static void DumpProjectileMap(ConCommandArgs args)
	{
		string[] array = new string[projectilePrefabs.Length];
		for (int i = 0; i < projectilePrefabs.Length; i++)
		{
			array[i] = $"[{i}] = {((Object)projectilePrefabs[i]).name}";
		}
		Debug.Log((object)string.Join("\n", array));
	}
}
