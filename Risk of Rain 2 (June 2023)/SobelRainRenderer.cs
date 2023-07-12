using RoR2;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class SobelRainRenderer : PostProcessEffectRenderer<SobelRain>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		PropertySheet val = context.propertySheets.Get(LegacyShaderAPI.Find("Hidden/PostProcess/SobelRain"));
		val.properties.SetFloat("_RainIntensity", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.rainIntensity));
		val.properties.SetFloat("_OutlineScale", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.outlineScale));
		val.properties.SetFloat("_RainDensity", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.rainDensity));
		val.properties.SetTexture("_RainTexture", ParameterOverride<Texture>.op_Implicit((ParameterOverride<Texture>)(object)base.settings.rainTexture));
		val.properties.SetColor("_RainColor", ParameterOverride<Color>.op_Implicit((ParameterOverride<Color>)(object)base.settings.rainColor));
		RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, val, 0, false, (Rect?)null);
	}
}
