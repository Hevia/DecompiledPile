using System;
using UnityEngine;

namespace RoR2.Items;

public static class ExtraLifeVoidManager
{
	private const ulong seedSalt = 733uL;

	private static Xoroshiro128Plus rng;

	private static string[] voidBodyNames;

	public static GameObject rezEffectPrefab { get; private set; }

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		rezEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/VoidRezEffect");
		voidBodyNames = new string[1] { "NullifierBody" };
		Run.onRunStartGlobal += OnRunStart;
	}

	private static void OnRunStart(Run run)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		rng = new Xoroshiro128Plus(run.seed ^ 0x2DD);
	}

	public static GameObject GetNextBodyPrefab()
	{
		return BodyCatalog.FindBodyPrefab(voidBodyNames[rng.RangeInt(0, voidBodyNames.Length)]);
	}
}
