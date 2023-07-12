using UnityEngine;

namespace RoR2;

public abstract class PlatformManager
{
	public PlatformManager()
	{
	}

	public virtual void InitializePlatformManager()
	{
		Debug.Log((object)"Initialize Platform Manager in base class.");
		RoR2Application.onUpdate += UpdatePlatformManager;
	}

	public virtual void StartSinglePlayer()
	{
	}

	protected abstract void UpdatePlatformManager();
}
