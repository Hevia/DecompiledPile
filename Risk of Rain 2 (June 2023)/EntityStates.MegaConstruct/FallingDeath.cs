using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MegaConstruct;

public class FallingDeath : GenericCharacterDeath
{
	public static float deathDelay;

	public static GameObject enterEffectPrefab;

	public static GameObject deathEffectPrefab;

	public static float explosionForce;

	public static string standableSurfaceChildName;

	private bool hasDied;

	public override void OnEnter()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		EffectManager.SimpleImpactEffect(enterEffectPrefab, base.characterBody.corePosition, Vector3.up, transmit: true);
		MasterSpawnSlotController component = GetComponent<MasterSpawnSlotController>();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)component))
		{
			component.KillAll();
		}
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			Transform val = modelChildLocator.FindChild(standableSurfaceChildName);
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(false);
			}
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			Rigidbody component = GetComponent<Rigidbody>();
			RagdollController component2 = ((Component)modelTransform).GetComponent<RagdollController>();
			if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component))
			{
				component2.BeginRagdoll(component.velocity);
			}
			ExplodeRigidbodiesOnStart component3 = ((Component)modelTransform).GetComponent<ExplodeRigidbodiesOnStart>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.force = explosionForce;
				((Behaviour)component3).enabled = true;
			}
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			((Behaviour)modelAnimator).enabled = false;
		}
		base.OnExit();
	}
}
