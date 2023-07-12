using System;
using System.Collections.Generic;
using HG;
using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public static class ProjectileGhostReplacementManager
{
	private struct SkinGhostPair
	{
		public SkinDef skinDef;

		public GameObject projectileGhost;
	}

	private static SkinGhostPair[][] projectileToSkinGhostPairs = Array.Empty<SkinGhostPair[]>();

	public static GameObject FindProjectileGhostPrefab(ProjectileController projectileController)
	{
		SkinGhostPair[] safe = ArrayUtils.GetSafe<SkinGhostPair[]>(projectileToSkinGhostPairs, projectileController.catalogIndex);
		if (safe != null && Object.op_Implicit((Object)(object)projectileController.owner))
		{
			CharacterBody component = projectileController.owner.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				SkinDef bodySkinDef = SkinCatalog.GetBodySkinDef(component.bodyIndex, (int)component.skinIndex);
				if (Object.op_Implicit((Object)(object)bodySkinDef))
				{
					for (int i = 0; i < safe.Length; i++)
					{
						if (safe[i].skinDef == bodySkinDef)
						{
							return safe[i].projectileGhost;
						}
					}
				}
			}
		}
		return projectileController.ghostPrefab;
	}

	[SystemInitializer(new Type[]
	{
		typeof(SkinCatalog),
		typeof(ProjectileCatalog)
	})]
	private static void Init()
	{
		BuildTable();
	}

	private static void BuildTable()
	{
		List<SkinDef> list = new List<SkinDef>();
		SkinIndex skinIndex = (SkinIndex)0;
		for (SkinIndex skinCount = (SkinIndex)SkinCatalog.skinCount; skinIndex < skinCount; skinIndex++)
		{
			SkinDef skinDef = SkinCatalog.GetSkinDef(skinIndex);
			if (skinDef.projectileGhostReplacements.Length != 0)
			{
				list.Add(skinDef);
			}
		}
		projectileToSkinGhostPairs = new SkinGhostPair[ProjectileCatalog.projectilePrefabCount][];
		foreach (SkinDef item in list)
		{
			SkinDef.ProjectileGhostReplacement[] projectileGhostReplacements = item.projectileGhostReplacements;
			for (int i = 0; i < item.projectileGhostReplacements.Length; i++)
			{
				SkinDef.ProjectileGhostReplacement projectileGhostReplacement = projectileGhostReplacements[i];
				int catalogIndex = projectileGhostReplacement.projectilePrefab.GetComponent<ProjectileController>().catalogIndex;
				if (projectileToSkinGhostPairs[catalogIndex] == null)
				{
					projectileToSkinGhostPairs[catalogIndex] = Array.Empty<SkinGhostPair>();
				}
				SkinGhostPair skinGhostPair = default(SkinGhostPair);
				skinGhostPair.projectileGhost = projectileGhostReplacement.projectileGhostReplacementPrefab;
				skinGhostPair.skinDef = item;
				SkinGhostPair skinGhostPair2 = skinGhostPair;
				ArrayUtils.ArrayAppend<SkinGhostPair>(ref projectileToSkinGhostPairs[catalogIndex], ref skinGhostPair2);
			}
		}
	}
}
