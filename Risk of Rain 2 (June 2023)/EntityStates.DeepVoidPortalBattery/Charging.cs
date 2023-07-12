using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DeepVoidPortalBattery;

public class Charging : BaseDeepVoidPortalBatteryState
{
	[SerializeField]
	public GameObject chargingPositionIndicator;

	private CombatDirector combatDirector;

	private HoldoutZoneController holdoutZoneController;

	private VoidStageMissionController.FogRequest fogRequest;

	private ChargeIndicatorController chargeIndicatorController;

	public override void OnEnter()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		holdoutZoneController = GetComponent<HoldoutZoneController>();
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			((Behaviour)holdoutZoneController).enabled = true;
		}
		Transform targetTransform = ((Component)FindModelChild("PositionIndicatorPosition")).transform;
		PositionIndicator component = Object.Instantiate<GameObject>(chargingPositionIndicator, base.transform.position, Quaternion.identity).GetComponent<PositionIndicator>();
		component.targetTransform = targetTransform;
		chargeIndicatorController = ((Component)component).GetComponent<ChargeIndicatorController>();
		chargeIndicatorController.holdoutZoneController = holdoutZoneController;
		if (NetworkServer.active)
		{
			combatDirector = GetComponent<CombatDirector>();
			if (Object.op_Implicit((Object)(object)combatDirector))
			{
				((Behaviour)combatDirector).enabled = true;
				combatDirector.SetNextSpawnAsBoss();
			}
			if (Object.op_Implicit((Object)(object)holdoutZoneController) && Object.op_Implicit((Object)(object)VoidStageMissionController.instance))
			{
				fogRequest = VoidStageMissionController.instance.RequestFog(holdoutZoneController);
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeIndicatorController))
		{
			EntityState.Destroy((Object)(object)((Component)chargeIndicatorController).gameObject);
		}
		if (Object.op_Implicit((Object)(object)holdoutZoneController))
		{
			((Behaviour)holdoutZoneController).enabled = false;
		}
		if (NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)combatDirector))
			{
				((Behaviour)combatDirector).enabled = false;
			}
			fogRequest?.Dispose();
			fogRequest = null;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && holdoutZoneController.charge >= 1f)
		{
			outer.SetNextState(new Charged());
		}
	}
}
