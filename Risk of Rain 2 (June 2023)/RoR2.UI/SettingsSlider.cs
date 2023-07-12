using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

public class SettingsSlider : BaseSettingsControl
{
	public Slider slider;

	public HGTextMeshProUGUI valueText;

	public float minValue;

	public float maxValue;

	public string formatString = "{0:0.00}";

	protected new void Awake()
	{
		base.Awake();
		if (Object.op_Implicit((Object)(object)slider))
		{
			slider.minValue = minValue;
			slider.maxValue = maxValue;
			((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)OnSliderValueChanged);
		}
	}

	private void OnSliderValueChanged(float newValue)
	{
		if (!base.inUpdateControls)
		{
			SubmitSetting(TextSerialization.ToStringInvariant(newValue));
		}
	}

	private void OnInputFieldSubmit(string newString)
	{
		if (!base.inUpdateControls && TextSerialization.TryParseInvariant(newString, out float result))
		{
			SubmitSetting(TextSerialization.ToStringInvariant(result));
		}
	}

	protected override void OnUpdateControls()
	{
		base.OnUpdateControls();
		if (TextSerialization.TryParseInvariant(GetCurrentValue(), out float result))
		{
			float num = Mathf.Clamp(result, minValue, maxValue);
			if (Object.op_Implicit((Object)(object)slider))
			{
				slider.value = num;
			}
			if (Object.op_Implicit((Object)(object)valueText))
			{
				((TMP_Text)valueText).text = string.Format(CultureInfo.InvariantCulture, formatString, num);
			}
		}
	}

	public void MoveSlider(float delta)
	{
		if (Object.op_Implicit((Object)(object)slider))
		{
			slider.normalizedValue += delta;
		}
	}
}
