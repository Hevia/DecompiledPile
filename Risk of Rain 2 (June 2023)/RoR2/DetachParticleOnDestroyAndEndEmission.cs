using UnityEngine;

namespace RoR2;

public class DetachParticleOnDestroyAndEndEmission : MonoBehaviour
{
	public ParticleSystem particleSystem;

	private void OnDisable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)particleSystem))
		{
			EmissionModule emission = particleSystem.emission;
			((EmissionModule)(ref emission)).enabled = false;
			MainModule main = particleSystem.main;
			((MainModule)(ref main)).stopAction = (ParticleSystemStopAction)2;
			particleSystem.Stop();
			((Component)particleSystem).transform.SetParent((Transform)null);
		}
	}
}
