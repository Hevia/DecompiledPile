using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HG;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using RoR2.Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoR2;

public static class SceneCatalog
{
	private static SceneDef[] indexToSceneDef = Array.Empty<SceneDef>();

	private static SceneDef[] _stageSceneDefs = Array.Empty<SceneDef>();

	private static string[] _baseSceneNames = Array.Empty<string>();

	private static readonly Dictionary<string, SceneIndex> nameToIndex = new Dictionary<string, SceneIndex>(StringComparer.OrdinalIgnoreCase);

	private static SceneDef currentSceneDef;

	public static ResourceAvailability availability;

	public static int sceneDefCount => indexToSceneDef.Length;

	public static ReadOnlyArray<SceneDef> allSceneDefs => ReadOnlyArray<SceneDef>.op_Implicit(indexToSceneDef);

	public static ReadOnlyArray<SceneDef> allStageSceneDefs => ReadOnlyArray<SceneDef>.op_Implicit(_stageSceneDefs);

	public static ReadOnlyArray<string> allBaseSceneNames => ReadOnlyArray<string>.op_Implicit(_baseSceneNames);

	[NotNull]
	public static SceneDef mostRecentSceneDef { get; private set; }

	public static event Action<SceneDef> onMostRecentSceneDefChanged;

	[Obsolete("Use IContentPackProvider instead.")]
	public static event Action<List<SceneDef>> getAdditionalEntries
	{
		add
		{
			LegacyModContentPackProvider.instance.HandleLegacyGetAdditionalEntries("RoR2.SceneCatalog.getAdditionalEntries", value, LegacyModContentPackProvider.instance.registrationContentPack.sceneDefs);
		}
		remove
		{
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		SetSceneDefs(ContentManager.sceneDefs);
		availability.MakeAvailable();
	}

	private static void SetSceneDefs([NotNull] SceneDef[] newSceneDefs)
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		SceneDef[] array = indexToSceneDef;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sceneDefIndex = SceneIndex.Invalid;
		}
		ArrayUtils.CloneTo<SceneDef>(newSceneDefs, ref indexToSceneDef);
		Array.Sort(indexToSceneDef, (SceneDef a, SceneDef b) => string.CompareOrdinal(a.cachedName, b.cachedName));
		for (int j = 0; j < indexToSceneDef.Length; j++)
		{
			indexToSceneDef[j].sceneDefIndex = (SceneIndex)j;
		}
		nameToIndex.Clear();
		for (int k = 0; k < indexToSceneDef.Length; k++)
		{
			SceneDef sceneDef2 = indexToSceneDef[k];
			nameToIndex[sceneDef2.cachedName] = (SceneIndex)k;
		}
		_stageSceneDefs = indexToSceneDef.Where((SceneDef sceneDef) => sceneDef.sceneType == SceneType.Stage).ToArray();
		_baseSceneNames = indexToSceneDef.Select((SceneDef sceneDef) => sceneDef.baseSceneName).Distinct().ToArray();
		Scene activeScene = SceneManager.GetActiveScene();
		currentSceneDef = GetSceneDefFromSceneName(((Scene)(ref activeScene)).name);
		mostRecentSceneDef = currentSceneDef;
	}

	private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
	{
		currentSceneDef = GetSceneDefFromSceneName(((Scene)(ref newScene)).name);
		if (currentSceneDef != null)
		{
			mostRecentSceneDef = currentSceneDef;
			SceneCatalog.onMostRecentSceneDefChanged?.Invoke(mostRecentSceneDef);
		}
	}

	[NotNull]
	public static UnlockableDef GetUnlockableLogFromBaseSceneName([NotNull] string baseSceneName)
	{
		return UnlockableCatalog.GetUnlockableDef(string.Format(CultureInfo.InvariantCulture, "Logs.Stages.{0}", baseSceneName));
	}

	[CanBeNull]
	public static SceneDef GetSceneDefForCurrentScene()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetSceneDefFromScene(SceneManager.GetActiveScene());
	}

	[CanBeNull]
	public static SceneDef GetSceneDef(SceneIndex sceneIndex)
	{
		return ArrayUtils.GetSafe<SceneDef>(indexToSceneDef, (int)sceneIndex);
	}

	[CanBeNull]
	public static SceneDef GetSceneDefFromSceneName([NotNull] string sceneName)
	{
		SceneDef sceneDef = FindSceneDef(sceneName);
		if (sceneDef != null)
		{
			return sceneDef;
		}
		Debug.LogWarningFormat("Could not find scene with name \"{0}\".", new object[1] { sceneName });
		return null;
	}

	[CanBeNull]
	public static SceneDef GetSceneDefFromScene(Scene scene)
	{
		return GetSceneDefFromSceneName(((Scene)(ref scene)).name);
	}

	public static SceneIndex FindSceneIndex([NotNull] string sceneName)
	{
		if (nameToIndex.TryGetValue(sceneName, out var value))
		{
			return value;
		}
		return SceneIndex.Invalid;
	}

	public static SceneDef FindSceneDef([NotNull] string sceneName)
	{
		return ArrayUtils.GetSafe<SceneDef>(indexToSceneDef, (int)FindSceneIndex(sceneName));
	}
}
