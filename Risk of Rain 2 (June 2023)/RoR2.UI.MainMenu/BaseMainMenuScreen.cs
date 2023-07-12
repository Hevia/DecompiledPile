using System;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2.UI.MainMenu;

[RequireComponent(typeof(RectTransform))]
public class BaseMainMenuScreen : MonoBehaviour
{
	public Transform desiredCameraTransform;

	[HideInInspector]
	public bool shouldDisplay;

	protected MainMenuController myMainMenuController;

	protected FirstSelectedObjectProvider firstSelectedObjectProvider;

	public UnityEvent onEnter;

	public UnityEvent onExit;

	public void Awake()
	{
		firstSelectedObjectProvider = ((Component)this).GetComponent<FirstSelectedObjectProvider>();
	}

	public virtual bool IsReadyToLeave()
	{
		return true;
	}

	public void Update()
	{
		if (SimpleDialogBox.instancesList.Count == 0)
		{
			firstSelectedObjectProvider?.EnsureSelectedObject();
		}
	}

	public virtual void OnEnter(MainMenuController mainMenuController)
	{
		Debug.LogFormat("BaseMainMenuScreen: OnEnter()", Array.Empty<object>());
		myMainMenuController = mainMenuController;
		if (SimpleDialogBox.instancesList.Count == 0)
		{
			firstSelectedObjectProvider?.EnsureSelectedObject();
		}
		onEnter.Invoke();
	}

	public virtual void OnExit(MainMenuController mainMenuController)
	{
		if ((Object)(object)myMainMenuController == (Object)(object)mainMenuController)
		{
			myMainMenuController = null;
		}
		onExit.Invoke();
	}
}
