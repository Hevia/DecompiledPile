using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2.Skills;

[CreateAssetMenu(menuName = "RoR2/SkillFamily")]
public class SkillFamily : ScriptableObject
{
	[Serializable]
	public struct Variant
	{
		public SkillDef skillDef;

		[Obsolete("Use 'unlockableDef' instead.")]
		public string unlockableName;

		public UnlockableDef unlockableDef;

		public ViewablesCatalog.Node viewableNode { get; set; }
	}

	[FormerlySerializedAs("Entries")]
	public Variant[] variants = Array.Empty<Variant>();

	[FormerlySerializedAs("defaultEntryIndex")]
	public uint defaultVariantIndex;

	[Obsolete("Accessing UnityEngine.Object.Name causes allocations on read. Look up the name from the catalog instead. If absolutely necessary to perform direct access, cast to ScriptableObject first.")]
	public string name => null;

	public int catalogIndex { get; set; }

	public SkillDef defaultSkillDef => variants[defaultVariantIndex].skillDef;

	public void OnValidate()
	{
		if (variants == null)
		{
			string text = ((Object)this).name;
			Debug.LogError((object)("Skill Family \"" + text + "\" has a null variants array"));
		}
		else if (defaultVariantIndex >= variants.Length)
		{
			string arg = ((Object)this).name;
			Debug.LogError((object)$"Skill Family \"{arg}\" defaultVariantIndex ({defaultVariantIndex}) is outside the bounds of the variants array ({variants.Length}).");
		}
	}

	public string GetVariantName(int variantIndex)
	{
		return SkillCatalog.GetSkillName(variants[variantIndex].skillDef.skillIndex);
	}

	public int GetVariantIndex(string variantName)
	{
		for (int i = 0; i < variants.Length; i++)
		{
			if (GetVariantName(i).Equals(variantName, StringComparison.Ordinal))
			{
				return i;
			}
		}
		return -1;
	}

	[ContextMenu("Upgrade unlockableName to unlockableDef")]
	public void UpgradeUnlockableNameToUnlockableDef()
	{
		for (int i = 0; i < variants.Length; i++)
		{
			ref Variant reference = ref variants[i];
			if (!string.IsNullOrEmpty(reference.unlockableName) && !Object.op_Implicit((Object)(object)reference.unlockableDef))
			{
				UnlockableDef unlockableDef = LegacyResourcesAPI.Load<UnlockableDef>("UnlockableDefs/" + reference.unlockableName);
				if (Object.op_Implicit((Object)(object)unlockableDef))
				{
					Debug.Log((object)unlockableDef);
					reference.unlockableDef = unlockableDef;
					reference.unlockableName = null;
				}
			}
		}
		EditorUtil.SetDirty((Object)(object)this);
	}
}
