using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/GameObjectFactory")]
public class GameObjectFactory : ScriptableObject
{
	public bool clearOnPerformInstantiate = true;

	private GameObject prefab;

	private Vector3 spawnPosition;

	private Quaternion spawnRotation;

	private bool isNetworkPrefab;

	private AnimationClip spawnModificationsClip;

	public void Reset()
	{
		Clear();
		clearOnPerformInstantiate = true;
	}

	public void Clear()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		prefab = null;
		spawnPosition = Vector3.zero;
		spawnRotation = Quaternion.identity;
		isNetworkPrefab = false;
		spawnModificationsClip = null;
	}

	public void SetPrefab(GameObject newPrefab)
	{
		prefab = newPrefab;
		GameObject obj = prefab;
		isNetworkPrefab = Object.op_Implicit((Object)(object)((obj != null) ? obj.GetComponent<NetworkIdentity>() : null));
	}

	public void SetSpawnLocation(Transform newSpawnLocation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		spawnPosition = newSpawnLocation.position;
		spawnRotation = newSpawnLocation.rotation;
	}

	public void SetSpawnModificationsClip(AnimationClip newSpawnModificationsClip)
	{
		spawnModificationsClip = newSpawnModificationsClip;
	}

	public void PerformInstantiate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)prefab))
		{
			GameObject val = Object.Instantiate<GameObject>(prefab, spawnPosition, spawnRotation);
			if (Object.op_Implicit((Object)(object)spawnModificationsClip))
			{
				Animator obj = val.AddComponent<Animator>();
				spawnModificationsClip.SampleAnimation(val, 0f);
				Object.Destroy((Object)(object)obj);
			}
			val.SetActive(true);
			if (isNetworkPrefab && NetworkServer.active)
			{
				NetworkServer.Spawn(val);
			}
			if (clearOnPerformInstantiate)
			{
				Clear();
			}
		}
	}
}
