using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Arena.NullWard;

public class Active : NullWardBaseState
{
	public static string soundEntryEvent;

	public static string soundLoopStartEvent;

	public static string soundLoopEndEvent;

	private static Run.FixedTimeStamp startTime;

	private HoldoutZoneController holdoutZoneController;

	public override void OnEnter()
	{
		base.OnEnter();
		holdoutZoneController = GetComponent<HoldoutZoneController>();
		((Behaviour)holdoutZoneController).enabled = true;
		holdoutZoneController.baseRadius = NullWardBaseState.wardRadiusOn;
		purchaseInteraction.SetAvailable(newAvailable: false);
		base.arenaMissionController.rewardSpawnPosition = ((Component)childLocator.FindChild("RewardSpawn")).gameObject;
		base.arenaMissionController.monsterSpawnPosition = ((Component)childLocator.FindChild("MonsterSpawn")).gameObject;
		((Component)childLocator.FindChild("ActiveEffect")).gameObject.SetActive(true);
		if (NetworkServer.active)
		{
			base.arenaMissionController.BeginRound();
		}
		if (base.isAuthority)
		{
			startTime = Run.FixedTimeStamp.now;
		}
		Util.PlaySound(soundEntryEvent, base.gameObject);
		Util.PlaySound(soundLoopStartEvent, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		sphereZone.Networkradius = holdoutZoneController.currentRadius;
		if (base.isAuthority && holdoutZoneController.charge >= 1f)
		{
			outer.SetNextState(new Complete());
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			((Behaviour)holdoutZoneController).enabled = false;
		}
		Util.PlaySound(soundLoopEndEvent, base.gameObject);
		((Component)childLocator.FindChild("ActiveEffect")).gameObject.SetActive(false);
		((Component)childLocator.FindChild("WardOnEffect")).gameObject.SetActive(false);
		base.OnExit();
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(startTime);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		startTime = reader.ReadFixedTimeStamp();
	}
}
