using System;
using System.Collections.Generic;
using EntityStates;
using HG;
using JetBrains.Annotations;
using RoR2.Skills;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public sealed class GenericSkill : MonoBehaviour, ILifeBehavior
{
	public class SkillOverride : IEquatable<SkillOverride>
	{
		public readonly object source;

		public readonly SkillDef skillDef;

		public readonly SkillOverridePriority priority;

		public int stock;

		public float rechargeStopwatch;

		public SkillOverride(object source, SkillDef skillDef, SkillOverridePriority priority)
		{
			this.source = source;
			this.skillDef = skillDef;
			this.priority = priority;
			stock = 0;
			rechargeStopwatch = 0f;
		}

		public bool Equals(SkillOverride other)
		{
			if (object.Equals(source, other.source) && object.Equals(skillDef, other.skillDef))
			{
				return priority == other.priority;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SkillOverride other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((source != null) ? source.GetHashCode() : 0) * 397) ^ (((Object)(object)skillDef != (Object)null) ? ((object)skillDef).GetHashCode() : 0)) * 397) ^ (int)priority;
		}
	}

	public enum SkillOverridePriority
	{
		Default,
		Loadout,
		Upgrade,
		Replacement,
		Contextual,
		Network
	}

	public class SkillOverrideHandle : IDisposable
	{
		public readonly object source;

		public readonly SkillDef skill;

		public readonly GenericSkill skillSlot;

		public readonly SkillOverridePriority priority;

		public void Dispose()
		{
		}
	}

	public delegate void StateMachineResolver(GenericSkill genericSkill, SkillDef skillDef, ref EntityStateMachine targetStateMachine);

	[SerializeField]
	private SkillFamily _skillFamily;

	public string skillName;

	public bool hideInCharacterSelect;

	private static readonly List<EntityStateMachine> stateMachineLookupBuffer = new List<EntityStateMachine>();

	private SkillOverride[] skillOverrides = Array.Empty<SkillOverride>();

	private int currentSkillOverride = -1;

	private int bonusStockFromBody;

	private int baseStock;

	private float finalRechargeInterval;

	private float _cooldownScale = 1f;

	private float _flatCooldownReduction = 1f;

	private float baseRechargeStopwatch;

	[HideInInspector]
	public bool hasExecutedSuccessfully;

	public SkillDef skillDef { get; private set; }

	public SkillFamily skillFamily => _skillFamily;

	public SkillDef baseSkill { get; private set; }

	public string skillNameToken => skillDef.GetCurrentNameToken(this);

	public string skillDescriptionToken => skillDef.GetCurrentDescriptionToken(this);

	public float baseRechargeInterval => skillDef.GetRechargeInterval(this);

	public int rechargeStock => skillDef.GetRechargeStock(this);

	public bool beginSkillCooldownOnSkillEnd => skillDef.beginSkillCooldownOnSkillEnd;

	public SerializableEntityStateType activationState => skillDef.activationState;

	public InterruptPriority interruptPriority => skillDef.interruptPriority;

	public bool isCombatSkill => skillDef.isCombatSkill;

	public bool mustKeyPress => skillDef.mustKeyPress;

	public Sprite icon => skillDef.GetCurrentIcon(this);

	[CanBeNull]
	public EntityStateMachine stateMachine { get; private set; }

	[CanBeNull]
	public SkillDef.BaseSkillInstanceData skillInstanceData { get; set; }

	public CharacterBody characterBody { get; private set; }

	public SkillDef defaultSkillDef { get; private set; }

	public int maxStock { get; private set; } = 1;


	public int stock
	{
		get
		{
			if (currentSkillOverride >= 0 && currentSkillOverride < skillOverrides.Length)
			{
				return skillOverrides[currentSkillOverride].stock;
			}
			return baseStock;
		}
		set
		{
			if (currentSkillOverride >= 0 && currentSkillOverride < skillOverrides.Length)
			{
				skillOverrides[currentSkillOverride].stock = value;
			}
			else
			{
				baseStock = value;
			}
		}
	}

	public float cooldownScale
	{
		get
		{
			return _cooldownScale;
		}
		set
		{
			if (_cooldownScale != value)
			{
				_cooldownScale = value;
				RecalculateFinalRechargeInterval();
			}
		}
	}

	public float flatCooldownReduction
	{
		get
		{
			return _flatCooldownReduction;
		}
		set
		{
			if (_flatCooldownReduction != value)
			{
				_flatCooldownReduction = value;
				RecalculateFinalRechargeInterval();
			}
		}
	}

	public float rechargeStopwatch
	{
		get
		{
			if (currentSkillOverride >= 0 && currentSkillOverride < skillOverrides.Length)
			{
				return skillOverrides[currentSkillOverride].rechargeStopwatch;
			}
			return baseRechargeStopwatch;
		}
		set
		{
			if (currentSkillOverride >= 0 && currentSkillOverride < skillOverrides.Length)
			{
				skillOverrides[currentSkillOverride].rechargeStopwatch = value;
			}
			else
			{
				baseRechargeStopwatch = value;
			}
		}
	}

	public float cooldownRemaining
	{
		get
		{
			if (stock != maxStock && rechargeStock != 0)
			{
				return finalRechargeInterval - rechargeStopwatch;
			}
			return 0f;
		}
	}

	public event Action<GenericSkill> onSkillChanged;

	private event StateMachineResolver _customStateMachineResolver;

	public event StateMachineResolver customStateMachineResolver
	{
		add
		{
			_customStateMachineResolver += value;
			PickTargetStateMachine();
		}
		remove
		{
			_customStateMachineResolver -= value;
			PickTargetStateMachine();
		}
	}

	private int FindSkillOverrideIndex(ref SkillOverride skillOverride)
	{
		for (int i = 0; i < skillOverrides.Length; i++)
		{
			if (skillOverrides[i].Equals(skillOverride))
			{
				return i;
			}
		}
		return -1;
	}

	public void SetSkillOverride(object source, SkillDef skillDef, SkillOverridePriority priority)
	{
		SkillOverride skillOverride = new SkillOverride(source, skillDef, priority);
		if (FindSkillOverrideIndex(ref skillOverride) == -1)
		{
			ArrayUtils.ArrayAppend<SkillOverride>(ref skillOverrides, ref skillOverride);
			PickCurrentOverride();
		}
	}

	public void UnsetSkillOverride(object source, SkillDef skillDef, SkillOverridePriority priority)
	{
		SkillOverride skillOverride = new SkillOverride(source, skillDef, priority);
		int num = FindSkillOverrideIndex(ref skillOverride);
		if (num != -1)
		{
			ArrayUtils.ArrayRemoveAtAndResize<SkillOverride>(ref skillOverrides, num, 1);
			PickCurrentOverride();
		}
	}

	public bool HasSkillOverrideOfPriority(SkillOverridePriority priority)
	{
		for (int i = 0; i < skillOverrides.Length; i++)
		{
			if (priority == skillOverrides[i].priority)
			{
				return true;
			}
		}
		return false;
	}

	private void PickCurrentOverride()
	{
		currentSkillOverride = -1;
		SkillOverridePriority skillOverridePriority = SkillOverridePriority.Default;
		for (int i = 0; i < skillOverrides.Length; i++)
		{
			SkillOverridePriority priority = skillOverrides[i].priority;
			if (skillOverridePriority <= priority)
			{
				currentSkillOverride = i;
				skillOverridePriority = priority;
			}
		}
		if (currentSkillOverride == -1)
		{
			SetSkillInternal(baseSkill);
		}
		else
		{
			SetSkillInternal(skillOverrides[currentSkillOverride].skillDef);
		}
	}

	private void SetSkillInternal(SkillDef newSkillDef)
	{
		if (!((Object)(object)skillDef == (Object)(object)newSkillDef))
		{
			UnassignSkill();
			AssignSkill(newSkillDef);
			this.onSkillChanged?.Invoke(this);
		}
	}

	public void SetBaseSkill(SkillDef newSkillDef)
	{
		baseSkill = newSkillDef;
		PickCurrentOverride();
	}

	private void UnassignSkill()
	{
		if (skillDef != null)
		{
			skillDef.OnUnassigned(this);
			skillInstanceData = null;
			skillDef = null;
		}
	}

	private void AssignSkill(SkillDef newSkillDef)
	{
		skillDef = newSkillDef;
		if (skillDef != null)
		{
			PickTargetStateMachine();
			RecalculateMaxStock();
			if (skillDef.fullRestockOnAssign && stock < maxStock)
			{
				stock = maxStock;
			}
			if (skillDef.dontAllowPastMaxStocks)
			{
				stock = Mathf.Min(maxStock, stock);
			}
			skillInstanceData = skillDef.OnAssigned(this);
			RecalculateFinalRechargeInterval();
		}
	}

	public void SetBonusStockFromBody(int newBonusStockFromBody)
	{
		bonusStockFromBody = newBonusStockFromBody;
		RecalculateMaxStock();
	}

	private void RecalculateMaxStock()
	{
		maxStock = skillDef.GetMaxStock(this) + bonusStockFromBody;
	}

	private void Awake()
	{
		defaultSkillDef = skillFamily.defaultSkillDef;
		baseSkill = defaultSkillDef;
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		AssignSkill(defaultSkillDef);
	}

	private void OnDestroy()
	{
		UnassignSkill();
	}

	private void Start()
	{
		RecalculateMaxStock();
		stock = maxStock;
	}

	private void FixedUpdate()
	{
		skillDef?.OnFixedUpdate(this);
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
	}

	public bool CanExecute()
	{
		return skillDef?.CanExecute(this) ?? false;
	}

	public bool IsReady()
	{
		return skillDef?.IsReady(this) ?? false;
	}

	public bool ExecuteIfReady()
	{
		hasExecutedSuccessfully = CanExecute();
		if (hasExecutedSuccessfully)
		{
			OnExecute();
			return true;
		}
		return false;
	}

	public void RunRecharge(float dt)
	{
		if (stock < maxStock)
		{
			if (!beginSkillCooldownOnSkillEnd || !(stateMachine.state.GetType() == activationState.stateType))
			{
				rechargeStopwatch += dt;
			}
			if (rechargeStopwatch >= finalRechargeInterval)
			{
				RestockSteplike();
			}
		}
	}

	public void Reset()
	{
		rechargeStopwatch = 0f;
		stock = maxStock;
	}

	public bool CanApplyAmmoPack()
	{
		if (stock < maxStock)
		{
			return true;
		}
		return false;
	}

	public void ApplyAmmoPack()
	{
		if (stock < maxStock)
		{
			stock += rechargeStock;
			if (stock > maxStock)
			{
				stock = maxStock;
			}
		}
	}

	public void AddOneStock()
	{
		int num = stock + 1;
		stock = num;
		rechargeStopwatch = 0f;
	}

	public void RemoveAllStocks()
	{
		stock = 0;
		rechargeStopwatch = 0f;
	}

	public void DeductStock(int count)
	{
		stock = Mathf.Max(0, stock - count);
	}

	private void OnExecute()
	{
		skillDef.OnExecute(this);
	}

	private void RestockContinuous()
	{
		if (finalRechargeInterval == 0f)
		{
			stock = maxStock;
			rechargeStopwatch = 0f;
			return;
		}
		int num = Mathf.FloorToInt(rechargeStopwatch / finalRechargeInterval * (float)rechargeStock);
		stock += num;
		if (stock >= maxStock)
		{
			stock = maxStock;
			rechargeStopwatch = 0f;
		}
		else
		{
			rechargeStopwatch -= (float)num * finalRechargeInterval;
		}
	}

	private void RestockSteplike()
	{
		stock += rechargeStock;
		if (stock >= maxStock)
		{
			stock = maxStock;
		}
		rechargeStopwatch = 0f;
	}

	public float CalculateFinalRechargeInterval()
	{
		return Mathf.Min(baseRechargeInterval, Mathf.Max(0.5f, baseRechargeInterval * cooldownScale - flatCooldownReduction));
	}

	private void RecalculateFinalRechargeInterval()
	{
		finalRechargeInterval = CalculateFinalRechargeInterval();
	}

	public void RecalculateValues()
	{
		RecalculateMaxStock();
		RecalculateFinalRechargeInterval();
	}

	private void PickTargetStateMachine()
	{
		EntityStateMachine targetStateMachine = stateMachine;
		if (!string.Equals(stateMachine?.customName, skillDef.activationStateMachineName, StringComparison.Ordinal))
		{
			targetStateMachine = EntityStateMachine.FindByCustomName(((Component)this).gameObject, skillDef.activationStateMachineName);
		}
		this._customStateMachineResolver?.Invoke(this, skillDef, ref targetStateMachine);
		stateMachine = targetStateMachine;
	}

	[AssetCheck(typeof(GenericSkill))]
	private static void CheckGenericSkillStateMachine(AssetCheckArgs args)
	{
		if (((GenericSkill)(object)args.asset).stateMachine.customName == string.Empty)
		{
			args.LogError("Unnamed state machine.", (Object)(object)((Component)(GenericSkill)(object)args.asset).gameObject);
		}
	}
}
