namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactGlass", "Artifacts.Glass", null, null)]
public class ObtainArtifactGlassAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.glassArtifactDef;
}
