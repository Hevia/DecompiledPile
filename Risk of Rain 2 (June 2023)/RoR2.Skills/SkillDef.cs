using System;
using EntityStates;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2.Skills;

[CreateAssetMenu(menuName = "RoR2/SkillDef/Generic")]
public class SkillDef : ScriptableObject
{
	public class BaseSkillInstanceData
	{
	}

	[Header("Skill Identifier")]
	[Tooltip("The name of the skill. This is mainly for purposes of identification in the inspector and currently has no direct effect.")]
	public string skillName = "";

	[Tooltip("The language token with the name of this skill.")]
	[Header("User-Facing Info")]
	public string skillNameToken = "";

	[Tooltip("The language token with the description of this skill.")]
	public string skillDescriptionToken = "";

	[Tooltip("Extra tooltips when hovered over in character select. Currently only used in that area!")]
	public string[] keywordTokens;

	[Tooltip("The icon to display for this skill.")]
	[ShowThumbnail]
	public Sprite icon;

	[Tooltip("The state machine this skill operates upon.")]
	[Header("State Machine Parameters")]
	public string activationStateMachineName;

	[Tooltip("The state to enter when this skill is activated.")]
	public SerializableEntityStateType activationState;

	[Tooltip("The priority of this skill.")]
	public InterruptPriority interruptPriority = InterruptPriority.Skill;

	[Header("Stock and Cooldown")]
	[Tooltip("How long it takes for this skill to recharge after being used.")]
	public float baseRechargeInterval = 1f;

	[Tooltip("Maximum number of charges this skill can carry.")]
	public int baseMaxStock = 1;

	[Tooltip("How much stock to restore on a recharge.")]
	public int rechargeStock = 1;

	[Tooltip("How much stock is required to activate this skill.")]
	public int requiredStock = 1;

	[Tooltip("How much stock to deduct when the skill is activated.")]
	public int stockToConsume = 1;

	[Header("Optional Parameters, Stock")]
	[Tooltip("Whether or not it resets any progress on cooldowns.")]
	[FormerlySerializedAs("isBullets")]
	public bool resetCooldownTimerOnUse;

	[Tooltip("Whether or not to fully restock this skill when it's assigned.")]
	public bool fullRestockOnAssign = true;

	[Tooltip("Whether or not this skill can hold past it's maximum stock.")]
	public bool dontAllowPastMaxStocks;

	[Tooltip("Whether or not the cooldown waits until it leaves the set state")]
	public bool beginSkillCooldownOnSkillEnd;

	[Tooltip("Whether or not activating the skill forces off sprinting.")]
	[FormerlySerializedAs("noSprint")]
	[Header("Optional Parameters, Sprinting")]
	public bool cancelSprintingOnActivation = true;

	[Tooltip("Whether or not this skill is considered 'mobility'. Currently just forces sprint.")]
	[FormerlySerializedAs("mobilitySkill")]
	public bool forceSprintDuringState;

	[Tooltip("Whether or not sprinting sets the skill's state to be reset.")]
	public bool canceledFromSprinting;

	[Tooltip("Whether or not this is considered a combat skill.")]
	[Header("Optional Parameters, Misc")]
	public bool isCombatSkill = true;

	[Tooltip("The skill can't be activated if the key is held.")]
	public bool mustKeyPress;

	[Obsolete("Accessing UnityEngine.Object.Name causes allocations on read. Look up the name from the catalog instead. If absolutely necessary to perform direct access, cast to ScriptableObject first.")]
	public string name => null;

	public int skillIndex { get; set; }

	public virtual BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return null;
	}

	public virtual void OnUnassigned([NotNull] GenericSkill skillSlot)
	{
	}

	public virtual Sprite GetCurrentIcon([NotNull] GenericSkill skillSlot)
	{
		return icon;
	}

	public virtual string GetCurrentNameToken([NotNull] GenericSkill skillSlot)
	{
		return skillNameToken;
	}

	public virtual string GetCurrentDescriptionToken([NotNull] GenericSkill skillSlot)
	{
		return skillDescriptionToken;
	}

	protected bool HasRequiredStockAndDelay([NotNull] GenericSkill skillSlot)
	{
		return skillSlot.stock >= requiredStock;
	}

	public virtual bool CanExecute([NotNull] GenericSkill skillSlot)
	{
		if (HasRequiredStockAndDelay(skillSlot) && IsReady(skillSlot) && Object.op_Implicit((Object)(object)skillSlot.stateMachine) && !skillSlot.stateMachine.HasPendingState())
		{
			return skillSlot.stateMachine.CanInterruptState(interruptPriority);
		}
		return false;
	}

	public virtual bool IsReady([NotNull] GenericSkill skillSlot)
	{
		return HasRequiredStockAndDelay(skillSlot);
	}

	protected virtual EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
	{
		EntityState entityState = EntityStateCatalog.InstantiateState(activationState);
		if (entityState is ISkillState skillState)
		{
			skillState.activatorSkillSlot = skillSlot;
		}
		return entityState;
	}

	public virtual void OnExecute([NotNull] GenericSkill skillSlot)
	{
		skillSlot.stateMachine.SetInterruptState(InstantiateNextState(skillSlot), interruptPriority);
		if (cancelSprintingOnActivation)
		{
			skillSlot.characterBody.isSprinting = false;
		}
		skillSlot.stock -= stockToConsume;
		if (resetCooldownTimerOnUse)
		{
			skillSlot.rechargeStopwatch = 0f;
		}
		if (Object.op_Implicit((Object)(object)skillSlot.characterBody))
		{
			skillSlot.characterBody.OnSkillActivated(skillSlot);
		}
	}

	public virtual void OnFixedUpdate([NotNull] GenericSkill skillSlot)
	{
		skillSlot.RunRecharge(Time.fixedDeltaTime);
		if (((canceledFromSprinting && skillSlot.characterBody.isSprinting) || forceSprintDuringState) && skillSlot.stateMachine.state.GetType() == activationState.stateType)
		{
			if (canceledFromSprinting && skillSlot.characterBody.isSprinting)
			{
				skillSlot.stateMachine.SetNextStateToMain();
			}
			if (forceSprintDuringState)
			{
				skillSlot.characterBody.isSprinting = true;
			}
		}
	}

	public bool IsAlreadyInState([NotNull] GenericSkill skillSlot)
	{
		return skillSlot?.stateMachine.state.GetType() == activationState.stateType;
	}

	public virtual int GetMaxStock([NotNull] GenericSkill skillSlot)
	{
		return baseMaxStock;
	}

	public virtual int GetRechargeStock([NotNull] GenericSkill skillSlot)
	{
		return rechargeStock;
	}

	public virtual float GetRechargeInterval([NotNull] GenericSkill skillSlot)
	{
		return baseRechargeInterval;
	}
}
