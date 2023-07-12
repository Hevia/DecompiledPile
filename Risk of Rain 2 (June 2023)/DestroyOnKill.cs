using RoR2;
using UnityEngine;

public class DestroyOnKill : MonoBehaviour, IOnKilledServerReceiver
{
	public GameObject effectPrefab;

	public void OnKilledServer(DamageReport damageReport)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(effectPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
