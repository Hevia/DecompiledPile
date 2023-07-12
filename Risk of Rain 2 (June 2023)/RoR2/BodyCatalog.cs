using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HG;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using RoR2.Modding;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public static class BodyCatalog
{
	public static ResourceAvailability availability = default(ResourceAvailability);

	private static string[] bodyNames;

	private static GameObject[] bodyPrefabs;

	private static CharacterBody[] bodyPrefabBodyComponents;

	private static Component[][] bodyComponents;

	private static GenericSkill[][] skillSlots;

	private static SkinDef[][] skins;

	private static readonly Dictionary<string, BodyIndex> nameToIndexMap = new Dictionary<string, BodyIndex>();

	public static int bodyCount { get; private set; }

	public static IEnumerable<GameObject> allBodyPrefabs => bodyPrefabs;

	public static IEnumerable<CharacterBody> allBodyPrefabBodyBodyComponents => bodyPrefabBodyComponents;

	[Obsolete("Use IContentPackProvider instead.")]
	public static event Action<List<GameObject>> getAdditionalEntries
	{
		add
		{
			LegacyModContentPackProvider.instance.HandleLegacyGetAdditionalEntries("RoR2.BodyCatalog.getAdditionalEntries", value, LegacyModContentPackProvider.instance.registrationContentPack.bodyPrefabs);
		}
		remove
		{
		}
	}

	public static GameObject GetBodyPrefab(BodyIndex bodyIndex)
	{
		return ArrayUtils.GetSafe<GameObject>(bodyPrefabs, (int)bodyIndex);
	}

	[CanBeNull]
	public static CharacterBody GetBodyPrefabBodyComponent(BodyIndex bodyIndex)
	{
		return ArrayUtils.GetSafe<CharacterBody>(bodyPrefabBodyComponents, (int)bodyIndex);
	}

	public static string GetBodyName(BodyIndex bodyIndex)
	{
		return ArrayUtils.GetSafe<string>(bodyNames, (int)bodyIndex);
	}

	public static BodyIndex FindBodyIndex([NotNull] string bodyName)
	{
		if (nameToIndexMap.TryGetValue(bodyName, out var value))
		{
			return value;
		}
		return BodyIndex.None;
	}

	public static BodyIndex FindBodyIndexCaseInsensitive([NotNull] string bodyName)
	{
		for (BodyIndex bodyIndex = (BodyIndex)0; (int)bodyIndex < bodyPrefabs.Length; bodyIndex++)
		{
			if (string.Compare(((Object)bodyPrefabs[(int)bodyIndex]).name, bodyName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return bodyIndex;
			}
		}
		return BodyIndex.None;
	}

	public static BodyIndex FindBodyIndex(GameObject bodyObject)
	{
		return FindBodyIndex(Object.op_Implicit((Object)(object)bodyObject) ? bodyObject.GetComponent<CharacterBody>() : null);
	}

	public static BodyIndex FindBodyIndex(CharacterBody characterBody)
	{
		return characterBody?.bodyIndex ?? BodyIndex.None;
	}

	[CanBeNull]
	public static GameObject FindBodyPrefab([NotNull] string bodyName)
	{
		BodyIndex bodyIndex = FindBodyIndex(bodyName);
		if (bodyIndex != BodyIndex.None)
		{
			return GetBodyPrefab(bodyIndex);
		}
		return null;
	}

	[CanBeNull]
	public static GameObject FindBodyPrefab(CharacterBody characterBody)
	{
		return GetBodyPrefab(FindBodyIndex(characterBody));
	}

	[CanBeNull]
	public static GameObject FindBodyPrefab(GameObject characterBodyObject)
	{
		return GetBodyPrefab(FindBodyIndex(characterBodyObject));
	}

	[CanBeNull]
	public static GenericSkill[] GetBodyPrefabSkillSlots(BodyIndex bodyIndex)
	{
		return ArrayUtils.GetSafe<GenericSkill[]>(skillSlots, (int)bodyIndex);
	}

	public static SkinDef[] GetBodySkins(BodyIndex bodyIndex)
	{
		SkinDef[][] array = skins;
		SkinDef[] array2 = Array.Empty<SkinDef>();
		return ArrayUtils.GetSafe<SkinDef[]>(array, (int)bodyIndex, ref array2);
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SetBodyPrefabs(ContentManager.bodyPrefabs);
		availability.MakeAvailable();
	}

	private static void SetBodyPrefabs([NotNull] GameObject[] newBodyPrefabs)
	{
		bodyPrefabs = ArrayUtils.Clone<GameObject>(newBodyPrefabs);
		Array.Sort(bodyPrefabs, (GameObject a, GameObject b) => string.CompareOrdinal(((Object)a).name, ((Object)b).name));
		bodyPrefabBodyComponents = new CharacterBody[bodyPrefabs.Length];
		bodyNames = new string[bodyPrefabs.Length];
		bodyComponents = new Component[bodyPrefabs.Length][];
		skillSlots = new GenericSkill[bodyPrefabs.Length][];
		skins = new SkinDef[bodyPrefabs.Length][];
		nameToIndexMap.Clear();
		for (BodyIndex bodyIndex = (BodyIndex)0; (int)bodyIndex < bodyPrefabs.Length; bodyIndex++)
		{
			GameObject val = bodyPrefabs[(int)bodyIndex];
			string name = ((Object)val).name;
			bodyNames[(int)bodyIndex] = name;
			bodyComponents[(int)bodyIndex] = val.GetComponents<Component>();
			skillSlots[(int)bodyIndex] = val.GetComponents<GenericSkill>();
			nameToIndexMap.Add(name, bodyIndex);
			nameToIndexMap.Add(name + "(Clone)", bodyIndex);
			CharacterBody characterBody = (bodyPrefabBodyComponents[(int)bodyIndex] = val.GetComponent<CharacterBody>());
			characterBody.bodyIndex = bodyIndex;
			Texture2D val2 = LegacyResourcesAPI.Load<Texture2D>("Textures/BodyIcons/" + name);
			SkinDef[][] array = skins;
			BodyIndex num = bodyIndex;
			ModelLocator component = val.GetComponent<ModelLocator>();
			object obj;
			if (component == null)
			{
				obj = null;
			}
			else
			{
				Transform modelTransform = component.modelTransform;
				obj = ((modelTransform == null) ? null : ((Component)modelTransform).GetComponent<ModelSkinController>()?.skins);
			}
			if (obj == null)
			{
				obj = Array.Empty<SkinDef>();
			}
			array[(int)num] = (SkinDef[])obj;
			if (Object.op_Implicit((Object)(object)val2))
			{
				characterBody.portraitIcon = (Texture)(object)val2;
			}
			else if ((Object)(object)characterBody.portraitIcon == (Object)null)
			{
				characterBody.portraitIcon = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texMysteryIcon");
			}
			if (string.IsNullOrEmpty(characterBody.baseNameToken))
			{
				characterBody.baseNameToken = "UNIDENTIFIED";
			}
		}
		bodyCount = bodyPrefabs.Length;
	}

	private static IEnumerator GeneratePortraits(bool forceRegeneration)
	{
		Debug.Log((object)"Starting portrait generation.");
		ModelPanel modelPanel = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/IconGenerator")).GetComponentInChildren<ModelPanel>();
		yield return (object)new WaitForEndOfFrame();
		int i = 0;
		while (i < bodyPrefabs.Length)
		{
			CharacterBody characterBody = bodyPrefabBodyComponents[i];
			if (Object.op_Implicit((Object)(object)characterBody) && (forceRegeneration || IconIsNotSuitable(characterBody.portraitIcon)))
			{
				float num = 1f;
				try
				{
					Debug.LogFormat("Generating portrait for {0}", new object[1] { ((Object)bodyPrefabs[i]).name });
					ModelPanel modelPanel2 = modelPanel;
					ModelLocator component = bodyPrefabs[i].GetComponent<ModelLocator>();
					modelPanel2.modelPrefab = ((component != null) ? ((Component)component.modelTransform).gameObject : null);
					modelPanel.SetAnglesForCharacterThumbnail(setZoom: true);
					GameObject modelPrefab = modelPanel.modelPrefab;
					PrintController printController;
					if ((printController = ((modelPrefab != null) ? modelPrefab.GetComponentInChildren<PrintController>() : null)) != null)
					{
						num = Mathf.Max(num, printController.printTime + 1f);
					}
					GameObject modelPrefab2 = modelPanel.modelPrefab;
					TemporaryOverlay temporaryOverlay;
					if ((temporaryOverlay = ((modelPrefab2 != null) ? modelPrefab2.GetComponentInChildren<TemporaryOverlay>() : null)) != null)
					{
						num = Mathf.Max(num, temporaryOverlay.duration + 1f);
					}
				}
				catch (Exception ex)
				{
					Debug.Log((object)ex);
				}
				RoR2Application.onLateUpdate += UpdateCamera;
				yield return (object)new WaitForSeconds(num);
				modelPanel.SetAnglesForCharacterThumbnail(setZoom: true);
				yield return (object)new WaitForEndOfFrame();
				yield return (object)new WaitForEndOfFrame();
				try
				{
					Texture2D val = new Texture2D(((Texture)modelPanel.renderTexture).width, ((Texture)modelPanel.renderTexture).height, (TextureFormat)5, false, false);
					RenderTexture active = RenderTexture.active;
					RenderTexture.active = modelPanel.renderTexture;
					val.ReadPixels(new Rect(0f, 0f, (float)((Texture)modelPanel.renderTexture).width, (float)((Texture)modelPanel.renderTexture).height), 0, 0, false);
					RenderTexture.active = active;
					byte[] array = ImageConversion.EncodeToPNG(val);
					using FileStream fileStream = new FileStream("Assets/RoR2/GeneratedPortraits/" + ((Object)bodyPrefabs[i]).name + ".png", FileMode.Create, FileAccess.Write);
					fileStream.Write(array, 0, array.Length);
				}
				catch (Exception ex2)
				{
					Debug.Log((object)ex2);
				}
				RoR2Application.onLateUpdate -= UpdateCamera;
				yield return (object)new WaitForEndOfFrame();
			}
			int num2 = i + 1;
			i = num2;
		}
		Object.Destroy((Object)(object)((Component)((Component)modelPanel).transform.root).gameObject);
		Debug.Log((object)"Portrait generation complete.");
		static bool IconIsNotSuitable(Texture texture)
		{
			if (!Object.op_Implicit((Object)(object)texture))
			{
				return true;
			}
			string name = ((Object)texture).name;
			if (name == "texMysteryIcon" || name == "texNullIcon")
			{
				return true;
			}
			return false;
		}
		void UpdateCamera()
		{
			modelPanel.SetAnglesForCharacterThumbnail(setZoom: true);
		}
	}

	[ConCommand(commandName = "body_generate_portraits", flags = ConVarFlags.None, helpText = "Generates portraits for all bodies that are currently using the default.")]
	private static void CCBodyGeneratePortraits(ConCommandArgs args)
	{
		((MonoBehaviour)RoR2Application.instance).StartCoroutine(GeneratePortraits(args.TryGetArgBool(0) ?? false));
	}

	[ConCommand(commandName = "body_list", flags = ConVarFlags.None, helpText = "Prints a list of all character bodies in the game.")]
	private static void CCBodyList(ConCommandArgs args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < bodyComponents.Length; i++)
		{
			stringBuilder.Append("[").Append(i).Append("]=")
				.Append(bodyNames[i])
				.AppendLine();
		}
		Debug.Log((object)stringBuilder);
	}

	[ConCommand(commandName = "body_reload_all", flags = ConVarFlags.Cheat, helpText = "Reloads all bodies and repopulates the BodyCatalog.")]
	private static void CCBodyReloadAll(ConCommandArgs args)
	{
		SetBodyPrefabs(Resources.LoadAll<GameObject>("Prefabs/CharacterBodies/"));
	}
}
