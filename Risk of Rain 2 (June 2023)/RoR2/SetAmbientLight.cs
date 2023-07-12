using UnityEngine;
using UnityEngine.Rendering;

namespace RoR2;

[ExecuteAlways]
public class SetAmbientLight : MonoBehaviour
{
	public bool setSkyboxMaterial;

	public bool setAmbientLightColor;

	public Material skyboxMaterial;

	public AmbientMode ambientMode;

	public float ambientIntensity;

	[ColorUsage(true, true)]
	public Color ambientSkyColor = Color.black;

	[ColorUsage(true, true)]
	public Color ambientEquatorColor = Color.black;

	[ColorUsage(true, true)]
	public Color ambientGroundColor = Color.black;

	private void OnEnable()
	{
	}

	private void ApplyLighting()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (setAmbientLightColor)
		{
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientSkyColor = ambientSkyColor * ambientIntensity;
			RenderSettings.ambientEquatorColor = ambientEquatorColor * ambientIntensity;
			RenderSettings.ambientGroundColor = ambientGroundColor * ambientIntensity;
		}
		if (setSkyboxMaterial)
		{
			RenderSettings.skybox = skyboxMaterial;
		}
	}
}
