using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SceneCollection")]
public class SceneCollection : ScriptableObject
{
	[Serializable]
	public struct SceneEntry
	{
		public SceneDef sceneDef;

		[SerializeField]
		private float weightMinusOne;

		public float weight
		{
			get
			{
				return weightMinusOne + 1f;
			}
			set
			{
				weightMinusOne = value - 1f;
			}
		}
	}

	[FormerlySerializedAs("sceneReferences")]
	[SerializeField]
	private SceneEntry[] _sceneEntries = Array.Empty<SceneEntry>();

	public ReadOnlyArray<SceneEntry> sceneEntries => ReadOnlyArray<SceneEntry>.op_Implicit(_sceneEntries);

	public bool isEmpty => _sceneEntries.Length == 0;

	public void SetSceneEntries(IReadOnlyList<SceneEntry> newSceneReferences)
	{
		Array.Resize(ref _sceneEntries, newSceneReferences.Count);
		for (int i = 0; i < 0; i++)
		{
			_sceneEntries[i] = newSceneReferences[i];
		}
	}

	public void AddToWeightedSelection(WeightedSelection<SceneDef> dest, Func<SceneDef, bool> canAdd = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<SceneEntry> enumerator = sceneEntries.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SceneEntry current = enumerator.Current;
				SceneDef sceneDef = current.sceneDef;
				if (Object.op_Implicit((Object)(object)sceneDef) && (canAdd == null || canAdd(sceneDef)))
				{
					dest.AddChoice(sceneDef, current.weight);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
	}
}
