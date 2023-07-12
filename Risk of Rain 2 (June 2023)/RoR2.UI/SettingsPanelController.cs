using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

public class SettingsPanelController : MonoBehaviour
{
	[FormerlySerializedAs("carouselControllers")]
	private BaseSettingsControl[] settingsControllers;

	public MPButton revertButton;

	private void Start()
	{
		settingsControllers = ((Component)this).GetComponentsInChildren<BaseSettingsControl>();
	}

	private void Update()
	{
		bool interactable = false;
		if (settingsControllers != null)
		{
			for (int i = 0; i < settingsControllers.Length; i++)
			{
				BaseSettingsControl baseSettingsControl = settingsControllers[i];
				if (Object.op_Implicit((Object)(object)baseSettingsControl) && baseSettingsControl.hasBeenChanged)
				{
					interactable = true;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)revertButton))
		{
			((Selectable)revertButton).interactable = interactable;
		}
	}

	public void RevertChanges()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			for (int i = 0; i < settingsControllers.Length; i++)
			{
				settingsControllers[i].Revert();
			}
		}
	}
}
