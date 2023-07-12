using UnityEngine;
using UnityEngine.Rendering;

namespace RoR2.PostProcessing;

[RequireComponent(typeof(Camera))]
public class VisionLimitEffect : MonoBehaviour
{
	private static class ShaderParamsIDs
	{
		public static int origin = Shader.PropertyToID("_Origin");

		public static int rangeStart = Shader.PropertyToID("_RangeStart");

		public static int rangeEnd = Shader.PropertyToID("_RangeEnd");

		public static int color = Shader.PropertyToID("_Color");
	}

	public CameraRigController cameraRigController;

	private Camera camera;

	private float desiredVisionDistance = float.PositiveInfinity;

	private CommandBuffer commandBuffer;

	private Material material;

	private Vector3 lastKnownTargetPosition;

	private float currentAlpha;

	private float alphaVelocity;

	private float currentVisionDistance;

	private float currentVisionDistanceVelocity;

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		camera = ((Component)this).GetComponent<Camera>();
		commandBuffer = new CommandBuffer();
		commandBuffer.name = "VisionLimitEffect";
		Shader val = LegacyShaderAPI.Find("Hopoo Games/Internal/VisionLimit");
		material = new Material(val);
		((Object)material).name = "VisionLimitEffectMaterial";
	}

	private void OnDestroy()
	{
		DestroyTemporaryAsset((Object)(object)material);
		commandBuffer = null;
		camera = null;
	}

	private void OnEnable()
	{
		camera.AddCommandBuffer((CameraEvent)13, commandBuffer);
	}

	private void OnDisable()
	{
		camera.RemoveCommandBuffer((CameraEvent)13, commandBuffer);
	}

	private void OnPreCull()
	{
		UpdateCommandBuffer();
	}

	private void LateUpdate()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Transform val = (Object.op_Implicit((Object)(object)cameraRigController.target) ? cameraRigController.target.transform : null);
		CharacterBody targetBody = cameraRigController.targetBody;
		if (Object.op_Implicit((Object)(object)val))
		{
			lastKnownTargetPosition = val.position;
		}
		desiredVisionDistance = (Object.op_Implicit((Object)(object)targetBody) ? targetBody.visionDistance : float.PositiveInfinity);
		float num = 0f;
		float num2 = 4000f;
		if (desiredVisionDistance != float.PositiveInfinity)
		{
			num = 1f;
			num2 = desiredVisionDistance;
		}
		currentAlpha = Mathf.SmoothDamp(currentAlpha, num, ref alphaVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
		currentVisionDistance = Mathf.SmoothDamp(currentVisionDistance, num2, ref currentVisionDistanceVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
	}

	private void UpdateCommandBuffer()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		commandBuffer.Clear();
		if (!(currentAlpha <= 0f))
		{
			float num = Mathf.Max(0f, currentVisionDistance * 0.5f);
			float num2 = Mathf.Max(num + 0.01f, currentVisionDistance);
			material.SetVector(ShaderParamsIDs.origin, Vector4.op_Implicit(lastKnownTargetPosition));
			material.SetFloat(ShaderParamsIDs.rangeStart, num);
			material.SetFloat(ShaderParamsIDs.rangeEnd, num2);
			material.SetColor(ShaderParamsIDs.color, new Color(0f, 0f, 0f, currentAlpha));
			commandBuffer.Blit((Texture)null, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), material);
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
