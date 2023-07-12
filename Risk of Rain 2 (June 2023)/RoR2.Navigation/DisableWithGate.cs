using UnityEngine;

namespace RoR2.Navigation;

public class DisableWithGate : MonoBehaviour
{
	public string gateToMatch;

	public bool invert;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)SceneInfo.instance) && SceneInfo.instance.groundNodes.IsGateOpen(gateToMatch) == invert)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
