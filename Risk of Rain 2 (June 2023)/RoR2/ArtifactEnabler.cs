using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ArtifactEnabler : MonoBehaviour
{
	[SerializeField]
	private ArtifactDef artifactDef;

	private bool artifactWasEnabled;

	private void OnEnable()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)artifactDef))
		{
			artifactWasEnabled = RunArtifactManager.instance.IsArtifactEnabled(artifactDef);
			RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, newEnabled: true);
		}
	}

	private void OnDisable()
	{
		if (NetworkServer.active && Object.op_Implicit((Object)(object)artifactDef) && Object.op_Implicit((Object)(object)RunArtifactManager.instance))
		{
			RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, artifactWasEnabled);
		}
	}
}
