using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drone;

public class MegaDroneDeathState : GenericCharacterDeath
{
	public static string initialSoundString;

	public static GameObject initialEffect;

	public static float initialEffectScale;

	public static float velocityMagnitude;

	public static float explosionForce;

	public override void OnEnter()
	{
		if (NetworkServer.active)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		base.OnExit();
		Util.PlaySound(initialSoundString, base.gameObject);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform) && NetworkServer.active)
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Component)component.FindChild("LeftJet")).gameObject.SetActive(false);
				((Component)component.FindChild("RightJet")).gameObject.SetActive(false);
				if (Object.op_Implicit((Object)(object)initialEffect))
				{
					EffectManager.SpawnEffect(initialEffect, new EffectData
					{
						origin = base.transform.position,
						scale = initialEffectScale
					}, transmit: true);
				}
			}
		}
		Rigidbody component2 = GetComponent<Rigidbody>();
		RagdollController component3 = ((Component)modelTransform).GetComponent<RagdollController>();
		if (Object.op_Implicit((Object)(object)component3) && Object.op_Implicit((Object)(object)component2))
		{
			component3.BeginRagdoll(component2.velocity * velocityMagnitude);
		}
		ExplodeRigidbodiesOnStart component4 = ((Component)modelTransform).GetComponent<ExplodeRigidbodiesOnStart>();
		if (Object.op_Implicit((Object)(object)component4))
		{
			component4.force = explosionForce;
			((Behaviour)component4).enabled = true;
		}
	}
}
