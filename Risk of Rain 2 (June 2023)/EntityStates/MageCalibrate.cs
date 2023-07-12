using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class MageCalibrate : BaseState
{
	public MageElement element;

	public MageCalibrationController calibrationController;

	private bool shouldApply;

	public override void OnEnter()
	{
		calibrationController = GetComponent<MageCalibrationController>();
		shouldApply = NetworkServer.active;
		base.OnEnter();
	}

	public override void OnExit()
	{
		ApplyElement();
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		outer.SetNextStateToMain();
	}

	private void ApplyElement()
	{
		Debug.Log((object)"MageCalibrate.ApplyElement()");
		if (shouldApply && Object.op_Implicit((Object)(object)calibrationController))
		{
			shouldApply = false;
			calibrationController.SetElement(element);
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		writer.Write((byte)element);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		element = (MageElement)reader.ReadByte();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
