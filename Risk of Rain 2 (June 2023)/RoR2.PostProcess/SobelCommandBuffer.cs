using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoR2.PostProcess;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class SobelCommandBuffer : MonoBehaviour
{
	public CameraEvent cameraEvent = (CameraEvent)6;

	private Camera camera;

	private RenderTexture sobelInfoTex;

	private CommandBuffer sobelCommandBuffer;

	private CameraEvent sobelCommandBufferSubscribedEvent;

	private Material sobelBufferMaterial;

	private void Awake()
	{
		Initialize();
	}

	private void OnDestroy()
	{
		DestroyTemporaryAsset((Object)(object)sobelBufferMaterial);
	}

	private void OnEnable()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			Initialize();
		}
		camera.AddCommandBuffer(cameraEvent, sobelCommandBuffer);
		sobelCommandBufferSubscribedEvent = cameraEvent;
	}

	private void OnDisable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		DestroyTemporaryAsset((Object)(object)sobelInfoTex);
		camera.RemoveCommandBuffer(sobelCommandBufferSubscribedEvent, sobelCommandBuffer);
		sobelCommandBuffer.Clear();
	}

	private void OnPreRender()
	{
		Rebuild();
	}

	private void Initialize()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (sobelCommandBuffer == null)
		{
			camera = ((Component)this).GetComponent<Camera>();
			sobelBufferMaterial = new Material(LegacyShaderAPI.Find("Hopoo Games/Internal/SobelBuffer"));
			sobelCommandBuffer = new CommandBuffer();
			sobelCommandBuffer.name = "Sobel Command Buffer";
		}
	}

	private void Rebuild()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = default(Vector2Int);
		((Vector2Int)(ref val))._002Ector(camera.pixelWidth, camera.pixelHeight);
		RenderTexture obj = sobelInfoTex;
		if (Object.op_Implicit((Object)(object)sobelInfoTex) && new Vector2Int(((Texture)sobelInfoTex).width, ((Texture)sobelInfoTex).height) != val)
		{
			DestroyTemporaryAsset((Object)(object)sobelInfoTex);
			sobelInfoTex = null;
		}
		if (!Object.op_Implicit((Object)(object)sobelInfoTex) && ((Vector2Int)(ref val)).x > 0 && ((Vector2Int)(ref val)).y > 0)
		{
			sobelInfoTex = new RenderTexture(((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y, 0, (GraphicsFormat)5, 0);
			((Object)sobelInfoTex).name = "Sobel Outline Information";
		}
		if ((Object)(object)obj != (Object)(object)sobelInfoTex)
		{
			int num = Shader.PropertyToID("_SobelTex");
			RenderTargetIdentifier val2 = default(RenderTargetIdentifier);
			((RenderTargetIdentifier)(ref val2))._002Ector((Texture)(object)sobelInfoTex);
			RenderTargetIdentifier val3 = default(RenderTargetIdentifier);
			((RenderTargetIdentifier)(ref val3))._002Ector((BuiltinRenderTextureType)5);
			RenderTargetIdentifier renderTarget = default(RenderTargetIdentifier);
			((RenderTargetIdentifier)(ref renderTarget))._002Ector((BuiltinRenderTextureType)2);
			sobelCommandBuffer.Clear();
			sobelCommandBuffer.Blit(val3, val2, sobelBufferMaterial);
			sobelCommandBuffer.SetGlobalTexture(num, val2);
			sobelCommandBuffer.SetRenderTarget(renderTarget);
		}
	}

	private void DestroyTemporaryAsset(Object temporaryAsset)
	{
		if (Application.isPlaying)
		{
			Object.Destroy(temporaryAsset);
		}
		else
		{
			Object.DestroyImmediate(temporaryAsset);
		}
	}
}
