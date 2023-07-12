using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2;

public class AddressablesTextDataManager : TextDataManager
{
	private bool configInitialized;

	private bool locInitialized;

	private List<TextAsset> configFiles = new List<TextAsset>();

	public override bool InitializedConfigFiles => configInitialized;

	public override bool InitializedLocFiles => locInitialized;

	public AddressablesTextDataManager()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		AsyncOperationHandle<IList<IResourceLocation>> val = Addressables.LoadResourceLocationsAsync((object)"Config", (Type)null);
		val.Completed += delegate(AsyncOperationHandle<IList<IResourceLocation>> addressesResult)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			AsyncOperationHandle<IList<TextAsset>> val2 = Addressables.LoadAssetsAsync<TextAsset>(addressesResult.Result, (Action<TextAsset>)delegate(TextAsset conf)
			{
				configFiles.Add(conf);
			});
			val2.Completed += delegate
			{
				configInitialized = true;
			};
		};
		locInitialized = true;
	}

	public override string GetConfFile(string fileName, string path)
	{
		string fileNameLower = fileName.ToLower();
		TextAsset val = configFiles.Find((TextAsset x) => ((Object)x).name.ToLower().Contains(fileNameLower));
		if ((Object)(object)val != (Object)null)
		{
			return val.text;
		}
		return null;
	}

	public override void GetLocFiles(string folderPath, Action<string[]> callback)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		AsyncOperationHandle<IList<IResourceLocation>> val = Addressables.LoadResourceLocationsAsync((object)"Localization", (Type)null);
		val.Completed += delegate(AsyncOperationHandle<IList<IResourceLocation>> addressesResult)
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			List<IResourceLocation> list = new List<IResourceLocation>();
			for (int num = addressesResult.Result.Count - 1; num >= 0; num--)
			{
				if (addressesResult.Result[num].PrimaryKey.StartsWith(folderPath))
				{
					list.Add(addressesResult.Result[num]);
				}
			}
			List<TextAsset> textAssetResults = new List<TextAsset>(list.Count);
			AsyncOperationHandle<IList<TextAsset>> val2 = Addressables.LoadAssetsAsync<TextAsset>((IList<IResourceLocation>)list, (Action<TextAsset>)delegate(TextAsset x)
			{
				if ((Object)(object)x != (Object)null)
				{
					textAssetResults.Add(x);
				}
			});
			val2.Completed += delegate
			{
				int count = textAssetResults.Count;
				string[] array = new string[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = textAssetResults[i].text;
				}
				callback?.Invoke(array);
			};
		};
	}
}
