using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2.UI;

public class ResolutionControl : BaseSettingsControl
{
	private class ResolutionOption
	{
		public Vector2Int size;

		public readonly List<int> supportedRefreshRates = new List<int>();

		public string GenerateDisplayString()
		{
			return $"{((Vector2Int)(ref size)).x}x{((Vector2Int)(ref size)).y}";
		}
	}

	public MPDropdown resolutionDropdown;

	public MPDropdown refreshRateDropdown;

	private Resolution[] resolutions;

	private ResolutionOption[] resolutionOptions = Array.Empty<ResolutionOption>();

	private static Vector2Int ResolutionToVector2Int(Resolution resolution)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(((Resolution)(ref resolution)).width, ((Resolution)(ref resolution)).height);
	}

	private ResolutionOption GetCurrentSelectedResolutionOption()
	{
		if (((TMP_Dropdown)resolutionDropdown).value >= 0)
		{
			return resolutionOptions[((TMP_Dropdown)resolutionDropdown).value];
		}
		return null;
	}

	private void GenerateResolutionOptions()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] array = Screen.resolutions;
		resolutionOptions = (from v in array.Select(ResolutionToVector2Int).Distinct()
			select new ResolutionOption
			{
				size = v
			}).ToArray();
		ResolutionOption[] array2 = resolutionOptions;
		foreach (ResolutionOption resolutionOption in array2)
		{
			Resolution[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				Resolution resolution = array3[j];
				if (ResolutionToVector2Int(resolution) == resolutionOption.size)
				{
					resolutionOption.supportedRefreshRates.Add(((Resolution)(ref resolution)).refreshRate);
				}
			}
		}
		List<OptionData> list = new List<OptionData>();
		array2 = resolutionOptions;
		foreach (ResolutionOption resolutionOption2 in array2)
		{
			list.Add(new OptionData
			{
				text = resolutionOption2.GenerateDisplayString()
			});
		}
		((TMP_Dropdown)resolutionDropdown).ClearOptions();
		((TMP_Dropdown)resolutionDropdown).AddOptions(list);
		int value = -1;
		Vector2Int val = ResolutionToVector2Int(Screen.currentResolution);
		for (int k = 0; k < resolutionOptions.Length; k++)
		{
			if (val == resolutionOptions[k].size)
			{
				value = k;
				break;
			}
		}
		((TMP_Dropdown)resolutionDropdown).value = value;
	}

	private void GenerateRefreshRateOptions()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Dropdown)refreshRateDropdown).ClearOptions();
		ResolutionOption currentSelectedResolutionOption = GetCurrentSelectedResolutionOption();
		if (currentSelectedResolutionOption == null)
		{
			return;
		}
		List<OptionData> list = new List<OptionData>();
		foreach (int supportedRefreshRate in currentSelectedResolutionOption.supportedRefreshRates)
		{
			list.Add(new OptionData(supportedRefreshRate + "Hz"));
		}
		((TMP_Dropdown)refreshRateDropdown).AddOptions(list);
		List<int> supportedRefreshRates = currentSelectedResolutionOption.supportedRefreshRates;
		Resolution currentResolution = Screen.currentResolution;
		int num = supportedRefreshRates.IndexOf(((Resolution)(ref currentResolution)).refreshRate);
		if (num == -1)
		{
			num = currentSelectedResolutionOption.supportedRefreshRates.Count - 1;
		}
		((TMP_Dropdown)refreshRateDropdown).value = num;
	}

	protected new void Awake()
	{
		base.Awake();
		((UnityEvent<int>)(object)((TMP_Dropdown)resolutionDropdown).onValueChanged).AddListener((UnityAction<int>)OnResolutionDropdownValueChanged);
		((UnityEvent<int>)(object)((TMP_Dropdown)refreshRateDropdown).onValueChanged).AddListener((UnityAction<int>)OnRefreshRateDropdownValueChanged);
	}

	protected new void OnEnable()
	{
		base.OnEnable();
		GenerateResolutionOptions();
		GenerateRefreshRateOptions();
	}

	private void OnResolutionDropdownValueChanged(int newValue)
	{
		if (newValue >= 0)
		{
			GenerateRefreshRateOptions();
		}
	}

	private void OnRefreshRateDropdownValueChanged(int newValue)
	{
		_ = 0;
	}

	public void SubmitCurrentValue()
	{
		if (((TMP_Dropdown)resolutionDropdown).value != -1 && ((TMP_Dropdown)refreshRateDropdown).value != -1)
		{
			ResolutionOption resolutionOption = resolutionOptions[((TMP_Dropdown)resolutionDropdown).value];
			SubmitSetting(string.Format(CultureInfo.InvariantCulture, "{0}x{1}x{2}", ((Vector2Int)(ref resolutionOption.size)).x, ((Vector2Int)(ref resolutionOption.size)).y, resolutionOption.supportedRefreshRates[((TMP_Dropdown)refreshRateDropdown).value]));
		}
	}
}
