using UnityEngine;
using UnityEngine.Events;

namespace RoR2.EntityLogic;

public class TeleporterEventRelay : MonoBehaviour
{
	public UnityEvent onTeleporterBeginCharging;

	public UnityEvent onTeleporterCharged;

	public UnityEvent onTeleporterFinish;

	private TeleporterInteraction.ActivationState recordedActivationState;

	public void OnEnable()
	{
		TeleporterInteraction.onTeleporterBeginChargingGlobal += CheckTeleporterBeginCharging;
		TeleporterInteraction.onTeleporterChargedGlobal += CheckTeleporterCharged;
		TeleporterInteraction.onTeleporterFinishGlobal += CheckTeleporterFinish;
		CheckTeleporterBeginCharging(TeleporterInteraction.instance);
		CheckTeleporterCharged(TeleporterInteraction.instance);
		CheckTeleporterFinish(TeleporterInteraction.instance);
	}

	public void OnDisable()
	{
		TeleporterInteraction.onTeleporterBeginChargingGlobal -= CheckTeleporterBeginCharging;
		TeleporterInteraction.onTeleporterChargedGlobal -= CheckTeleporterCharged;
		TeleporterInteraction.onTeleporterFinishGlobal -= CheckTeleporterFinish;
	}

	private void CheckTeleporterBeginCharging(TeleporterInteraction teleporter)
	{
		if (Object.op_Implicit((Object)(object)teleporter) && teleporter.activationState >= TeleporterInteraction.ActivationState.Charging && recordedActivationState < TeleporterInteraction.ActivationState.Charging)
		{
			UnityEvent obj = onTeleporterBeginCharging;
			if (obj != null)
			{
				obj.Invoke();
			}
			recordedActivationState = TeleporterInteraction.ActivationState.Charging;
		}
	}

	private void CheckTeleporterCharged(TeleporterInteraction teleporter)
	{
		if (Object.op_Implicit((Object)(object)teleporter) && teleporter.activationState >= TeleporterInteraction.ActivationState.Charged && recordedActivationState < TeleporterInteraction.ActivationState.Charged)
		{
			UnityEvent obj = onTeleporterCharged;
			if (obj != null)
			{
				obj.Invoke();
			}
			recordedActivationState = TeleporterInteraction.ActivationState.Charged;
		}
	}

	private void CheckTeleporterFinish(TeleporterInteraction teleporter)
	{
		if (Object.op_Implicit((Object)(object)teleporter) && teleporter.activationState >= TeleporterInteraction.ActivationState.Finished && recordedActivationState < TeleporterInteraction.ActivationState.Finished)
		{
			UnityEvent obj = onTeleporterFinish;
			if (obj != null)
			{
				obj.Invoke();
			}
			recordedActivationState = TeleporterInteraction.ActivationState.Finished;
		}
	}
}
