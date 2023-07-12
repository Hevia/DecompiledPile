using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2;

[Serializable]
public class AssetReferenceScene : AssetReference
{
	public AssetReferenceScene(string guid)
		: base(guid)
	{
	}

	public override bool ValidateAsset(string path)
	{
		if (((AssetReference)this).ValidateAsset(path))
		{
			return path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override bool ValidateAsset(Object obj)
	{
		bool num = ((AssetReference)this).ValidateAsset(obj);
		bool flag = false;
		return num && flag;
	}
}
