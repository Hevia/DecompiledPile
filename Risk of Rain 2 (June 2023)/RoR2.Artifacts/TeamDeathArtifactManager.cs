using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Artifacts;

public static class TeamDeathArtifactManager
{
	private static GameObject forceSpectatePrefab;

	private static bool inTeamKill;

	private static ArtifactDef myArtifactDef => RoR2Content.Artifacts.teamDeathArtifactDef;

	[SystemInitializer(new Type[] { typeof(ArtifactCatalog) })]
	private static void Init()
	{
		RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
		RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
		forceSpectatePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ForceSpectate");
	}

	private static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifactDef))
		{
			GlobalEventManager.onCharacterDeathGlobal += OnServerCharacterDeathGlobal;
		}
	}

	private static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
	{
		if (!((Object)(object)artifactDef != (Object)(object)myArtifactDef))
		{
			GlobalEventManager.onCharacterDeathGlobal -= OnServerCharacterDeathGlobal;
		}
	}

	private static void OnServerCharacterDeathGlobal(DamageReport damageReport)
	{
		TeamIndex teamIndex;
		NetworkUser victimNetworkUser;
		if (!inTeamKill && Object.op_Implicit((Object)(object)damageReport.victimMaster) && Object.op_Implicit((Object)(object)damageReport.victimMaster.playerCharacterMasterController))
		{
			if (damageReport.victimMaster.inventory.GetItemCount(RoR2Content.Items.ExtraLife) <= 0 && damageReport.victimMaster.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) <= 0)
			{
				GameObject obj = Object.Instantiate<GameObject>(forceSpectatePrefab);
				obj.GetComponent<ForceSpectate>().Networktarget = ((Component)damageReport.victimBody).gameObject;
				NetworkServer.Spawn(obj);
				teamIndex = damageReport.victimTeamIndex;
				PlayerCharacterMasterController playerCharacterMasterController = damageReport.victimMaster.playerCharacterMasterController;
				victimNetworkUser = (Object.op_Implicit((Object)(object)playerCharacterMasterController) ? playerCharacterMasterController.networkUser : null);
				RoR2Application.onNextUpdate += KillTeam;
			}
		}
		void KillTeam()
		{
			inTeamKill = true;
			Chat.SendBroadcastChat(new SubjectChatMessage
			{
				baseToken = "ARTIFACT_TEAMDEATH_DEATHMESSAGE",
				subjectAsNetworkUser = victimNetworkUser
			});
			ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
			for (int num = readOnlyInstancesList.Count - 1; num >= 0; num--)
			{
				if (readOnlyInstancesList[num].teamIndex == teamIndex)
				{
					readOnlyInstancesList[num].TrueKill();
				}
			}
			inTeamKill = false;
		}
	}
}
