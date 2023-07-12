using EntityStates;
using UnityEngine;

namespace RoR2;

public class AimTurnStateController : MonoBehaviour
{
	[Tooltip("The component we use to determine the current orientation")]
	[SerializeField]
	private CharacterDirection characterDirection;

	[Tooltip("The component we use to determine the current aim")]
	[SerializeField]
	private InputBankTest inputBank;

	[Tooltip("The state machine we should modify")]
	[SerializeField]
	private EntityStateMachine targetStateMachine;

	[Tooltip("The state we should push")]
	[SerializeField]
	private SerializableEntityStateType turnStateType;

	[SerializeField]
	[Tooltip("The priority of the new state")]
	private InterruptPriority interruptPriority;

	[Tooltip("The minimum difference between the current orientation and the aim before we should push the state")]
	[SerializeField]
	private float minTriggerDegrees;

	[Tooltip("The minimum time before we should push the state again")]
	[SerializeField]
	private float retriggerDelaySeconds;

	[Tooltip("The aim/direction vectors are multiplied by this vector and normalized before comparison.  This can be used to exclude a dimension from the calculation.")]
	[SerializeField]
	private Vector3 aimScale = new Vector3(1f, 1f, 1f);

	private float lastTriggerTime;

	private void FixedUpdate()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!(Run.instance.fixedTime - lastTriggerTime > retriggerDelaySeconds))
		{
			return;
		}
		Vector3 aimDirection = inputBank.aimDirection;
		((Vector3)(ref aimDirection)).Scale(aimScale);
		((Vector3)(ref aimDirection)).Normalize();
		Vector3 forward = characterDirection.forward;
		((Vector3)(ref forward)).Scale(aimScale);
		((Vector3)(ref forward)).Normalize();
		if (Vector3.Angle(aimDirection, forward) > minTriggerDegrees)
		{
			lastTriggerTime = Run.instance.fixedTime;
			if (Object.op_Implicit((Object)(object)targetStateMachine))
			{
				EntityState newNextState = EntityStateCatalog.InstantiateState(turnStateType);
				targetStateMachine.SetInterruptState(newNextState, interruptPriority);
			}
		}
	}
}
