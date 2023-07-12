using RoR2;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class RampFogRenderer : PostProcessEffectRenderer<RampFog>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		PropertySheet val = context.propertySheets.Get(LegacyShaderAPI.Find("Hidden/PostProcess/RampFog"));
		val.properties.SetFloat("_FogIntensity", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogIntensity));
		val.properties.SetFloat("_FogPower", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogPower));
		val.properties.SetFloat("_FogZero", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogZero));
		val.properties.SetFloat("_FogOne", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogOne));
		val.properties.SetFloat("_FogHeightStart", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogHeightStart));
		val.properties.SetFloat("_FogHeightEnd", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogHeightEnd));
		val.properties.SetFloat("_FogHeightIntensity", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.fogHeightIntensity));
		val.properties.SetColor("_FogColorStart", ParameterOverride<Color>.op_Implicit((ParameterOverride<Color>)(object)base.settings.fogColorStart));
		val.properties.SetColor("_FogColorMid", ParameterOverride<Color>.op_Implicit((ParameterOverride<Color>)(object)base.settings.fogColorMid));
		val.properties.SetColor("_FogColorEnd", ParameterOverride<Color>.op_Implicit((ParameterOverride<Color>)(object)base.settings.fogColorEnd));
		val.properties.SetFloat("_SkyboxStrength", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.skyboxStrength));
		RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, val, 0, false, (Rect?)null);
	}
}
