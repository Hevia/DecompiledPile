using System.Collections.Generic;
using UnityEngine;

public class FriendSessionBrowserController : MonoBehaviour
{
	public SessionButtonController SessionButtonPrefab;

	public Transform SessionButtonContainer;

	public RectTransform InProgressSpinner;

	private List<SessionButtonController> sessionButtons;
}
