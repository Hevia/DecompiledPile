using UnityEngine;

public class ScaleLineRenderer : MonoBehaviour
{
	private LineRenderer line;

	public float scaleSize = 1f;

	public Vector3[] positions;

	private void Start()
	{
		line = ((Component)this).GetComponent<LineRenderer>();
		SetScale();
	}

	private void Update()
	{
	}

	private void SetScale()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		line.SetPosition(0, positions[0]);
		line.SetPosition(1, positions[1]);
		((Renderer)line).material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(positions[0], positions[1]) * scaleSize, 1f));
	}
}
