using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class InstantiatePrefabBehavior : MonoBehaviour
{
	[Tooltip("The prefab to instantiate.")]
	public GameObject prefab;

	[Tooltip("The object upon which the prefab will be positioned.")]
	public Transform targetTransform;

	[Tooltip("The transform upon which to instantiate the prefab.")]
	public bool copyTargetRotation;

	[Tooltip("Whether or not to parent the instantiated prefab to the specified transform.")]
	public bool parentToTarget;

	[Tooltip("Whether or not this is a networked prefab. If so, this will only run on the server, and will be spawned over the network.")]
	public bool networkedPrefab;

	[Tooltip("Whether or not to instantiate this prefab. If so, this will only run on the server, and will be spawned over the network.")]
	public bool instantiateOnStart = true;

	public void Start()
	{
		if (instantiateOnStart)
		{
			InstantiatePrefab();
		}
	}

	public void InstantiatePrefab()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!networkedPrefab || NetworkServer.active)
		{
			Vector3 val = (Object.op_Implicit((Object)(object)targetTransform) ? targetTransform.position : Vector3.zero);
			Quaternion val2 = (copyTargetRotation ? targetTransform.rotation : Quaternion.identity);
			Transform val3 = (parentToTarget ? targetTransform : null);
			GameObject val4 = Object.Instantiate<GameObject>(prefab, val, val2, val3);
			if (networkedPrefab)
			{
				NetworkServer.Spawn(val4);
			}
		}
	}
}
