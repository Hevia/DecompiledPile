using UnityEngine;

namespace RoR2;

public class RemapLightIntensityToParticleAlpha : MonoBehaviour
{
	public Light light;

	public ParticleSystem particleSystem;

	public float lowerLightIntensity;

	public float upperLightIntensity = 1f;

	public float lowerParticleAlpha;

	public float upperParticleAlpha = 1f;

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = particleSystem.main;
		MinMaxGradient startColor = ((MainModule)(ref main)).startColor;
		Color color = ((MinMaxGradient)(ref startColor)).color;
		color.a = Util.Remap(light.intensity, lowerLightIntensity, upperLightIntensity, lowerParticleAlpha, upperParticleAlpha);
		((MinMaxGradient)(ref startColor)).color = color;
		((MainModule)(ref main)).startColor = startColor;
	}
}
