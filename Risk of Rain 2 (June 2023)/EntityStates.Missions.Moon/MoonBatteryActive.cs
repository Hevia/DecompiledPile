using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Moon;

public class MoonBatteryActive : MoonBatteryBaseState
{
	public static string soundEntryEvent;

	public static string soundLoopStartEvent;

	public static string soundLoopEndEvent;

	public static string soundExitEvent;

	public static string activeTriggerName;

	public static string completeTriggerName;

	private HoldoutZoneController holdoutZoneController;

	private ChargeIndicatorController chargeIndicatorController;

	public override void OnEnter()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		holdoutZoneController = GetComponent<HoldoutZoneController>();
		((Behaviour)holdoutZoneController).enabled = true;
		if (NetworkServer.active)
		{
			((Behaviour)GetComponent<CombatDirector>()).enabled = true;
		}
		Animator[] array = animators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTrigger(activeTriggerName);
		}
		Util.PlaySound(soundEntryEvent, base.gameObject);
		Util.PlaySound(soundLoopStartEvent, base.gameObject);
		((Component)FindModelChild("ChargingFX")).gameObject.SetActive(true);
		Transform targetTransform = ((Component)FindModelChild("PositionIndicatorPosition")).transform;
		PositionIndicator component = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/PositionIndicators/PillarChargingPositionIndicator"), base.transform.position, Quaternion.identity).GetComponent<PositionIndicator>();
		component.targetTransform = targetTransform;
		chargeIndicatorController = ((Component)component).GetComponent<ChargeIndicatorController>();
		chargeIndicatorController.holdoutZoneController = holdoutZoneController;
	}

	public override void Update()
	{
		base.Update();
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			Animator[] array = animators;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("Active.cycleOffset", holdoutZoneController.charge * 0.99f);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && holdoutZoneController.charge >= 1f)
		{
			outer.SetNextState(new MoonBatteryComplete());
		}
	}

	public override void OnExit()
	{
		((Component)FindModelChild("ChargingFX")).gameObject.SetActive(false);
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			((Behaviour)holdoutZoneController).enabled = false;
		}
		Animator[] array = animators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTrigger(completeTriggerName);
		}
		Util.PlaySound(soundLoopEndEvent, base.gameObject);
		Util.PlaySound(soundExitEvent, base.gameObject);
		if (Object.op_Implicit((Object)(object)chargeIndicatorController))
		{
			EntityState.Destroy((Object)(object)((Component)chargeIndicatorController).gameObject);
		}
		base.OnExit();
	}
}
