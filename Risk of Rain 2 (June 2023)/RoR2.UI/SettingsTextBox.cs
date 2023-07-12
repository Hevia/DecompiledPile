using TMPro;
using UnityEngine.Events;

namespace RoR2.UI;

public class SettingsTextBox : BaseSettingsControl
{
	public TMP_InputField textbox;

	protected new void OnEnable()
	{
		base.OnEnable();
		TMP_InputField obj = textbox;
		if (obj != null)
		{
			((UnityEvent<string>)(object)obj.onValueChanged)?.AddListener((UnityAction<string>)OnTextBoxValueChanged);
		}
	}

	protected void OnDisable()
	{
		TMP_InputField obj = textbox;
		if (obj != null)
		{
			((UnityEvent<string>)(object)obj.onValueChanged)?.RemoveListener((UnityAction<string>)OnTextBoxValueChanged);
		}
	}

	private void OnTextBoxValueChanged(string newValue)
	{
		SubmitSetting(newValue);
	}

	protected override void OnUpdateControls()
	{
		string currentValue = GetCurrentValue();
		TMP_InputField obj = textbox;
		if (obj != null)
		{
			obj.SetTextWithoutNotify(currentValue);
		}
	}
}
