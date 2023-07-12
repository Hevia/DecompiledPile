using System;
using JetBrains.Annotations;

namespace RoR2;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AssetCheckAttribute : Attribute
{
	public Type assetType;

	public AssetCheckAttribute(Type assetType)
	{
		this.assetType = assetType;
	}
}
