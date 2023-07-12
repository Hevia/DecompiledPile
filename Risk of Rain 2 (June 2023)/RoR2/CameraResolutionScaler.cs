using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraResolutionScaler : MonoBehaviour
{
	private class ResolutionScaleConVar : BaseConVar
	{
		private static ResolutionScaleConVar instance = new ResolutionScaleConVar("resolution_scale", ConVarFlags.Archive, null, "Resolution scale. Currently nonfunctional.");

		private ResolutionScaleConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			TextSerialization.TryParseInvariant(newValue, out float _);
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(resolutionScale);
		}
	}

	private RenderTexture oldRenderTexture;

	private static float resolutionScale = 1f;

	private RenderTexture scalingRenderTexture;

	public Camera camera { get; private set; }

	private void Awake()
	{
		camera = ((Component)this).GetComponent<Camera>();
	}

	private void OnPreRender()
	{
		ApplyScalingRenderTexture();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!Object.op_Implicit((Object)(object)scalingRenderTexture))
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		camera.targetTexture = oldRenderTexture;
		Graphics.Blit((Texture)(object)source, oldRenderTexture);
		oldRenderTexture = null;
	}

	private static void SetResolutionScale(float newResolutionScale)
	{
		if (resolutionScale != newResolutionScale)
		{
			resolutionScale = newResolutionScale;
		}
	}

	private void ApplyScalingRenderTexture()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		oldRenderTexture = camera.targetTexture;
		bool flag = resolutionScale != 1f;
		camera.targetTexture = null;
		Rect pixelRect = camera.pixelRect;
		int num = Mathf.FloorToInt(((Rect)(ref pixelRect)).width * resolutionScale);
		int num2 = Mathf.FloorToInt(((Rect)(ref pixelRect)).height * resolutionScale);
		if (Object.op_Implicit((Object)(object)scalingRenderTexture) && (((Texture)scalingRenderTexture).width != num || ((Texture)scalingRenderTexture).height != num2))
		{
			Object.Destroy((Object)(object)scalingRenderTexture);
			scalingRenderTexture = null;
		}
		if (flag != Object.op_Implicit((Object)(object)scalingRenderTexture))
		{
			if (flag)
			{
				scalingRenderTexture = new RenderTexture(num, num2, 24);
				scalingRenderTexture.autoGenerateMips = false;
				((Texture)scalingRenderTexture).filterMode = (FilterMode)((double)resolutionScale > 1.0);
			}
			else
			{
				Object.Destroy((Object)(object)scalingRenderTexture);
				scalingRenderTexture = null;
			}
		}
		if (flag)
		{
			camera.targetTexture = scalingRenderTexture;
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)scalingRenderTexture))
		{
			Object.Destroy((Object)(object)scalingRenderTexture);
			scalingRenderTexture = null;
		}
	}
}
