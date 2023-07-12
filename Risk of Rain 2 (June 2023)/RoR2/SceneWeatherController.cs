using System;
using UnityEngine;

namespace RoR2;

[ExecuteInEditMode]
public class SceneWeatherController : MonoBehaviour
{
	[Serializable]
	public struct WeatherParams
	{
		[ColorUsage(true, true)]
		public Color sunColor;

		public float sunIntensity;

		public float fogStart;

		public float fogScale;

		public float fogIntensity;
	}

	private static SceneWeatherController _instance;

	public WeatherParams initialWeatherParams;

	public WeatherParams finalWeatherParams;

	public Light sun;

	public Material fogMaterial;

	public string rtpcWeather;

	public float rtpcMin;

	public float rtpcMax = 100f;

	public AnimationCurve weatherLerpOverChargeTime;

	[Range(0f, 1f)]
	public float weatherLerp;

	public static SceneWeatherController instance => _instance;

	private void OnEnable()
	{
		if (!Object.op_Implicit((Object)(object)_instance))
		{
			_instance = this;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)_instance == (Object)(object)this)
		{
			_instance = null;
		}
	}

	private WeatherParams GetWeatherParams(float t)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		WeatherParams result = default(WeatherParams);
		result.sunColor = Color.Lerp(initialWeatherParams.sunColor, finalWeatherParams.sunColor, t);
		result.sunIntensity = Mathf.Lerp(initialWeatherParams.sunIntensity, finalWeatherParams.sunIntensity, t);
		result.fogStart = Mathf.Lerp(initialWeatherParams.fogStart, finalWeatherParams.fogStart, t);
		result.fogScale = Mathf.Lerp(initialWeatherParams.fogScale, finalWeatherParams.fogScale, t);
		result.fogIntensity = Mathf.Lerp(initialWeatherParams.fogIntensity, finalWeatherParams.fogIntensity, t);
		return result;
	}

	private void Update()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		WeatherParams weatherParams = GetWeatherParams(weatherLerp);
		if (Object.op_Implicit((Object)(object)sun))
		{
			sun.color = weatherParams.sunColor;
			sun.intensity = weatherParams.sunIntensity;
		}
		if (Object.op_Implicit((Object)(object)fogMaterial))
		{
			fogMaterial.SetFloat("_FogPicker", weatherLerp);
			fogMaterial.SetFloat("_FogStart", weatherParams.fogStart);
			fogMaterial.SetFloat("_FogScale", weatherParams.fogScale);
			fogMaterial.SetFloat("_FogIntensity", weatherParams.fogIntensity);
		}
		if (true && rtpcWeather.Length != 0)
		{
			AkSoundEngine.SetRTPCValue(rtpcWeather, Mathf.Lerp(rtpcMin, rtpcMax, weatherLerp), ((Component)this).gameObject);
		}
	}
}
