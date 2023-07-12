using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AffixEarthHealer;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialExplosion;

	public static float duration;

	public static string enterSoundString;

	public override void OnEnter()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)initialExplosion))
		{
			EffectManager.SimpleEffect(initialExplosion, base.transform.position, base.transform.rotation, transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge > duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
