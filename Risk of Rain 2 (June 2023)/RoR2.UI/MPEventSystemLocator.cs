using System;
using UnityEngine;

namespace RoR2.UI;

public class MPEventSystemLocator : MonoBehaviour
{
	private MPEventSystemProvider _eventSystemProvider;

	private MPEventSystem _eventSystem;

	public MPEventSystemProvider eventSystemProvider
	{
		get
		{
			return _eventSystemProvider;
		}
		internal set
		{
			if (_eventSystemProvider != value)
			{
				_eventSystemProvider?.RemoveListener(this);
				_eventSystemProvider = value;
				_eventSystemProvider?.AddListener(this);
			}
		}
	}

	public MPEventSystem eventSystem
	{
		get
		{
			return _eventSystem;
		}
		internal set
		{
			if (_eventSystem != value)
			{
				if (_eventSystem != null)
				{
					OnEventSystemLost(_eventSystem);
				}
				_eventSystem = value;
				if (_eventSystem != null)
				{
					OnEventSystemDiscovered(_eventSystem);
				}
			}
		}
	}

	public event Action<MPEventSystem> onEventSystemDiscovered;

	public event Action<MPEventSystem> onEventSystemLost;

	private void Awake()
	{
		eventSystemProvider = ((Component)this).GetComponentInParent<MPEventSystemProvider>();
	}

	private void OnDestroy()
	{
		eventSystemProvider = null;
	}

	private void OnEventSystemDiscovered(MPEventSystem discoveredEventSystem)
	{
		this.onEventSystemDiscovered?.Invoke(discoveredEventSystem);
	}

	private void OnEventSystemLost(MPEventSystem lostEventSystem)
	{
		this.onEventSystemLost?.Invoke(lostEventSystem);
	}

	internal void OnProviderDestroyed(MPEventSystemProvider destroyedEventSystemProvider)
	{
		if (destroyedEventSystemProvider == eventSystemProvider)
		{
			eventSystemProvider = null;
		}
	}
}
