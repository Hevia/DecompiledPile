using System;
using UnityEngine;

[ExecuteAlways]
public class BeamPointsFromTransforms : MonoBehaviour
{
	[Tooltip("Line Renderer to set the positions of.")]
	public LineRenderer target;

	[SerializeField]
	[Tooltip("Transforms to use as the points for the line renderer.")]
	private Transform[] pointTransforms = Array.Empty<Transform>();

	private void Start()
	{
		UpdateBeamPositions();
	}

	private void Update()
	{
		UpdateBeamPositions();
	}

	private void UpdateBeamPositions()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		int num = pointTransforms.Length;
		target.positionCount = num;
		for (int i = 0; i < num; i++)
		{
			Transform val = pointTransforms[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				target.SetPosition(i, val.position);
			}
		}
	}
}
