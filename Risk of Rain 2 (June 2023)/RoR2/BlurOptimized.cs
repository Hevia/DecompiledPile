using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Camera))]
public class BlurOptimized : MonoBehaviour
{
	public enum BlurType
	{
		StandardGauss,
		SgxGauss
	}

	[Range(0f, 2f)]
	public int downsample = 1;

	[Range(0f, 10f)]
	public float blurSize = 3f;

	[Range(1f, 4f)]
	public int blurIterations = 2;

	public BlurType blurType;

	[HideInInspector]
	public Material blurMaterial;

	public void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		blurMaterial = new Material(LegacyShaderAPI.Find("Hidden/FastBlur"));
		((Behaviour)this).enabled = false;
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / (1f * (float)(1 << downsample));
		blurMaterial.SetVector("_Parameter", new Vector4(blurSize * num, (0f - blurSize) * num, 0f, 0f));
		((Texture)source).filterMode = (FilterMode)1;
		int num2 = ((Texture)source).width >> downsample;
		int num3 = ((Texture)source).height >> downsample;
		RenderTexture val = RenderTexture.GetTemporary(num2, num3, 0, source.format);
		((Texture)val).filterMode = (FilterMode)1;
		Graphics.Blit((Texture)(object)source, val, blurMaterial, 0);
		int num4 = ((blurType != 0) ? 2 : 0);
		for (int i = 0; i < blurIterations; i++)
		{
			float num5 = (float)i * 1f;
			blurMaterial.SetVector("_Parameter", new Vector4(blurSize * num + num5, (0f - blurSize) * num - num5, 0f, 0f));
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, source.format);
			((Texture)temporary).filterMode = (FilterMode)1;
			Graphics.Blit((Texture)(object)val, temporary, blurMaterial, 1 + num4);
			RenderTexture.ReleaseTemporary(val);
			val = temporary;
			temporary = RenderTexture.GetTemporary(num2, num3, 0, source.format);
			((Texture)temporary).filterMode = (FilterMode)1;
			Graphics.Blit((Texture)(object)val, temporary, blurMaterial, 2 + num4);
			RenderTexture.ReleaseTemporary(val);
			val = temporary;
		}
		Graphics.Blit((Texture)(object)val, destination);
		RenderTexture.ReleaseTemporary(val);
	}
}
