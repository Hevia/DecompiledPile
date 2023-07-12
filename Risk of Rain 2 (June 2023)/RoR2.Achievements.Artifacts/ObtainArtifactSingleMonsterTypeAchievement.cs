namespace RoR2.Achievements.Artifacts;

[RegisterAchievement("ObtainArtifactSingleMonsterType", "Artifacts.SingleMonsterType", null, null)]
public class ObtainArtifactSingleMonsterTypeAchievement : BaseObtainArtifactAchievement
{
	protected override ArtifactDef artifactDef => RoR2Content.Artifacts.singleMonsterTypeArtifactDef;
}
