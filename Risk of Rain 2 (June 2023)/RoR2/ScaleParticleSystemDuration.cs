using UnityEngine;

namespace RoR2;

public class ScaleParticleSystemDuration : MonoBehaviour
{
	public float initialDuration = 1f;

	private float _newDuration = 1f;

	public ParticleSystem[] particleSystems;

	public float newDuration
	{
		get
		{
			return _newDuration;
		}
		set
		{
			if (_newDuration != value)
			{
				_newDuration = value;
				UpdateParticleDurations();
			}
		}
	}

	private void Start()
	{
		UpdateParticleDurations();
	}

	private void UpdateParticleDurations()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		float simulationSpeed = initialDuration / _newDuration;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem val = particleSystems[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				MainModule main = val.main;
				((MainModule)(ref main)).simulationSpeed = simulationSpeed;
			}
		}
	}
}
