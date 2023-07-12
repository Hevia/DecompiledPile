using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("LoaderKillLoaders", "Skills.Loader.Thunderslam", "DefeatSuperRoboBallBoss", typeof(LoaderKillLoadersServerAchievement))]
public class LoaderKillLoadersAchievement : BaseAchievement
{
	private class LoaderKillLoadersServerAchievement : BaseServerAchievement
	{
		private BodyIndex bodyIndex;

		private SceneDef requiredSceneDef;

		private int numKills;

		public override void OnInstall()
		{
			base.OnInstall();
			bodyIndex = BodyCatalog.FindBodyIndex("LoaderBody");
			requiredSceneDef = SceneCatalog.GetSceneDefFromSceneName("artifactworld");
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
		}

		public override void OnUninstall()
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
			base.OnUninstall();
		}

		private void OnCharacterDeath(DamageReport damageReport)
		{
			if (Object.op_Implicit((Object)(object)damageReport.victimBody))
			{
				if (damageReport.victimBody.bodyIndex == bodyIndex && IsCurrentBody(damageReport.attackerBody) && (Object)(object)SceneCatalog.mostRecentSceneDef == (Object)(object)requiredSceneDef)
				{
					numKills++;
				}
				if (numKills >= 3)
				{
					Grant();
				}
			}
		}
	}

	private const int requirement = 3;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("LoaderBody");
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
