using UnityEngine;

namespace RoR2.Navigation;

[RequireComponent(typeof(MapNode))]
public class MapNodeLink : MonoBehaviour
{
	public MapNode other;

	public float minJumpHeight;

	[Tooltip("The gate name associated with this link. If the named gate is closed, this link will not be used in pathfinding.")]
	public string gateName = "";

	public GameObject[] objectsToEnableDuringTest;

	public GameObject[] objectsToDisableDuringTest;

	private void OnValidate()
	{
		if ((Object)(object)other == (Object)(object)this)
		{
			Debug.LogWarning((object)"Map node link cannot link a node to itself.");
			other = null;
		}
		if (Object.op_Implicit((Object)(object)other) && (Object)(object)((Component)other).GetComponentInParent<MapNodeGroup>() != (Object)(object)((Component)this).GetComponentInParent<MapNodeGroup>())
		{
			Debug.LogWarning((object)"Map node link cannot link to a node in a separate node group.");
			other = null;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)other))
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 position2 = ((Component)other).transform.position;
			Vector3 val = (position + position2) * 0.5f;
			Color yellow = Color.yellow;
			yellow.a = 0.5f;
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(position, val);
			Gizmos.color = yellow;
			Gizmos.DrawLine(val, position2);
		}
	}
}
