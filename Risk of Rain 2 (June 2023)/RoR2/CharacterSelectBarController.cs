using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(MPEventSystemLocator))]
public class CharacterSelectBarController : MonoBehaviour
{
	public struct SurvivorPickInfo
	{
		public LocalUser localUser;

		public SurvivorDef pickedSurvivor;
	}

	[Serializable]
	public class SurvivorPickInfoUnityEvent : UnityEvent<SurvivorPickInfo>
	{
	}

	[Header("Prefabs")]
	public GameObject choiceButtonPrefab;

	public GameObject fillButtonPrefab;

	[ShowFieldObsolete]
	[Obsolete("No longer used.", false)]
	public GameObject WIPButtonPrefab;

	[Header("Required References")]
	[FormerlySerializedAs("gridLayoutGroup")]
	public GridLayoutGroup iconContainerGrid;

	[Header("Events")]
	public SurvivorPickInfoUnityEvent onSurvivorPicked;

	private MPEventSystemLocator eventSystemLocator;

	private UIElementAllocator<SurvivorIconController> survivorIconControllers;

	private UIElementAllocator<RectTransform> fillerIcons;

	private LocalUser currentLocalUser;

	[Obsolete("Use iconContainerGrid instead", false)]
	public ref GridLayoutGroup gridLayoutGroup => ref iconContainerGrid;

	public SurvivorIconController pickedIcon { get; private set; }

	private bool isEclipseRun
	{
		get
		{
			if (Object.op_Implicit((Object)(object)PreGameController.instance))
			{
				return PreGameController.instance.gameModeIndex == GameModeCatalog.FindGameModeIndex("EclipseRun");
			}
			return false;
		}
	}

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		survivorIconControllers = new UIElementAllocator<SurvivorIconController>((RectTransform)((Component)iconContainerGrid).transform, choiceButtonPrefab);
		survivorIconControllers.onCreateElement = OnCreateSurvivorIcon;
		fillerIcons = new UIElementAllocator<RectTransform>((RectTransform)((Component)iconContainerGrid).transform, fillButtonPrefab);
	}

	private void LateUpdate()
	{
		EnforceValidChoice();
	}

	private void OnEnable()
	{
		eventSystemLocator.onEventSystemDiscovered += OnEventSystemDiscovered;
		if (Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem))
		{
			OnEventSystemDiscovered(eventSystemLocator.eventSystem);
		}
	}

	private void OnDisable()
	{
		eventSystemLocator.onEventSystemDiscovered -= OnEventSystemDiscovered;
	}

	public void ApplyPickToPickerSurvivorPreference(SurvivorPickInfo survivorPickInfo)
	{
		if (survivorPickInfo.localUser != null)
		{
			survivorPickInfo.localUser.userProfile.SetSurvivorPreference(survivorPickInfo.pickedSurvivor);
		}
	}

	public void ApplyPickToEclipseSurvivorPreference(SurvivorPickInfo survivorPickInfo)
	{
	}

	private void OnEventSystemDiscovered(MPEventSystem eventSystem)
	{
		currentLocalUser = eventSystem.localUser;
		Build();
		SurvivorDef localUserExistingSurvivorPreference = GetLocalUserExistingSurvivorPreference();
		PickIconBySurvivorDef(localUserExistingSurvivorPreference);
	}

	private void OnCreateSurvivorIcon(int index, SurvivorIconController survivorIconController)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		survivorIconController.characterSelectBarController = this;
		((UnityEvent)((Button)survivorIconController.hgButton).onClick).AddListener((UnityAction)delegate
		{
			PickIcon(survivorIconController);
		});
	}

	private int FindIconIndexForSurvivorDef(SurvivorDef survivorDef)
	{
		ReadOnlyCollection<SurvivorIconController> elements = survivorIconControllers.elements;
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i].survivorDef == survivorDef)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindIconIndex(SurvivorIconController survivorIconController)
	{
		ReadOnlyCollection<SurvivorIconController> elements = survivorIconControllers.elements;
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i] == survivorIconController)
			{
				return i;
			}
		}
		return -1;
	}

	private void PickIconBySurvivorDef(SurvivorDef survivorDef)
	{
		int num = FindIconIndexForSurvivorDef(survivorDef);
		if (num >= 0)
		{
			PickIcon(survivorIconControllers.elements[num]);
		}
	}

	private void PickIcon(SurvivorIconController newPickedIcon)
	{
		if (pickedIcon != newPickedIcon)
		{
			pickedIcon = newPickedIcon;
			((UnityEvent<SurvivorPickInfo>)onSurvivorPicked)?.Invoke(new SurvivorPickInfo
			{
				localUser = currentLocalUser,
				pickedSurvivor = newPickedIcon.survivorDef
			});
		}
	}

	private bool ShouldDisplaySurvivor(SurvivorDef survivorDef)
	{
		if (Object.op_Implicit((Object)(object)survivorDef) && !survivorDef.hidden)
		{
			return survivorDef.CheckUserHasRequiredEntitlement(currentLocalUser);
		}
		return false;
	}

	private SurvivorDef GetLocalUserExistingSurvivorPreference()
	{
		return currentLocalUser?.userProfile.GetSurvivorPreference();
	}

	private void Build()
	{
		List<SurvivorDef> list = new List<SurvivorDef>();
		foreach (SurvivorDef orderedSurvivorDef in SurvivorCatalog.orderedSurvivorDefs)
		{
			if (ShouldDisplaySurvivor(orderedSurvivorDef))
			{
				list.Add(orderedSurvivorDef);
			}
		}
		int count = list.Count;
		int desiredCount = Math.Max(CalcGridCellCount(count, iconContainerGrid.constraintCount) - count, 0);
		survivorIconControllers.AllocateElements(count);
		fillerIcons.AllocateElements(desiredCount);
		fillerIcons.MoveElementsToContainerEnd();
		ReadOnlyCollection<SurvivorIconController> elements = survivorIconControllers.elements;
		for (int i = 0; i < count; i++)
		{
			SurvivorDef survivorDef = list[i];
			SurvivorIconController survivorIconController = elements[i];
			survivorIconController.survivorDef = survivorDef;
			survivorIconController.hgButton.defaultFallbackButton = i == 0;
		}
	}

	private static int CalcGridCellCount(int elementCount, int gridFixedDimensionLength)
	{
		return (int)Mathf.Ceil((float)elementCount / (float)gridFixedDimensionLength) * gridFixedDimensionLength;
	}

	private void EnforceValidChoice()
	{
		int num = FindIconIndex(pickedIcon);
		if (Object.op_Implicit((Object)(object)pickedIcon) && pickedIcon.survivorIsAvailable)
		{
			return;
		}
		int num2 = -1;
		ReadOnlyCollection<SurvivorIconController> elements = survivorIconControllers.elements;
		while (num2 < elements.Count)
		{
			int num3 = num + num2;
			if (0 <= num3 && num3 < elements.Count)
			{
				SurvivorIconController survivorIconController = elements[num3];
				if (survivorIconController.survivorIsAvailable)
				{
					PickIcon(survivorIconController);
					break;
				}
			}
			if (num2 >= 0)
			{
				num2++;
			}
			num2 = -num2;
		}
	}
}
