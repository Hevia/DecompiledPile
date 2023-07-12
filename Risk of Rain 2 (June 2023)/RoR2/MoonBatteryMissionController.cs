using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EntityStates.Missions.Moon;
using EntityStates.MoonElevator;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(EntityStateMachine))]
public class MoonBatteryMissionController : NetworkBehaviour
{
	[SerializeField]
	private GameObject[] moonBatteries;

	[SerializeField]
	private GameObject[] elevators;

	[SerializeField]
	private int _numRequiredBatteries;

	[SerializeField]
	private string _objectiveToken;

	private HoldoutZoneController[] batteryHoldoutZones;

	private EntityStateMachine[] batteryStateMachines;

	private EntityStateMachine[] elevatorStateMachines;

	[SyncVar]
	private int _numChargedBatteries;

	public static MoonBatteryMissionController instance { get; private set; }

	public int numChargedBatteries => _numChargedBatteries;

	public int numRequiredBatteries => _numRequiredBatteries;

	public string objectiveToken => _objectiveToken;

	public int Network_numChargedBatteries
	{
		get
		{
			return _numChargedBatteries;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref _numChargedBatteries, 1u);
		}
	}

	public static event Action onInstanceChangedGlobal;

	private void Awake()
	{
		batteryHoldoutZones = new HoldoutZoneController[moonBatteries.Length];
		batteryStateMachines = new EntityStateMachine[moonBatteries.Length];
		for (int i = 0; i < moonBatteries.Length; i++)
		{
			GameObject val = moonBatteries[i];
			batteryHoldoutZones[i] = val.GetComponent<HoldoutZoneController>();
			batteryStateMachines[i] = val.GetComponent<EntityStateMachine>();
		}
		elevatorStateMachines = new EntityStateMachine[elevators.Length];
		for (int j = 0; j < elevators.Length; j++)
		{
			elevatorStateMachines[j] = elevators[j].GetComponent<EntityStateMachine>();
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<MoonBatteryMissionController>(instance, this);
		MoonBatteryMissionController.onInstanceChangedGlobal?.Invoke();
		ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
		for (int i = 0; i < batteryHoldoutZones.Length; i++)
		{
			((UnityEvent<HoldoutZoneController>)batteryHoldoutZones[i].onCharged).AddListener((UnityAction<HoldoutZoneController>)OnBatteryCharged);
		}
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<MoonBatteryMissionController>(instance, this);
		MoonBatteryMissionController.onInstanceChangedGlobal?.Invoke();
		ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
		for (int i = 0; i < batteryHoldoutZones.Length; i++)
		{
			((UnityEvent<HoldoutZoneController>)batteryHoldoutZones[i].onCharged).RemoveListener((UnityAction<HoldoutZoneController>)OnBatteryCharged);
		}
	}

	private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
	{
		if (_numChargedBatteries > 0 && _numChargedBatteries < _numRequiredBatteries)
		{
			objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
			{
				master = master,
				objectiveType = typeof(MoonBatteryMissionObjectiveTracker),
				source = (Object)(object)this
			});
		}
	}

	private void OnBatteryCharged(HoldoutZoneController holdoutZone)
	{
		Network_numChargedBatteries = _numChargedBatteries + 1;
		if (_numChargedBatteries < _numRequiredBatteries || !NetworkServer.active)
		{
			return;
		}
		for (int i = 0; i < batteryHoldoutZones.Length; i++)
		{
			if (((Behaviour)batteryHoldoutZones[i]).enabled)
			{
				batteryHoldoutZones[i].FullyChargeHoldoutZone();
				((UnityEvent<HoldoutZoneController>)batteryHoldoutZones[i].onCharged).RemoveListener((UnityAction<HoldoutZoneController>)OnBatteryCharged);
			}
		}
		batteryHoldoutZones = new HoldoutZoneController[0];
		for (int j = 0; j < batteryStateMachines.Length; j++)
		{
			if (!(batteryStateMachines[j].state is MoonBatteryComplete))
			{
				batteryStateMachines[j].SetNextState(new MoonBatteryDisabled());
			}
		}
		for (int k = 0; k < elevatorStateMachines.Length; k++)
		{
			elevatorStateMachines[k].SetNextState(new InactiveToReady());
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)_numChargedBatteries);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)_numChargedBatteries);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			_numChargedBatteries = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_numChargedBatteries = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
