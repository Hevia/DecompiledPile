namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactTeamDeath", "Artifacts.TeamDeath", null, null)]
public class ObtainArtifactTeamDeathAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.teamDeathArtifactDef;
}
