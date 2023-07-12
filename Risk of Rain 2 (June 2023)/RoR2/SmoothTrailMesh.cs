using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class SmoothTrailMesh : MonoBehaviour
{
	[Serializable]
	private struct Point
	{
		public Vector3 vertex1;

		public Vector3 vertex2;

		public float time;
	}

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh mesh;

	public float timeStep = 1f / 180f;

	public float width = 1f;

	public Material[] sharedMaterials;

	public float trailLifetime = 1f;

	public bool fadeVertexAlpha = true;

	private Vector3 previousPosition;

	private Vector3 previousUp;

	private float previousTime;

	private Queue<Point> pointsQueue = new Queue<Point>();

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		mesh = new Mesh();
		mesh.MarkDynamic();
		GameObject val = new GameObject("SmoothTrailMeshRenderer");
		meshFilter = val.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		meshRenderer = val.AddComponent<MeshRenderer>();
		((Renderer)meshRenderer).sharedMaterials = sharedMaterials;
	}

	private void AddCurrentPoint()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = ((Component)this).transform.up * width * 0.5f;
		pointsQueue.Enqueue(new Point
		{
			vertex1 = position + val,
			vertex2 = position - val,
			time = time
		});
	}

	private void OnEnable()
	{
		AddCurrentPoint();
	}

	private void OnDisable()
	{
		pointsQueue.Clear();
		mesh.Clear();
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)meshFilter))
		{
			meshFilter.mesh = null;
			Object.Destroy((Object)(object)((Component)meshFilter).gameObject);
		}
		Object.Destroy((Object)(object)mesh);
	}

	private void Simulate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = ((Component)this).transform.up * width * 0.5f;
		float num = time - previousTime;
		if (num > 0f)
		{
			float num2 = 1f / num;
			for (float num3 = previousTime; num3 <= time; num3 += timeStep)
			{
				float num4 = (num3 - previousTime) * num2;
				Vector3 val2 = Vector3.LerpUnclamped(previousPosition, position, num4);
				Vector3 val3 = Vector3.SlerpUnclamped(previousUp, val, num4);
				pointsQueue.Enqueue(new Point
				{
					vertex1 = val2 + val3,
					vertex2 = val2 - val3,
					time = num3
				});
			}
		}
		float num5 = time - trailLifetime;
		while (pointsQueue.Count > 0 && pointsQueue.Peek().time < num5)
		{
			pointsQueue.Dequeue();
		}
		previousTime = time;
		previousPosition = position;
		previousUp = val;
	}

	private void LateUpdate()
	{
		Simulate();
		GenerateMesh();
	}

	private void GenerateMesh()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[pointsQueue.Count * 2];
		Vector2[] array2 = (Vector2[])(object)new Vector2[pointsQueue.Count * 2];
		Color[] array3 = (Color[])(object)new Color[pointsQueue.Count * 2];
		_ = 1f / (float)pointsQueue.Count;
		int num = 0;
		if (pointsQueue.Count > 0)
		{
			float time = pointsQueue.Peek().time;
			float time2 = Time.time;
			float num2 = time2 - time;
			float num3 = 1f / num2;
			foreach (Point item in pointsQueue)
			{
				float num4 = (time2 - item.time) * num3;
				array[num] = item.vertex1;
				array2[num] = new Vector2(1f, num4);
				array3[num] = new Color(1f, 1f, 1f, fadeVertexAlpha ? (1f - num4) : 1f);
				num++;
				array[num] = item.vertex2;
				array2[num] = new Vector2(0f, num4);
				num++;
			}
		}
		int num5 = pointsQueue.Count - 1;
		int[] array4 = new int[num5 * 2 * 3];
		int num6 = 0;
		int num7 = 0;
		for (int i = 0; i < num5; i++)
		{
			array4[num6] = num7;
			array4[num6 + 1] = num7 + 1;
			array4[num6 + 2] = num7 + 2;
			array4[num6 + 3] = num7 + 3;
			array4[num6 + 4] = num7 + 1;
			array4[num6 + 5] = num7 + 2;
			num6 += 6;
			num7 += 2;
		}
		mesh.Clear();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array4;
		mesh.colors = array3;
		mesh.RecalculateBounds();
		mesh.UploadMeshData(false);
	}
}
