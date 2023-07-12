using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class SobelRain : PostProcessEffectSettings
{
	[Range(0f, 100f)]
	[Tooltip("The intensity of the rain.")]
	public FloatParameter rainIntensity;

	[Tooltip("The falloff of the outline. Higher values means it relies less on the sobel.")]
	[Range(0f, 10f)]
	public FloatParameter outlineScale;

	[Range(0f, 1f)]
	[Tooltip("The density of rain.")]
	public FloatParameter rainDensity;

	public TextureParameter rainTexture;

	public ColorParameter rainColor;

	public SobelRain()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0054: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		FloatParameter val = new FloatParameter();
		((ParameterOverride<float>)val).value = 0.5f;
		rainIntensity = val;
		FloatParameter val2 = new FloatParameter();
		((ParameterOverride<float>)val2).value = 1f;
		outlineScale = val2;
		FloatParameter val3 = new FloatParameter();
		((ParameterOverride<float>)val3).value = 1f;
		rainDensity = val3;
		TextureParameter val4 = new TextureParameter();
		((ParameterOverride<Texture>)val4).value = null;
		rainTexture = val4;
		ColorParameter val5 = new ColorParameter();
		((ParameterOverride<Color>)val5).value = Color.white;
		rainColor = val5;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
