using UnityEngine;

[ExecuteInEditMode]
public class RenderDepth : MonoBehaviour
{
	private void OnEnable()
	{
		((Component)this).GetComponent<Camera>().depthTextureMode = (DepthTextureMode)2;
	}
}
