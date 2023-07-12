using RoR2;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class SobelOutlineRenderer : PostProcessEffectRenderer<SobelOutline>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		PropertySheet val = context.propertySheets.Get(LegacyShaderAPI.Find("Hidden/PostProcess/SobelOutline"));
		val.properties.SetFloat("_OutlineIntensity", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.outlineIntensity));
		val.properties.SetFloat("_OutlineScale", ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.outlineScale));
		RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, val, 0, false, (Rect?)null);
	}
}
