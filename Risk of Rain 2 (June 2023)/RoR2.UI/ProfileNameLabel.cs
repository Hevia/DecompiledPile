using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class ProfileNameLabel : MonoBehaviour
{
	public string token;

	private MPEventSystemLocator eventSystemLocator;

	private TextMeshProUGUI label;

	private string currentUserName;

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		label = ((Component)this).GetComponent<TextMeshProUGUI>();
	}

	private void LateUpdate()
	{
		string text = eventSystemLocator.eventSystem?.localUser?.userProfile.name ?? string.Empty;
		if (text != currentUserName)
		{
			currentUserName = text;
			((TMP_Text)label).text = Language.GetStringFormatted(token, currentUserName);
		}
	}
}
