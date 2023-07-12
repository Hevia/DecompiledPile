using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SocialUsernameLabel : MonoBehaviour
{
	protected TextMeshProUGUI textMeshComponent;

	private UserID _userId;

	public int subPlayerIndex;

	public UserID userId
	{
		get
		{
			return _userId;
		}
		set
		{
			_userId = value;
		}
	}

	private void Awake()
	{
		textMeshComponent = ((Component)this).GetComponent<TextMeshProUGUI>();
	}

	public virtual void Refresh()
	{
		if ((Object)(object)textMeshComponent != (Object)null)
		{
			((TMP_Text)textMeshComponent).text = PlatformSystems.lobbyManager.GetUserDisplayName(_userId);
		}
	}
}
