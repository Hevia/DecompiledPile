using System;
using EntityStates;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/GameEndingDef")]
public class GameEndingDef : ScriptableObject
{
	public string endingTextToken;

	public Color backgroundColor;

	public Color foregroundColor;

	public Sprite icon;

	public Material material;

	[Tooltip("The body prefab to use as the killer when this ending is triggered while players are still alive.")]
	public GameObject defaultKillerOverride;

	public bool isWin;

	public bool showCredits;

	public SerializableEntityStateType gameOverControllerState;

	public uint lunarCoinReward;

	private string _cachedName;

	public GameEndingIndex gameEndingIndex { get; set; }

	[Obsolete(".name should not be used. Use .cachedName instead. If retrieving the value from the engine is absolutely necessary, cast to ScriptableObject first.", true)]
	public string name
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string cachedName
	{
		get
		{
			return _cachedName;
		}
		set
		{
			((Object)this).name = value;
			_cachedName = value;
		}
	}

	private void Awake()
	{
		_cachedName = ((Object)this).name;
	}

	private void OnValidate()
	{
		_cachedName = ((Object)this).name;
	}
}
