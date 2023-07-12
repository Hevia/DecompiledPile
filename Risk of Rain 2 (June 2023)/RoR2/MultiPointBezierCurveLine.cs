using System;
using UnityEngine;

namespace RoR2;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class MultiPointBezierCurveLine : MonoBehaviour
{
	[Serializable]
	public struct Vertex
	{
		public Transform vertexTransform;

		public Vector3 position;

		public Vector3 localVelocity;
	}

	public Vertex[] vertexList;

	public Vector3[] linePositionList;

	[HideInInspector]
	public LineRenderer lineRenderer;

	private void Start()
	{
		lineRenderer = ((Component)this).GetComponent<LineRenderer>();
	}

	private void LateUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < linePositionList.Length; i++)
		{
			float globalT = (float)i / (float)(linePositionList.Length - 1);
			linePositionList[i] = EvaluateBezier(globalT);
		}
		lineRenderer.SetPositions(linePositionList);
	}

	private Vector3 EvaluateBezier(float globalT)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		int num = vertexList.Length - 1;
		int num2;
		int num3 = Mathf.Min((num2 = Mathf.FloorToInt((float)num * globalT)) + 1, num);
		Vertex vertex = vertexList[num2];
		Vertex vertex2 = vertexList[num3];
		Vector3 val = (Object.op_Implicit((Object)(object)vertex.vertexTransform) ? vertex.vertexTransform.position : vertex.position);
		Vector3 val2 = (Object.op_Implicit((Object)(object)vertex2.vertexTransform) ? vertex2.vertexTransform.position : vertex2.position);
		Vector3 val3 = (Object.op_Implicit((Object)(object)vertex.vertexTransform) ? vertex.vertexTransform.TransformVector(vertex.localVelocity) : vertex.localVelocity);
		Vector3 val4 = (Object.op_Implicit((Object)(object)vertex2.vertexTransform) ? vertex2.vertexTransform.TransformVector(vertex2.localVelocity) : vertex2.localVelocity);
		if (num2 == num3)
		{
			return val;
		}
		float inMin = (float)num2 / (float)num;
		float inMax = (float)num3 / (float)num;
		float num4 = Util.Remap(globalT, inMin, inMax, 0f, 1f);
		Vector3 val5 = Vector3.Lerp(val, val + val3, num4);
		Vector3 val6 = Vector3.Lerp(val2, val2 + val4, 1f - num4);
		return Vector3.Lerp(val5, val6, num4);
	}
}
