using System;
using RoR2.Stats;
using UnityEngine;

namespace RoR2.Achievements.Commando;

[RegisterAchievement("CommandoNonLunarEndurance", "Skills.Commando.ThrowGrenade", null, null)]
public class CommandoNonLunarEnduranceAchievement : BaseAchievement
{
	private static readonly ulong requirement = 20uL;

	private PlayerCharacterMasterController cachedMasterController;

	private PlayerStatsComponent cachedStatsComponent;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CommandoBody");
	}

	private static bool EverPickedUpLunarItems(StatSheet statSheet)
	{
		foreach (ItemIndex lunarItem in ItemCatalog.lunarItemList)
		{
			if (statSheet.GetStatValueULong(PerItemStatDef.totalCollected.FindStatDef(lunarItem)) != 0)
			{
				return true;
			}
		}
		foreach (EquipmentIndex equipment in EquipmentCatalog.equipmentList)
		{
			if (EquipmentCatalog.GetEquipmentDef(equipment).isLunar && statSheet.GetStatValueDouble(PerEquipmentStatDef.totalTimeHeld.FindStatDef(equipment)) > 0.0)
			{
				return true;
			}
		}
		return false;
	}

	private void OnMasterChanged()
	{
		if (base.localUser != null)
		{
			SetMasterController(base.localUser.cachedMasterController);
		}
	}

	private void SetMasterController(PlayerCharacterMasterController playerCharacterMasterController)
	{
		if (base.localUser.cachedMasterController == cachedMasterController)
		{
			return;
		}
		bool num = cachedStatsComponent != null;
		cachedMasterController = base.localUser.cachedMasterController;
		cachedStatsComponent = (Object.op_Implicit((Object)(object)cachedMasterController) ? ((Component)cachedMasterController).GetComponent<PlayerStatsComponent>() : null);
		bool flag = cachedStatsComponent != null;
		if (num != flag && base.userProfile != null)
		{
			if (flag)
			{
				UserProfile obj = base.userProfile;
				obj.onStatsReceived = (Action)Delegate.Combine(obj.onStatsReceived, new Action(OnStatsChanged));
			}
			else
			{
				UserProfile obj2 = base.userProfile;
				obj2.onStatsReceived = (Action)Delegate.Remove(obj2.onStatsReceived, new Action(OnStatsChanged));
			}
		}
	}

	private void OnStatsChanged()
	{
		if (cachedStatsComponent != null && requirement <= cachedStatsComponent.currentStats.GetStatValueULong(StatDef.totalStagesCompleted))
		{
			if (EverPickedUpLunarItems(cachedStatsComponent.currentStats))
			{
				SetMasterController(null);
			}
			else
			{
				Grant();
			}
		}
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		base.localUser.onMasterChanged += OnMasterChanged;
		OnMasterChanged();
	}

	protected override void OnBodyRequirementBroken()
	{
		base.localUser.onMasterChanged -= OnMasterChanged;
		SetMasterController(null);
		base.OnBodyRequirementBroken();
	}
}
