using System;
using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using UnityEngine;

namespace VileMod.Modules;

internal class ContentPacks : IContentPackProvider
{
	internal ContentPack contentPack = new ContentPack();

	public static List<GameObject> bodyPrefabs = new List<GameObject>();

	public static List<GameObject> masterPrefabs = new List<GameObject>();

	public static List<GameObject> projectilePrefabs = new List<GameObject>();

	public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();

	public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();

	public static List<SkillFamily> skillFamilies = new List<SkillFamily>();

	public static List<SkillDef> skillDefs = new List<SkillDef>();

	public static List<Type> entityStates = new List<Type>();

	public static List<BuffDef> buffDefs = new List<BuffDef>();

	public static List<EffectDef> effectDefs = new List<EffectDef>();

	public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

	public string identifier => "com.BLKNeko.VileModV3";

	public void Initialize()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		ContentManager.collectContentPackProviders += new CollectContentPackProvidersDelegate(ContentManager_collectContentPackProviders);
	}

	private void ContentManager_collectContentPackProviders(AddContentPackProviderDelegate addContentPackProvider)
	{
		addContentPackProvider.Invoke((IContentPackProvider)(object)this);
	}

	public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
	{
		contentPack.identifier = identifier;
		contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
		contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
		contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
		contentPack.survivorDefs.Add(survivorDefs.ToArray());
		contentPack.unlockableDefs.Add(unlockableDefs.ToArray());
		contentPack.skillDefs.Add(skillDefs.ToArray());
		contentPack.skillFamilies.Add(skillFamilies.ToArray());
		contentPack.entityStateTypes.Add(entityStates.ToArray());
		contentPack.buffDefs.Add(buffDefs.ToArray());
		contentPack.effectDefs.Add(effectDefs.ToArray());
		contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());
		args.ReportProgress(1f);
		yield break;
	}

	public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
	{
		ContentPack.Copy(contentPack, args.output);
		args.ReportProgress(1f);
		yield break;
	}

	public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
	{
		args.ReportProgress(1f);
		yield break;
	}
}
