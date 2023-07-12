using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Wisp1Monster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialExplosion;

	public override void OnEnter()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelTransform).gameObject);
			}
		}
		if (NetworkServer.active)
		{
			EffectManager.SimpleEffect(initialExplosion, base.transform.position, base.transform.rotation, transmit: true);
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
