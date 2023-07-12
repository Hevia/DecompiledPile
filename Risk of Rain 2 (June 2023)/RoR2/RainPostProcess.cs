using UnityEngine;

namespace RoR2;

[ExecuteInEditMode]
public class RainPostProcess : MonoBehaviour
{
	public Material mat;

	private RenderTexture renderTex;

	private void Start()
	{
		((Component)this).GetComponent<Camera>().depthTextureMode = (DepthTextureMode)1;
	}

	private void Update()
	{
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit((Texture)(object)source, destination, mat);
	}
}
