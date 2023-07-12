using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class SpectatorLabel : MonoBehaviour
{
	public HUD hud;

	public HGTextMeshProUGUI label;

	public GameObject labelRoot;

	private GameObject cachedTarget;

	private HudElement hudElement;

	private void Awake()
	{
		labelRoot.SetActive(false);
	}

	private void Update()
	{
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		CameraRigController cameraRigController = hud.cameraRigController;
		GameObject val = null;
		GameObject val2 = null;
		if (Object.op_Implicit((Object)(object)cameraRigController))
		{
			val = cameraRigController.target;
			val2 = cameraRigController.localUserViewer.cachedBodyObject;
		}
		if (val == val2 || !((Object)(object)val != (Object)null))
		{
			labelRoot.SetActive(false);
			cachedTarget = null;
			return;
		}
		labelRoot.SetActive(true);
		if (cachedTarget != val)
		{
			string text = (Object.op_Implicit((Object)(object)val) ? Util.GetBestBodyName(val) : "");
			((TMP_Text)label).SetText(Language.GetStringFormatted("SPECTATING_NAME_FORMAT", text), true);
			cachedTarget = val;
		}
	}
}
