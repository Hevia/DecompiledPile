using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

public class AssetCheckArgs
{
	[NotNull]
	public ProjectIssueChecker projectIssueChecker;

	[NotNull]
	public Object asset;

	[CanBeNull]
	public Component assetComponent
	{
		get
		{
			Object obj = asset;
			return (Component)(object)((obj is Component) ? obj : null);
		}
	}

	[CanBeNull]
	public GameObject gameObject
	{
		get
		{
			Component obj = assetComponent;
			if (obj == null)
			{
				return null;
			}
			return obj.gameObject;
		}
	}

	[CanBeNull]
	public GameObject gameObjectRoot
	{
		get
		{
			GameObject obj = gameObject;
			if (obj == null)
			{
				return null;
			}
			Transform root = obj.transform.root;
			if (root == null)
			{
				return null;
			}
			return ((Component)root).gameObject;
		}
	}

	public bool isPrefab => GameObjectIsPrefab(gameObjectRoot);

	public bool isPrefabVariant => GameObjectIsPrefabVariant(gameObjectRoot);

	[CanBeNull]
	public GameObject prefabRoot
	{
		get
		{
			GameObject result = gameObjectRoot;
			if (GameObjectIsPrefab(result))
			{
				return result;
			}
			return null;
		}
	}

	private static bool GameObjectIsPrefab(GameObject gameObject)
	{
		return false;
	}

	private static bool GameObjectIsPrefabVariant(GameObject gameObject)
	{
		return false;
	}

	public void UpdatePrefab()
	{
		Object.op_Implicit((Object)(object)prefabRoot);
	}

	public void Log(string str, Object context = null)
	{
		projectIssueChecker.Log(str, context);
	}

	public void LogError(string str, Object context = null)
	{
		projectIssueChecker.LogError(str, context);
	}

	public void LogFormat(Object context, string format, params object[] formatArgs)
	{
		projectIssueChecker.LogFormat(context, format, formatArgs);
	}

	public void LogErrorFormat(Object context, string format, params object[] formatArgs)
	{
		projectIssueChecker.LogErrorFormat(context, format, formatArgs);
	}

	public void EnsurePath(string path)
	{
		Directory.Exists(path);
	}

	public T LoadAsset<T>(string fullFilePath) where T : Object
	{
		File.Exists(fullFilePath);
		return default(T);
	}

	public static void CreateAsset<T>(string path)
	{
	}
}
