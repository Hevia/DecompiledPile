using UnityEngine;

namespace RoR2;

public class ReplaceLightColorsInChildren : MonoBehaviour
{
	public Color newLightColor;

	public float intensityMultiplier;

	public Material newParticleSystemMaterial;

	private void Awake()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Light[] componentsInChildren = ((Component)this).GetComponentsInChildren<Light>();
		foreach (Light obj in componentsInChildren)
		{
			obj.color = newLightColor;
			obj.intensity *= intensityMultiplier;
		}
		if (!Object.op_Implicit((Object)(object)newParticleSystemMaterial))
		{
			return;
		}
		ParticleSystem[] componentsInChildren2 = ((Component)this).GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			ParticleSystemRenderer component = ((Component)componentsInChildren2[i]).GetComponent<ParticleSystemRenderer>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Renderer)component).material = newParticleSystemMaterial;
			}
		}
	}

	private void Update()
	{
	}
}
