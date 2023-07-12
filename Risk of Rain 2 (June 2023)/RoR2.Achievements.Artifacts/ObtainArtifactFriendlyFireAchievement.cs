namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactFriendlyFire", "Artifacts.FriendlyFire", null, null)]
public class ObtainArtifactFriendlyFireAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.friendlyFireArtifactDef;
}
