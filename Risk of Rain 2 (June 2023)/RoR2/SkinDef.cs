using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SkinDef")]
public class SkinDef : ScriptableObject
{
	[Serializable]
	public struct GameObjectActivation
	{
		[PrefabReference]
		public GameObject gameObject;

		public bool shouldActivate;
	}

	[Serializable]
	public struct MeshReplacement
	{
		[PrefabReference]
		public Renderer renderer;

		public Mesh mesh;
	}

	[Serializable]
	public struct ProjectileGhostReplacement
	{
		public GameObject projectilePrefab;

		public GameObject projectileGhostReplacementPrefab;
	}

	[Serializable]
	public struct MinionSkinReplacement
	{
		public GameObject minionBodyPrefab;

		public SkinDef minionSkin;
	}

	private struct RendererInfoTemplate
	{
		public string path;

		public CharacterModel.RendererInfo data;
	}

	private struct GameObjectActivationTemplate
	{
		public string path;

		public bool shouldActivate;
	}

	private struct MeshReplacementTemplate
	{
		public string path;

		public Mesh mesh;
	}

	private class RuntimeSkin
	{
		public RendererInfoTemplate[] rendererInfoTemplates;

		public GameObjectActivationTemplate[] gameObjectActivationTemplates;

		public MeshReplacementTemplate[] meshReplacementTemplates;

		private static readonly List<CharacterModel.RendererInfo> rendererInfoBuffer = new List<CharacterModel.RendererInfo>();

		public void Apply(GameObject modelObject)
		{
			Transform transform = modelObject.transform;
			for (int i = 0; i < rendererInfoTemplates.Length; i++)
			{
				ref RendererInfoTemplate reference = ref rendererInfoTemplates[i];
				CharacterModel.RendererInfo data = reference.data;
				Transform val = transform.Find(reference.path);
				if (Object.op_Implicit((Object)(object)val))
				{
					Renderer component = ((Component)val).GetComponent<Renderer>();
					if (Object.op_Implicit((Object)(object)component))
					{
						data.renderer = component;
						rendererInfoBuffer.Add(data);
					}
					else
					{
						Debug.LogWarningFormat("No renderer at {0}/{1}", new object[2]
						{
							((Object)transform).name,
							reference.path
						});
					}
				}
				else
				{
					Debug.LogWarningFormat("Could not find transform \"{0}\" relative to \"{1}\".", new object[2]
					{
						reference.path,
						((Object)transform).name
					});
				}
			}
			modelObject.GetComponent<CharacterModel>().baseRendererInfos = rendererInfoBuffer.ToArray();
			rendererInfoBuffer.Clear();
			for (int j = 0; j < gameObjectActivationTemplates.Length; j++)
			{
				ref GameObjectActivationTemplate reference2 = ref gameObjectActivationTemplates[j];
				bool shouldActivate = reference2.shouldActivate;
				((Component)transform.Find(reference2.path)).gameObject.SetActive(shouldActivate);
			}
			for (int k = 0; k < meshReplacementTemplates.Length; k++)
			{
				ref MeshReplacementTemplate reference3 = ref meshReplacementTemplates[k];
				Mesh mesh = reference3.mesh;
				Renderer component2 = ((Component)transform.Find(reference3.path)).GetComponent<Renderer>();
				SkinnedMeshRenderer val2;
				if (component2 is MeshRenderer)
				{
					((Component)component2).GetComponent<MeshFilter>().sharedMesh = mesh;
				}
				else if ((val2 = (SkinnedMeshRenderer)(object)((component2 is SkinnedMeshRenderer) ? component2 : null)) != null)
				{
					val2.sharedMesh = mesh;
				}
			}
		}
	}

	[Tooltip("The skins which will be applied before this one.")]
	public SkinDef[] baseSkins;

	[ShowThumbnail]
	public Sprite icon;

	public string nameToken;

	[Obsolete("Use 'unlockableDef' instead.")]
	public string unlockableName;

	public UnlockableDef unlockableDef;

	[PrefabReference]
	public GameObject rootObject;

	public CharacterModel.RendererInfo[] rendererInfos = Array.Empty<CharacterModel.RendererInfo>();

	public GameObjectActivation[] gameObjectActivations = Array.Empty<GameObjectActivation>();

	public MeshReplacement[] meshReplacements = Array.Empty<MeshReplacement>();

	public ProjectileGhostReplacement[] projectileGhostReplacements = Array.Empty<ProjectileGhostReplacement>();

	public MinionSkinReplacement[] minionSkinReplacements = Array.Empty<MinionSkinReplacement>();

	private RuntimeSkin runtimeSkin;

	public SkinIndex skinIndex { get; set; } = SkinIndex.None;


	private void Bake()
	{
		if (runtimeSkin != null)
		{
			return;
		}
		runtimeSkin = new RuntimeSkin();
		List<RendererInfoTemplate> rendererInfoTemplates = new List<RendererInfoTemplate>();
		List<GameObjectActivationTemplate> gameObjectActivationTemplates = new List<GameObjectActivationTemplate>();
		List<MeshReplacementTemplate> meshReplacementTemplates = new List<MeshReplacementTemplate>();
		SkinDef[] array = baseSkins;
		foreach (SkinDef skinDef in array)
		{
			if (!Object.op_Implicit((Object)(object)skinDef))
			{
				continue;
			}
			skinDef.Bake();
			if (skinDef.runtimeSkin == null)
			{
				continue;
			}
			if (skinDef.runtimeSkin.rendererInfoTemplates != null)
			{
				RendererInfoTemplate[] rendererInfoTemplates2 = skinDef.runtimeSkin.rendererInfoTemplates;
				for (int j = 0; j < rendererInfoTemplates2.Length; j++)
				{
					AddRendererInfoTemplate(rendererInfoTemplates2[j]);
				}
			}
			if (skinDef.runtimeSkin.gameObjectActivationTemplates != null)
			{
				GameObjectActivationTemplate[] gameObjectActivationTemplates2 = skinDef.runtimeSkin.gameObjectActivationTemplates;
				for (int j = 0; j < gameObjectActivationTemplates2.Length; j++)
				{
					AddGameObjectActivationTemplate(gameObjectActivationTemplates2[j]);
				}
			}
			if (skinDef.runtimeSkin.meshReplacementTemplates != null)
			{
				MeshReplacementTemplate[] meshReplacementTemplates2 = skinDef.runtimeSkin.meshReplacementTemplates;
				for (int j = 0; j < meshReplacementTemplates2.Length; j++)
				{
					AddMeshReplacementTemplate(meshReplacementTemplates2[j]);
				}
			}
		}
		for (int k = 0; k < rendererInfos.Length; k++)
		{
			ref CharacterModel.RendererInfo reference = ref rendererInfos[k];
			if (!Object.op_Implicit((Object)(object)reference.renderer))
			{
				Debug.LogErrorFormat("Skin {0} has an empty renderer field in its rendererInfos.", new object[1] { this });
				continue;
			}
			RendererInfoTemplate rendererInfoTemplate2 = default(RendererInfoTemplate);
			rendererInfoTemplate2.data = reference;
			rendererInfoTemplate2.path = Util.BuildPrefabTransformPath(rootObject.transform, ((Component)reference.renderer).transform);
			AddRendererInfoTemplate(rendererInfoTemplate2);
		}
		runtimeSkin.rendererInfoTemplates = rendererInfoTemplates.ToArray();
		for (int l = 0; l < gameObjectActivations.Length; l++)
		{
			ref GameObjectActivation reference2 = ref gameObjectActivations[l];
			if (!Object.op_Implicit((Object)(object)reference2.gameObject))
			{
				Debug.LogErrorFormat("Skin {0} has an empty gameObject field in its gameObjectActivations.", new object[1] { this });
				continue;
			}
			GameObjectActivationTemplate gameObjectActivationTemplate2 = default(GameObjectActivationTemplate);
			gameObjectActivationTemplate2.shouldActivate = reference2.shouldActivate;
			gameObjectActivationTemplate2.path = Util.BuildPrefabTransformPath(rootObject.transform, reference2.gameObject.transform);
			AddGameObjectActivationTemplate(gameObjectActivationTemplate2);
		}
		runtimeSkin.gameObjectActivationTemplates = gameObjectActivationTemplates.ToArray();
		for (int m = 0; m < meshReplacements.Length; m++)
		{
			ref MeshReplacement reference3 = ref meshReplacements[m];
			if (!Object.op_Implicit((Object)(object)reference3.renderer))
			{
				Debug.LogErrorFormat("Skin {0} has an empty renderer field in its meshReplacements.", new object[1] { this });
				continue;
			}
			MeshReplacementTemplate meshReplacementTemplate2 = default(MeshReplacementTemplate);
			meshReplacementTemplate2.mesh = reference3.mesh;
			meshReplacementTemplate2.path = Util.BuildPrefabTransformPath(rootObject.transform, ((Component)reference3.renderer).transform);
			AddMeshReplacementTemplate(meshReplacementTemplate2);
		}
		runtimeSkin.meshReplacementTemplates = meshReplacementTemplates.ToArray();
		void AddGameObjectActivationTemplate(GameObjectActivationTemplate gameObjectActivationTemplate)
		{
			int num = 0;
			for (int count2 = gameObjectActivationTemplates.Count; num < count2; num++)
			{
				if (gameObjectActivationTemplates[num].path == gameObjectActivationTemplate.path)
				{
					gameObjectActivationTemplates[num] = gameObjectActivationTemplate;
					return;
				}
			}
			gameObjectActivationTemplates.Add(gameObjectActivationTemplate);
		}
		void AddMeshReplacementTemplate(MeshReplacementTemplate meshReplacementTemplate)
		{
			int n = 0;
			for (int count = meshReplacementTemplates.Count; n < count; n++)
			{
				if (meshReplacementTemplates[n].path == meshReplacementTemplate.path)
				{
					meshReplacementTemplates[n] = meshReplacementTemplate;
					return;
				}
			}
			meshReplacementTemplates.Add(meshReplacementTemplate);
		}
		void AddRendererInfoTemplate(RendererInfoTemplate rendererInfoTemplate)
		{
			int num2 = 0;
			for (int count3 = rendererInfoTemplates.Count; num2 < count3; num2++)
			{
				if (rendererInfoTemplates[num2].path == rendererInfoTemplate.path)
				{
					rendererInfoTemplates[num2] = rendererInfoTemplate;
					return;
				}
			}
			rendererInfoTemplates.Add(rendererInfoTemplate);
		}
	}

	public void Apply(GameObject modelObject)
	{
		Bake();
		runtimeSkin.Apply(modelObject);
	}

	private void Awake()
	{
		if (Application.IsPlaying((Object)(object)this))
		{
			Bake();
		}
	}

	[ContextMenu("Upgrade unlockableName to unlockableDef")]
	public void UpgradeUnlockableNameToUnlockableDef()
	{
		if (!string.IsNullOrEmpty(unlockableName) && !Object.op_Implicit((Object)(object)this.unlockableDef))
		{
			UnlockableDef unlockableDef = LegacyResourcesAPI.Load<UnlockableDef>("UnlockableDefs/" + unlockableName);
			if (Object.op_Implicit((Object)(object)unlockableDef))
			{
				this.unlockableDef = unlockableDef;
				unlockableName = null;
			}
		}
		EditorUtil.SetDirty((Object)(object)this);
	}
}
