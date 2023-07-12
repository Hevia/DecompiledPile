using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab;

public class ChargeGauntlet : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public SkillDef skillDefToReplaceAtStocksEmpty;

	[SerializeField]
	public SkillDef nextSkillDef;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(enterSoundString, base.gameObject);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			Transform val = modelChildLocator.FindChild(muzzleName) ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
				chargeEffectInstance.transform.parent = val;
				ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.newDuration = duration;
				}
			}
		}
		PhasedInventorySetter component2 = GetComponent<PhasedInventorySetter>();
		if (Object.op_Implicit((Object)(object)component2) && NetworkServer.active)
		{
			component2.AdvancePhase();
		}
		if (Object.op_Implicit((Object)(object)nextSkillDef))
		{
			GenericSkill genericSkill = base.skillLocator.FindSkillByDef(skillDefToReplaceAtStocksEmpty);
			if (Object.op_Implicit((Object)(object)genericSkill) && genericSkill.stock == 0)
			{
				genericSkill.SetBaseSkill(nextSkillDef);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance);
			outer.SetNextState(new ChannelGauntlet());
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}