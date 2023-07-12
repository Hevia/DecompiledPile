namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactWeakAssKnees", "Artifacts.WeakAssKnees", null, null)]
public class ObtainArtifactWeakAssKneesAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.weakAssKneesArtifactDef;
}
