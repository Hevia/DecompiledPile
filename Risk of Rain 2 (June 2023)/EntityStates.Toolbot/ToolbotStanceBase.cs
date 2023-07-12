using System;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Toolbot;

public class ToolbotStanceBase : BaseState
{
	protected enum WeaponStance
	{
		None = -1,
		Nailgun,
		Spear,
		Grenade,
		Buzzsaw
	}

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	public static GameObject emptyCrosshairPrefab;

	public static AnimationCurve emptyCrosshairSpreadCurve;

	protected Inventory inventory;

	public Type swapStateType;

	protected void SetPrimarySkill()
	{
		base.skillLocator.primary = GetCurrentPrimarySkill();
	}

	protected void SetSecondarySkill(string skillName)
	{
		if (Object.op_Implicit((Object)(object)base.skillLocator))
		{
			base.skillLocator.secondary = base.skillLocator.FindSkill(skillName);
		}
	}

	protected string GetSkillSlotStance(GenericSkill skillSlot)
	{
		return (skillSlot.skillDef as ToolbotWeaponSkillDef)?.stanceName ?? string.Empty;
	}

	protected GenericSkill GetPrimarySkill1()
	{
		return base.skillLocator.FindSkillByFamilyName("ToolbotBodyPrimary1");
	}

	protected GenericSkill GetPrimarySkill2()
	{
		return base.skillLocator.FindSkillByFamilyName("ToolbotBodyPrimary2");
	}

	protected virtual GenericSkill GetCurrentPrimarySkill()
	{
		return null;
	}

	protected void UpdateCrosshairParameters(ToolbotWeaponSkillDef weaponSkillDef)
	{
		GameObject crosshairPrefab = emptyCrosshairPrefab;
		AnimationCurve crosshairSpreadCurve = emptyCrosshairSpreadCurve;
		if (Object.op_Implicit((Object)(object)weaponSkillDef))
		{
			crosshairPrefab = weaponSkillDef.crosshairPrefab;
			crosshairSpreadCurve = weaponSkillDef.crosshairSpreadCurve;
		}
		crosshairOverrideRequest?.Dispose();
		crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
		base.characterBody.spreadBloomCurve = crosshairSpreadCurve;
	}

	protected void SetEquipmentSlot(byte i)
	{
		if (Object.op_Implicit((Object)(object)inventory))
		{
			inventory.SetActiveEquipmentSlot(i);
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		inventory = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.inventory : null);
		SetPrimarySkill();
		ToolbotWeaponSkillDef toolbotWeaponSkillDef = GetCurrentPrimarySkill()?.skillDef as ToolbotWeaponSkillDef;
		if (Object.op_Implicit((Object)(object)toolbotWeaponSkillDef))
		{
			SendWeaponStanceToAnimator(toolbotWeaponSkillDef);
			PlayCrossfade("Gesture, Additive", toolbotWeaponSkillDef.enterGestureAnimState, 0.2f);
		}
		UpdateCrosshairParameters(toolbotWeaponSkillDef);
	}

	public override void OnExit()
	{
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	protected void SendWeaponStanceToAnimator(ToolbotWeaponSkillDef weaponSkillDef)
	{
		if (Object.op_Implicit((Object)(object)weaponSkillDef))
		{
			GetModelAnimator().SetInteger("weaponStance", weaponSkillDef.animatorWeaponIndex);
		}
	}

	protected static WeaponStance GetSkillStance(GenericSkill skillSlot)
	{
		return (skillSlot?.skillDef as ToolbotWeaponSkillDef)?.stanceName switch
		{
			"Nailgun" => WeaponStance.Nailgun, 
			"Spear" => WeaponStance.Spear, 
			"Grenade" => WeaponStance.Grenade, 
			"Buzzsaw" => WeaponStance.Buzzsaw, 
			_ => WeaponStance.None, 
		};
	}
}
