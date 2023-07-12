using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(/*Could not decode attribute arguments.*/)]
public sealed class HopooSSR : PostProcessEffectSettings
{
	[Tooltip("Choose a quality preset, or use \"Custom\" to fine tune it. Don't use a preset higher than \"Medium\" if you care about performances on consoles.")]
	public ScreenSpaceReflectionPresetParameter preset;

	[Range(0f, 256f)]
	[Tooltip("Maximum iteration count.")]
	public IntParameter maximumIterationCount;

	[Tooltip("Changes the size of the SSR buffer. Downsample it to maximize performances or supersample it to get slow but higher quality results.")]
	public ScreenSpaceReflectionResolutionParameter resolution;

	[Range(1f, 64f)]
	[Tooltip("Ray thickness. Lower values are more expensive but allow the effect to detect smaller details.")]
	public FloatParameter thickness;

	[Tooltip("Maximum distance to traverse after which it will stop drawing reflections.")]
	public FloatParameter maximumMarchDistance;

	[Range(0f, 1f)]
	[Tooltip("Fades reflections close to the near planes.")]
	public FloatParameter distanceFade;

	[Tooltip("Fades reflections close to the screen edges.")]
	[Range(0f, 1f)]
	public FloatParameter vignette;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		if (ParameterOverride<bool>.op_Implicit((ParameterOverride<bool>)(object)base.enabled) && (int)context.camera.actualRenderingPath == 3 && SystemInfo.supportsMotionVectors && SystemInfo.supportsComputeShaders && (int)SystemInfo.copyTextureSupport > 0 && Object.op_Implicit((Object)(object)context.resources.shaders.screenSpaceReflections) && context.resources.shaders.screenSpaceReflections.isSupported)
		{
			return Object.op_Implicit((Object)(object)context.resources.computeShaders.gaussianDownsample);
		}
		return false;
	}

	public HopooSSR()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0025: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0079: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_008f: Expected O, but got Unknown
		ScreenSpaceReflectionPresetParameter val = new ScreenSpaceReflectionPresetParameter();
		((ParameterOverride<ScreenSpaceReflectionPreset>)val).value = (ScreenSpaceReflectionPreset)2;
		preset = val;
		IntParameter val2 = new IntParameter();
		((ParameterOverride<int>)val2).value = 16;
		maximumIterationCount = val2;
		ScreenSpaceReflectionResolutionParameter val3 = new ScreenSpaceReflectionResolutionParameter();
		((ParameterOverride<ScreenSpaceReflectionResolution>)val3).value = (ScreenSpaceReflectionResolution)0;
		resolution = val3;
		FloatParameter val4 = new FloatParameter();
		((ParameterOverride<float>)val4).value = 8f;
		thickness = val4;
		FloatParameter val5 = new FloatParameter();
		((ParameterOverride<float>)val5).value = 100f;
		maximumMarchDistance = val5;
		FloatParameter val6 = new FloatParameter();
		((ParameterOverride<float>)val6).value = 0.5f;
		distanceFade = val6;
		FloatParameter val7 = new FloatParameter();
		((ParameterOverride<float>)val7).value = 0.5f;
		vignette = val7;
		((PostProcessEffectSettings)this)._002Ector();
	}
}
