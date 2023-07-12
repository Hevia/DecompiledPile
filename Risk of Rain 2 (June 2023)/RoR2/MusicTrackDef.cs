using System;
using AK.Wwise;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/MusicTrackDef")]
public class MusicTrackDef : ScriptableObject
{
	public Bank soundBank;

	public State[] states;

	[Multiline]
	public string comment;

	private string _cachedName;

	public MusicTrackIndex catalogIndex { get; set; }

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

	public virtual void Preload()
	{
		Bank obj = soundBank;
		if (obj != null)
		{
			obj.Load(false, false);
		}
	}

	public virtual void Play()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Preload();
		State[] array = states;
		foreach (State val in array)
		{
			AkSoundEngine.SetState(((BaseGroupType)val).GroupId, ((BaseType)val).Id);
		}
	}

	public virtual void Stop()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		State[] array = states;
		for (int i = 0; i < array.Length; i++)
		{
			AkSoundEngine.SetState(((BaseGroupType)array[i]).GroupId, 0u);
		}
	}
}
