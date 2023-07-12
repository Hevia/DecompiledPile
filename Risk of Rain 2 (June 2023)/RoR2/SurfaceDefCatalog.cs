using System;
using HG;
using RoR2.ContentManagement;
using UnityEngine;

namespace RoR2;

public static class SurfaceDefCatalog
{
	private static SurfaceDef[] surfaceDefs = Array.Empty<SurfaceDef>();

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SetSurfaceDefs(ContentManager.surfaceDefs);
	}

	private static void SetSurfaceDefs(SurfaceDef[] newSurfaceDefs)
	{
		SurfaceDef[] array = surfaceDefs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].surfaceDefIndex = SurfaceDefIndex.Invalid;
		}
		ArrayUtils.CloneTo<SurfaceDef>(newSurfaceDefs, ref surfaceDefs);
		Array.Sort(surfaceDefs, (SurfaceDef a, SurfaceDef b) => string.CompareOrdinal(((Object)a).name, ((Object)b).name));
		for (int j = 0; j < surfaceDefs.Length; j++)
		{
			surfaceDefs[j].surfaceDefIndex = (SurfaceDefIndex)j;
		}
	}

	public static SurfaceDef GetSurfaceDef(SurfaceDefIndex surfaceDefIndex)
	{
		return ArrayUtils.GetSafe<SurfaceDef>(surfaceDefs, (int)surfaceDefIndex);
	}
}
