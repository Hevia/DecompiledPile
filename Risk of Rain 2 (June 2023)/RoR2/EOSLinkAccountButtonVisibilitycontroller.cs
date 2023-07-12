using Epic.OnlineServices;
using UnityEngine;

namespace RoR2;

[DefaultExecutionOrder(-5)]
public class EOSLinkAccountButtonVisibilitycontroller : MonoBehaviour
{
	public GameObject egsLinkAccountButton;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)egsLinkAccountButton))
		{
			if (PlatformSystems.ShouldUseEpicOnlineSystems && (Handle)(object)EOSLoginManager.loggedInAuthId == (Handle)null)
			{
				egsLinkAccountButton.SetActive(true);
			}
			else
			{
				egsLinkAccountButton.SetActive(false);
			}
		}
	}
}
