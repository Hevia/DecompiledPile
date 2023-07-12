using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoR2;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(SceneCamera))]
public class OutlineHighlight : MonoBehaviour
{
	private enum Passes
	{
		FillPass,
		Blit
	}

	public enum RTResolution
	{
		Quarter = 4,
		Half = 2,
		Full = 1
	}

	public struct HighlightInfo
	{
		public Color color;

		public Renderer renderer;
	}

	public RTResolution m_resolution = RTResolution.Full;

	public readonly Queue<HighlightInfo> highlightQueue = new Queue<HighlightInfo>();

	private Material highlightMaterial;

	private CommandBuffer commandBuffer;

	private int m_RTWidth = 512;

	private int m_RTHeight = 512;

	public static Action<OutlineHighlight> onPreRenderOutlineHighlight;

	public SceneCamera sceneCamera { get; private set; }

	private void Awake()
	{
		sceneCamera = ((Component)this).GetComponent<SceneCamera>();
		CreateBuffers();
		CreateMaterials();
		m_RTWidth = (int)((float)Screen.width / (float)m_resolution);
		m_RTHeight = (int)((float)Screen.height / (float)m_resolution);
	}

	private void CreateBuffers()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		commandBuffer = new CommandBuffer();
	}

	private void ClearCommandBuffers()
	{
		commandBuffer.Clear();
	}

	private void CreateMaterials()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		highlightMaterial = new Material(LegacyShaderAPI.Find("Hopoo Games/Internal/Outline Highlight"));
	}

	private void RenderHighlights(RenderTexture rt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		RenderTargetIdentifier renderTarget = default(RenderTargetIdentifier);
		((RenderTargetIdentifier)(ref renderTarget))._002Ector((Texture)(object)rt);
		commandBuffer.SetRenderTarget(renderTarget);
		foreach (Highlight readonlyHighlight in Highlight.readonlyHighlightList)
		{
			if (readonlyHighlight.isOn && Object.op_Implicit((Object)(object)readonlyHighlight.targetRenderer))
			{
				highlightQueue.Enqueue(new HighlightInfo
				{
					color = readonlyHighlight.GetColor() * readonlyHighlight.strength,
					renderer = readonlyHighlight.targetRenderer
				});
			}
		}
		onPreRenderOutlineHighlight?.Invoke(this);
		while (highlightQueue.Count > 0)
		{
			HighlightInfo highlightInfo = highlightQueue.Dequeue();
			if (Object.op_Implicit((Object)(object)highlightInfo.renderer))
			{
				highlightMaterial.SetColor("_Color", highlightInfo.color);
				commandBuffer.DrawRenderer(highlightInfo.renderer, highlightMaterial, 0, 0);
				RenderTexture.active = rt;
				Graphics.ExecuteCommandBuffer(commandBuffer);
				RenderTexture.active = null;
				ClearCommandBuffers();
			}
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)((float)((Texture)destination).width / (float)m_resolution);
		int num2 = (int)((float)((Texture)destination).height / (float)m_resolution);
		RenderTexture val = (RenderTexture.active = RenderTexture.GetTemporary(num, num2, 0, (RenderTextureFormat)0));
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = null;
		ClearCommandBuffers();
		RenderHighlights(val);
		highlightMaterial.SetTexture("_OutlineMap", (Texture)(object)val);
		highlightMaterial.SetColor("_Color", Color.white);
		Graphics.Blit((Texture)(object)source, destination, highlightMaterial, 1);
		RenderTexture.ReleaseTemporary(val);
	}
}
