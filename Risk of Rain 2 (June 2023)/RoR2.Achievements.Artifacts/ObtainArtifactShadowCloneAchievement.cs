namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactShadowClone", "Artifacts.ShadowClone", null, null)]
public class ObtainArtifactShadowCloneAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.shadowCloneArtifactDef;
}
