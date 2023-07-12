using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TeleporterHealNovaController;

public class TeleporterHealNovaGeneratorMain : BaseState
{
	public static GameObject pulsePrefab;

	[SerializeField]
	public float minSecondsBetweenPulses;

	private HoldoutZoneController holdoutZone;

	private TeamIndex teamIndex;

	private float previousPulseFraction;

	private int pulseCount;

	private float secondsUntilPulseAvailable;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform parent = base.transform.parent;
		if (Object.op_Implicit((Object)(object)parent))
		{
			holdoutZone = ((Component)parent).GetComponentInParent<HoldoutZoneController>();
			previousPulseFraction = GetCurrentTeleporterChargeFraction();
		}
		TeamFilter component = GetComponent<TeamFilter>();
		teamIndex = (Object.op_Implicit((Object)(object)component) ? component.teamIndex : TeamIndex.None);
	}

	private float GetCurrentTeleporterChargeFraction()
	{
		return holdoutZone.charge;
	}

	public override void FixedUpdate()
	{
		if (!NetworkServer.active || !(Time.fixedDeltaTime > 0f))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)holdoutZone) || holdoutZone.charge >= 1f)
		{
			EntityState.Destroy((Object)(object)((Component)outer).gameObject);
			return;
		}
		if (secondsUntilPulseAvailable > 0f)
		{
			secondsUntilPulseAvailable -= Time.fixedDeltaTime;
			return;
		}
		pulseCount = CalculatePulseCount(teamIndex);
		float num = CalculateNextPulseFraction(pulseCount, previousPulseFraction);
		float currentTeleporterChargeFraction = GetCurrentTeleporterChargeFraction();
		if (num < currentTeleporterChargeFraction)
		{
			Pulse();
			previousPulseFraction = num;
			secondsUntilPulseAvailable = minSecondsBetweenPulses;
		}
	}

	private static int CalculatePulseCount(TeamIndex teamIndex)
	{
		int num = 0;
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CharacterMaster characterMaster = readOnlyInstancesList[i];
			if (characterMaster.teamIndex == teamIndex)
			{
				num += characterMaster.inventory.GetItemCount(RoR2Content.Items.TPHealingNova);
			}
		}
		return num;
	}

	private static float CalculateNextPulseFraction(int pulseCount, float previousPulseFraction)
	{
		float num = 1f / (float)(pulseCount + 1);
		for (int i = 1; i <= pulseCount; i++)
		{
			float num2 = (float)i * num;
			if (!(num2 <= previousPulseFraction))
			{
				return num2;
			}
		}
		return 1f;
	}

	protected void Pulse()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(pulsePrefab, base.transform.position, base.transform.rotation, base.transform.parent);
		obj.GetComponent<TeamFilter>().teamIndex = teamIndex;
		NetworkServer.Spawn(obj);
	}
}
