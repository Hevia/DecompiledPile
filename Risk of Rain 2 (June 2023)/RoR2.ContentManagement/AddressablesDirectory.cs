using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using HG.AssetManagement;
using HG.AsyncOperations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2.ContentManagement;

public class AddressablesDirectory : IAssetRepository
{
	public class AddressablesLoadAsyncOperationWrapper<TAsset> : BaseAsyncOperation<TAsset[]> where TAsset : Object
	{
		private AsyncOperationHandle<TAsset>[] handles;

		private int completionCount;

		private List<TAsset> results = new List<TAsset>();

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
					num += handles[i].PercentComplete * num2;
				}
				return num;
			}
		}

		public AddressablesLoadAsyncOperationWrapper(IReadOnlyList<AsyncOperationHandle<TAsset>> handles)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			((BaseAsyncOperation<TAsset[][]>)(object)this)._002Ector();
			if (handles.Count == 0)
			{
				this.handles = Array.Empty<AsyncOperationHandle<TAsset>>();
				((BaseAsyncOperation<TAsset[][]>)(object)this).Complete((TAsset[][])(object)Array.Empty<TAsset>());
				return;
			}
			results = new List<TAsset>(handles.Count);
			this.handles = new AsyncOperationHandle<TAsset>[handles.Count];
			Action<AsyncOperationHandle<TAsset>> action = OnChildOperationCompleted;
			for (int i = 0; i < handles.Count; i++)
			{
				this.handles[i] = handles[i];
				AsyncOperationHandle<TAsset> val = handles[i];
				val.Completed += action;
			}
		}

		private void OnChildOperationCompleted(AsyncOperationHandle<TAsset> handle)
		{
			if (Object.op_Implicit((Object)(object)handle.Result))
			{
				results.Add(handle.Result);
			}
			completionCount++;
			Debug.Log((object)$"{handle.Result} ({completionCount}/{handles.Length})");
			if (completionCount == handles.Length)
			{
				((BaseAsyncOperation<TAsset[][]>)(object)this).Complete((TAsset[][])(object)results.ToArray());
			}
		}
	}

	private string baseDirectory;

	private FilePathTree folderTree;

	private IResourceLocator[] resourceLocators = Array.Empty<IResourceLocator>();

	public AddressablesDirectory(string baseDirectory, IEnumerable<IResourceLocator> resourceLocators)
	{
		baseDirectory = baseDirectory ?? string.Empty;
		if (baseDirectory.Length > 0 && !baseDirectory.EndsWith("/"))
		{
			throw new ArgumentException("'baseDirectory' must be empty or end with '/'. baseDirectory=\"" + baseDirectory + "\"");
		}
		this.baseDirectory = baseDirectory;
		this.resourceLocators = resourceLocators.ToArray();
		List<string> list = new List<string>();
		IResourceLocator[] array = this.resourceLocators;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (object key in array[i].Keys)
			{
				if (key is string text && text.StartsWith(baseDirectory))
				{
					list.Add(text);
				}
			}
		}
		folderTree = FilePathTree.Create<List<string>>(list);
	}

	public BaseAsyncOperation<TAsset[]> LoadAllAsync<TAsset>(string folderPath) where TAsset : Object
	{
		List<string> list = new List<string>();
		string text = baseDirectory + folderPath;
		folderTree.GetEntriesInFolder(text, list);
		if (list.Count == 0)
		{
			Debug.Log((object)("No entries for path \"" + text + "\"."));
		}
		return IssueLoadOperationAsGroup<TAsset>(list);
	}

	private BaseAsyncOperation<TAsset[]> IssueLoadOperationAsGroup<TAsset>(List<string> entriesToLoad) where TAsset : Object
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		List<IResourceLocation> list = FilterPathsByType<TAsset>(entriesToLoad);
		List<TAsset> results = new List<TAsset>();
		AsyncOperationHandle<IList<TAsset>> loadOperationHandle = Addressables.LoadAssetsAsync<TAsset>((IList<IResourceLocation>)list, (Action<TAsset>)delegate(TAsset result)
		{
			results.Add(result);
		});
		ScriptedAsyncOperation<TAsset[]> resultOperation = new ScriptedAsyncOperation<TAsset[]>((Func<float>)(() => loadOperationHandle.PercentComplete));
		loadOperationHandle.Completed += delegate
		{
			resultOperation.Complete(results.ToArray());
		};
		return (BaseAsyncOperation<TAsset[]>)(object)resultOperation;
	}

	private BaseAsyncOperation<TAsset[]> IssueLoadOperationsIndividually<TAsset>(List<string> entriesToLoad) where TAsset : Object
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		List<IResourceLocation> list = FilterPathsByType<TAsset>(entriesToLoad);
		AsyncOperationHandle<TAsset>[] array = new AsyncOperationHandle<TAsset>[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			AsyncOperationHandle<TAsset> val = Addressables.LoadAssetAsync<TAsset>(list[i]);
			array[i] = val;
		}
		return new AddressablesLoadAsyncOperationWrapper<TAsset>(array);
	}

	private List<IResourceLocation> FilterPathsByType<TAsset>(List<string> paths)
	{
		Type typeFromHandle = typeof(TAsset);
		bool[] array = new bool[paths.Count];
		List<IResourceLocation> list = new List<IResourceLocation>();
		IResourceLocator[] array2 = resourceLocators;
		IList<IResourceLocation> list2 = default(IList<IResourceLocation>);
		foreach (IResourceLocator val in array2)
		{
			for (int j = 0; j < paths.Count; j++)
			{
				if (array[j] || !val.Locate((object)paths[j], (Type)null, ref list2))
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					IResourceLocation val2 = list2[k];
					if (typeFromHandle.IsAssignableFrom(val2.ResourceType))
					{
						list.Add(val2);
						array[j] = true;
						break;
					}
				}
			}
		}
		return list;
	}
}
