using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MinorConstruct;

public class DeathState : GenericCharacterDeath
{
	public static GameObject explosionPrefab;

	public override void OnEnter()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			EffectManager.SimpleEffect(explosionPrefab, base.transform.position, base.transform.rotation, transmit: true);
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
