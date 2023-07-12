using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/Infinite Tower/Infinite Tower Wave Artifact Prerequisites")]
public class InfiniteTowerWaveArtifactPrerequisites : InfiniteTowerWavePrerequisites
{
	[Tooltip("This wave cannot be selected while this artifact is enabled.")]
	[SerializeField]
	private ArtifactDef bannedArtifact;

	public override bool AreMet(InfiniteTowerRun run)
	{
		return !RunArtifactManager.instance.IsArtifactEnabled(bannedArtifact);
	}
}
