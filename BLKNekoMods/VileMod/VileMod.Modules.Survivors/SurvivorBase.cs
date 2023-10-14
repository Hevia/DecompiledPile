using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using VileMod.Modules.Characters;

namespace VileMod.Modules.Survivors;

internal abstract class SurvivorBase : CharacterBase
{
	public abstract string survivorTokenPrefix { get; }

	public abstract UnlockableDef characterUnlockableDef { get; }

	public virtual ConfigEntry<bool> characterEnabledConfig { get; }

	public virtual GameObject displayPrefab { get; set; }

	public override void InitializeCharacter()
	{
		if (characterEnabledConfig == null || characterEnabledConfig.Value)
		{
			InitializeUnlockables();
			base.InitializeCharacter();
			InitializeSurvivor();
		}
	}

	protected override void InitializeCharacterBodyAndModel()
	{
		base.InitializeCharacterBodyAndModel();
		InitializeDisplayPrefab();
	}

	protected virtual void InitializeSurvivor()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RegisterNewSurvivor(bodyPrefab, displayPrefab, Color.grey, survivorTokenPrefix, characterUnlockableDef, bodyInfo.sortPosition);
	}

	protected virtual void InitializeDisplayPrefab()
	{
		displayPrefab = Prefabs.CreateDisplayPrefab(prefabBodyName + "Display", bodyPrefab, bodyInfo);
	}

	public virtual void InitializeUnlockables()
	{
	}

	public static void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string tokenPrefix)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		RegisterNewSurvivor(bodyPrefab, displayPrefab, charColor, tokenPrefix, null, 100f);
	}

	public static void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string tokenPrefix, float sortPosition)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		RegisterNewSurvivor(bodyPrefab, displayPrefab, charColor, tokenPrefix, null, sortPosition);
	}

	public static void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string tokenPrefix, UnlockableDef unlockableDef)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		RegisterNewSurvivor(bodyPrefab, displayPrefab, charColor, tokenPrefix, unlockableDef, 100f);
	}

	public static void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string tokenPrefix, UnlockableDef unlockableDef, float sortPosition)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		SurvivorDef val = ScriptableObject.CreateInstance<SurvivorDef>();
		val.bodyPrefab = bodyPrefab;
		val.displayPrefab = displayPrefab;
		val.primaryColor = charColor;
		val.cachedName = ((Object)bodyPrefab).name.Replace("Body", "");
		val.displayNameToken = tokenPrefix + "NAME";
		val.descriptionToken = tokenPrefix + "DESCRIPTION";
		val.outroFlavorToken = tokenPrefix + "OUTRO_FLAVOR";
		val.mainEndingEscapeFailureFlavorToken = tokenPrefix + "OUTRO_FAILURE";
		val.desiredSortPosition = sortPosition;
		val.unlockableDef = unlockableDef;
		Content.AddSurvivorDef(val);
	}

	protected virtual void AddCssPreviewSkill(int indexFromEditor, SkillFamily skillFamily, SkillDef skillDef)
	{
		CharacterSelectSurvivorPreviewDisplayController component = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			Log.Error("trying to add skillChangeResponse to null CharacterSelectSurvivorPreviewDisplayController.\nMake sure you created one on your Display prefab in editor");
			return;
		}
		component.skillChangeResponses[indexFromEditor].triggerSkillFamily = skillFamily;
		component.skillChangeResponses[indexFromEditor].triggerSkill = skillDef;
	}

	protected virtual void AddCssPreviewSkin(int indexFromEditor, SkinDef skinDef)
	{
		CharacterSelectSurvivorPreviewDisplayController component = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			Log.Error("trying to add skinChangeResponse to null CharacterSelectSurvivorPreviewDisplayController.\nMake sure you created one on your Display prefab in editor");
		}
		else
		{
			component.skinChangeResponses[indexFromEditor].triggerSkin = skinDef;
		}
	}

	protected virtual void FinalizeCSSPreviewDisplayController()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)displayPrefab))
		{
			return;
		}
		CharacterSelectSurvivorPreviewDisplayController component = displayPrefab.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		component.bodyPrefab = bodyPrefab;
		List<SkillChangeResponse> list = new List<SkillChangeResponse>();
		for (int i = 0; i < component.skillChangeResponses.Length; i++)
		{
			if ((Object)(object)component.skillChangeResponses[i].triggerSkillFamily != (Object)null)
			{
				list.Add(component.skillChangeResponses[i]);
			}
		}
		component.skillChangeResponses = list.ToArray();
	}
}
