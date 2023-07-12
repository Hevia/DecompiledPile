using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class LocalUserSignInCardController : MonoBehaviour
{
	public TextMeshProUGUI nameLabel;

	public TextMeshProUGUI promptLabel;

	public Image cardImage;

	public Sprite playerCardNone;

	public Sprite playerCardKBM;

	public Sprite playerCardController;

	public Color unselectedColor;

	public Color selectedColor;

	private UserProfileListController userProfileSelectionList;

	public GameObject userProfileSelectionListPrefab;

	private Player _rewiredPlayer;

	private UserProfile _requestedUserProfile;

	public Player rewiredPlayer
	{
		get
		{
			return _rewiredPlayer;
		}
		set
		{
			if (_rewiredPlayer != value)
			{
				_rewiredPlayer = value;
				if (_rewiredPlayer == null)
				{
					requestedUserProfile = null;
				}
			}
		}
	}

	public UserProfile requestedUserProfile
	{
		get
		{
			return _requestedUserProfile;
		}
		private set
		{
			if (_requestedUserProfile != value)
			{
				if (_requestedUserProfile != null)
				{
					_requestedUserProfile.isClaimed = false;
				}
				_requestedUserProfile = value;
				if (_requestedUserProfile != null)
				{
					_requestedUserProfile.isClaimed = true;
				}
			}
		}
	}

	private void Update()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if (requestedUserProfile != null != Object.op_Implicit((Object)(object)userProfileSelectionList))
		{
			if (!Object.op_Implicit((Object)(object)userProfileSelectionList))
			{
				GameObject val = Object.Instantiate<GameObject>(userProfileSelectionListPrefab, ((Component)this).transform);
				userProfileSelectionList = val.GetComponent<UserProfileListController>();
				((Component)userProfileSelectionList).GetComponent<MPEventSystemProvider>().eventSystem = MPEventSystemManager.FindEventSystem(rewiredPlayer);
				userProfileSelectionList.onProfileSelected += OnUserSelectedUserProfile;
			}
			else
			{
				Object.Destroy((Object)(object)((Component)userProfileSelectionList).gameObject);
				userProfileSelectionList = null;
			}
		}
		if (rewiredPlayer != null)
		{
			((Graphic)cardImage).color = selectedColor;
			((Component)nameLabel).gameObject.SetActive(true);
			if (requestedUserProfile == null)
			{
				cardImage.sprite = playerCardNone;
				((TMP_Text)nameLabel).text = "";
				((TMP_Text)promptLabel).text = "...";
			}
			else
			{
				cardImage.sprite = playerCardKBM;
				((TMP_Text)nameLabel).text = requestedUserProfile.name;
				((TMP_Text)promptLabel).text = "";
			}
		}
		else
		{
			((Component)nameLabel).gameObject.SetActive(false);
			((TMP_Text)promptLabel).text = "Press 'Start'";
			((Graphic)cardImage).color = unselectedColor;
			cardImage.sprite = playerCardNone;
		}
	}

	private void OnUserSelectedUserProfile(UserProfile userProfile)
	{
		requestedUserProfile = userProfile;
	}
}
