using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class RampFog : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Fog intensity.")]
	public FloatParameter fogIntensity;

	[Range(0f, 20f)]
	[Tooltip("Fog Power")]
	public FloatParameter fogPower;

	[Range(-1f, 1f)]
	[Tooltip("The zero value for the fog depth remap.")]
	public FloatParameter fogZero;

	[Range(-1f, 1f)]
	[Tooltip("The one value for the fog depth remap.")]
	public FloatParameter fogOne;

	[Range(-100f, 100f)]
	[Tooltip("The world position value where the height fog begins.")]
	public FloatParameter fogHeightStart;

	[Tooltip("The world position value where the height fog ends.")]
	[Range(-100f, 600f)]
	public FloatParameter fogHeightEnd;

	[Tooltip("The overall strength of the height fog.")]
	[Range(0f, 5f)]
	public FloatParameter fogHeightIntensity;

	[Tooltip("Color of the fog at the beginning.")]
	public ColorParameter fogColorStart;

	[Tooltip("Color of the fog at the middle.")]
	public ColorParameter fogColorMid;

	[Tooltip("Color of the fog at the end.")]
	public ColorParameter fogColorEnd;

	[Range(0f, 1f)]
	[Tooltip("How much of the skybox will leak through?")]
	public FloatParameter skyboxStrength;

	public RampFog()
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
		//IL_0053: Expected O, but got Unknown
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_006e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0084: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00f2: Expected O, but got Unknown
		FloatParameter val = new FloatParameter();
		((ParameterOverride<float>)val).value = 0.5f;
		fogIntensity = val;
		FloatParameter val2 = new FloatParameter();
		((ParameterOverride<float>)val2).value = 1f;
		fogPower = val2;
		FloatParameter val3 = new FloatParameter();
		((ParameterOverride<float>)val3).value = 0f;
		fogZero = val3;
		FloatParameter val4 = new FloatParameter();
		((ParameterOverride<float>)val4).value = 1f;
		fogOne = val4;
		FloatParameter val5 = new FloatParameter();
		((ParameterOverride<float>)val5).value = 0f;
		fogHeightStart = val5;
		FloatParameter val6 = new FloatParameter();
		((ParameterOverride<float>)val6).value = 100f;
		fogHeightEnd = val6;
		FloatParameter val7 = new FloatParameter();
		((ParameterOverride<float>)val7).value = 0f;
		fogHeightIntensity = val7;
		ColorParameter val8 = new ColorParameter();
		((ParameterOverride<Color>)val8).value = Color.white;
		fogColorStart = val8;
		ColorParameter val9 = new ColorParameter();
		((ParameterOverride<Color>)val9).value = Color.white;
		fogColorMid = val9;
		ColorParameter val10 = new ColorParameter();
		((ParameterOverride<Color>)val10).value = Color.white;
		fogColorEnd = val10;
		FloatParameter val11 = new FloatParameter();
		((ParameterOverride<float>)val11).value = 0f;
		skyboxStrength = val11;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
