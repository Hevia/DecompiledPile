using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class EscapeSequenceExtractionZone : MonoBehaviour
{
	private class EscapeSequenceExtractionZoneObjectiveTracker : ObjectivePanelController.ObjectiveTracker
	{
		private int cachedLivingPlayersCount;

		private int cachedPlayersInRadiusCount;

		private EscapeSequenceExtractionZone extractionZone => (EscapeSequenceExtractionZone)(object)sourceDescriptor.source;

		public override string ToString()
		{
			cachedLivingPlayersCount = extractionZone.livingPlayerCount;
			cachedPlayersInRadiusCount = extractionZone.playersInRadius.Count;
			string text = Language.GetString(extractionZone.objectiveToken);
			if (cachedLivingPlayersCount >= 1)
			{
				text = Language.GetStringFormatted("OBJECTIVE_FRACTION_PROGRESS_FORMAT", text, cachedPlayersInRadiusCount, cachedLivingPlayersCount);
			}
			return text;
		}

		protected override bool IsDirty()
		{
			if (cachedLivingPlayersCount != extractionZone.livingPlayerCount)
			{
				return cachedPlayersInRadiusCount != extractionZone.playersInRadius.Count;
			}
			return false;
		}
	}

	public float radius;

	public string objectiveToken;

	public GameEndingDef successEnding;

	private int livingPlayerCount;

	private List<CharacterBody> playersInRadius;

	private TeamIndex teamIndex = TeamIndex.Player;

	private void Awake()
	{
		playersInRadius = new List<CharacterBody>();
	}

	private void FixedUpdate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		livingPlayerCount = CountLivingPlayers(teamIndex);
		playersInRadius.Clear();
		GetPlayersInRadius(((Component)this).transform.position, radius * radius, teamIndex, playersInRadius);
		if (NetworkServer.active && livingPlayerCount > 0 && playersInRadius.Count >= livingPlayerCount)
		{
			HandleEndingServer();
		}
	}

	private void OnEnable()
	{
		if (InstanceTracker.GetInstancesList<EscapeSequenceExtractionZone>().Count == 0)
		{
			ObjectivePanelController.collectObjectiveSources += ReportObjectives;
		}
		InstanceTracker.Add(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove(this);
		if (InstanceTracker.GetInstancesList<EscapeSequenceExtractionZone>().Count == 0)
		{
			ObjectivePanelController.collectObjectiveSources -= ReportObjectives;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(((Component)this).transform.position, radius);
	}

	public void KillAllStragglers()
	{
		List<CharacterMaster> list = new List<CharacterMaster>(CharacterMaster.readOnlyInstancesList);
		for (int i = 0; i < list.Count; i++)
		{
			CharacterMaster characterMaster = list[i];
			if (Object.op_Implicit((Object)(object)characterMaster) && !playersInRadius.Contains(characterMaster.GetBody()))
			{
				characterMaster.TrueKill(null, null, DamageType.Silent | DamageType.VoidDeath);
			}
		}
	}

	public void HandleEndingServer()
	{
		if (!Run.instance.isGameOverServer)
		{
			KillAllStragglers();
			if (playersInRadius.Count > 0)
			{
				Run.instance.BeginGameOver(successEnding);
			}
		}
	}

	private static void ReportObjectives(CharacterMaster characterMaster, List<ObjectivePanelController.ObjectiveSourceDescriptor> dest)
	{
		List<EscapeSequenceExtractionZone> instancesList = InstanceTracker.GetInstancesList<EscapeSequenceExtractionZone>();
		for (int i = 0; i < instancesList.Count; i++)
		{
			EscapeSequenceExtractionZone escapeSequenceExtractionZone = instancesList[i];
			if (characterMaster.teamIndex == escapeSequenceExtractionZone.teamIndex)
			{
				ObjectivePanelController.ObjectiveSourceDescriptor objectiveSourceDescriptor = default(ObjectivePanelController.ObjectiveSourceDescriptor);
				objectiveSourceDescriptor.master = characterMaster;
				objectiveSourceDescriptor.objectiveType = typeof(EscapeSequenceExtractionZoneObjectiveTracker);
				objectiveSourceDescriptor.source = (Object)(object)escapeSequenceExtractionZone;
			}
		}
	}

	private static bool IsPointInRadius(Vector3 origin, float chargingRadiusSqr, Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - origin;
		if (((Vector3)(ref val)).sqrMagnitude <= chargingRadiusSqr)
		{
			return true;
		}
		return false;
	}

	private static bool IsBodyInRadius(Vector3 origin, float chargingRadiusSqr, CharacterBody characterBody)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsPointInRadius(origin, chargingRadiusSqr, characterBody.corePosition);
	}

	private static int CountLivingPlayers(TeamIndex teamIndex)
	{
		int num = 0;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			if (teamMembers[i].body.isPlayerControlled)
			{
				num++;
			}
		}
		return num;
	}

	public int CountPlayersInRadius(Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			TeamComponent teamComponent = teamMembers[i];
			if (teamComponent.body.isPlayerControlled && IsBodyInRadius(origin, chargingRadiusSqr, teamComponent.body))
			{
				num++;
			}
		}
		return num;
	}

	private int GetPlayersInRadius(Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex, List<CharacterBody> dest)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int result = 0;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			TeamComponent teamComponent = teamMembers[i];
			if (teamComponent.body.isPlayerControlled && IsBodyInRadius(origin, chargingRadiusSqr, teamComponent.body))
			{
				dest.Add(teamComponent.body);
			}
		}
		return result;
	}

	public bool IsBodyInRadius(CharacterBody body)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return false;
		}
		return IsBodyInRadius(((Component)this).transform.position, radius * radius, body);
	}
}
