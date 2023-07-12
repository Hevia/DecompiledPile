using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class UserProfileListController : MonoBehaviour
{
	public delegate void ProfileSelectedDelegate(UserProfile userProfile);

	public GameObject elementPrefab;

	public RectTransform contentRect;

	[Tooltip("Whether or not \"default\" profile appears as a selectable option.")]
	public bool allowDefault = true;

	private MPEventSystemLocator eventSystemLocator;

	private readonly List<UserProfileListElementController> elementsList = new List<UserProfileListElementController>();

	private EventSystem eventSystem => (EventSystem)(object)eventSystemLocator.eventSystem;

	public event ProfileSelectedDelegate onProfileSelected;

	public event Action onListRebuilt;

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private void OnEnable()
	{
		RebuildElements();
		SaveSystem.onAvailableUserProfilesChanged += RebuildElements;
	}

	private void OnDisable()
	{
		SaveSystem.onAvailableUserProfilesChanged -= RebuildElements;
	}

	private void RebuildElements()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in (Transform)contentRect)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		elementsList.Clear();
		List<string> availableProfileNames = PlatformSystems.saveSystem.GetAvailableProfileNames();
		for (int i = 0; i < availableProfileNames.Count; i++)
		{
			if (allowDefault || !availableProfileNames[i].Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				GameObject obj = Object.Instantiate<GameObject>(elementPrefab, (Transform)(object)contentRect);
				UserProfileListElementController component = obj.GetComponent<UserProfileListElementController>();
				MPButton component2 = obj.GetComponent<MPButton>();
				component.listController = this;
				component.userProfile = PlatformSystems.saveSystem.GetProfile(availableProfileNames[i]);
				elementsList.Add(component);
				obj.SetActive(true);
				if (i == 0)
				{
					component2.defaultFallbackButton = true;
				}
			}
		}
		if (this.onListRebuilt != null)
		{
			this.onListRebuilt();
		}
	}

	public ReadOnlyCollection<UserProfileListElementController> GetReadOnlyElementsList()
	{
		return new ReadOnlyCollection<UserProfileListElementController>(elementsList);
	}

	public void SendProfileSelection(UserProfile userProfile)
	{
		this.onProfileSelected?.Invoke(userProfile);
	}
}
