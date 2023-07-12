using System;
using EntityStates.InfiniteTowerSafeWard;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public class InfiniteTowerSafeWardController : MonoBehaviour
{
	[SerializeField]
	private EntityStateMachine wardStateMachine;

	[SerializeField]
	private VerticalTubeZone _safeZone;

	[SerializeField]
	private GameObject positionIndicatorPrefab;

	[SerializeField]
	private HoldoutZoneController holdoutZoneController;

	private PositionIndicator positionIndicator;

	private ChargeIndicatorController chargeIndicatorController;

	public IZone safeZone => _safeZone;

	public bool isAwaitingInteraction
	{
		get
		{
			if (Object.op_Implicit((Object)(object)wardStateMachine))
			{
				return wardStateMachine.state is AwaitingActivation;
			}
			return false;
		}
	}

	public event Action<InfiniteTowerSafeWardController> onActivated;

	private void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)positionIndicatorPrefab))
		{
			return;
		}
		GameObject val = Object.Instantiate<GameObject>(positionIndicatorPrefab, ((Component)this).transform.position, Quaternion.identity);
		if (Object.op_Implicit((Object)(object)val))
		{
			positionIndicator = val.GetComponent<PositionIndicator>();
			if (Object.op_Implicit((Object)(object)positionIndicator))
			{
				positionIndicator.targetTransform = ((Component)this).transform;
			}
			chargeIndicatorController = val.GetComponent<ChargeIndicatorController>();
			if (Object.op_Implicit((Object)(object)chargeIndicatorController))
			{
				chargeIndicatorController.holdoutZoneController = holdoutZoneController;
			}
			val.SetActive(false);
		}
	}

	public void Activate()
	{
		if (Object.op_Implicit((Object)(object)wardStateMachine) && wardStateMachine.state is AwaitingActivation awaitingActivation)
		{
			awaitingActivation.Activate();
			this.onActivated?.Invoke(this);
		}
	}

	public void SelfDestruct()
	{
		if (Object.op_Implicit((Object)(object)wardStateMachine) && wardStateMachine.state is Active active)
		{
			active.SelfDestruct();
		}
	}

	public void RandomizeLocation(Xoroshiro128Plus rng)
	{
		if (Object.op_Implicit((Object)(object)wardStateMachine))
		{
			wardStateMachine.SetNextState(new Unburrow(rng));
		}
	}

	public void WaitForPortal()
	{
		if (Object.op_Implicit((Object)(object)wardStateMachine))
		{
			wardStateMachine.SetNextState(new AwaitingPortalUse());
		}
	}

	public void SetIndicatorEnabled(bool enabled)
	{
		if (Object.op_Implicit((Object)(object)positionIndicator))
		{
			((Component)positionIndicator).gameObject.SetActive(enabled);
		}
	}
}
