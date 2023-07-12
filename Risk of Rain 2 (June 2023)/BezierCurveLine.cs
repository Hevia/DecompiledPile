using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BezierCurveLine : MonoBehaviour
{
	private Vector3[] vertexList = Array.Empty<Vector3>();

	private Vector3 p0 = Vector3.zero;

	public Vector3 v0 = Vector3.zero;

	public Vector3 p1 = Vector3.zero;

	public Vector3 v1 = Vector3.zero;

	public Transform endTransform;

	public bool animateBezierWind;

	public Vector3 windMagnitude;

	public Vector3 windFrequency;

	private Vector3 windPhaseShift;

	private Vector3 lastWind;

	private Vector3 finalv0;

	private Vector3 finalv1;

	private float windTime;

	public LineRenderer lineRenderer { get; private set; }

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		lineRenderer = ((Component)this).GetComponent<LineRenderer>();
		windPhaseShift = Random.insideUnitSphere * 360f;
		Array.Resize(ref vertexList, lineRenderer.positionCount + 1);
		UpdateBezier(0f);
	}

	public void OnEnable()
	{
		Array.Resize(ref vertexList, lineRenderer.positionCount + 1);
	}

	private void LateUpdate()
	{
		UpdateBezier(Time.deltaTime);
	}

	public void UpdateBezier(float deltaTime)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		windTime += deltaTime;
		p0 = ((Component)this).transform.position;
		if (Object.op_Implicit((Object)(object)endTransform))
		{
			p1 = endTransform.position;
		}
		if (animateBezierWind)
		{
			finalv0 = v0 + new Vector3(Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.x) * windFrequency.x) * windMagnitude.x, Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.y) * windFrequency.y) * windMagnitude.y, Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.z) * windFrequency.z) * windMagnitude.z);
			finalv1 = v1 + new Vector3(Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.x + p1.x) * windFrequency.x) * windMagnitude.x, Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.y + p1.z) * windFrequency.y) * windMagnitude.y, Mathf.Sin(MathF.PI / 180f * (windTime * 360f + windPhaseShift.z + p1.y) * windFrequency.z) * windMagnitude.z);
		}
		else
		{
			finalv0 = v0;
			finalv1 = v1;
		}
		for (int i = 0; i < vertexList.Length; i++)
		{
			float t = (float)i / (float)(vertexList.Length - 2);
			vertexList[i] = EvaluateBezier(t);
		}
		lineRenderer.SetPositions(vertexList);
	}

	private Vector3 EvaluateBezier(float t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Lerp(p0, p0 + finalv0, t);
		Vector3 val2 = Vector3.Lerp(p1, p1 + finalv1, 1f - t);
		return Vector3.Lerp(val, val2, t);
	}
}
