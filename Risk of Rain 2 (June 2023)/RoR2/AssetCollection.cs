using System;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/AssetCollection")]
public class AssetCollection : ScriptableObject
{
	public Object[] assets = Array.Empty<Object>();

	[ContextMenu("Add selected assets.")]
	private void AddSelectedAssets()
	{
		Object[] additionalAssets = Array.Empty<Object>();
		AddAssets(additionalAssets);
	}

	public void AddAssets(Object[] additionalAssets)
	{
		int num = assets.Length;
		Array.Resize(ref assets, assets.Length + additionalAssets.Length);
		for (int i = 0; i < additionalAssets.Length; i++)
		{
			assets[num + i] = additionalAssets[i];
		}
	}
}
