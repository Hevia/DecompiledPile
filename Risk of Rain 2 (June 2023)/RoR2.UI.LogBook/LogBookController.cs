using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using EntityStates;
using HG;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI.LogBook;

public class LogBookController : MonoBehaviour
{
	private class NavigationPageInfo
	{
		public CategoryDef categoryDef;

		public Entry[] entries;

		public int index;

		public int indexInCategory;
	}

	private class LogBookState : EntityState
	{
		protected LogBookController logBookController;

		protected float unscaledAge;

		public override void OnEnter()
		{
			base.OnEnter();
			logBookController = GetComponent<LogBookController>();
		}

		public override void Update()
		{
			base.Update();
			unscaledAge += Time.unscaledDeltaTime;
		}
	}

	private class FadeState : LogBookState
	{
		private CanvasGroup canvasGroup;

		public float duration = 0.5f;

		public float endValue;

		public override void OnEnter()
		{
			base.OnEnter();
			canvasGroup = GetComponent<CanvasGroup>();
			if (Object.op_Implicit((Object)(object)canvasGroup))
			{
				canvasGroup.alpha = 0f;
			}
		}

		public override void OnExit()
		{
			if (Object.op_Implicit((Object)(object)canvasGroup))
			{
				canvasGroup.alpha = endValue;
			}
			base.OnExit();
		}

		public override void Update()
		{
			if (Object.op_Implicit((Object)(object)canvasGroup))
			{
				canvasGroup.alpha = unscaledAge / duration;
				if (canvasGroup.alpha >= 1f)
				{
					outer.SetNextState(new Idle());
				}
			}
			base.Update();
		}
	}

	private class ChangeEntriesPageState : LogBookState
	{
		private int oldPageIndex;

		public NavigationPageInfo newNavigationPageInfo;

		public float duration = 0.1f;

		public Vector2 moveDirection;

		private GameObject oldPage;

		private GameObject newPage;

		private Vector2 oldPageTargetPosition;

		private Vector2 newPageTargetPosition;

		private Vector2 containerSize = Vector2.zero;

		public override void OnEnter()
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			if (Object.op_Implicit((Object)(object)logBookController))
			{
				oldPageIndex = logBookController.currentPageIndex;
				oldPage = logBookController.currentEntriesPageObject;
				newPage = logBookController.BuildEntriesPage(newNavigationPageInfo);
				Rect rect = logBookController.entryPageContainer.rect;
				containerSize = ((Rect)(ref rect)).size;
			}
			SetPagePositions(0f);
		}

		public override void OnExit()
		{
			base.OnExit();
			EntityState.Destroy((Object)(object)oldPage);
			if (Object.op_Implicit((Object)(object)logBookController))
			{
				logBookController.currentEntriesPageObject = newPage;
				logBookController.currentPageIndex = newNavigationPageInfo.indexInCategory;
			}
		}

		private void SetPagePositions(float t)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(containerSize.x * (0f - moveDirection.x), containerSize.y * moveDirection.y);
			Vector2 val2 = val * t;
			if (Object.op_Implicit((Object)(object)oldPage))
			{
				oldPage.transform.localPosition = Vector2.op_Implicit(val2);
			}
			if (Object.op_Implicit((Object)(object)newPage))
			{
				newPage.transform.localPosition = Vector2.op_Implicit(val2 - val);
			}
		}

		public override void Update()
		{
			base.Update();
			float num = Mathf.Clamp01(unscaledAge / duration);
			SetPagePositions(num);
			if (num == 1f)
			{
				outer.SetNextState(new Idle());
			}
		}
	}

	private class ChangeCategoryState : LogBookState
	{
		private int oldCategoryIndex;

		public int newCategoryIndex;

		public bool goToLastPage;

		public float duration = 0.1f;

		private GameObject oldPage;

		private GameObject newPage;

		private Vector2 oldPageTargetPosition;

		private Vector2 newPageTargetPosition;

		private Vector2 moveDirection;

		private Vector2 containerSize = Vector2.zero;

		private NavigationPageInfo[] newNavigationPages;

		private int destinationPageIndex;

		private NavigationPageInfo newNavigationPageInfo;

		private int frame;

		public override void OnEnter()
		{
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			if (Object.op_Implicit((Object)(object)logBookController))
			{
				oldCategoryIndex = logBookController.currentCategoryIndex;
				oldPage = logBookController.currentEntriesPageObject;
				newNavigationPages = logBookController.GetCategoryPages(newCategoryIndex);
				destinationPageIndex = newNavigationPages[0].index;
				if (goToLastPage)
				{
					destinationPageIndex = newNavigationPages[newNavigationPages.Length - 1].index;
					Debug.LogFormat("goToLastPage=true destinationPageIndex={0}", new object[1] { destinationPageIndex });
				}
				newNavigationPageInfo = logBookController.allNavigationPages[destinationPageIndex];
				newPage = logBookController.BuildEntriesPage(newNavigationPageInfo);
				Rect rect = logBookController.entryPageContainer.rect;
				containerSize = ((Rect)(ref rect)).size;
				moveDirection = new Vector2(Mathf.Sign((float)(newCategoryIndex - oldCategoryIndex)), 0f);
			}
			SetPagePositions(0f);
		}

		public override void OnExit()
		{
			EntityState.Destroy((Object)(object)oldPage);
			if (Object.op_Implicit((Object)(object)logBookController))
			{
				logBookController.currentEntriesPageObject = newPage;
				logBookController.currentPageIndex = newNavigationPageInfo.indexInCategory;
				logBookController.desiredPageIndex = newNavigationPageInfo.indexInCategory;
				logBookController.currentCategoryIndex = newCategoryIndex;
				logBookController.availableNavigationPages = newNavigationPages;
				logBookController.currentCategoryLabel.token = categories[newCategoryIndex].nameToken;
				((Transform)logBookController.categoryHightlightRect).SetParent(((Component)logBookController.navigationCategoryButtonAllocator.elements[newCategoryIndex]).transform, false);
				((Component)logBookController.categoryHightlightRect).gameObject.SetActive(false);
				((Component)logBookController.categoryHightlightRect).gameObject.SetActive(true);
				if (logBookController.moveNavigationPageIndicatorContainerToCategoryButton)
				{
					((Transform)logBookController.navigationPageIndicatorContainer).SetParent(((Component)logBookController.navigationCategoryButtonAllocator.elements[newCategoryIndex]).transform, false);
				}
			}
			base.OnExit();
		}

		private void SetPagePositions(float t)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(containerSize.x * (0f - moveDirection.x), containerSize.y * moveDirection.y);
			Vector2 val2 = val * t;
			if (Object.op_Implicit((Object)(object)oldPage))
			{
				oldPage.transform.localPosition = Vector2.op_Implicit(val2);
			}
			if (Object.op_Implicit((Object)(object)newPage))
			{
				newPage.transform.localPosition = Vector2.op_Implicit(val2 - val);
				if (frame == 4)
				{
					((Behaviour)newPage.GetComponent<GridLayoutGroup>()).enabled = false;
				}
			}
		}

		public override void Update()
		{
			base.Update();
			frame++;
			float num = Mathf.Clamp01(unscaledAge / duration);
			SetPagePositions(num);
			if (num == 1f)
			{
				outer.SetNextState(new Idle());
			}
		}
	}

	private class EnterLogViewState : LogBookState
	{
		public Texture iconTexture;

		public RectTransform startRectTransform;

		public RectTransform endRectTransform;

		public Entry entry;

		private GameObject flyingIcon;

		private RectTransform flyingIconTransform;

		private RawImage flyingIconImage;

		private float duration = 0.75f;

		private Rect startRect;

		private Rect midRect;

		private Rect endRect;

		private bool submittedViewEntry;

		public override void OnEnter()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			flyingIcon = new GameObject("FlyingIcon", new Type[3]
			{
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(RawImage)
			});
			flyingIconTransform = (RectTransform)flyingIcon.transform;
			((Transform)flyingIconTransform).SetParent(((Component)logBookController).transform, false);
			((Transform)flyingIconTransform).localScale = Vector3.one;
			flyingIconImage = ((Component)flyingIconTransform).GetComponent<RawImage>();
			flyingIconImage.texture = iconTexture;
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			startRectTransform.GetWorldCorners(array);
			startRect = GetRectRelativeToParent(array);
			Rect rect = ((RectTransform)((Component)logBookController).transform).rect;
			midRect = new Rect(((Rect)(ref rect)).center, ((Rect)(ref startRect)).size);
			endRectTransform.GetWorldCorners(array);
			endRect = GetRectRelativeToParent(array);
			SetIconRect(startRect);
		}

		private void SetIconRect(Rect rect)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			((Transform)flyingIconTransform).position = Vector2.op_Implicit(((Rect)(ref rect)).position);
			flyingIconTransform.offsetMin = ((Rect)(ref rect)).min;
			flyingIconTransform.offsetMax = ((Rect)(ref rect)).max;
		}

		private Rect GetRectRelativeToParent(Vector3[] corners)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < 4; i++)
			{
				corners[i] = ((Component)logBookController).transform.InverseTransformPoint(corners[i]);
			}
			Rect result = default(Rect);
			((Rect)(ref result)).xMin = corners[0].x;
			((Rect)(ref result)).xMax = corners[2].x;
			((Rect)(ref result)).yMin = corners[0].y;
			((Rect)(ref result)).yMax = corners[2].y;
			return result;
		}

		private static Rect RectFromWorldCorners(Vector3[] corners)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			Rect result = default(Rect);
			((Rect)(ref result)).xMin = corners[0].x;
			((Rect)(ref result)).xMax = corners[2].x;
			((Rect)(ref result)).yMin = corners[0].y;
			((Rect)(ref result)).yMax = corners[2].y;
			return result;
		}

		private static Rect LerpRect(Rect a, Rect b, float t)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Rect result = default(Rect);
			((Rect)(ref result)).min = Vector2.LerpUnclamped(((Rect)(ref a)).min, ((Rect)(ref b)).min, t);
			((Rect)(ref result)).max = Vector2.LerpUnclamped(((Rect)(ref a)).max, ((Rect)(ref b)).max, t);
			return result;
		}

		public override void OnExit()
		{
			EntityState.Destroy((Object)(object)flyingIcon);
			base.OnExit();
		}

		public override void Update()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			base.Update();
			float num = Mathf.Min(unscaledAge / duration, 1f);
			if (num < 0.1f)
			{
				Util.Remap(num, 0f, 0.1f, 0f, 1f);
				SetIconRect(startRect);
			}
			if (num < 0.2f)
			{
				float t = Util.Remap(num, 0.1f, 0.2f, 0f, 1f);
				SetIconRect(LerpRect(startRect, midRect, t));
			}
			else if (num < 0.4f)
			{
				Util.Remap(num, 0.2f, 0.4f, 0f, 1f);
				SetIconRect(midRect);
			}
			else if (num < 0.6f)
			{
				float t2 = Util.Remap(num, 0.4f, 0.6f, 0f, 1f);
				SetIconRect(LerpRect(midRect, endRect, t2));
			}
			else if (num < 1f)
			{
				float num2 = Util.Remap(num, 0.6f, 1f, 0f, 1f);
				((Graphic)flyingIconImage).color = new Color(1f, 1f, 1f, 1f - num2);
				SetIconRect(endRect);
				if (!submittedViewEntry)
				{
					submittedViewEntry = true;
					logBookController.ViewEntry(entry);
				}
			}
			else
			{
				outer.SetNextState(new Idle());
			}
		}
	}

	[Header("Navigation")]
	public GameObject navigationPanel;

	public RectTransform categoryContainer;

	public GameObject categorySpaceFiller;

	public int categorySpaceFillerCount;

	public Color spaceFillerColor;

	private UIElementAllocator<MPButton> navigationCategoryButtonAllocator;

	public RectTransform entryPageContainer;

	public GameObject entryPagePrefab;

	public RectTransform navigationPageIndicatorContainer;

	public GameObject navigationPageIndicatorPrefab;

	public bool moveNavigationPageIndicatorContainerToCategoryButton;

	private UIElementAllocator<MPButton> navigationPageIndicatorAllocator;

	public MPButton previousPageButton;

	public MPButton nextPageButton;

	public LanguageTextMeshController currentCategoryLabel;

	private RectTransform categoryHightlightRect;

	[Header("PageViewer")]
	public UnityEvent OnViewEntry;

	public GameObject pageViewerPanel;

	public MPButton pageViewerBackButton;

	[Header("Misc")]
	public GameObject categoryButtonPrefab;

	public GameObject headerHighlightPrefab;

	public LanguageTextMeshController hoverLanguageTextMeshController;

	public string hoverDescriptionFormatString;

	private EntityStateMachine stateMachine;

	private UILayerKey uiLayerKey;

	public static CategoryDef[] categories;

	public static ResourceAvailability availability;

	private static bool IsInitialized;

	private NavigationPageInfo[] _availableNavigationPages = Array.Empty<NavigationPageInfo>();

	private GameObject currentEntriesPageObject;

	private int currentCategoryIndex;

	private int desiredCategoryIndex;

	private int currentPageIndex;

	private int desiredPageIndex;

	private bool goToEndOfNextCategory;

	private NavigationPageInfo[] allNavigationPages;

	private NavigationPageInfo[][] navigationPagesByCategory;

	private NavigationPageInfo[] availableNavigationPages
	{
		get
		{
			return _availableNavigationPages;
		}
		set
		{
			int num = _availableNavigationPages.Length;
			_availableNavigationPages = value;
			if (num != availableNavigationPages.Length)
			{
				navigationPageIndicatorAllocator.AllocateElements(availableNavigationPages.Length);
			}
		}
	}

	private void Awake()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		navigationCategoryButtonAllocator = new UIElementAllocator<MPButton>(categoryContainer, categoryButtonPrefab);
		navigationPageIndicatorAllocator = new UIElementAllocator<MPButton>(navigationPageIndicatorContainer, navigationPageIndicatorPrefab);
		navigationPageIndicatorAllocator.onCreateElement = delegate(int index, MPButton button)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			((UnityEvent)((Button)button).onClick).AddListener((UnityAction)delegate
			{
				desiredPageIndex = index;
			});
		};
		((UnityEvent)((Button)previousPageButton).onClick).AddListener(new UnityAction(OnLeftButton));
		((UnityEvent)((Button)nextPageButton).onClick).AddListener(new UnityAction(OnRightButton));
		((UnityEvent)((Button)pageViewerBackButton).onClick).AddListener(new UnityAction(ReturnToNavigation));
		stateMachine = ((Component)this).gameObject.AddComponent<EntityStateMachine>();
		uiLayerKey = ((Component)this).gameObject.GetComponent<UILayerKey>();
		stateMachine.initialStateType = default(SerializableEntityStateType);
		categoryHightlightRect = (RectTransform)Object.Instantiate<GameObject>(headerHighlightPrefab, ((Component)this).transform.parent).transform;
		((Component)categoryHightlightRect).gameObject.SetActive(false);
	}

	private void Start()
	{
		GeneratePages(LocalUserManager.GetFirstLocalUser()?.userProfile);
		BuildCategoriesButtons();
		stateMachine.SetNextState(new ChangeCategoryState
		{
			newCategoryIndex = 0
		});
	}

	private void BuildCategoriesButtons()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		Debug.Log((object)"Building category buttons.");
		navigationCategoryButtonAllocator.AllocateElements(categories.Length);
		ReadOnlyCollection<MPButton> elements = navigationCategoryButtonAllocator.elements;
		for (int i = 0; i < categories.Length; i++)
		{
			int categoryIndex = i;
			MPButton mPButton = elements[i];
			CategoryDef categoryDef = categories[i];
			((TMP_Text)((Component)mPButton).GetComponentInChildren<TextMeshProUGUI>()).text = Language.GetString(categoryDef.nameToken);
			((UnityEventBase)((Button)mPButton).onClick).RemoveAllListeners();
			((UnityEvent)((Button)mPButton).onClick).AddListener((UnityAction)delegate
			{
				OnCategoryClicked(categoryIndex);
			});
			mPButton.requiredTopLayer = uiLayerKey;
			ViewableTag viewableTag = ((Component)mPButton).gameObject.GetComponent<ViewableTag>();
			if (!Object.op_Implicit((Object)(object)viewableTag))
			{
				viewableTag = ((Component)mPButton).gameObject.AddComponent<ViewableTag>();
			}
			viewableTag.viewableName = categoryDef.viewableNode.fullName;
		}
		if (Object.op_Implicit((Object)(object)categorySpaceFiller))
		{
			for (int j = 0; j < categorySpaceFillerCount; j++)
			{
				Object.Instantiate<GameObject>(categorySpaceFiller, (Transform)(object)categoryContainer).gameObject.SetActive(true);
			}
		}
	}

	[SystemInitializer(new Type[]
	{
		typeof(BodyCatalog),
		typeof(SceneCatalog),
		typeof(AchievementManager),
		typeof(ItemCatalog),
		typeof(EquipmentCatalog),
		typeof(UnlockableCatalog),
		typeof(RunReport),
		typeof(SurvivorCatalog),
		typeof(EntitlementManager),
		typeof(ExpansionCatalog)
	})]
	public static void Init()
	{
		if (LocalUserManager.isAnyUserSignedIn)
		{
			BuildStaticData();
		}
		LocalUserManager.onUserSignIn += OnUserSignIn;
		IsInitialized = true;
	}

	private static void OnUserSignIn(LocalUser obj)
	{
		BuildStaticData();
	}

	private static EntryStatus GetPickupStatus(in Entry entry, UserProfile viewerProfile)
	{
		UnlockableDef unlockableDef = null;
		PickupIndex pickupIndex = (PickupIndex)entry.extraData;
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
		EquipmentIndex equipmentIndex = pickupDef?.equipmentIndex ?? EquipmentIndex.None;
		if (itemIndex != ItemIndex.None)
		{
			unlockableDef = ItemCatalog.GetItemDef(itemIndex).unlockableDef;
		}
		else
		{
			if (equipmentIndex == EquipmentIndex.None)
			{
				return EntryStatus.Unimplemented;
			}
			unlockableDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex).unlockableDef;
		}
		if (!viewerProfile.HasUnlockable(unlockableDef))
		{
			return EntryStatus.Locked;
		}
		if (!viewerProfile.HasDiscoveredPickup(pickupIndex))
		{
			return EntryStatus.Unencountered;
		}
		return EntryStatus.Available;
	}

	private static TooltipContent GetPickupTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		UnlockableDef unlockableDef = PickupCatalog.GetPickupDef((PickupIndex)entry.extraData).unlockableDef;
		TooltipContent result = default(TooltipContent);
		if (status >= EntryStatus.Available)
		{
			result.overrideTitleText = entry.GetDisplayName(userProfile);
			result.titleColor = entry.color;
			if ((Object)(object)unlockableDef != (Object)null)
			{
				result.overrideBodyText = unlockableDef.getUnlockedString();
			}
			result.bodyToken = "";
			result.bodyColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unlockable));
		}
		else
		{
			result.titleToken = "UNIDENTIFIED";
			result.titleColor = Color.gray;
			result.bodyToken = "";
			switch (status)
			{
			case EntryStatus.Unimplemented:
				result.titleToken = "TOOLTIP_WIP_CONTENT_NAME";
				result.bodyToken = "TOOLTIP_WIP_CONTENT_DESCRIPTION";
				break;
			case EntryStatus.Unencountered:
				result.overrideBodyText = Language.GetString("LOGBOOK_UNLOCK_ITEM_LOG");
				break;
			case EntryStatus.Locked:
				result.overrideBodyText = unlockableDef.getHowToUnlockString();
				break;
			}
			result.bodyColor = Color.white;
		}
		return result;
	}

	private static TooltipContent GetMonsterTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		TooltipContent result = default(TooltipContent);
		result.titleColor = entry.color;
		if (status >= EntryStatus.Available)
		{
			result.overrideTitleText = entry.GetDisplayName(userProfile);
			result.titleColor = entry.color;
			result.bodyToken = "";
		}
		else
		{
			result.titleToken = "UNIDENTIFIED";
			result.titleColor = Color.gray;
			result.bodyToken = "LOGBOOK_UNLOCK_ITEM_MONSTER";
		}
		return result;
	}

	private static TooltipContent GetStageTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		TooltipContent result = default(TooltipContent);
		result.titleColor = entry.color;
		if (status >= EntryStatus.Available)
		{
			result.overrideTitleText = entry.GetDisplayName(userProfile);
			result.titleColor = entry.color;
			result.bodyToken = "";
		}
		else
		{
			result.titleToken = "UNIDENTIFIED";
			result.titleColor = Color.gray;
			result.bodyToken = "LOGBOOK_UNLOCK_ITEM_STAGE";
		}
		return result;
	}

	private static TooltipContent GetSurvivorTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		TooltipContent result = default(TooltipContent);
		UnlockableDef unlockableDef = SurvivorCatalog.FindSurvivorDefFromBody(((Component)(CharacterBody)entry.extraData).gameObject).unlockableDef;
		if (status >= EntryStatus.Available)
		{
			result.overrideTitleText = entry.GetDisplayName(userProfile);
			result.titleColor = entry.color;
			result.bodyToken = "";
		}
		else
		{
			result.titleToken = "UNIDENTIFIED";
			result.bodyToken = "";
			result.titleColor = Color.gray;
			switch (status)
			{
			case EntryStatus.Unencountered:
				result.overrideBodyText = Language.GetString("LOGBOOK_UNLOCK_ITEM_SURVIVOR");
				break;
			case EntryStatus.Locked:
				result.overrideBodyText = unlockableDef.getHowToUnlockString();
				break;
			}
		}
		return result;
	}

	private static TooltipContent GetAchievementTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		TooltipContent result = default(TooltipContent);
		UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(((AchievementDef)entry.extraData).unlockableRewardIdentifier);
		result.titleColor = entry.color;
		result.bodyToken = "";
		if ((Object)(object)unlockableDef == (Object)null)
		{
			result.overrideTitleText = entry.GetDisplayName(userProfile);
			result.titleColor = Color.gray;
			result.overrideBodyText = "ACHIEVEMENT HAS NO UNLOCKABLE DEFINED";
			result.bodyColor = Color.white;
			return result;
		}
		if (status >= EntryStatus.Available)
		{
			result.titleToken = entry.GetDisplayName(userProfile);
			result.titleColor = entry.color;
			result.overrideBodyText = unlockableDef.getUnlockedString();
		}
		else
		{
			result.titleToken = "UNIDENTIFIED";
			result.titleColor = Color.gray;
			if (status == EntryStatus.Locked)
			{
				result.overrideBodyText = Language.GetString("UNIDENTIFIED_DESCRIPTION");
			}
			else
			{
				result.overrideBodyText = unlockableDef.getHowToUnlockString();
			}
		}
		return result;
	}

	private static TooltipContent GetWIPTooltipContent(in Entry entry, UserProfile userProfile, EntryStatus status)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TooltipContent result = default(TooltipContent);
		result.titleColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.WIP));
		result.titleToken = "TOOLTIP_WIP_CONTENT_NAME";
		result.bodyToken = "TOOLTIP_WIP_CONTENT_DESCRIPTION";
		return result;
	}

	private static EntryStatus GetAlwaysAvailable(UserProfile userProfile, Entry entry)
	{
		return EntryStatus.Available;
	}

	private static EntryStatus GetUnimplemented(in Entry entry, UserProfile viewerProfile)
	{
		return EntryStatus.Unimplemented;
	}

	private static EntryStatus GetStageStatus(in Entry entry, UserProfile viewerProfile)
	{
		UnlockableDef unlockableLogFromBaseSceneName = SceneCatalog.GetUnlockableLogFromBaseSceneName((entry.extraData as SceneDef).baseSceneName);
		if ((Object)(object)unlockableLogFromBaseSceneName != (Object)null && viewerProfile.HasUnlockable(unlockableLogFromBaseSceneName))
		{
			return EntryStatus.Available;
		}
		return EntryStatus.Unencountered;
	}

	private static EntryStatus GetMonsterStatus(in Entry entry, UserProfile viewerProfile)
	{
		CharacterBody characterBody = (CharacterBody)entry.extraData;
		UnlockableDef unlockableDef = ((Component)characterBody).GetComponent<DeathRewards>()?.logUnlockableDef;
		if (!Object.op_Implicit((Object)(object)unlockableDef))
		{
			return EntryStatus.None;
		}
		if (viewerProfile.HasUnlockable(unlockableDef))
		{
			return EntryStatus.Available;
		}
		if (viewerProfile.statSheet.GetStatValueULong(PerBodyStatDef.killsAgainst, ((Object)((Component)characterBody).gameObject).name) != 0)
		{
			return EntryStatus.Unencountered;
		}
		return EntryStatus.Locked;
	}

	private static EntryStatus GetSurvivorStatus(in Entry entry, UserProfile viewerProfile)
	{
		CharacterBody characterBody = (CharacterBody)entry.extraData;
		SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(((Component)characterBody).gameObject);
		if (!viewerProfile.HasUnlockable(survivorDef.unlockableDef))
		{
			return EntryStatus.Locked;
		}
		if (viewerProfile.statSheet.GetStatValueULong(PerBodyStatDef.totalWins, ((Object)((Component)characterBody).gameObject).name) == 0L)
		{
			return EntryStatus.Unencountered;
		}
		return EntryStatus.Available;
	}

	private static EntryStatus GetAchievementStatus(in Entry entry, UserProfile viewerProfile)
	{
		string identifier = ((AchievementDef)entry.extraData).identifier;
		bool flag = viewerProfile.HasAchievement(identifier);
		if (!viewerProfile.CanSeeAchievement(identifier))
		{
			return EntryStatus.Locked;
		}
		if (!flag)
		{
			return EntryStatus.Unencountered;
		}
		return EntryStatus.Available;
	}

	private static void BuildStaticData()
	{
		categories = BuildCategories();
		RegisterViewables(categories);
		availability.MakeAvailable();
	}

	static LogBookController()
	{
		categories = Array.Empty<CategoryDef>();
		availability = default(ResourceAvailability);
		IsInitialized = false;
	}

	private NavigationPageInfo[] GetCategoryPages(int categoryIndex)
	{
		if (navigationPagesByCategory.GetLength(0) <= categoryIndex)
		{
			return new NavigationPageInfo[0];
		}
		return navigationPagesByCategory[categoryIndex];
	}

	public void OnLeftButton()
	{
		desiredPageIndex--;
	}

	public void OnRightButton()
	{
		desiredPageIndex++;
	}

	private void OnCategoryClicked(int categoryIndex)
	{
		desiredCategoryIndex = categoryIndex;
		goToEndOfNextCategory = false;
	}

	private void GeneratePages(UserProfile viewerProfile)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInitialized)
		{
			Init();
		}
		navigationPagesByCategory = new NavigationPageInfo[categories.Length][];
		IEnumerable<NavigationPageInfo> enumerable = Array.Empty<NavigationPageInfo>();
		int num = 0;
		for (int i = 0; i < categories.Length; i++)
		{
			CategoryDef categoryDef = categories[i];
			Entry[] array = categoryDef.BuildEntries(viewerProfile);
			bool fullWidth = categoryDef.fullWidth;
			Rect rect = entryPageContainer.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			if (fullWidth)
			{
				categoryDef.iconSize.x = size.x;
			}
			int num2 = Mathf.FloorToInt(Mathf.Max(size.x / categoryDef.iconSize.x, 1f));
			int num3 = Mathf.FloorToInt(Mathf.Max(size.y / categoryDef.iconSize.y, 1f));
			int num4 = num2 * num3;
			int num5 = Mathf.CeilToInt((float)array.Length / (float)num4);
			if (num5 <= 0)
			{
				num5 = 1;
			}
			NavigationPageInfo[] array2 = new NavigationPageInfo[num5];
			for (int j = 0; j < num5; j++)
			{
				int num6 = j * num4;
				int num7 = array.Length - num6;
				int num8 = num4;
				if (num8 > num7)
				{
					num8 = num7;
				}
				Entry[] array3 = new Entry[num4];
				Array.Copy(array, num6, array3, 0, num8);
				NavigationPageInfo navigationPageInfo = new NavigationPageInfo();
				navigationPageInfo.categoryDef = categoryDef;
				navigationPageInfo.entries = array3;
				navigationPageInfo.index = num++;
				navigationPageInfo.indexInCategory = j;
				array2[j] = navigationPageInfo;
			}
			navigationPagesByCategory[i] = array2;
			enumerable = enumerable.Concat(array2);
		}
		allNavigationPages = enumerable.ToArray();
	}

	private void Update()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		if (desiredPageIndex > availableNavigationPages.Length - 1)
		{
			desiredPageIndex = availableNavigationPages.Length - 1;
			desiredCategoryIndex++;
			goToEndOfNextCategory = false;
		}
		if (desiredPageIndex < 0)
		{
			desiredCategoryIndex--;
			desiredPageIndex = 0;
			goToEndOfNextCategory = true;
		}
		if (desiredCategoryIndex > categories.Length - 1)
		{
			desiredCategoryIndex = categories.Length - 1;
		}
		if (desiredCategoryIndex < 0)
		{
			desiredCategoryIndex = 0;
		}
		foreach (MPButton element in navigationPageIndicatorAllocator.elements)
		{
			ColorBlock colors = ((Selectable)element).colors;
			((ColorBlock)(ref colors)).colorMultiplier = 1f;
			((Selectable)element).colors = colors;
		}
		if (currentPageIndex < navigationPageIndicatorAllocator.elements.Count)
		{
			MPButton mPButton = navigationPageIndicatorAllocator.elements[currentPageIndex];
			ColorBlock colors2 = ((Selectable)mPButton).colors;
			((ColorBlock)(ref colors2)).colorMultiplier = 2f;
			((Selectable)mPButton).colors = colors2;
		}
		if (desiredCategoryIndex != currentCategoryIndex)
		{
			if (stateMachine.state is Idle)
			{
				int num = ((desiredCategoryIndex > currentCategoryIndex) ? 1 : (-1));
				stateMachine.SetNextState(new ChangeCategoryState
				{
					newCategoryIndex = currentCategoryIndex + num,
					goToLastPage = goToEndOfNextCategory
				});
			}
		}
		else if (desiredPageIndex != currentPageIndex && stateMachine.state is Idle)
		{
			int num2 = ((desiredPageIndex > currentPageIndex) ? 1 : (-1));
			stateMachine.SetNextState(new ChangeEntriesPageState
			{
				newNavigationPageInfo = GetCategoryPages(currentCategoryIndex)[currentPageIndex + num2],
				moveDirection = new Vector2((float)num2, 0f)
			});
		}
	}

	private UserProfile LookUpUserProfile()
	{
		return LocalUserManager.readOnlyLocalUsersList.FirstOrDefault((LocalUser v) => v != null)?.userProfile;
	}

	private GameObject BuildEntriesPage(NavigationPageInfo navigationPageInfo)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		Entry[] entries = navigationPageInfo.entries;
		CategoryDef categoryDef = navigationPageInfo.categoryDef;
		GameObject val = Object.Instantiate<GameObject>(entryPagePrefab, (Transform)(object)entryPageContainer);
		val.GetComponent<GridLayoutGroup>().cellSize = categoryDef.iconSize;
		UIElementAllocator<RectTransform> uIElementAllocator = new UIElementAllocator<RectTransform>((RectTransform)val.transform, categoryDef.iconPrefab);
		uIElementAllocator.AllocateElements(entries.Length);
		UserProfile userProfile = LookUpUserProfile();
		ReadOnlyCollection<RectTransform> elements = uIElementAllocator.elements;
		for (int i = 0; i < elements.Count; i++)
		{
			RectTransform val2 = elements[i];
			HGButton component = ((Component)val2).GetComponent<HGButton>();
			Entry entry = ((i < entries.Length) ? entries[i] : null);
			EntryStatus entryStatus = entry?.GetStatus(userProfile) ?? EntryStatus.None;
			if (entryStatus != 0)
			{
				TooltipContent tooltipContent = entry.GetTooltipContent(userProfile, entryStatus);
				categoryDef.initializeElementGraphics?.Invoke(((Component)val2).gameObject, entry, entryStatus, userProfile);
				if (Object.op_Implicit((Object)(object)component))
				{
					bool flag = entryStatus >= EntryStatus.Available;
					((Selectable)component).interactable = true;
					component.disableGamepadClick = component.disableGamepadClick || !flag;
					component.disablePointerClick = component.disablePointerClick || !flag;
					component.imageOnInteractable = (flag ? component.imageOnInteractable : null);
					component.requiredTopLayer = uiLayerKey;
					component.updateTextOnHover = true;
					component.hoverLanguageTextMeshController = hoverLanguageTextMeshController;
					string titleText = tooltipContent.GetTitleText();
					string bodyText = tooltipContent.GetBodyText();
					Color titleColor = tooltipContent.titleColor;
					titleColor.a = 0.2f;
					component.hoverToken = Language.GetStringFormatted("LOGBOOK_HOVER_DESCRIPTION_FORMAT", titleText, bodyText, ColorUtility.ToHtmlStringRGBA(titleColor));
				}
				if (entry.viewableNode != null)
				{
					ViewableTag viewableTag = ((Component)val2).gameObject.GetComponent<ViewableTag>();
					if (!Object.op_Implicit((Object)(object)viewableTag))
					{
						viewableTag = ((Component)val2).gameObject.AddComponent<ViewableTag>();
						viewableTag.viewableVisualStyle = ViewableTag.ViewableVisualStyle.Icon;
					}
					viewableTag.viewableName = entry.viewableNode.fullName;
				}
			}
			if (entryStatus >= EntryStatus.Available && Object.op_Implicit((Object)(object)component))
			{
				((UnityEvent)((Button)component).onClick).AddListener((UnityAction)delegate
				{
					ViewEntry(entry);
				});
			}
			if (entryStatus == EntryStatus.None)
			{
				if (Object.op_Implicit((Object)(object)component))
				{
					((Behaviour)component).enabled = false;
					((Selectable)component).targetGraphic.color = spaceFillerColor;
				}
				else
				{
					((Graphic)((Component)val2).GetComponent<Image>()).color = spaceFillerColor;
				}
				for (int num = ((Transform)val2).childCount - 1; num >= 0; num--)
				{
					Object.Destroy((Object)(object)((Component)((Transform)val2).GetChild(num)).gameObject);
				}
			}
			if (Object.op_Implicit((Object)(object)component) && i == 0)
			{
				component.defaultFallbackButton = true;
			}
		}
		val.gameObject.SetActive(true);
		GridLayoutGroup gridLayoutGroup = val.GetComponent<GridLayoutGroup>();
		Action destroyLayoutGroup = null;
		int frameTimer = 2;
		destroyLayoutGroup = delegate
		{
			int num2 = frameTimer - 1;
			frameTimer = num2;
			if (frameTimer <= 0)
			{
				((Behaviour)gridLayoutGroup).enabled = false;
				RoR2Application.onLateUpdate -= destroyLayoutGroup;
			}
		};
		RoR2Application.onLateUpdate += destroyLayoutGroup;
		return val;
	}

	private void ViewEntry(Entry entry)
	{
		OnViewEntry.Invoke();
		LogBookPage component = pageViewerPanel.GetComponent<LogBookPage>();
		component.SetEntry(LookUpUserProfile(), entry);
		component.modelPanel.SetAnglesForCharacterThumbnailForSeconds(0.5f);
		ViewableTrigger.TriggerView(entry.viewableNode?.fullName);
	}

	private void ReturnToNavigation()
	{
		navigationPanel.SetActive(true);
		pageViewerPanel.SetActive(false);
	}

	private static bool UnlockableExists(UnlockableDef unlockableDef)
	{
		return Object.op_Implicit((Object)(object)unlockableDef);
	}

	private static bool IsEntryBodyWithoutLore(in Entry entry)
	{
		CharacterBody obj = (CharacterBody)entry.extraData;
		bool flag = false;
		string text = "";
		string baseNameToken = obj.baseNameToken;
		if (!string.IsNullOrEmpty(baseNameToken))
		{
			text = baseNameToken.Replace("_NAME", "_LORE");
			if (Language.english.TokenIsRegistered(text))
			{
				flag = true;
			}
		}
		return !flag;
	}

	private static bool IsEntryPickupItemWithoutLore(in Entry entry)
	{
		ItemDef itemDef = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef((PickupIndex)entry.extraData).itemIndex);
		return !Language.english.TokenIsRegistered(itemDef.loreToken);
	}

	private static bool IsEntryPickupEquipmentWithoutLore(in Entry entry)
	{
		EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(PickupCatalog.GetPickupDef((PickupIndex)entry.extraData).equipmentIndex);
		return !Language.english.TokenIsRegistered(equipmentDef.loreToken);
	}

	private static bool CanSelectItemEntry(ItemDef itemDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if ((Object)(object)itemDef != (Object)null)
		{
			ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
			if (Object.op_Implicit((Object)(object)itemTierDef) && itemTierDef.isDroppable)
			{
				if (!((Object)(object)itemDef.requiredExpansion == (Object)null) && expansionAvailability.ContainsKey(itemDef.requiredExpansion))
				{
					return expansionAvailability[itemDef.requiredExpansion];
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private static bool CanSelectEquipmentEntry(EquipmentDef equipmentDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if ((Object)(object)equipmentDef != (Object)null)
		{
			if (equipmentDef.canDrop)
			{
				if (!((Object)(object)equipmentDef.requiredExpansion == (Object)null) && expansionAvailability.ContainsKey(equipmentDef.requiredExpansion))
				{
					return expansionAvailability[equipmentDef.requiredExpansion];
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private static Entry[] BuildPickupEntries(Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		new Entry
		{
			nameToken = "TOOLTIP_WIP_CONTENT_NAME",
			color = Color.white,
			iconTexture = LegacyResourcesAPI.Load<Texture>("Textures/MiscIcons/texWIPIcon"),
			getStatusImplementation = GetUnimplemented,
			getTooltipContentImplementation = GetWIPTooltipContent
		};
		IEnumerable<Entry> first = from pickupDef in PickupCatalog.allPickups
			select ItemCatalog.GetItemDef(pickupDef.itemIndex) into itemDef
			where CanSelectItemEntry(itemDef, expansionAvailability)
			orderby (int)(itemDef.tier + ((itemDef.tier == ItemTier.Lunar) ? 100 : 0))
			select new Entry
			{
				nameToken = itemDef.nameToken,
				color = Color32.op_Implicit(ColorCatalog.GetColor(itemDef.darkColorIndex)),
				iconTexture = itemDef.pickupIconTexture,
				bgTexture = itemDef.bgIconTexture,
				extraData = PickupCatalog.FindPickupIndex(itemDef.itemIndex),
				modelPrefab = itemDef.pickupModelPrefab,
				getStatusImplementation = GetPickupStatus,
				getTooltipContentImplementation = GetPickupTooltipContent,
				pageBuilderMethod = PageBuilder.SimplePickup,
				isWIPImplementation = IsEntryPickupItemWithoutLore
			};
		IEnumerable<Entry> second = from pickupDef in PickupCatalog.allPickups
			select EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex) into equipmentDef
			where CanSelectEquipmentEntry(equipmentDef, expansionAvailability)
			orderby !equipmentDef.isLunar
			select new Entry
			{
				nameToken = equipmentDef.nameToken,
				color = Color32.op_Implicit(ColorCatalog.GetColor(equipmentDef.colorIndex)),
				iconTexture = equipmentDef.pickupIconTexture,
				bgTexture = equipmentDef.bgIconTexture,
				extraData = PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex),
				modelPrefab = equipmentDef.pickupModelPrefab,
				getStatusImplementation = GetPickupStatus,
				getTooltipContentImplementation = GetPickupTooltipContent,
				pageBuilderMethod = PageBuilder.SimplePickup,
				isWIPImplementation = IsEntryPickupEquipmentWithoutLore
			};
		return first.Concat(second).ToArray();
	}

	private static bool CanSelectMonsterEntry(CharacterBody characterBody, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			ExpansionRequirementComponent component = ((Component)characterBody).GetComponent<ExpansionRequirementComponent>();
			if (!Object.op_Implicit((Object)(object)component) || !Object.op_Implicit((Object)(object)component.requiredExpansion) || !expansionAvailability.ContainsKey(component.requiredExpansion) || expansionAvailability[component.requiredExpansion])
			{
				return UnlockableExists(((Component)characterBody).GetComponent<DeathRewards>()?.logUnlockableDef);
			}
			return false;
		}
		return false;
	}

	private static Entry[] BuildMonsterEntries(Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		return (from characterBody in BodyCatalog.allBodyPrefabBodyBodyComponents
			where CanSelectMonsterEntry(characterBody, expansionAvailability)
			orderby characterBody.baseMaxHealth
			select characterBody).Select(delegate(CharacterBody characterBody)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Entry obj = new Entry
			{
				nameToken = characterBody.baseNameToken,
				color = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.HardDifficulty)),
				iconTexture = characterBody.portraitIcon,
				extraData = characterBody
			};
			ModelLocator component = ((Component)characterBody).GetComponent<ModelLocator>();
			object modelPrefab;
			if (component == null)
			{
				modelPrefab = null;
			}
			else
			{
				Transform modelTransform = component.modelTransform;
				modelPrefab = ((modelTransform != null) ? ((Component)modelTransform).gameObject : null);
			}
			obj.modelPrefab = (GameObject)modelPrefab;
			obj.getStatusImplementation = GetMonsterStatus;
			obj.getTooltipContentImplementation = GetMonsterTooltipContent;
			obj.pageBuilderMethod = PageBuilder.MonsterBody;
			obj.bgTexture = (characterBody.isChampion ? LegacyResourcesAPI.Load<Texture>("Textures/ItemIcons/BG/texTier3BGIcon") : LegacyResourcesAPI.Load<Texture>("Textures/ItemIcons/BG/texTier1BGIcon"));
			obj.isWIPImplementation = IsEntryBodyWithoutLore;
			return obj;
		}).ToArray();
	}

	private static bool CanSelectStageEntry(SceneDef sceneDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if (Object.op_Implicit((Object)(object)sceneDef))
		{
			ExpansionDef requiredExpansion = sceneDef.requiredExpansion;
			if (!Object.op_Implicit((Object)(object)requiredExpansion) || !expansionAvailability.ContainsKey(requiredExpansion) || expansionAvailability[requiredExpansion])
			{
				return sceneDef.shouldIncludeInLogbook;
			}
			return false;
		}
		return false;
	}

	private static Entry[] BuildStageEntries(Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return (from sceneDef in (IEnumerable<SceneDef>)(object)SceneCatalog.allSceneDefs
			where CanSelectStageEntry(sceneDef, expansionAvailability)
			orderby sceneDef.stageOrder
			select new Entry
			{
				nameToken = sceneDef.nameToken,
				iconTexture = sceneDef.previewTexture,
				color = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier2ItemDark)),
				getStatusImplementation = GetStageStatus,
				modelPrefab = sceneDef.dioramaPrefab,
				getTooltipContentImplementation = GetStageTooltipContent,
				pageBuilderMethod = PageBuilder.Stage,
				extraData = sceneDef,
				isWIPImplementation = delegate(in Entry entry)
				{
					return !Language.english.TokenIsRegistered(((SceneDef)entry.extraData).loreToken);
				}
			}).ToArray();
	}

	private static bool CanSelectSurvivorBodyEntry(CharacterBody body, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			ExpansionRequirementComponent component = ((Component)body).GetComponent<ExpansionRequirementComponent>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.requiredExpansion) && expansionAvailability.ContainsKey(component.requiredExpansion))
			{
				return expansionAvailability[component.requiredExpansion];
			}
			return true;
		}
		return false;
	}

	private static Entry[] BuildSurvivorEntries(Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		return (from survivorDef in SurvivorCatalog.orderedSurvivorDefs
			select BodyCatalog.GetBodyPrefabBodyComponent(SurvivorCatalog.GetBodyIndexFromSurvivorIndex(survivorDef.survivorIndex)) into body
			where CanSelectSurvivorBodyEntry(body, expansionAvailability)
			select body).Select(delegate(CharacterBody characterBody)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Entry obj = new Entry
			{
				nameToken = characterBody.baseNameToken,
				color = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty)),
				iconTexture = characterBody.portraitIcon,
				bgTexture = LegacyResourcesAPI.Load<Texture>("Textures/ItemIcons/BG/texSurvivorBGIcon"),
				extraData = characterBody
			};
			ModelLocator component = ((Component)characterBody).GetComponent<ModelLocator>();
			object modelPrefab;
			if (component == null)
			{
				modelPrefab = null;
			}
			else
			{
				Transform modelTransform = component.modelTransform;
				modelPrefab = ((modelTransform != null) ? ((Component)modelTransform).gameObject : null);
			}
			obj.modelPrefab = (GameObject)modelPrefab;
			obj.getStatusImplementation = GetSurvivorStatus;
			obj.getTooltipContentImplementation = GetSurvivorTooltipContent;
			obj.pageBuilderMethod = PageBuilder.SurvivorBody;
			obj.isWIPImplementation = IsEntryBodyWithoutLore;
			return obj;
		}).ToArray();
	}

	private static bool CanSelectAchievementEntry(AchievementDef achievementDef, Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		if (achievementDef != null)
		{
			ExpansionDef expansionDefForUnlockable = UnlockableCatalog.GetExpansionDefForUnlockable(UnlockableCatalog.GetUnlockableDef(achievementDef.unlockableRewardIdentifier)?.index ?? UnlockableIndex.None);
			if (Object.op_Implicit((Object)(object)expansionDefForUnlockable) && expansionAvailability.ContainsKey(expansionDefForUnlockable))
			{
				return expansionAvailability[expansionDefForUnlockable];
			}
			return true;
		}
		return false;
	}

	private static Entry[] BuildAchievementEntries(Dictionary<ExpansionDef, bool> expansionAvailability)
	{
		return AchievementManager.allAchievementDefs.Where((AchievementDef achievementDef) => CanSelectAchievementEntry(achievementDef, expansionAvailability)).Select(delegate(AchievementDef achievementDef)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Entry obj = new Entry
			{
				nameToken = achievementDef.nameToken,
				color = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty))
			};
			Sprite achievedIcon = achievementDef.GetAchievedIcon();
			obj.iconTexture = (Texture)(object)((achievedIcon != null) ? achievedIcon.texture : null);
			obj.extraData = achievementDef;
			obj.modelPrefab = null;
			obj.getStatusImplementation = GetAchievementStatus;
			obj.getTooltipContentImplementation = GetAchievementTooltipContent;
			return obj;
		}).ToArray();
	}

	private static Entry[] BuildProfileEntries(UserProfile viewerProfile)
	{
		List<Entry> entries = new List<Entry>();
		if (true)
		{
			foreach (string availableProfileName in PlatformSystems.saveSystem.GetAvailableProfileNames())
			{
				AddProfileStatsEntry(PlatformSystems.saveSystem.GetProfile(availableProfileName));
			}
		}
		else if (viewerProfile != null)
		{
			AddProfileStatsEntry(viewerProfile);
		}
		return entries.ToArray();
		void AddProfileStatsEntry(UserProfile userProfile)
		{
			Entry item = new Entry
			{
				pageBuilderMethod = PageBuilder.StatsPanel,
				getStatusImplementation = delegate
				{
					return EntryStatus.Available;
				},
				extraData = userProfile,
				getDisplayNameImplementation = delegate(in Entry entry, UserProfile _viewerProfile)
				{
					return ((UserProfile)entry.extraData).name;
				},
				iconTexture = userProfile.portraitTexture
			};
			entries.Add(item);
		}
	}

	private static Entry[] BuildMorgueEntries(UserProfile viewerProfile)
	{
		List<Entry> entries = CollectionPool<Entry, List<Entry>>.RentCollection();
		List<RunReport> list = CollectionPool<RunReport, List<RunReport>>.RentCollection();
		MorgueManager.LoadHistoryRunReports(list);
		foreach (RunReport item2 in list)
		{
			AddRunReportEntry(item2);
		}
		CollectionPool<RunReport, List<RunReport>>.ReturnCollection(list);
		Entry[] result = entries.ToArray();
		CollectionPool<Entry, List<Entry>>.ReturnCollection(entries);
		return result;
		void AddRunReportEntry(RunReport runReport)
		{
			GetPrimaryPlayerInfo(runReport, out var _, out var primaryPlayerBody3);
			Entry item = new Entry
			{
				pageBuilderMethod = PageBuilder.RunReportPanel,
				getStatusImplementation = GetEntryStatus,
				extraData = runReport,
				getDisplayNameImplementation = GetDisplayName,
				getTooltipContentImplementation = GetTooltipContent,
				iconTexture = primaryPlayerBody3?.portraitIcon
			};
			entries.Add(item);
		}
		static string GetDisplayName(in Entry entry, UserProfile _viewerProfile)
		{
			return ((RunReport)entry.extraData).snapshotTimeUtc.ToLocalTime().ToString("G", CultureInfo.CurrentCulture);
		}
		static EntryStatus GetEntryStatus(in Entry entry, UserProfile _viewerProfile)
		{
			return EntryStatus.Available;
		}
		void GetPrimaryPlayerInfo(RunReport _runReport, out RunReport.PlayerInfo primaryPlayerInfo, out CharacterBody primaryPlayerBody)
		{
			primaryPlayerInfo = _runReport.FindPlayerInfo(viewerProfile) ?? _runReport.FindFirstPlayerInfo();
			primaryPlayerBody = BodyCatalog.GetBodyPrefabBodyComponent(primaryPlayerInfo?.bodyIndex ?? BodyIndex.None);
		}
		TooltipContent GetTooltipContent(in Entry entry, UserProfile _viewerProfile, EntryStatus entryStatus)
		{
			RunReport runReport2 = (RunReport)entry.extraData;
			GetPrimaryPlayerInfo(runReport2, out var _, out var primaryPlayerBody2);
			TooltipContent result2 = default(TooltipContent);
			result2.overrideTitleText = Language.GetStringFormatted("LOGBOOK_ENTRY_RUNREPORT_TOOLTIP_TITLE_FORMAT", runReport2.snapshotTimeUtc.ToLocalTime().ToString("G", CultureInfo.CurrentCulture));
			result2.overrideBodyText = Language.GetStringFormatted("LOGBOOK_ENTRY_RUNREPORT_TOOLTIP_BODY_FORMAT", Language.GetString(primaryPlayerBody2?.baseNameToken ?? string.Empty), Language.GetString(runReport2.gameMode?.nameToken ?? string.Empty), Language.GetString(runReport2.gameEnding?.endingTextToken ?? string.Empty));
			return result2;
		}
	}

	private static CategoryDef[] BuildCategories()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<ExpansionDef, bool> dictionary = new Dictionary<ExpansionDef, bool>();
		Enumerator<ExpansionDef> enumerator = ExpansionCatalog.expansionDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ExpansionDef current = enumerator.Current;
				dictionary.Add(current, !Object.op_Implicit((Object)(object)current.requiredEntitlement) || EntitlementManager.localUserEntitlementTracker.AnyUserHasEntitlement(current.requiredEntitlement));
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		Entry[] pickupEntries = BuildPickupEntries(dictionary);
		Entry[] monsterEntries = BuildMonsterEntries(dictionary);
		Entry[] stageEntries = BuildStageEntries(dictionary);
		Entry[] survivorEntries = BuildSurvivorEntries(dictionary);
		Entry[] achievementEntries = BuildAchievementEntries(dictionary);
		return new CategoryDef[7]
		{
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_ITEMANDEQUIPMENT",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/ItemEntryIcon"),
				buildEntries = (UserProfile viewerProfile) => pickupEntries
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_MONSTER",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/MonsterEntryIcon"),
				buildEntries = (UserProfile viewerProfile) => monsterEntries
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_STAGE",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/StageEntryIcon"),
				buildEntries = (UserProfile viewerProfile) => stageEntries
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_SURVIVOR",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/SurvivorEntryIcon"),
				buildEntries = (UserProfile viewerProfile) => survivorEntries
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_ACHIEVEMENTS",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/AchievementEntryIcon"),
				buildEntries = (UserProfile viewerProfile) => achievementEntries,
				initializeElementGraphics = CategoryDef.InitializeChallenge
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_STATS",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/StatsEntryIcon"),
				buildEntries = BuildProfileEntries,
				initializeElementGraphics = CategoryDef.InitializeStats
			},
			new CategoryDef
			{
				nameToken = "LOGBOOK_CATEGORY_MORGUE",
				iconPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Logbook/MorgueEntryIcon"),
				buildEntries = BuildMorgueEntries,
				initializeElementGraphics = CategoryDef.InitializeMorgue
			}
		};
	}

	[ConCommand(commandName = "logbook_list_unfinished_lore", flags = ConVarFlags.None, helpText = "Prints all logbook entries which still have undefined lore.")]
	private static void CCLogbookListUnfinishedLore(ConCommandArgs args)
	{
		List<string> list = new List<string>();
		CategoryDef[] array = categories;
		for (int i = 0; i < array.Length; i++)
		{
			Entry[] array2 = array[i].BuildEntries(args.GetSenderLocalUser()?.userProfile);
			foreach (Entry entry in array2)
			{
				string text = "";
				object extraData = entry.extraData;
				Object val;
				if ((val = (Object)((extraData is Object) ? extraData : null)) != null)
				{
					text = val.name;
				}
				if (entry.isWip)
				{
					list.Add(entry.extraData?.GetType()?.Name + " \"" + text + "\" \"" + Language.GetString(entry.nameToken) + "\"");
				}
			}
		}
		args.Log(string.Join("\n", list));
	}

	private static void RegisterViewables(CategoryDef[] categoriesToGenerateFrom)
	{
		ViewablesCatalog.Node node = new ViewablesCatalog.Node("Logbook", isFolder: true);
		CategoryDef[] array = categories;
		foreach (CategoryDef obj in array)
		{
			ViewablesCatalog.Node parent = (obj.viewableNode = new ViewablesCatalog.Node(obj.nameToken, isFolder: true, node));
			Entry[] array2 = obj.BuildEntries(null);
			foreach (Entry entry in array2)
			{
				string nameToken = entry.nameToken;
				ViewablesCatalog.Node entryNode;
				AchievementDef achievementDef;
				bool hasPrereq;
				if (!entry.isWip && !(nameToken == "TOOLTIP_WIP_CONTENT_NAME") && !string.IsNullOrEmpty(nameToken))
				{
					entryNode = new ViewablesCatalog.Node(nameToken, isFolder: false, parent);
					if ((achievementDef = entry.extraData as AchievementDef) != null)
					{
						hasPrereq = !string.IsNullOrEmpty(achievementDef.prerequisiteAchievementIdentifier);
						entryNode.shouldShowUnviewed = Check;
					}
					else
					{
						entryNode.shouldShowUnviewed = Check;
					}
					entry.viewableNode = entryNode;
				}
				bool Check(UserProfile userProfile)
				{
					if (userProfile.HasViewedViewable(entryNode.fullName))
					{
						return false;
					}
					if (userProfile.HasAchievement(achievementDef.identifier))
					{
						return false;
					}
					if (hasPrereq)
					{
						return userProfile.HasAchievement(achievementDef.prerequisiteAchievementIdentifier);
					}
					return true;
				}
				bool Check(UserProfile viewerProfile)
				{
					if (viewerProfile.HasViewedViewable(entryNode.fullName))
					{
						return false;
					}
					return entry.GetStatus(viewerProfile) == EntryStatus.Available;
				}
			}
		}
		ViewablesCatalog.AddNodeToRoot(node);
	}
}
