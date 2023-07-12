using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

public class CarouselController : BaseSettingsControl
{
	[Serializable]
	public struct Choice
	{
		public string suboptionDisplayToken;

		public string convarValue;

		public Sprite customSprite;
	}

	public GameObject leftArrowButton;

	public GameObject rightArrowButton;

	public Image optionalImage;

	public TextMeshProUGUI optionalText;

	[SerializeField]
	[NotNull]
	[FormerlySerializedAs("choices")]
	private Choice[] _choices = Array.Empty<Choice>();

	public bool forceValidChoice;

	private int selectionIndex;

	[NotNull]
	public Choice[] choices
	{
		get
		{
			return _choices;
		}
		set
		{
			_choices = value;
			UpdateFromCurrentValue();
		}
	}

	protected override void OnUpdateControls()
	{
		UpdateFromCurrentValue();
	}

	public void MoveCarousel(int direction)
	{
		selectionIndex = Mathf.Clamp(selectionIndex + direction, 0, choices.Length - 1);
		UpdateFromSelectionIndex();
		SubmitSetting(choices[selectionIndex].convarValue);
	}

	public void BoolCarousel()
	{
		selectionIndex = ((selectionIndex == 0) ? 1 : 0);
		UpdateFromSelectionIndex();
		SubmitSetting(choices[selectionIndex].convarValue);
	}

	private void UpdateFromCurrentValue()
	{
		string currentValue = GetCurrentValue();
		bool flag = false;
		for (int i = 0; i < choices.Length; i++)
		{
			if (choices[i].convarValue == currentValue)
			{
				flag = true;
				selectionIndex = i;
				break;
			}
		}
		if (!flag && forceValidChoice)
		{
			selectionIndex = 0;
		}
		UpdateFromSelectionIndex();
	}

	private void UpdateFromSelectionIndex()
	{
		string text = "OPTION_CUSTOM";
		Sprite sprite = null;
		if (0 <= selectionIndex && selectionIndex < choices.Length)
		{
			Choice choice = choices[selectionIndex];
			text = choice.suboptionDisplayToken;
			sprite = choice.customSprite;
		}
		else if (choices.Length == 0)
		{
			text = string.Empty;
		}
		if (Object.op_Implicit((Object)(object)optionalText))
		{
			((Component)optionalText).GetComponent<LanguageTextMeshController>().token = text ?? string.Empty;
		}
		if (Object.op_Implicit((Object)(object)optionalImage))
		{
			optionalImage.sprite = sprite;
		}
	}

	protected override void Update()
	{
		base.Update();
		bool active = true;
		bool active2 = true;
		if (selectionIndex == 0)
		{
			active = false;
		}
		if (selectionIndex == choices.Length - 1)
		{
			active2 = false;
		}
		if (Object.op_Implicit((Object)(object)leftArrowButton))
		{
			leftArrowButton.SetActive(active);
		}
		if (Object.op_Implicit((Object)(object)rightArrowButton))
		{
			rightArrowButton.SetActive(active2);
		}
	}
}
