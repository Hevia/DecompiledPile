namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactSwarms", "Artifacts.Swarms", null, null)]
public class ObtainArtifactSwarmsAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.swarmsArtifactDef;
}
