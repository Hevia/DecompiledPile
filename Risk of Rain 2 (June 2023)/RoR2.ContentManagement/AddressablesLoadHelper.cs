using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using HG;
using HG.AsyncOperations;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2.ContentManagement;

public class AddressablesLoadHelper
{
	private class Operation
	{
		public float weight;

		public IEnumerator coroutine;

		public float progress;
	}

	public class AddressablesLoadAsyncOperationWrapper<TAsset> : BaseAsyncOperation<TAsset[]> where TAsset : Object
	{
		private AsyncOperationHandle<IList<TAsset>>[] handles;

		private int completionCount;

		public override float progress
		{
			get
			{
				float num = 0f;
				if (handles.Length == 0)
				{
					return 0f;
				}
				float num2 = 1f / (float)handles.Length;
				for (int i = 0; i < handles.Length; i++)
				{
					num += Unsafe.As<AsyncOperationHandle<IList<TAsset>>, AsyncOperationHandle<IList<IList<TAsset>>>>(ref handles[i]).PercentComplete * num2;
				}
				return num;
			}
		}

		public unsafe AddressablesLoadAsyncOperationWrapper(IReadOnlyList<AsyncOperationHandle<IList<TAsset>>> handles)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			((BaseAsyncOperation<TAsset[][]>)(object)this)._002Ector();
			if (handles.Count == 0)
			{
				this.handles = Array.Empty<AsyncOperationHandle<IList<TAsset>>>();
				((BaseAsyncOperation<TAsset[][]>)(object)this).Complete((TAsset[][])(object)Array.Empty<TAsset>());
				return;
			}
			Action<AsyncOperationHandle<IList<TAsset>>> action = OnChildOperationCompleted;
			this.handles = new AsyncOperationHandle<IList<TAsset>>[handles.Count];
			for (int i = 0; i < handles.Count; i++)
			{
				this.handles[i] = handles[i];
				AsyncOperationHandle<IList<TAsset>> val = handles[i];
				((AsyncOperationHandle<IList<IList<TAsset>>>*)(&val))->Completed += (Action<AsyncOperationHandle<IList<IList<TAsset>>>>)(object)action;
			}
		}

		private unsafe void OnChildOperationCompleted(AsyncOperationHandle<IList<TAsset>> completedOperationHandle)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			completionCount++;
			if (completionCount != handles.Length)
			{
				return;
			}
			List<TAsset> list = new List<TAsset>();
			AsyncOperationHandle<IList<TAsset>>[] array = handles;
			for (int i = 0; i < array.Length; i++)
			{
				AsyncOperationHandle<IList<TAsset>> val = array[i];
				if (((AsyncOperationHandle<IList<IList<TAsset>>>*)(&val))->Result != null)
				{
					list.AddRange(((AsyncOperationHandle<IList<IList<TAsset>>>*)(&val))->Result);
				}
			}
			((BaseAsyncOperation<TAsset[][]>)(object)this).Complete((TAsset[][])(object)list.ToArray());
		}
	}

	private readonly IResourceLocator[] resourceLocators = Array.Empty<IResourceLocator>();

	private readonly object[] requiredKeys = Array.Empty<object>();

	public int maxConcurrentOperations = 2;

	public float timeoutDuration = float.PositiveInfinity;

	private readonly List<Operation> allOperations = new List<Operation>();

	private readonly Queue<Operation> pendingLoadOperations = new Queue<Operation>();

	private readonly List<Operation> runningLoadOperations = new List<Operation>();

	private readonly List<Operation> allGenericOperations = new List<Operation>();

	public ReadableProgress<float> progress { get; private set; }

	public IEnumerator coroutine { get; private set; }

	public AddressablesLoadHelper(IReadOnlyList<IResourceLocator> resourceLocators, object[] requiredKeys = null)
	{
		ArrayUtils.CloneTo<IResourceLocator, IReadOnlyList<IResourceLocator>>(resourceLocators, ref this.resourceLocators);
		if (requiredKeys != null)
		{
			ArrayUtils.CloneTo<object>(requiredKeys, ref this.requiredKeys);
		}
		progress = new ReadableProgress<float>();
		coroutine = Coroutine((IProgress<float>)progress);
	}

	public AddressablesLoadHelper(IResourceLocator resourceLocator, object[] requiredKeys = null)
		: this((IReadOnlyList<IResourceLocator>)(object)new IResourceLocator[1] { resourceLocator }, requiredKeys)
	{
	}

	public static AddressablesLoadHelper CreateUsingDefaultResourceLocator(object[] requiredKeys = null)
	{
		return new AddressablesLoadHelper(Addressables.ResourceLocators.First(), requiredKeys);
	}

	public static AddressablesLoadHelper CreateUsingDefaultResourceLocator(object requiredKey)
	{
		return CreateUsingDefaultResourceLocator(new object[1] { requiredKey });
	}

	public void AddContentPackLoadOperation(ContentPack contentPack)
	{
		this.AddLoadOperation<GameObject>(AddressablesLabels.characterBody, (Action<GameObject[]>)contentPack.bodyPrefabs.Add, 1f);
		this.AddLoadOperation<GameObject>(AddressablesLabels.characterMaster, (Action<GameObject[]>)contentPack.masterPrefabs.Add, 1f);
		this.AddLoadOperation<GameObject>(AddressablesLabels.projectile, (Action<GameObject[]>)contentPack.projectilePrefabs.Add, 1f);
		this.AddLoadOperation<GameObject>(AddressablesLabels.gameMode, (Action<GameObject[]>)contentPack.gameModePrefabs.Add, 1f);
		this.AddLoadOperation<GameObject>(AddressablesLabels.networkedObject, (Action<GameObject[]>)contentPack.networkedObjectPrefabs.Add, 1f);
		this.AddLoadOperation<SkillFamily>(AddressablesLabels.skillFamily, (Action<SkillFamily[]>)contentPack.skillFamilies.Add, 1f);
		this.AddLoadOperation<SkillDef>(AddressablesLabels.skillDef, (Action<SkillDef[]>)contentPack.skillDefs.Add, 1f);
		this.AddLoadOperation<UnlockableDef>(AddressablesLabels.unlockableDef, (Action<UnlockableDef[]>)contentPack.unlockableDefs.Add, 1f);
		this.AddLoadOperation<SurfaceDef>(AddressablesLabels.surfaceDef, (Action<SurfaceDef[]>)contentPack.surfaceDefs.Add, 1f);
		this.AddLoadOperation<SceneDef>(AddressablesLabels.sceneDef, (Action<SceneDef[]>)contentPack.sceneDefs.Add, 1f);
		this.AddLoadOperation<NetworkSoundEventDef>(AddressablesLabels.networkSoundEventDef, (Action<NetworkSoundEventDef[]>)contentPack.networkSoundEventDefs.Add, 1f);
		this.AddLoadOperation<MusicTrackDef>(AddressablesLabels.musicTrackDef, (Action<MusicTrackDef[]>)contentPack.musicTrackDefs.Add, 1f);
		this.AddLoadOperation<GameEndingDef>(AddressablesLabels.gameEndingDef, (Action<GameEndingDef[]>)contentPack.gameEndingDefs.Add, 1f);
		this.AddLoadOperation<ItemDef>(AddressablesLabels.itemDef, (Action<ItemDef[]>)contentPack.itemDefs.Add, 1f);
		this.AddLoadOperation<ItemTierDef>(AddressablesLabels.itemTierDef, (Action<ItemTierDef[]>)contentPack.itemTierDefs.Add, 1f);
		this.AddLoadOperation<ItemRelationshipProvider>(AddressablesLabels.itemRelationshipProvider, (Action<ItemRelationshipProvider[]>)contentPack.itemRelationshipProviders.Add, 1f);
		this.AddLoadOperation<ItemRelationshipType>(AddressablesLabels.itemRelationshipType, (Action<ItemRelationshipType[]>)contentPack.itemRelationshipTypes.Add, 1f);
		this.AddLoadOperation<EquipmentDef>(AddressablesLabels.equipmentDef, (Action<EquipmentDef[]>)contentPack.equipmentDefs.Add, 1f);
		this.AddLoadOperation<MiscPickupDef>(AddressablesLabels.miscPickupDef, (Action<MiscPickupDef[]>)contentPack.miscPickupDefs.Add, 1f);
		this.AddLoadOperation<BuffDef>(AddressablesLabels.buffDef, (Action<BuffDef[]>)contentPack.buffDefs.Add, 1f);
		this.AddLoadOperation<EliteDef>(AddressablesLabels.eliteDef, (Action<EliteDef[]>)contentPack.eliteDefs.Add, 1f);
		this.AddLoadOperation<SurvivorDef>(AddressablesLabels.survivorDef, (Action<SurvivorDef[]>)contentPack.survivorDefs.Add, 1f);
		this.AddLoadOperation<ArtifactDef>(AddressablesLabels.artifactDef, (Action<ArtifactDef[]>)contentPack.artifactDefs.Add, 1f);
		this.AddLoadOperation<GameObject, EffectDef>(AddressablesLabels.effect, (Action<EffectDef[]>)contentPack.effectDefs.Add, (Func<GameObject, EffectDef>)((GameObject asset) => new EffectDef(asset)), 1f);
		this.AddLoadOperation<EntityStateConfiguration>(AddressablesLabels.entityStateConfiguration, (Action<EntityStateConfiguration[]>)contentPack.entityStateConfigurations.Add, 1f);
		this.AddLoadOperation<ExpansionDef>(AddressablesLabels.expansionDef, (Action<ExpansionDef[]>)contentPack.expansionDefs.Add, 1f);
		this.AddLoadOperation<EntitlementDef>(AddressablesLabels.entitlementDef, (Action<EntitlementDef[]>)contentPack.entitlementDefs.Add, 1f);
		AddGenericOperation(AddEntityStateTypes(), 0.05f);
		IEnumerator AddEntityStateTypes()
		{
			contentPack.entityStateTypes.Add((from v in contentPack.entityStateConfigurations
				select (Type)v.targetType into v
				where v != null
				select v).ToArray());
			yield return null;
		}
	}

	public void AddLoadOperation<TAssetSrc>(string key, Action<TAssetSrc[]> onComplete, float weight = 1f) where TAssetSrc : Object
	{
		AddLoadOperation<TAssetSrc, TAssetSrc>(key, onComplete, null, weight);
	}

	public void AddLoadOperation<TAssetSrc, TAssetDest>(string key, Action<TAssetDest[]> onComplete, Func<TAssetSrc, TAssetDest> selector = null, float weight = 1f) where TAssetSrc : Object
	{
		Operation loadOperation = new Operation
		{
			weight = weight
		};
		loadOperation.coroutine = Coroutine();
		allOperations.Add(loadOperation);
		pendingLoadOperations.Enqueue(loadOperation);
		IEnumerator Coroutine()
		{
			yield return null;
			List<object> list = new List<object>();
			ListUtils.AddRange<object>(list, requiredKeys);
			list.Add(key);
			List<AsyncOperationHandle<IList<TAssetSrc>>> list2 = new List<AsyncOperationHandle<IList<TAssetSrc>>>(resourceLocators.Length);
			for (int j = 0; j < resourceLocators.Length; j++)
			{
				Action<AsyncOperationHandle, Exception> oldResourceManagerExceptionHandler = ResourceManager.ExceptionHandler;
				ResourceManager.ExceptionHandler = ResourceManagerExceptionHandler;
				try
				{
					AsyncOperationHandle<IList<TAssetSrc>> item = Addressables.LoadAssetsAsync<TAssetSrc>((IEnumerable)list, (Action<TAssetSrc>)null, (MergeMode)2);
					list2.Add(item);
				}
				finally
				{
					ResourceManager.ExceptionHandler = oldResourceManagerExceptionHandler;
				}
				void ResourceManagerExceptionHandler(AsyncOperationHandle operationHandle, Exception exception)
				{
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					if (!(exception is InvalidKeyException))
					{
						oldResourceManagerExceptionHandler?.Invoke(operationHandle, exception);
					}
				}
			}
			AddressablesLoadAsyncOperationWrapper<TAssetSrc> combinedLoadOperation = new AddressablesLoadAsyncOperationWrapper<TAssetSrc>(list2);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!((BaseAsyncOperation)combinedLoadOperation).isDone)
			{
				loadOperation.progress = ((BaseAsyncOperation)combinedLoadOperation).progress;
				float num = (float)stopwatch.Elapsed.TotalSeconds;
				if (num > timeoutDuration)
				{
					throw new Exception($"Loading exceeded timeout. elapsedSeconds={num} timeoutDuration={timeoutDuration}");
				}
				yield return null;
			}
			TAssetSrc[] loadedAssets = ((BaseAsyncOperation<TAssetSrc[]>)combinedLoadOperation).result.Where((TAssetSrc asset) => Object.op_Implicit((Object)(object)asset)).ToArray();
			loadOperation.progress = 0.97f;
			yield return null;
			if (onComplete != null)
			{
				TAssetDest[] convertedAssets;
				if (selector == null)
				{
					if (!(typeof(TAssetSrc) == typeof(TAssetDest)))
					{
						throw new ArgumentNullException("selector", "Converter must be provided when TAssetSrc and TAssetDest differ.");
					}
					convertedAssets = (TAssetDest[])(object)loadedAssets;
				}
				else
				{
					convertedAssets = new TAssetDest[loadedAssets.Length];
					int i = 0;
					while (i < loadedAssets.Length)
					{
						yield return null;
						convertedAssets[i] = selector(loadedAssets[i]);
						int num2 = i + 1;
						i = num2;
					}
				}
				yield return null;
				string[] assetNames = new string[loadedAssets.Length];
				for (int k = 0; k < loadedAssets.Length; k++)
				{
					assetNames[k] = ((Object)loadedAssets[k]).name;
				}
				yield return null;
				Array.Sort(assetNames, convertedAssets, (IComparer<string>?)StringComparer.Ordinal);
				onComplete?.Invoke(convertedAssets);
			}
			loadOperation.progress = 1f;
		}
	}

	public void AddGenericOperation(IEnumerator coroutine, float weight = 1f)
	{
		Operation item = new Operation
		{
			weight = weight,
			coroutine = coroutine
		};
		allOperations.Add(item);
		allGenericOperations.Add(item);
	}

	public void AddGenericOperation(IEnumerable coroutineProvider, float weight = 1f)
	{
		AddGenericOperation(coroutineProvider.GetEnumerator(), weight);
	}

	public void AddGenericOperation(Func<IEnumerator> coroutineMethod, float weight = 1f)
	{
		AddGenericOperation(coroutineMethod(), weight);
	}

	public void AddGenericOperation(Action action, float weight = 1f)
	{
		AddGenericOperation(Coroutine(), weight);
		IEnumerator Coroutine()
		{
			action();
			yield return null;
		}
	}

	private IEnumerator Coroutine(IProgress<float> progressReceiver)
	{
		while (pendingLoadOperations.Count > 0 || runningLoadOperations.Count > 0)
		{
			while (pendingLoadOperations.Count > 0 && runningLoadOperations.Count < maxConcurrentOperations)
			{
				runningLoadOperations.Add(pendingLoadOperations.Dequeue());
			}
			int i = 0;
			while (i < runningLoadOperations.Count)
			{
				Operation operation = runningLoadOperations[i];
				int num;
				if (operation.coroutine.MoveNext())
				{
					UpdateProgress();
					yield return operation.coroutine.Current;
				}
				else
				{
					runningLoadOperations.RemoveAt(i);
					num = i - 1;
					i = num;
				}
				num = i + 1;
				i = num;
			}
		}
		foreach (Operation genericOperation in allGenericOperations)
		{
			while (genericOperation.coroutine.MoveNext())
			{
				UpdateProgress();
				yield return genericOperation.coroutine.Current;
			}
		}
		progressReceiver.Report(1f);
		void UpdateProgress()
		{
			float num2 = 0f;
			float num3 = 0f;
			for (int j = 0; j < allOperations.Count; j++)
			{
				Operation operation2 = allOperations[j];
				num2 += operation2.weight;
				num3 += operation2.progress * operation2.weight;
			}
			if (num2 == 0f)
			{
				num2 = 1f;
				num3 = 0.5f;
			}
			progressReceiver.Report(num3 / num2);
		}
	}
}
