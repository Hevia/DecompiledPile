using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SteamBuildIdLabel : MonoBehaviour
{
	private void Start()
	{
		string text = "ver. " + RoR2Application.GetBuildId();
		if (!string.IsNullOrEmpty(""))
		{
			text += ".";
		}
		((TMP_Text)((Component)this).GetComponent<TextMeshProUGUI>()).text = text;
	}
}
