using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class SobelOutline : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	[Tooltip("The intensity of the outline.")]
	public FloatParameter outlineIntensity;

	[Tooltip("The falloff of the outline.")]
	[Range(0f, 10f)]
	public FloatParameter outlineScale;

	public SobelOutline()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_002c: Expected O, but got Unknown
		FloatParameter val = new FloatParameter();
		((ParameterOverride<float>)val).value = 0.5f;
		outlineIntensity = val;
		FloatParameter val2 = new FloatParameter();
		((ParameterOverride<float>)val2).value = 1f;
		outlineScale = val2;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
