using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Captain.Weapon;

public class CallSupplyDropBase : BaseSkillState
{
	[SerializeField]
	public GameObject muzzleflashEffect;

	[SerializeField]
	public GameObject supplyDropPrefab;

	public static string muzzleString;

	public static float baseDuration;

	public static float impactDamageCoefficient;

	public static float impactDamageForce;

	public SetupSupplyDrop.PlacementInfo placementInfo;

	private float duration => baseDuration / attackSpeedStat;

	public override void OnEnter()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			placementInfo = SetupSupplyDrop.GetPlacementInfo(GetAimRay(), base.gameObject);
			if (placementInfo.ok)
			{
				base.activatorSkillSlot.DeductStock(1);
			}
		}
		if (placementInfo.ok)
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, muzzleString, transmit: false);
			base.characterBody.SetAimTimer(3f);
			PlayAnimation("Gesture, Override", "CallSupplyDrop", "CallSupplyDrop.playbackRate", duration);
			PlayAnimation("Gesture, Additive", "CallSupplyDrop", "CallSupplyDrop.playbackRate", duration);
			if (NetworkServer.active)
			{
				GameObject obj = Object.Instantiate<GameObject>(supplyDropPrefab, placementInfo.position, placementInfo.rotation);
				obj.GetComponent<TeamFilter>().teamIndex = base.teamComponent.teamIndex;
				obj.GetComponent<GenericOwnership>().ownerObject = base.gameObject;
				Deployable component = obj.GetComponent<Deployable>();
				if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)base.characterBody.master))
				{
					base.characterBody.master.AddDeployable(component, DeployableSlot.CaptainSupplyDrop);
				}
				ProjectileDamage component2 = obj.GetComponent<ProjectileDamage>();
				component2.crit = RollCrit();
				component2.damage = damageStat * impactDamageCoefficient;
				component2.damageColorIndex = DamageColorIndex.Default;
				component2.force = impactDamageForce;
				component2.damageType = DamageType.Generic;
				NetworkServer.Spawn(obj);
			}
		}
		else
		{
			PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
			PlayCrossfade("Gesture, Additive", "BufferEmpty", 0.1f);
		}
		EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Skillswap");
		if (Object.op_Implicit((Object)(object)entityStateMachine))
		{
			entityStateMachine.SetNextStateToMain();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		placementInfo.Serialize(writer);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		placementInfo.Deserialize(reader);
	}
}
