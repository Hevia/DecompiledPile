using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2.UI;

public class MPEventSystemProvider : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("eventSystem")]
	private MPEventSystem _eventSystem;

	public bool fallBackToMainEventSystem = true;

	private MPEventSystem _resolvedEventSystem;

	private List<MPEventSystemLocator> listeners = new List<MPEventSystemLocator>();

	public MPEventSystem eventSystem
	{
		get
		{
			return _eventSystem;
		}
		set
		{
			if (_eventSystem != value)
			{
				_eventSystem = value;
				ResolveEventSystem();
			}
		}
	}

	public MPEventSystem resolvedEventSystem
	{
		get
		{
			return _resolvedEventSystem;
		}
		private set
		{
			if (_resolvedEventSystem != value)
			{
				_resolvedEventSystem = value;
				for (int num = listeners.Count - 1; num >= 0; num--)
				{
					listeners[num].eventSystem = _resolvedEventSystem;
				}
			}
		}
	}

	private void Awake()
	{
		ResolveEventSystem();
	}

	private void OnEnable()
	{
		ResolveEventSystem();
	}

	private void OnDisable()
	{
		resolvedEventSystem = null;
	}

	private void OnDestroy()
	{
		for (int num = listeners.Count - 1; num >= 0; num--)
		{
			listeners[num].OnProviderDestroyed(this);
		}
	}

	private void Update()
	{
		ResolveEventSystem();
	}

	private void ResolveEventSystem()
	{
		if (Object.op_Implicit((Object)(object)eventSystem))
		{
			resolvedEventSystem = eventSystem;
		}
		else if (fallBackToMainEventSystem)
		{
			resolvedEventSystem = MPEventSystemManager.primaryEventSystem;
		}
	}

	internal void AddListener(MPEventSystemLocator listener)
	{
		listeners.Add(listener);
		listener.eventSystem = resolvedEventSystem;
	}

	internal void RemoveListener(MPEventSystemLocator listener)
	{
		listener.eventSystem = null;
		listeners.Remove(listener);
	}
}
