using UnityEngine;

namespace RoR2;

public class DirectorPlacementRule
{
	public enum PlacementMode
	{
		Direct,
		Approximate,
		ApproximateSimple,
		NearestNode,
		Random,
		RandomNormalized
	}

	public Transform spawnOnTarget;

	public Vector3 position;

	public PlacementMode placementMode;

	public float minDistance;

	public float maxDistance;

	public bool preventOverhead;

	public Vector3 targetPosition
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)spawnOnTarget))
			{
				return position;
			}
			return spawnOnTarget.position;
		}
	}
}
