using RoR2.Skills;
using UnityEngine;

namespace RoR2.Achievements.Bandit2;

[RegisterAchievement("Bandit2ConsecutiveReset", "Skills.Bandit2.Rifle", "CompleteThreeStages", typeof(Bandit2ConsecutiveResetServerAchievement))]
public class Bandit2ConsecutiveResetAchievement : BaseAchievement
{
	private class Bandit2ConsecutiveResetServerAchievement : BaseServerAchievement
	{
		private int progress;

		private SkillDef requiredSkillDef;

		private bool waitingForKill;

		private CharacterBody _trackedBody;

		private CharacterBody trackedBody
		{
			get
			{
				return _trackedBody;
			}
			set
			{
				if (_trackedBody != value)
				{
					if (_trackedBody != null)
					{
						_trackedBody.onSkillActivatedServer -= OnBodySKillActivatedServer;
					}
					_trackedBody = value;
					if (_trackedBody != null)
					{
						_trackedBody.onSkillActivatedServer += OnBodySKillActivatedServer;
						progress = 0;
						waitingForKill = false;
					}
				}
			}
		}

		private void OnBodySKillActivatedServer(GenericSkill skillSlot)
		{
			if (skillSlot.skillDef == requiredSkillDef && requiredSkillDef != null)
			{
				if (waitingForKill)
				{
					progress = 0;
				}
				waitingForKill = true;
			}
		}

		public override void OnInstall()
		{
			base.OnInstall();
			requiredSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Bandit2.ResetRevolver"));
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
			RoR2Application.onFixedUpdate += FixedUpdate;
		}

		public override void OnUninstall()
		{
			trackedBody = null;
			RoR2Application.onFixedUpdate -= FixedUpdate;
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
			base.OnUninstall();
		}

		private void FixedUpdate()
		{
			trackedBody = GetCurrentBody();
		}

		private void OnCharacterDeathGlobal(DamageReport damageReport)
		{
			if (damageReport.attackerBody == trackedBody && Object.op_Implicit((Object)(object)damageReport.attackerBody) && (damageReport.damageInfo.damageType & DamageType.ResetCooldownsOnKill) == DamageType.ResetCooldownsOnKill)
			{
				waitingForKill = false;
				progress++;
				if (progress >= requirement)
				{
					Grant();
				}
			}
		}
	}

	private static readonly int requirement = 15;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("Bandit2Body");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		SetServerTracked(shouldTrack: true);
	}

	protected override void OnBodyRequirementBroken()
	{
		SetServerTracked(shouldTrack: false);
		base.OnBodyRequirementBroken();
	}
}
