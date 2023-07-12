using UnityEngine;

namespace RoR2.Navigation;

public class GateStateSetter : MonoBehaviour
{
	public string gateToEnableWhenEnabled;

	public string gateToDisableWhenEnabled;

	private void OnEnable()
	{
		UpdateGates(enabledState: true);
	}

	private void OnDisable()
	{
		UpdateGates(enabledState: false);
	}

	private void UpdateGates(bool enabledState)
	{
		if (Object.op_Implicit((Object)(object)SceneInfo.instance))
		{
			if (!string.IsNullOrEmpty(gateToEnableWhenEnabled))
			{
				SceneInfo.instance.SetGateState(gateToEnableWhenEnabled, enabledState);
			}
			if (!string.IsNullOrEmpty(gateToDisableWhenEnabled))
			{
				SceneInfo.instance.SetGateState(gateToDisableWhenEnabled, !enabledState);
			}
		}
	}
}
