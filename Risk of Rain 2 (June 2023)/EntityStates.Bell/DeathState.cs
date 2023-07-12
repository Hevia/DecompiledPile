using RoR2;
using UnityEngine;

namespace EntityStates.Bell;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialEffect;

	public static float initialEffectScale;

	public static float velocityMagnitude;

	public static float explosionForce;

	protected override bool shouldAutoDestroy => false;

	public override void OnEnter()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			if (Object.op_Implicit((Object)(object)((Component)modelTransform).GetComponent<ChildLocator>()) && Object.op_Implicit((Object)(object)initialEffect))
			{
				EffectManager.SpawnEffect(initialEffect, new EffectData
				{
					origin = base.transform.position,
					scale = initialEffectScale
				}, transmit: false);
			}
			RagdollController component = ((Component)modelTransform).GetComponent<RagdollController>();
			Rigidbody component2 = GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2))
			{
				component.BeginRagdoll(component2.velocity * velocityMagnitude);
			}
			ExplodeRigidbodiesOnStart component3 = ((Component)modelTransform).GetComponent<ExplodeRigidbodiesOnStart>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.force = explosionForce;
				((Behaviour)component3).enabled = true;
			}
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			base.modelLocator.autoUpdateModelTransform = false;
		}
		DestroyBodyAsapServer();
	}
}
