using UnityEngine;

namespace RoR2.Projectile;

public class ProjectileGhostCluster : MonoBehaviour
{
	public GameObject ghostClusterPrefab;

	public int clusterCount;

	public bool distributeEvenly;

	public float clusterDistance;

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / (Mathf.Log((float)clusterCount, 4f) + 1f);
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < clusterCount; i++)
		{
			Vector3 val = ((!distributeEvenly) ? (Random.insideUnitSphere * clusterDistance) : Vector3.zero);
			GameObject obj = Object.Instantiate<GameObject>(ghostClusterPrefab, position + val, Quaternion.identity, ((Component)this).transform);
			obj.transform.localScale = Vector3.one / (Mathf.Log((float)clusterCount, 4f) + 1f);
			TrailRenderer component = obj.GetComponent<TrailRenderer>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.widthMultiplier *= num;
			}
		}
	}

	private void Update()
	{
	}
}
