using System;
using System.Collections.Generic;
using On.RoR2;
using RoR2;
using UnityEngine;

namespace VileMod.Modules;

internal static class Skins
{
	internal struct SkinDefInfo
	{
		internal SkinDef[] BaseSkins;

		internal Sprite Icon;

		internal string NameToken;

		internal UnlockableDef UnlockableDef;

		internal GameObject RootObject;

		internal RendererInfo[] RendererInfos;

		internal MeshReplacement[] MeshReplacements;

		internal GameObjectActivation[] GameObjectActivations;

		internal ProjectileGhostReplacement[] ProjectileGhostReplacements;

		internal MinionSkinReplacement[] MinionSkinReplacements;

		internal string Name;
	}

	internal static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, RendererInfo[] defaultRendererInfos, GameObject root, UnlockableDef unlockableDef = null)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		SkinDefInfo skinDefInfo = default(SkinDefInfo);
		skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
		skinDefInfo.GameObjectActivations = (GameObjectActivation[])(object)new GameObjectActivation[0];
		skinDefInfo.Icon = skinIcon;
		skinDefInfo.MeshReplacements = (MeshReplacement[])(object)new MeshReplacement[0];
		skinDefInfo.MinionSkinReplacements = (MinionSkinReplacement[])(object)new MinionSkinReplacement[0];
		skinDefInfo.Name = skinName;
		skinDefInfo.NameToken = skinName;
		skinDefInfo.ProjectileGhostReplacements = (ProjectileGhostReplacement[])(object)new ProjectileGhostReplacement[0];
		skinDefInfo.RendererInfos = (RendererInfo[])(object)new RendererInfo[defaultRendererInfos.Length];
		skinDefInfo.RootObject = root;
		skinDefInfo.UnlockableDef = unlockableDef;
		SkinDefInfo skinDefInfo2 = skinDefInfo;
		SkinDef.Awake += new hook_Awake(DoNothing);
		SkinDef val = ScriptableObject.CreateInstance<SkinDef>();
		val.baseSkins = skinDefInfo2.BaseSkins;
		val.icon = skinDefInfo2.Icon;
		val.unlockableDef = skinDefInfo2.UnlockableDef;
		val.rootObject = skinDefInfo2.RootObject;
		defaultRendererInfos.CopyTo(skinDefInfo2.RendererInfos, 0);
		val.rendererInfos = skinDefInfo2.RendererInfos;
		val.gameObjectActivations = skinDefInfo2.GameObjectActivations;
		val.meshReplacements = skinDefInfo2.MeshReplacements;
		val.projectileGhostReplacements = skinDefInfo2.ProjectileGhostReplacements;
		val.minionSkinReplacements = skinDefInfo2.MinionSkinReplacements;
		val.nameToken = skinDefInfo2.NameToken;
		((Object)val).name = skinDefInfo2.Name;
		SkinDef.Awake -= new hook_Awake(DoNothing);
		return val;
	}

	private static void DoNothing(orig_Awake orig, SkinDef self)
	{
	}

	private static RendererInfo[] getRendererMaterials(RendererInfo[] defaultRenderers, params Material[] materials)
	{
		RendererInfo[] array = (RendererInfo[])(object)new RendererInfo[defaultRenderers.Length];
		defaultRenderers.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				array[i].defaultMaterial = materials[i];
			}
			catch
			{
				Log.Error("error adding skin rendererinfo material. make sure you're not passing in too many");
			}
		}
		return array;
	}

	internal static MeshReplacement[] getMeshReplacements(RendererInfo[] defaultRendererInfos, params string[] meshes)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		List<MeshReplacement> list = new List<MeshReplacement>();
		for (int i = 0; i < defaultRendererInfos.Length; i++)
		{
			if (!string.IsNullOrEmpty(meshes[i]))
			{
				list.Add(new MeshReplacement
				{
					renderer = defaultRendererInfos[i].renderer,
					mesh = Assets.mainAssetBundle.LoadAsset<Mesh>(meshes[i])
				});
			}
		}
		return list.ToArray();
	}
}
