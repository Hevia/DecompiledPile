namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactEliteOnly", "Artifacts.EliteOnly", null, null)]
public class ObtainArtifactAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.eliteOnlyArtifactDef;
}
