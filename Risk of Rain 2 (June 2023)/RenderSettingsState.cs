using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct RenderSettingsState
{
	public float haloStrength;

	public int defaultReflectionResolution;

	public DefaultReflectionMode defaultReflectionMode;

	public int reflectionBounces;

	public float reflectionIntensity;

	public Cubemap customReflection;

	public SphericalHarmonicsL2 ambientProbe;

	public Light sun;

	public Material skybox;

	public Color subtractiveShadowColor;

	public float flareStrength;

	[ColorUsage(false, true)]
	public Color ambientLight;

	[ColorUsage(false, true)]
	public Color ambientGroundColor;

	[ColorUsage(false, true)]
	public Color ambientEquatorColor;

	[ColorUsage(false, true)]
	public Color ambientSkyColor;

	public AmbientMode ambientMode;

	public float fogDensity;

	[ColorUsage(true, false)]
	public Color fogColor;

	public FogMode fogMode;

	public float fogEndDistance;

	public float fogStartDistance;

	public bool fog;

	public float ambientIntensity;

	public float flareFadeSpeed;

	public static RenderSettingsState FromCurrent()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		RenderSettingsState result = default(RenderSettingsState);
		result.haloStrength = RenderSettings.haloStrength;
		result.defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
		result.defaultReflectionMode = RenderSettings.defaultReflectionMode;
		result.reflectionBounces = RenderSettings.reflectionBounces;
		result.reflectionIntensity = RenderSettings.reflectionIntensity;
		result.customReflection = RenderSettings.customReflection;
		result.ambientProbe = RenderSettings.ambientProbe;
		result.sun = RenderSettings.sun;
		result.skybox = RenderSettings.skybox;
		result.subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
		result.flareStrength = RenderSettings.flareStrength;
		result.ambientLight = RenderSettings.ambientLight;
		result.ambientGroundColor = RenderSettings.ambientGroundColor;
		result.ambientEquatorColor = RenderSettings.ambientEquatorColor;
		result.ambientSkyColor = RenderSettings.ambientSkyColor;
		result.ambientMode = RenderSettings.ambientMode;
		result.fogDensity = RenderSettings.fogDensity;
		result.fogColor = RenderSettings.fogColor;
		result.fogMode = RenderSettings.fogMode;
		result.fogEndDistance = RenderSettings.fogEndDistance;
		result.fogStartDistance = RenderSettings.fogStartDistance;
		result.fog = RenderSettings.fog;
		result.ambientIntensity = RenderSettings.ambientIntensity;
		result.flareFadeSpeed = RenderSettings.flareFadeSpeed;
		return result;
	}

	public void Apply()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		RenderSettings.haloStrength = haloStrength;
		RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
		RenderSettings.defaultReflectionMode = defaultReflectionMode;
		RenderSettings.reflectionBounces = reflectionBounces;
		RenderSettings.reflectionIntensity = reflectionIntensity;
		RenderSettings.customReflection = customReflection;
		RenderSettings.ambientProbe = ambientProbe;
		RenderSettings.sun = sun;
		RenderSettings.skybox = skybox;
		RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
		RenderSettings.flareStrength = flareStrength;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.ambientGroundColor = ambientGroundColor;
		RenderSettings.ambientEquatorColor = ambientEquatorColor;
		RenderSettings.ambientSkyColor = ambientSkyColor;
		RenderSettings.ambientMode = ambientMode;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogMode = fogMode;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fog = fog;
		RenderSettings.ambientIntensity = ambientIntensity;
		RenderSettings.flareFadeSpeed = flareFadeSpeed;
	}
}
