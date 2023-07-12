using System.Text;
using HG;
using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelText : MonoBehaviour
{
	public CharacterBody source;

	public TextMeshProUGUI targetText;

	private uint displayData;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private void SetDisplayData(uint newDisplayData)
	{
		if (displayData != newDisplayData)
		{
			displayData = newDisplayData;
			uint value = displayData;
			sharedStringBuilder.Clear();
			sharedStringBuilder.AppendUint(value);
			((TMP_Text)targetText).SetText(sharedStringBuilder);
		}
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)source))
		{
			SetDisplayData(Convert.FloorToUIntClamped(source.level));
		}
	}

	private void OnValidate()
	{
		if (!Object.op_Implicit((Object)(object)targetText))
		{
			Debug.LogError((object)"targetText must be assigned.");
		}
	}
}
