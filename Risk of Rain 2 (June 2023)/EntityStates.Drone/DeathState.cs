using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drone;

public class DeathState : GenericCharacterDeath
{
	public class RigidbodyCollisionListener : MonoBehaviour
	{
		public DeathState deathState;

		private void OnCollisionEnter(Collision collision)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			DeathState obj = deathState;
			ContactPoint contact = collision.GetContact(0);
			obj.OnImpactServer(((ContactPoint)(ref contact)).point);
			deathState.Explode();
		}
	}

	[SerializeField]
	public GameObject initialExplosionEffect;

	[SerializeField]
	public GameObject deathExplosionEffect;

	[SerializeField]
	public string initialSoundString;

	[SerializeField]
	public string deathSoundString;

	[SerializeField]
	public float deathEffectRadius;

	[SerializeField]
	public float forceAmount = 20f;

	[SerializeField]
	public float deathDuration = 2f;

	[SerializeField]
	public bool destroyOnImpact;

	private RigidbodyCollisionListener rigidbodyCollisionListener;

	public override void OnEnter()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(initialSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			((Behaviour)base.rigidbodyMotor.forcePID).enabled = false;
			base.rigidbodyMotor.rigid.useGravity = true;
			base.rigidbodyMotor.rigid.AddForce(Vector3.up * forceAmount, (ForceMode)0);
			base.rigidbodyMotor.rigid.collisionDetectionMode = (CollisionDetectionMode)1;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyDirection))
		{
			((Behaviour)base.rigidbodyDirection).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)initialExplosionEffect))
		{
			EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
			{
				origin = base.characterBody.corePosition,
				scale = base.characterBody.radius + deathEffectRadius
			}, transmit: false);
		}
		if (base.isAuthority && destroyOnImpact)
		{
			rigidbodyCollisionListener = base.gameObject.AddComponent<RigidbodyCollisionListener>();
			rigidbodyCollisionListener.deathState = this;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge > deathDuration)
		{
			Explode();
		}
	}

	public void Explode()
	{
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public virtual void OnImpactServer(Vector3 contactPoint)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		string bodyName = BodyCatalog.GetBodyName(base.characterBody.bodyIndex);
		bodyName = bodyName.Replace("Body", "");
		bodyName = "iscBroken" + bodyName;
		SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/" + bodyName);
		if (!((Object)(object)spawnCard != (Object)null))
		{
			return;
		}
		DirectorPlacementRule placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Direct,
			position = contactPoint
		};
		GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, new Xoroshiro128Plus(0uL)));
		if (Object.op_Implicit((Object)(object)val))
		{
			PurchaseInteraction component = val.GetComponent<PurchaseInteraction>();
			if (Object.op_Implicit((Object)(object)component) && component.costType == CostTypeIndex.Money)
			{
				component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
			}
		}
	}

	public override void OnExit()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)deathExplosionEffect))
		{
			EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
			{
				origin = base.characterBody.corePosition,
				scale = base.characterBody.radius + deathEffectRadius
			}, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)rigidbodyCollisionListener))
		{
			EntityState.Destroy((Object)(object)rigidbodyCollisionListener);
		}
		Util.PlaySound(deathSoundString, base.gameObject);
		base.OnExit();
	}
}
