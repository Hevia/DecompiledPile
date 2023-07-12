using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPButton))]
public class UserProfileListElementController : MonoBehaviour
{
	public TextMeshProUGUI nameLabel;

	private MPButton button;

	public TextMeshProUGUI playTimeLabel;

	[NonSerialized]
	public UserProfileListController listController;

	private UserProfile _userProfile;

	public UserProfile userProfile
	{
		get
		{
			return _userProfile;
		}
		set
		{
			if (_userProfile != value)
			{
				_userProfile = value;
				string text = "???";
				uint num = 0u;
				if (_userProfile != null)
				{
					text = _userProfile.name;
					num = _userProfile.totalLoginSeconds;
				}
				if (Object.op_Implicit((Object)(object)nameLabel))
				{
					((TMP_Text)nameLabel).SetText(text, true);
				}
				if (Object.op_Implicit((Object)(object)playTimeLabel))
				{
					TimeSpan timeSpan = TimeSpan.FromSeconds(num);
					((TMP_Text)playTimeLabel).SetText($"{(uint)timeSpan.TotalHours}:{(uint)timeSpan.Minutes:D2}", true);
				}
			}
		}
	}

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		button = ((Component)this).GetComponent<MPButton>();
		((UnityEvent)((Button)button).onClick).AddListener(new UnityAction(InformListControllerOfSelection));
	}

	private void InformListControllerOfSelection()
	{
		if (!userProfile.isCorrupted)
		{
			listController.SendProfileSelection(userProfile);
		}
	}
}
