namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactSacrifice", "Artifacts.Sacrifice", null, null)]
public class ObtainArtifactSacrificeAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.sacrificeArtifactDef;
}
