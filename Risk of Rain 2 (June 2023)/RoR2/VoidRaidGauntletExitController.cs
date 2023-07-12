using UnityEngine;

namespace RoR2;

public class VoidRaidGauntletExitController : MonoBehaviour
{
	[SerializeField]
	private MapZone exitZone;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)exitZone))
		{
			exitZone.onBodyTeleport += OnBodyTeleport;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)exitZone))
		{
			exitZone.onBodyTeleport -= OnBodyTeleport;
		}
	}

	private void OnBodyTeleport(CharacterBody body)
	{
		if (Util.HasEffectiveAuthority(((Component)body).gameObject) && Object.op_Implicit((Object)(object)body.masterObject.GetComponent<PlayerCharacterMasterController>()) && Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance))
		{
			VoidRaidGauntletController.instance.OnAuthorityPlayerExit();
		}
	}
}
