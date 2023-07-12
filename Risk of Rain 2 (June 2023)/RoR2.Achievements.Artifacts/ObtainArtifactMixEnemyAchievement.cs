namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactMixEnemy", "Artifacts.MixEnemy", null, null)]
public class ObtainArtifactMixEnemyAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.mixEnemyArtifactDef;
}
