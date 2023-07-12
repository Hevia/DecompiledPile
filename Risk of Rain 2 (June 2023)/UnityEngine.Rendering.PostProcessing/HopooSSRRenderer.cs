using RoR2;

namespace UnityEngine.Rendering.PostProcessing;

public sealed class HopooSSRRenderer : PostProcessEffectRenderer<HopooSSR>
{
	private class QualityPreset
	{
		public int maximumIterationCount;

		public float thickness;

		public ScreenSpaceReflectionResolution downsampling;
	}

	private enum Pass
	{
		Test,
		Resolve,
		Reproject,
		Composite
	}

	private RenderTexture m_Resolve;

	private RenderTexture m_History;

	private int[] m_MipIDs;

	private readonly QualityPreset[] m_Presets = new QualityPreset[7]
	{
		new QualityPreset
		{
			maximumIterationCount = 10,
			thickness = 32f,
			downsampling = (ScreenSpaceReflectionResolution)0
		},
		new QualityPreset
		{
			maximumIterationCount = 16,
			thickness = 32f,
			downsampling = (ScreenSpaceReflectionResolution)0
		},
		new QualityPreset
		{
			maximumIterationCount = 32,
			thickness = 16f,
			downsampling = (ScreenSpaceReflectionResolution)0
		},
		new QualityPreset
		{
			maximumIterationCount = 48,
			thickness = 8f,
			downsampling = (ScreenSpaceReflectionResolution)0
		},
		new QualityPreset
		{
			maximumIterationCount = 16,
			thickness = 32f,
			downsampling = (ScreenSpaceReflectionResolution)1
		},
		new QualityPreset
		{
			maximumIterationCount = 48,
			thickness = 16f,
			downsampling = (ScreenSpaceReflectionResolution)1
		},
		new QualityPreset
		{
			maximumIterationCount = 128,
			thickness = 12f,
			downsampling = (ScreenSpaceReflectionResolution)2
		}
	};

	public override DepthTextureMode GetCameraFlags()
	{
		return (DepthTextureMode)5;
	}

	internal void CheckRT(ref RenderTexture rt, int width, int height, RenderTextureFormat format, FilterMode filterMode, bool useMipMap)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		if ((Object)(object)rt == (Object)null || !rt.IsCreated() || ((Texture)rt).width != width || ((Texture)rt).height != height || rt.format != format)
		{
			if ((Object)(object)rt != (Object)null)
			{
				rt.Release();
			}
			rt = new RenderTexture(width, height, 0, format)
			{
				filterMode = filterMode,
				useMipMap = useMipMap,
				autoGenerateMips = false,
				hideFlags = (HideFlags)61
			};
			rt.Create();
		}
	}

	public override void Render(PostProcessRenderContext context)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_0664: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("Screen-space Reflections");
		if ((int)((ParameterOverride<ScreenSpaceReflectionPreset>)(object)base.settings.preset).value != 7)
		{
			int num = (int)((ParameterOverride<ScreenSpaceReflectionPreset>)(object)base.settings.preset).value;
			((ParameterOverride<int>)(object)base.settings.maximumIterationCount).value = m_Presets[num].maximumIterationCount;
			((ParameterOverride<float>)(object)base.settings.thickness).value = m_Presets[num].thickness;
			((ParameterOverride<ScreenSpaceReflectionResolution>)(object)base.settings.resolution).value = m_Presets[num].downsampling;
		}
		((ParameterOverride<float>)(object)base.settings.maximumMarchDistance).value = Mathf.Max(0f, ((ParameterOverride<float>)(object)base.settings.maximumMarchDistance).value);
		int num2 = Mathf.ClosestPowerOfTwo(Mathf.Min(context.width, context.height));
		if ((int)((ParameterOverride<ScreenSpaceReflectionResolution>)(object)base.settings.resolution).value == 0)
		{
			num2 >>= 1;
		}
		else if ((int)((ParameterOverride<ScreenSpaceReflectionResolution>)(object)base.settings.resolution).value == 2)
		{
			num2 <<= 1;
		}
		int num3 = Mathf.FloorToInt(Mathf.Log((float)num2, 2f) - 3f);
		num3 = Mathf.Min(num3, 12);
		CheckRT(ref m_Resolve, num2, num2, context.sourceFormat, (FilterMode)2, useMipMap: true);
		Texture2D val = context.resources.blueNoise256[0];
		PropertySheet val2 = context.propertySheets.Get(LegacyShaderAPI.Find("Hidden/PostProcessing/HopooSSR"));
		val2.properties.SetTexture(Shader.PropertyToID("_Noise"), (Texture)(object)val);
		Matrix4x4 val3 = default(Matrix4x4);
		((Matrix4x4)(ref val3)).SetRow(0, new Vector4((float)num2 * 0.5f, 0f, 0f, (float)num2 * 0.5f));
		((Matrix4x4)(ref val3)).SetRow(1, new Vector4(0f, (float)num2 * 0.5f, 0f, (float)num2 * 0.5f));
		((Matrix4x4)(ref val3)).SetRow(2, new Vector4(0f, 0f, 1f, 0f));
		((Matrix4x4)(ref val3)).SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
		val3 *= gPUProjectionMatrix;
		val2.properties.SetMatrix(Shader.PropertyToID("_ViewMatrix"), context.camera.worldToCameraMatrix);
		MaterialPropertyBlock properties = val2.properties;
		int num4 = Shader.PropertyToID("_InverseViewMatrix");
		Matrix4x4 worldToCameraMatrix = context.camera.worldToCameraMatrix;
		properties.SetMatrix(num4, ((Matrix4x4)(ref worldToCameraMatrix)).inverse);
		val2.properties.SetMatrix(Shader.PropertyToID("_InverseProjectionMatrix"), ((Matrix4x4)(ref gPUProjectionMatrix)).inverse);
		val2.properties.SetMatrix(Shader.PropertyToID("_ScreenSpaceProjectionMatrix"), val3);
		val2.properties.SetVector(Shader.PropertyToID("_Params"), new Vector4(((ParameterOverride<float>)(object)base.settings.vignette).value, ((ParameterOverride<float>)(object)base.settings.distanceFade).value, ((ParameterOverride<float>)(object)base.settings.maximumMarchDistance).value, (float)num3));
		val2.properties.SetVector(Shader.PropertyToID("_Params2"), new Vector4((float)context.width / (float)context.height, (float)num2 / (float)((Texture)val).width, ((ParameterOverride<float>)(object)base.settings.thickness).value, (float)((ParameterOverride<int>)(object)base.settings.maximumIterationCount).value));
		command.GetTemporaryRT(Shader.PropertyToID("_Test"), num2, num2, 0, (FilterMode)0, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, RenderTargetIdentifier.op_Implicit(Shader.PropertyToID("_Test")), val2, 0, false, (Rect?)null);
		if (context.isSceneView)
		{
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, RenderTargetIdentifier.op_Implicit((Texture)(object)m_Resolve), val2, 1, false, (Rect?)null);
		}
		else
		{
			CheckRT(ref m_History, num2, num2, context.sourceFormat, (FilterMode)1, useMipMap: false);
			if (((PostProcessEffectRenderer)this).m_ResetHistory)
			{
				RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, RenderTargetIdentifier.op_Implicit((Texture)(object)m_History), false, (Rect?)null);
				((PostProcessEffectRenderer)this).m_ResetHistory = false;
			}
			command.GetTemporaryRT(Shader.PropertyToID("_SSRResolveTemp"), num2, num2, 0, (FilterMode)1, context.sourceFormat);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, RenderTargetIdentifier.op_Implicit(Shader.PropertyToID("_SSRResolveTemp")), val2, 1, false, (Rect?)null);
			val2.properties.SetTexture(Shader.PropertyToID("_History"), (Texture)(object)m_History);
			RuntimeUtilities.BlitFullscreenTriangle(command, RenderTargetIdentifier.op_Implicit(Shader.PropertyToID("_SSRResolveTemp")), RenderTargetIdentifier.op_Implicit((Texture)(object)m_Resolve), val2, 2, false, (Rect?)null);
			command.CopyTexture(RenderTargetIdentifier.op_Implicit((Texture)(object)m_Resolve), 0, 0, RenderTargetIdentifier.op_Implicit((Texture)(object)m_History), 0, 0);
			command.ReleaseTemporaryRT(Shader.PropertyToID("_SSRResolveTemp"));
		}
		command.ReleaseTemporaryRT(Shader.PropertyToID("_Test"));
		if (m_MipIDs == null || m_MipIDs.Length == 0)
		{
			m_MipIDs = new int[12];
			for (int i = 0; i < 12; i++)
			{
				m_MipIDs[i] = Shader.PropertyToID("_SSRGaussianMip" + i);
			}
		}
		ComputeShader gaussianDownsample = context.resources.computeShaders.gaussianDownsample;
		int num5 = gaussianDownsample.FindKernel("KMain");
		RenderTargetIdentifier val4 = default(RenderTargetIdentifier);
		((RenderTargetIdentifier)(ref val4))._002Ector((Texture)(object)m_Resolve);
		for (int j = 0; j < num3; j++)
		{
			num2 >>= 1;
			command.GetTemporaryRT(m_MipIDs[j], num2, num2, 0, (FilterMode)1, context.sourceFormat, (RenderTextureReadWrite)0, 1, true);
			command.SetComputeTextureParam(gaussianDownsample, num5, "_Source", val4);
			command.SetComputeTextureParam(gaussianDownsample, num5, "_Result", RenderTargetIdentifier.op_Implicit(m_MipIDs[j]));
			command.SetComputeVectorParam(gaussianDownsample, "_Size", new Vector4((float)num2, (float)num2, 1f / (float)num2, 1f / (float)num2));
			command.DispatchCompute(gaussianDownsample, num5, num2 / 8, num2 / 8, 1);
			command.CopyTexture(RenderTargetIdentifier.op_Implicit(m_MipIDs[j]), 0, 0, RenderTargetIdentifier.op_Implicit((Texture)(object)m_Resolve), 0, j + 1);
			val4 = RenderTargetIdentifier.op_Implicit(m_MipIDs[j]);
		}
		for (int k = 0; k < num3; k++)
		{
			command.ReleaseTemporaryRT(m_MipIDs[k]);
		}
		val2.properties.SetTexture(Shader.PropertyToID("_Resolve"), (Texture)(object)m_Resolve);
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, val2, 3, false, (Rect?)null);
		command.EndSample("Screen-space Reflections");
	}

	public override void Release()
	{
		RuntimeUtilities.Destroy((Object)(object)m_Resolve);
		RuntimeUtilities.Destroy((Object)(object)m_History);
		m_Resolve = null;
		m_History = null;
	}
}
