using RoR2;
using UnityEngine;

namespace EntityStates.VoidInfestor;

public class Spawn : BaseState
{
	public static GameObject spawnEffectPrefab;

	public static float velocityStrength;

	public static float spread;

	public override void OnEnter()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)spawnEffectPrefab))
		{
			EffectManager.SimpleImpactEffect(spawnEffectPrefab, base.characterBody.corePosition, Vector3.up, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			Vector3 val = (Vector3.up + Random.onUnitSphere * spread) * velocityStrength;
			base.characterMotor.ApplyForce(val, alwaysApply: true, disableAirControlUntilCollision: true);
			base.characterDirection.forward = val;
		}
		PlayAnimation("Base", "Spawn");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor) && base.characterMotor.isGrounded && base.fixedAge > 0.1f)
		{
			outer.SetNextStateToMain();
		}
	}
}
