namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactMonsterTeamGainsItems", "Artifacts.MonsterTeamGainsItems", null, null)]
public class ObtainArtifactMonsterTeamGainsItemsAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.monsterTeamGainsItemsArtifactDef;
}
