using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class GenericSceneSpawnPoint : MonoBehaviour
{
	public GameObject networkedObjectPrefab;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(Object.Instantiate<GameObject>(networkedObjectPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation));
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
