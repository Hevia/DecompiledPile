using System;
using HG;
using RoR2.ContentManagement;
using UnityEngine;

namespace RoR2;

public static class EffectCatalog
{
	private static EffectDef[] entries = Array.Empty<EffectDef>();

	public static int effectCount => entries.Length;

	[SystemInitializer(new Type[] { })]
	public static void Init()
	{
		SetEntries(ContentManager.effectDefs);
	}

	public static void SetEntries(EffectDef[] newEntries)
	{
		EffectDef[] array = entries;
		foreach (EffectDef obj in array)
		{
			obj.index = EffectIndex.Invalid;
			obj.prefabEffectComponent.effectIndex = EffectIndex.Invalid;
		}
		ArrayUtils.CloneTo<EffectDef>(newEntries, ref entries);
		Array.Sort(entries, (EffectDef a, EffectDef b) => string.CompareOrdinal(a.prefabName, b.prefabName));
		for (int j = 0; j < entries.Length; j++)
		{
			ref EffectDef reference = ref entries[j];
			reference.index = (EffectIndex)j;
			reference.prefabEffectComponent.effectIndex = reference.index;
		}
	}

	public static EffectIndex FindEffectIndexFromPrefab(GameObject effectPrefab)
	{
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectComponent component = effectPrefab.GetComponent<EffectComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				return component.effectIndex;
			}
		}
		return EffectIndex.Invalid;
	}

	public static EffectDef GetEffectDef(EffectIndex effectIndex)
	{
		EffectDef[] array = entries;
		EffectDef effectDef = null;
		return ArrayUtils.GetSafe<EffectDef>(array, (int)effectIndex, ref effectDef);
	}

	[ConCommand(commandName = "effects_reload", flags = ConVarFlags.Cheat, helpText = "Reloads the effect catalog.")]
	private static void CCEffectsReload(ConCommandArgs args)
	{
		throw new ConCommandException("Command unavailable outside editor until asset reloading is implemented at the ContentPack level.");
	}
}
