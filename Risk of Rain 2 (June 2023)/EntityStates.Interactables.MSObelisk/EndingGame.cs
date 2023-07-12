using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Interactables.MSObelisk;

public class EndingGame : BaseState
{
	public static GameObject destroyEffectPrefab;

	public static float timeBetweenDestroy;

	public static float timeUntilEndGame;

	private float destroyTimer;

	private float endGameTimer;

	private bool beginEndingGame;

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	private void FixedUpdateServer()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		destroyTimer -= Time.fixedDeltaTime;
		if (!beginEndingGame)
		{
			if (!(destroyTimer <= 0f))
			{
				return;
			}
			destroyTimer = timeBetweenDestroy;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
			if (teamMembers.Count > 0)
			{
				GameObject val = ((Component)teamMembers[0]).gameObject;
				CharacterBody component = val.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component))
				{
					EffectManager.SpawnEffect(destroyEffectPrefab, new EffectData
					{
						origin = component.corePosition,
						scale = component.radius
					}, transmit: true);
					EntityState.Destroy((Object)(object)val.gameObject);
				}
			}
			else
			{
				beginEndingGame = true;
			}
		}
		else
		{
			endGameTimer += Time.fixedDeltaTime;
			if (endGameTimer >= timeUntilEndGame && Object.op_Implicit((Object)(object)Run.instance))
			{
				DoFinalAction();
			}
		}
	}

	private void DoFinalAction()
	{
		bool flag = false;
		for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count; i++)
		{
			if (CharacterMaster.readOnlyInstancesList[i].inventory.GetItemCount(RoR2Content.Items.LunarTrinket) > 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			outer.SetNextState(new TransitionToNextStage());
			return;
		}
		Run.instance.BeginGameOver(RoR2Content.GameEndings.ObliterationEnding);
		outer.SetNextState(new Idle());
	}
}
