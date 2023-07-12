using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.FlyingVermin;

public class FallingDeath : GenericCharacterDeath
{
	public static float initialVerticalVelocity;

	public static float deathDelay;

	public static GameObject deathEffectPrefab;

	private bool hasDied;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity.y = initialVerticalVelocity;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge > deathDelay && NetworkServer.active && !hasDied)
		{
			hasDied = true;
			EffectManager.SimpleImpactEffect(deathEffectPrefab, base.characterBody.corePosition, Vector3.up, transmit: true);
			DestroyBodyAsapServer();
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
