using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class ArtifactEnabledResponse : MonoBehaviour
{
	public ArtifactDef artifact;

	public UnityEvent onDiscoveredArtifactEnabled;

	public UnityEvent onLostArtifactEnabled;

	private bool _active;

	private bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active == value)
			{
				return;
			}
			_active = value;
			if (active)
			{
				UnityEvent obj = onDiscoveredArtifactEnabled;
				if (obj != null)
				{
					obj.Invoke();
				}
			}
			else
			{
				UnityEvent obj2 = onLostArtifactEnabled;
				if (obj2 != null)
				{
					obj2.Invoke();
				}
			}
		}
	}

	private void OnEnable()
	{
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabledGlobal;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabledGlobal;
		active = RunArtifactManager.instance.IsArtifactEnabled(artifact);
	}

	private void OnDisable()
	{
		active = false;
		RunArtifactManager.onArtifactDisabledGlobal -= OnArtifactDisabledGlobal;
		RunArtifactManager.onArtifactEnabledGlobal -= OnArtifactEnabledGlobal;
	}

	private void OnArtifactEnabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)artifact))
		{
			active = true;
		}
	}

	private void OnArtifactDisabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)artifact))
		{
			active = false;
		}
	}
}
