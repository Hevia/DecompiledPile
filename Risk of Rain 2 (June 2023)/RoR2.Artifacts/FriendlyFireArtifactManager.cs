using System;
using UnityEngine;

namespace RoR2.Artifacts;

public static class FriendlyFireArtifactManager
{
	private static ArtifactDef myArtifact => RoR2Content.Artifacts.friendlyFireArtifactDef;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabledGlobal;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabledGlobal;
	}

	private static void OnArtifactEnabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			FriendlyFireManager.friendlyFireMode = FriendlyFireManager.FriendlyFireMode.FriendlyFire;
		}
	}

	private static void OnArtifactDisabledGlobal(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifact))
		{
			FriendlyFireManager.friendlyFireMode = FriendlyFireManager.FriendlyFireMode.Off;
		}
	}
}
