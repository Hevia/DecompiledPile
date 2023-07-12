using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.EngiWeapon;

public class EngiOtherShield : BaseState
{
	public CharacterBody target;

	public float minimumDuration;

	private Indicator indicator;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)target))
		{
			indicator = new Indicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiShieldRetractIndicator"));
			indicator.active = true;
			indicator.targetTransform = Util.GetCoreTransform(((Component)target).gameObject);
			target.AddBuff(RoR2Content.Buffs.EngiShield);
			target.RecalculateStats();
			HealthComponent component = ((Component)target).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.RechargeShieldFull();
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!Object.op_Implicit((Object)(object)target) || !base.characterBody.healthComponent.alive)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		base.skillLocator.utility = base.skillLocator.FindSkill("GiveShield");
		if (NetworkServer.active && Object.op_Implicit((Object)(object)target))
		{
			target.RemoveBuff(RoR2Content.Buffs.EngiShield);
		}
		if (base.isAuthority)
		{
			base.skillLocator.utility.RemoveAllStocks();
		}
		if (indicator != null)
		{
			indicator.active = false;
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge >= minimumDuration))
		{
			return InterruptPriority.PrioritySkill;
		}
		return InterruptPriority.Skill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(((Component)target).gameObject);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		GameObject val = reader.ReadGameObject();
		if (Object.op_Implicit((Object)(object)val))
		{
			target = val.GetComponent<CharacterBody>();
		}
	}
}
