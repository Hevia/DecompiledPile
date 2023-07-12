namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactEnigma", "Artifacts.Enigma", null, null)]
public class ObtainArtifactEnigmaAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.enigmaArtifactDef;
}
