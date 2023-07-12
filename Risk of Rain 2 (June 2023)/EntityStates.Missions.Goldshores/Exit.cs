using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Goldshores;

public class Exit : EntityState
{
	public override void OnEnter()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscGoldshoresPortal"), new DirectorPlacementRule
			{
				maxDistance = float.PositiveInfinity,
				minDistance = 10f,
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
				position = base.transform.position,
				spawnOnTarget = GoldshoresMissionController.instance.bossSpawnPosition
			}, Run.instance.stageRng));
			if (Object.op_Implicit((Object)(object)val))
			{
				Chat.SendBroadcastChat(new Chat.SimpleChatMessage
				{
					baseToken = "PORTAL_GOLDSHORES_OPEN"
				});
				val.GetComponent<SceneExitController>().useRunNextStageScene = true;
			}
			for (int num = CombatDirector.instancesList.Count - 1; num >= 0; num--)
			{
				((Behaviour)CombatDirector.instancesList[num]).enabled = false;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
