using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(MPEventSystemProvider))]
public class PauseScreenController : MonoBehaviour
{
	public static readonly List<PauseScreenController> instancesList = new List<PauseScreenController>();

	private MPEventSystemProvider eventSystemProvider;

	[Tooltip("The main panel to which any submenus will be parented.")]
	public RectTransform rootPanel;

	[Tooltip("The panel which contains the main controls. This will be disabled while in a submenu.")]
	public RectTransform mainPanel;

	public GameObject settingsPanelPrefab;

	private GameObject submenuObject;

	public GameObject exitGameButton;

	private static float oldTimeScale;

	private static bool paused = false;

	private void Awake()
	{
		eventSystemProvider = ((Component)this).GetComponent<MPEventSystemProvider>();
	}

	private void OnEnable()
	{
		if (instancesList.Count == 0)
		{
			paused = NetworkServer.dontListen;
			if (paused)
			{
				if (PauseManager.onPauseStartGlobal != null)
				{
					PauseManager.onPauseStartGlobal();
				}
				oldTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
		}
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
		if (instancesList.Count == 0 && paused)
		{
			Time.timeScale = oldTimeScale;
			paused = false;
			if (PauseManager.onPauseEndGlobal != null)
			{
				PauseManager.onPauseEndGlobal();
			}
		}
	}

	public void OpenSettingsMenu()
	{
		Object.Destroy((Object)(object)submenuObject);
		submenuObject = Object.Instantiate<GameObject>(settingsPanelPrefab, (Transform)(object)rootPanel);
		((Component)mainPanel).gameObject.SetActive(false);
	}

	public void Update()
	{
		if (!Object.op_Implicit((Object)(object)submenuObject))
		{
			((Component)mainPanel).gameObject.SetActive(true);
		}
		if (!NetworkManager.singleton.isNetworkActive)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
