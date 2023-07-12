using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

internal class DeathZone : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			HealthComponent component = ((Component)other).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.position = ((Component)other).transform.position;
				damageInfo.attacker = null;
				damageInfo.inflictor = ((Component)this).gameObject;
				damageInfo.damage = 999999f;
				component.TakeDamage(damageInfo);
			}
		}
	}
}
