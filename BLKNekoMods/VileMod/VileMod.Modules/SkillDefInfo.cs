using EntityStates;
using UnityEngine;

namespace VileMod.Modules;

internal class SkillDefInfo
{
	public string skillName;

	public string skillNameToken;

	public string skillDescriptionToken;

	public string[] keywordTokens = new string[0];

	public Sprite skillIcon;

	public SerializableEntityStateType activationState;

	public InterruptPriority interruptPriority;

	public string activationStateMachineName;

	public float baseRechargeInterval;

	public int baseMaxStock = 1;

	public int rechargeStock = 1;

	public int requiredStock = 1;

	public int stockToConsume = 1;

	public bool isCombatSkill = true;

	public bool canceledFromSprinting;

	public bool forceSprintDuringState;

	public bool cancelSprintingOnActivation = true;

	public bool beginSkillCooldownOnSkillEnd;

	public bool fullRestockOnAssign = true;

	public bool resetCooldownTimerOnUse;

	public bool mustKeyPress;

	public SkillDefInfo()
	{
	}

	public SkillDefInfo(string skillNameToken, string skillDescriptionToken, Sprite skillIcon, SerializableEntityStateType activationState, string activationStateMachineName = "Weapon", bool agile = false)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		skillName = skillNameToken;
		this.skillNameToken = skillNameToken;
		this.skillDescriptionToken = skillDescriptionToken;
		this.skillIcon = skillIcon;
		this.activationState = activationState;
		this.activationStateMachineName = activationStateMachineName;
		interruptPriority = (InterruptPriority)0;
		isCombatSkill = true;
		baseRechargeInterval = 0f;
		requiredStock = 0;
		stockToConsume = 0;
		cancelSprintingOnActivation = !agile;
		if (agile)
		{
			keywordTokens = new string[1] { "KEYWORD_AGILE" };
		}
	}
}
