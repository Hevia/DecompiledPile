using System;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class LineBetweenTransforms : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The list of transforms whose positions will drive the vertex positions of the sibling LineRenderer component.")]
	[FormerlySerializedAs("transformNodes")]
	private Transform[] _transformNodes = Array.Empty<Transform>();

	private LineRenderer lineRenderer;

	private Vector3[] vertexList = Array.Empty<Vector3>();

	public Transform[] transformNodes
	{
		get
		{
			return _transformNodes;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_transformNodes = value;
			UpdateVertexBufferSize();
		}
	}

	private void PushPositionsToLineRenderer()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = vertexList;
		Transform[] array2 = transformNodes;
		for (int i = 0; i < array.Length; i++)
		{
			Transform val = array2[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				array[i] = val.position;
			}
		}
		lineRenderer.SetPositions(array);
	}

	private void UpdateVertexBufferSize()
	{
		Array.Resize(ref vertexList, transformNodes.Length);
	}

	private void Awake()
	{
		lineRenderer = ((Component)this).GetComponent<LineRenderer>();
		UpdateVertexBufferSize();
	}

	private void LateUpdate()
	{
		PushPositionsToLineRenderer();
	}
}
