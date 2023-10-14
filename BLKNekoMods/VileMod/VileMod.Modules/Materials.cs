using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace VileMod.Modules;

internal static class Materials
{
	private static List<Material> cachedMaterials = new List<Material>();

	internal static Shader hotpoo = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

	public static Material CreateHopooMaterial(string materialName)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		Material val = cachedMaterials.Find(delegate(Material mat)
		{
			materialName.Replace(" (Instance)", "");
			return ((Object)mat).name.Contains(materialName);
		});
		if (Object.op_Implicit((Object)(object)val))
		{
			return val;
		}
		val = Assets.mainAssetBundle.LoadAsset<Material>(materialName);
		if (!Object.op_Implicit((Object)(object)val))
		{
			Log.Error("Failed to load material: " + materialName + " - Check to see that the material in your Unity project matches this name");
			return new Material(hotpoo);
		}
		return val.SetHopooMaterial();
	}

	public static Material SetHopooMaterial(this Material tempMat)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (cachedMaterials.Contains(tempMat))
		{
			return tempMat;
		}
		float? num = null;
		Color? val = null;
		if (tempMat.IsKeywordEnabled("_NORMALMAP"))
		{
			num = tempMat.GetFloat("_BumpScale");
		}
		if (tempMat.IsKeywordEnabled("_EMISSION"))
		{
			val = tempMat.GetColor("_EmissionColor");
		}
		tempMat.shader = hotpoo;
		tempMat.SetColor("_Color", tempMat.GetColor("_Color"));
		tempMat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
		tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
		tempMat.EnableKeyword("DITHER");
		if (num.HasValue)
		{
			tempMat.SetFloat("_NormalStrength", num.Value);
		}
		if (val.HasValue)
		{
			tempMat.SetColor("_EmColor", val.Value);
			tempMat.SetFloat("_EmPower", 1f);
		}
		if (tempMat.IsKeywordEnabled("NOCULL"))
		{
			tempMat.SetInt("_Cull", 0);
		}
		if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
		{
			tempMat.SetInt("_LimbRemovalOn", 1);
		}
		cachedMaterials.Add(tempMat);
		return tempMat;
	}

	public static Material MakeUnique(this Material material)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (cachedMaterials.Contains(material))
		{
			return new Material(material);
		}
		return material;
	}

	public static Material SetColor(this Material material, Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		material.SetColor("_Color", color);
		return material;
	}

	public static Material SetNormal(this Material material, float normalStrength = 1f)
	{
		material.SetFloat("_NormalStrength", normalStrength);
		return material;
	}

	public static Material SetEmission(this Material material)
	{
		return material.SetEmission(1f);
	}

	public static Material SetEmission(this Material material, float emission)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return material.SetEmission(emission, Color.white);
	}

	public static Material SetEmission(this Material material, float emission, Color emissionColor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		material.SetFloat("_EmPower", emission);
		material.SetColor("_EmColor", emissionColor);
		return material;
	}

	public static Material SetCull(this Material material, bool cull = false)
	{
		material.SetInt("_Cull", cull ? 1 : 0);
		return material;
	}
}
