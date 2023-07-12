using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LunarWisp;

public class DeathState : GenericCharacterDeath
{
	public static GameObject deathEffectPrefab;

	public static string deathEffectMuzzleName;

	public static float velocityMagnitude;

	public static float explosionForce;

	private LunarWispFXController FXController;

	protected override bool shouldAutoDestroy => false;

	public override void OnEnter()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		LunarWispFXController component = ((Component)base.characterBody).GetComponent<LunarWispFXController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.TurnOffFX();
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			RagdollController component2 = ((Component)modelTransform).GetComponent<RagdollController>();
			Rigidbody component3 = GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component3))
			{
				component2.BeginRagdoll(component3.velocity * velocityMagnitude);
			}
			ExplodeRigidbodiesOnStart component4 = ((Component)modelTransform).GetComponent<ExplodeRigidbodiesOnStart>();
			if (Object.op_Implicit((Object)(object)component4))
			{
				component4.force = explosionForce;
				((Behaviour)component4).enabled = true;
			}
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			base.modelLocator.autoUpdateModelTransform = false;
		}
		((Component)FindModelChild("StandableSurface")).gameObject.SetActive(false);
		if (NetworkServer.active)
		{
			EffectData effectData = new EffectData
			{
				origin = FindModelChild(deathEffectMuzzleName).position
			};
			EffectManager.SpawnEffect(deathEffectPrefab, effectData, transmit: true);
			DestroyBodyAsapServer();
		}
	}
}
