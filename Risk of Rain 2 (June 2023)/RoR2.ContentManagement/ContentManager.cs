using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HG;
using HG.Coroutines;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using UnityEngine;

namespace RoR2.ContentManagement;

public static class ContentManager
{
	private class ContentPackLoader
	{
		private delegate IEnumerator PerContentPackAction(ref ContentPackLoadInfo contentPackLoadInfo, ReadOnlyArray<ContentPackLoadInfo> readOnlyPeers, IProgress<float> progressReceiver);

		private ContentPackLoadInfo[] contentPackLoadInfos;

		public List<ReadOnlyContentPack> output = new List<ReadOnlyContentPack>();

		public ContentPackLoader(List<IContentPackProvider> contentPackProviders)
		{
			contentPackLoadInfos = new ContentPackLoadInfo[contentPackProviders.Count];
			for (int i = 0; i < contentPackProviders.Count; i++)
			{
				string identifier = contentPackProviders[i].identifier;
				contentPackLoadInfos[i] = new ContentPackLoadInfo
				{
					index = i,
					contentPackProviderIdentifier = identifier,
					contentPackProvider = contentPackProviders[i],
					previousContentPack = new ContentPack
					{
						identifier = identifier
					},
					retries = 0
				};
			}
		}

		private IEnumerator DoPerContentPackProviderCoroutine(IProgress<float> progressReceiver, PerContentPackAction action)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlyArray<ContentPackLoadInfo> readOnlyPeers = ReadOnlyArray<ContentPackLoadInfo>.op_Implicit(ArrayUtils.Clone<ContentPackLoadInfo>(contentPackLoadInfos));
			ParallelProgressCoroutine val = new ParallelProgressCoroutine(progressReceiver);
			for (int i = 0; i < contentPackLoadInfos.Length; i++)
			{
				ReadableProgress<float> val2 = new ReadableProgress<float>();
				IEnumerator enumerator = action(ref contentPackLoadInfos[i], readOnlyPeers, (IProgress<float>)val2);
				val.Add(enumerator, val2);
			}
			return (IEnumerator)val;
		}

		public IEnumerator InitialLoad(IProgress<float> progressReceiver)
		{
			yield return DoPerContentPackProviderCoroutine(progressReceiver, StartLoadCoroutine);
			static IEnumerator StartLoadCoroutine(ref ContentPackLoadInfo loadInfo, ReadOnlyArray<ContentPackLoadInfo> readOnlyPeers, IProgress<float> providedProgressReceiver)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				LoadStaticContentAsyncArgs args = new LoadStaticContentAsyncArgs(providedProgressReceiver, readOnlyPeers);
				return loadInfo.contentPackProvider.LoadStaticContentAsync(args);
			}
		}

		public IEnumerator StepGenerateContentPacks(IProgress<float> progressReceiver, IProgress<bool> completionReceiver, int retriesRemaining)
		{
			ContentPack[] newContentPacks = new ContentPack[contentPackLoadInfos.Length];
			for (int i = 0; i < newContentPacks.Length; i++)
			{
				newContentPacks[i] = new ContentPack
				{
					identifier = contentPackLoadInfos[i].contentPackProviderIdentifier
				};
			}
			yield return DoPerContentPackProviderCoroutine(progressReceiver, StartGeneratorCoroutine);
			bool flag = true;
			for (int j = 0; j < contentPackLoadInfos.Length; j++)
			{
				ContentPack contentPack = newContentPacks[j];
				contentPack.identifier = contentPackLoadInfos[j].contentPackProviderIdentifier;
				if (!contentPackLoadInfos[j].previousContentPack.Equals(contentPack))
				{
					flag = false;
					contentPackLoadInfos[j].retries++;
					contentPackLoadInfos[j].previousContentPack = contentPack;
				}
			}
			if (flag)
			{
				completionReceiver.Report(value: true);
			}
			IEnumerator StartGeneratorCoroutine(ref ContentPackLoadInfo loadInfo, ReadOnlyArray<ContentPackLoadInfo> readOnlyPeers, IProgress<float> providedProgressReceiver)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				ContentPack contentPack2 = newContentPacks[loadInfo.index];
				GetContentPackAsyncArgs args = new GetContentPackAsyncArgs(providedProgressReceiver, contentPack2, readOnlyPeers, retriesRemaining);
				return loadInfo.contentPackProvider.GenerateContentPackAsync(args);
			}
		}

		public IEnumerator LoadContentPacks(IProgress<float> progressReceiver)
		{
			int maxRetries = 10;
			bool complete = false;
			ReadableProgress<bool> completionReceiver = new ReadableProgress<bool>((Action<bool>)delegate(bool result)
			{
				complete = result;
			});
			int i = 0;
			while (i < maxRetries)
			{
				yield return StepGenerateContentPacks(progressReceiver, (IProgress<bool>)completionReceiver, maxRetries - i - 1);
				if (complete)
				{
					break;
				}
				int num = i + 1;
				i = num;
			}
			for (int j = 0; j < contentPackLoadInfos.Length; j++)
			{
				output.Add(contentPackLoadInfos[j].previousContentPack);
			}
		}

		public IEnumerator RunCleanup(IProgress<float> progressReceiver)
		{
			yield return DoPerContentPackProviderCoroutine(progressReceiver, StartFinalizeCoroutine);
			IEnumerator StartFinalizeCoroutine(ref ContentPackLoadInfo loadInfo, ReadOnlyArray<ContentPackLoadInfo> readOnlyPeers, IProgress<float> providedProgressReceiver)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				FinalizeAsyncArgs args = new FinalizeAsyncArgs(providedProgressReceiver, readOnlyPeers, contentPackLoadInfos[loadInfo.index].previousContentPack);
				return loadInfo.contentPackProvider.FinalizeAsync(args);
			}
		}
	}

	public delegate void AddContentPackProviderDelegate(IContentPackProvider contentPackProvider);

	public delegate void CollectContentPackProvidersDelegate(AddContentPackProviderDelegate addContentPackProvider);

	private static ContentPack[] contentPacks;

	public static ItemDef[] _itemDefs;

	public static ItemTierDef[] _itemTierDefs;

	private static ItemRelationshipProvider[] _itemRelationshipProviders;

	private static ItemRelationshipType[] _itemRelationshipTypes;

	public static EquipmentDef[] _equipmentDefs;

	public static BuffDef[] _buffDefs;

	public static EliteDef[] _eliteDefs;

	public static UnlockableDef[] _unlockableDefs;

	public static SurvivorDef[] _survivorDefs;

	public static GameObject[] _bodyPrefabs;

	public static GameObject[] _masterPrefabs;

	public static ArtifactDef[] _artifactDefs;

	public static EffectDef[] _effectDefs;

	public static SkillDef[] _skillDefs;

	public static SkillFamily[] _skillFamilies;

	public static SurfaceDef[] _surfaceDefs;

	public static SceneDef[] _sceneDefs;

	public static GameObject[] _projectilePrefabs;

	public static NetworkSoundEventDef[] _networkSoundEventDefs;

	public static MusicTrackDef[] _musicTrackDefs;

	public static GameObject[] _networkedObjectPrefabs;

	public static GameObject[] _gameModePrefabs;

	public static GameEndingDef[] _gameEndingDefs;

	public static EntityStateConfiguration[] _entityStateConfigurations;

	public static Type[] _entityStateTypes;

	public static ExpansionDef[] _expansionDefs;

	public static EntitlementDef[] _entitlementDefs;

	private static MiscPickupDef[] _miscPickupDefs;

	public static ItemDef[] itemDefs => _itemDefs;

	public static ItemTierDef[] itemTierDefs => _itemTierDefs;

	public static ItemRelationshipProvider[] itemRelationshipProviders => _itemRelationshipProviders;

	public static ItemRelationshipType[] itemRelationshipTypes => _itemRelationshipTypes;

	public static EquipmentDef[] equipmentDefs => _equipmentDefs;

	public static BuffDef[] buffDefs => _buffDefs;

	public static EliteDef[] eliteDefs => _eliteDefs;

	public static UnlockableDef[] unlockableDefs => _unlockableDefs;

	public static SurvivorDef[] survivorDefs => _survivorDefs;

	public static GameObject[] bodyPrefabs => _bodyPrefabs;

	public static GameObject[] masterPrefabs => _masterPrefabs;

	public static ArtifactDef[] artifactDefs => _artifactDefs;

	public static EffectDef[] effectDefs => _effectDefs;

	public static SkillDef[] skillDefs => _skillDefs;

	public static SkillFamily[] skillFamilies => _skillFamilies;

	public static SurfaceDef[] surfaceDefs => _surfaceDefs;

	public static SceneDef[] sceneDefs => _sceneDefs;

	public static GameObject[] projectilePrefabs => _projectilePrefabs;

	public static NetworkSoundEventDef[] networkSoundEventDefs => _networkSoundEventDefs;

	public static MusicTrackDef[] musicTrackDefs => _musicTrackDefs;

	public static GameObject[] networkedObjectPrefabs => _networkedObjectPrefabs;

	public static GameObject[] gameModePrefabs => _gameModePrefabs;

	public static GameEndingDef[] gameEndingDefs => _gameEndingDefs;

	public static EntityStateConfiguration[] entityStateConfigurations => _entityStateConfigurations;

	public static Type[] entityStateTypes => _entityStateTypes;

	public static ExpansionDef[] expansionDefs => _expansionDefs;

	public static EntitlementDef[] entitlementDefs => _entitlementDefs;

	public static MiscPickupDef[] miscPickupDefs => _miscPickupDefs;

	public static ReadOnlyArray<ReadOnlyContentPack> allLoadedContentPacks { get; private set; } = ReadOnlyArray<ReadOnlyContentPack>.op_Implicit(Array.Empty<ReadOnlyContentPack>());


	public static event CollectContentPackProvidersDelegate collectContentPackProviders;

	public static event Action<ReadOnlyArray<ReadOnlyContentPack>> onContentPacksAssigned;

	public static IEnumerator LoadContentPacks(IProgress<float> progressReceiver)
	{
		List<IContentPackProvider> contentPackProviders = new List<IContentPackProvider>();
		ContentManager.collectContentPackProviders?.Invoke(AddContentPackProvider);
		ContentManager.collectContentPackProviders = null;
		Debug.Log((object)"LoadContentPacks() start");
		ContentPackLoader loader = new ContentPackLoader(contentPackProviders);
		yield return loader.InitialLoad((IProgress<float>)new ReadableProgress<float>((Action<float>)delegate(float newValue)
		{
			progressReceiver.Report(Util.Remap(newValue, 0f, 1f, 0f, 0.8f));
		}));
		yield return loader.LoadContentPacks((IProgress<float>)new ReadableProgress<float>((Action<float>)delegate(float newValue)
		{
			progressReceiver.Report(Util.Remap(newValue, 0f, 1f, 0.8f, 0.95f));
		}));
		yield return loader.RunCleanup((IProgress<float>)new ReadableProgress<float>((Action<float>)delegate(float newValue)
		{
			progressReceiver.Report(Util.Remap(newValue, 0f, 1f, 0.95f, 1f));
		}));
		Debug.Log((object)"LoadContentPacks() end");
		SetContentPacks(loader.output);
		void AddContentPackProvider(IContentPackProvider contentPackProvider)
		{
			contentPackProviders.Add(contentPackProvider);
		}
	}

	private static void SetContentPacks(List<ReadOnlyContentPack> newContentPacks)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		allLoadedContentPacks = ReadOnlyArray<ReadOnlyContentPack>.op_Implicit(newContentPacks.ToArray());
		SetAssets<ItemDef>(ref _itemDefs, (ReadOnlyContentPack contentPack) => contentPack.itemDefs);
		SetAssets<ItemTierDef>(ref _itemTierDefs, (ReadOnlyContentPack contentPack) => contentPack.itemTierDefs);
		SetAssets<ItemRelationshipProvider>(ref _itemRelationshipProviders, (ReadOnlyContentPack contentPack) => contentPack.itemRelationshipProviders);
		SetAssets<ItemRelationshipType>(ref _itemRelationshipTypes, (ReadOnlyContentPack contentPack) => contentPack.itemRelationshipTypes);
		SetAssets<EquipmentDef>(ref _equipmentDefs, (ReadOnlyContentPack contentPack) => contentPack.equipmentDefs);
		SetAssets<BuffDef>(ref _buffDefs, (ReadOnlyContentPack contentPack) => contentPack.buffDefs);
		SetAssets<EliteDef>(ref _eliteDefs, (ReadOnlyContentPack contentPack) => contentPack.eliteDefs);
		SetAssets<UnlockableDef>(ref _unlockableDefs, (ReadOnlyContentPack contentPack) => contentPack.unlockableDefs);
		SetAssets<SurvivorDef>(ref _survivorDefs, (ReadOnlyContentPack contentPack) => contentPack.survivorDefs);
		SetAssets<GameObject>(ref _bodyPrefabs, (ReadOnlyContentPack contentPack) => contentPack.bodyPrefabs);
		SetAssets<GameObject>(ref _masterPrefabs, (ReadOnlyContentPack contentPack) => contentPack.masterPrefabs);
		SetAssets<ArtifactDef>(ref _artifactDefs, (ReadOnlyContentPack contentPack) => contentPack.artifactDefs);
		SetAssets<EffectDef>(ref _effectDefs, (ReadOnlyContentPack contentPack) => contentPack.effectDefs);
		SetAssets<SkillDef>(ref _skillDefs, (ReadOnlyContentPack contentPack) => contentPack.skillDefs);
		SetAssets<SkillFamily>(ref _skillFamilies, (ReadOnlyContentPack contentPack) => contentPack.skillFamilies);
		SetAssets<SurfaceDef>(ref _surfaceDefs, (ReadOnlyContentPack contentPack) => contentPack.surfaceDefs);
		SetAssets<SceneDef>(ref _sceneDefs, (ReadOnlyContentPack contentPack) => contentPack.sceneDefs);
		SetAssets<GameObject>(ref _projectilePrefabs, (ReadOnlyContentPack contentPack) => contentPack.projectilePrefabs);
		SetAssets<NetworkSoundEventDef>(ref _networkSoundEventDefs, (ReadOnlyContentPack contentPack) => contentPack.networkSoundEventDefs);
		SetAssets<MusicTrackDef>(ref _musicTrackDefs, (ReadOnlyContentPack contentPack) => contentPack.musicTrackDefs);
		SetAssets<GameObject>(ref _networkedObjectPrefabs, (ReadOnlyContentPack contentPack) => contentPack.networkedObjectPrefabs);
		SetAssets<GameObject>(ref _gameModePrefabs, (ReadOnlyContentPack contentPack) => contentPack.gameModePrefabs);
		SetAssets<GameEndingDef>(ref _gameEndingDefs, (ReadOnlyContentPack contentPack) => contentPack.gameEndingDefs);
		SetAssets<Type>(ref _entityStateTypes, (ReadOnlyContentPack contentPack) => contentPack.entityStateTypes);
		SetAssets<EntityStateConfiguration>(ref _entityStateConfigurations, (ReadOnlyContentPack contentPack) => contentPack.entityStateConfigurations);
		SetAssets<ExpansionDef>(ref _expansionDefs, (ReadOnlyContentPack contentPack) => contentPack.expansionDefs);
		SetAssets<EntitlementDef>(ref _entitlementDefs, (ReadOnlyContentPack contentPack) => contentPack.entitlementDefs);
		SetAssets<MiscPickupDef>(ref _miscPickupDefs, (ReadOnlyContentPack contentPack) => contentPack.miscPickupDefs);
		ContentManager.onContentPacksAssigned?.Invoke(allLoadedContentPacks);
		void SetAssets<T>(ref T[] field, Func<ReadOnlyContentPack, IEnumerable<T>> selector)
		{
			field = newContentPacks.SelectMany(selector).ToArray();
		}
	}

	public static ReadOnlyContentPack? FindContentPack(string contentPackIdentifier)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		while (true)
		{
			int num2 = num;
			ReadOnlyArray<ReadOnlyContentPack> val = allLoadedContentPacks;
			if (num2 >= val.Length)
			{
				break;
			}
			val = allLoadedContentPacks;
			ref ReadOnlyContentPack reference = val[num];
			if (string.Equals(reference.identifier, contentPackIdentifier, StringComparison.Ordinal))
			{
				return reference;
			}
			num++;
		}
		return null;
	}
}
