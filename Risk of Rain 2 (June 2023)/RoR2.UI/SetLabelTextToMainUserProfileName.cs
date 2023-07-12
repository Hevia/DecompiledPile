using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SetLabelTextToMainUserProfileName : MonoBehaviour
{
	private TextMeshProUGUI label;

	private void Awake()
	{
		label = ((Component)this).GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		Apply();
	}

	private void Apply()
	{
		LocalUser localUser = LocalUserManager.FindLocalUser(0);
		if (localUser != null)
		{
			string name = localUser.userProfile.name;
			((TMP_Text)label).text = string.Format(Language.GetString("TITLE_PROFILE"), name);
		}
		else
		{
			((TMP_Text)label).text = "NO USER";
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		Language.onCurrentLanguageChanged += OnCurrentLanguageChanged;
	}

	private static void OnCurrentLanguageChanged()
	{
		SetLabelTextToMainUserProfileName[] array = Object.FindObjectsOfType<SetLabelTextToMainUserProfileName>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply();
		}
	}
}
