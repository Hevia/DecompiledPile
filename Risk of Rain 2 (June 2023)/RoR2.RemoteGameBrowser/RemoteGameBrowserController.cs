using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HG;
using JetBrains.Annotations;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.RemoteGameBrowser;

public class RemoteGameBrowserController : MonoBehaviour
{
	private class FilterInfo
	{
		public RemoteGameFilterValue value;

		public string token;

		public Func<Component, RemoteGameFilterValue?> getUIValue;

		public Action<Component, RemoteGameFilterValue> setUIValue;

		public GameObject controlGameObject;

		public Component controlMainComponent;

		public LanguageTextMeshController labelController;
	}

	private class FilterManager
	{
		[MeansImplicitUse(/*Could not decode attribute arguments.*/)]
		private class SetupAttribute : Attribute
		{
			public RemoteGameFilterValue defaultValue;

			public int minValue;

			public int maxValue;

			public SetupAttribute(bool defaultValue)
			{
				this.defaultValue = defaultValue;
			}

			public SetupAttribute(int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
			{
				this.defaultValue = defaultValue;
				this.minValue = minValue;
				this.maxValue = maxValue;
			}

			public SetupAttribute(string defaultValue)
			{
				this.defaultValue = defaultValue;
			}
		}

		public readonly RemoteGameBrowserController owner;

		private RectTransform currentContainer;

		public List<FilterInfo> allFilters = new List<FilterInfo>();

		[Setup(true)]
		public FilterInfo showDedicatedServers;

		[Setup(true)]
		public FilterInfo showLobbies;

		[Setup(true)]
		public FilterInfo showDifficultyEasyGames;

		[Setup(true)]
		public FilterInfo showDifficultyNormalGames;

		[Setup(true)]
		public FilterInfo showDifficultyHardGames;

		[Setup(true)]
		public FilterInfo showGamesWithRuleVoting;

		[Setup(true)]
		public FilterInfo showGamesWithoutRuleVoting;

		[Setup(true)]
		public FilterInfo showPasswordedGames;

		[Setup(0, int.MinValue, int.MaxValue, minValue = 0, maxValue = 999)]
		public FilterInfo maxPing;

		[Setup(false)]
		public FilterInfo mustHavePlayers;

		[Setup(true)]
		public FilterInfo mustHaveEnoughSlots;

		[Setup(1, int.MinValue, int.MaxValue, minValue = 1)]
		public FilterInfo minMaxPlayers;

		[Setup(16, int.MinValue, int.MaxValue, minValue = 1)]
		public FilterInfo maxMaxPlayers;

		[Setup("")]
		public FilterInfo requiredTags;

		[Setup("")]
		public FilterInfo forbiddenTags;

		[Setup(false)]
		public FilterInfo showStartedGames;

		[Setup(true)]
		public FilterInfo hideIncompatibleGames;

		public FilterManager(RemoteGameBrowserController owner)
		{
			this.owner = owner;
			currentContainer = owner.filterControlContainer;
			GenerateFilters();
		}

		private FilterInfo AddFilter<T>(string token, RemoteGameFilterValue defaultValue, GameObject controlPrefab, Func<Component, RemoteGameFilterValue?> getUIValue, Action<Component, RemoteGameFilterValue> setUIValue) where T : Component
		{
			FilterInfo filterInfo = new FilterInfo
			{
				token = token,
				value = defaultValue,
				controlGameObject = Object.Instantiate<GameObject>(controlPrefab, (Transform)(object)currentContainer),
				getUIValue = getUIValue,
				setUIValue = setUIValue
			};
			filterInfo.controlMainComponent = (Component)(object)((Component)filterInfo.controlGameObject.transform.Find("MainControl")).GetComponent<T>();
			filterInfo.labelController = ((Component)filterInfo.controlGameObject.transform.Find("NameLabel")).GetComponent<LanguageTextMeshController>();
			filterInfo.controlGameObject.SetActive(true);
			filterInfo.setUIValue(filterInfo.controlMainComponent, defaultValue);
			allFilters.Add(filterInfo);
			return filterInfo;
		}

		private FilterInfo AddBoolFilter(string token, bool defaultValue)
		{
			return AddFilter<MPToggle>(token, (RemoteGameFilterValue)defaultValue, owner.togglePrefab, (Func<Component, RemoteGameFilterValue?>)GetUIValue, (Action<Component, RemoteGameFilterValue>)SetUIValue);
			static RemoteGameFilterValue? GetUIValue(Component c)
			{
				return ((Toggle)(MPToggle)(object)c).isOn;
			}
			static void SetUIValue(Component c, RemoteGameFilterValue value)
			{
				((Toggle)(MPToggle)(object)c).isOn = value.boolValue;
			}
		}

		private FilterInfo AddIntFilter(string token, int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue, uint minDigits = 1u, uint maxDigits = uint.MaxValue)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			FilterInfo filterInfo = AddFilter<TMP_InputField>(token, (RemoteGameFilterValue)defaultValue, owner.textFieldPrefab, (Func<Component, RemoteGameFilterValue?>)GetUIValue, (Action<Component, RemoteGameFilterValue>)SetUIValue);
			((TMP_InputField)filterInfo.controlMainComponent).characterValidation = (CharacterValidation)2;
			return filterInfo;
			RemoteGameFilterValue? GetUIValue(Component c)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				if (int.TryParse(((TMP_InputField)c).text, out var result))
				{
					return Mathf.Clamp(result, minValue, maxValue);
				}
				return null;
			}
			void SetUIValue(Component c, RemoteGameFilterValue v)
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
				stringBuilder.AppendInt(v.intValue, minDigits, maxDigits);
				((TMP_InputField)c).SetTextWithoutNotify(stringBuilder.ToString());
				StringBuilderPool.ReturnStringBuilder(stringBuilder);
			}
		}

		private FilterInfo AddStringFilter(string token, string defaultValue)
		{
			return AddFilter<TMP_InputField>(token, (RemoteGameFilterValue)defaultValue, owner.textFieldPrefab, (Func<Component, RemoteGameFilterValue?>)GetUIValue, (Action<Component, RemoteGameFilterValue>)SetUIValue);
			static RemoteGameFilterValue? GetUIValue(Component c)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return ((TMP_InputField)c).text;
			}
			static void SetUIValue(Component c, RemoteGameFilterValue v)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				((TMP_InputField)c).SetTextWithoutNotify(v.stringValue);
			}
		}

		private void GenerateFilters()
		{
			Regex regex = new Regex("([A-Z]?[a-z]+)");
			FieldInfo[] fields = typeof(FilterManager).GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldType != typeof(FilterInfo))
				{
					continue;
				}
				SetupAttribute customAttribute = fieldInfo.GetCustomAttribute<SetupAttribute>();
				if (customAttribute == null)
				{
					return;
				}
				string name = fieldInfo.Name;
				StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
				stringBuilder.Append("GAME_BROWSER_FILTER");
				foreach (Match item in regex.Matches(name))
				{
					stringBuilder.Append("_");
					for (int j = 0; j < item.Length; j++)
					{
						stringBuilder.Append(char.ToUpperInvariant(name[item.Index + j]));
					}
				}
				string token = stringBuilder.ToString();
				StringBuilderPool.ReturnStringBuilder(stringBuilder);
				FilterInfo filterInfo = null;
				filterInfo = customAttribute.defaultValue.valueType switch
				{
					RemoteGameFilterValue.ValueType.Bool => AddBoolFilter(token, customAttribute.defaultValue.boolValue), 
					RemoteGameFilterValue.ValueType.Int => AddIntFilter(token, customAttribute.defaultValue.intValue, customAttribute.minValue, customAttribute.maxValue), 
					RemoteGameFilterValue.ValueType.String => AddStringFilter(token, customAttribute.defaultValue.stringValue), 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				fieldInfo.SetValue(this, filterInfo);
				filterInfo.labelController.token = filterInfo.token;
			}
			CopyInternalValuesToUI();
		}

		public void CopyInternalValuesToUI()
		{
			foreach (FilterInfo allFilter in allFilters)
			{
				allFilter.setUIValue(allFilter.controlMainComponent, allFilter.value);
			}
		}

		public void CopyUIValuesToInternal()
		{
			foreach (FilterInfo allFilter in allFilters)
			{
				allFilter.value = allFilter.getUIValue(allFilter.controlMainComponent) ?? allFilter.value;
			}
		}
	}

	public GameObject cardPrefab;

	public RectTransform cardContainer;

	public MPButton previousPageButton;

	public MPButton nextPageButton;

	public HGTextMeshProUGUI pageNumberLabel;

	public GameObject togglePrefab;

	public GameObject textFieldPrefab;

	public RectTransform filterControlContainer;

	public MPDropdown sortTypeDropdown;

	public MPToggle sortAscendToggle;

	public Graphic busyIcon;

	private UIElementAllocator<RemoteGameCardController> cardAllocator;

	private bool displayDataDirty;

	private float cardPrefabHeight = 1f;

	private float initialRequestTime = float.PositiveInfinity;

	private IRemoteGameProvider primaryRemoteGameProvider;

	private PageRemoteGameProvider pageRemoteGameProvider;

	private SortRemoteGameProvider sortRemoteGameProvider;

	private AdvancedFilterRemoteGameProvider advancedFilterRemoteGameProvider;

	private AggregateRemoteGameProvider aggregateRemoteGameProvider;

	private SteamworksServerRemoteGameProvider serverRemoteGameProvider;

	private BaseAsyncRemoteGameProvider lobbyRemoteGameProvider;

	private FilterManager filters;

	private void Awake()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		cardAllocator = new UIElementAllocator<RemoteGameCardController>(cardContainer, cardPrefab);
		serverRemoteGameProvider = new SteamworksServerRemoteGameProvider(SteamworksServerRemoteGameProvider.Mode.Internet)
		{
			refreshOnFiltersChanged = true
		};
		if (PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			lobbyRemoteGameProvider = new EOSLobbyRemoteGameProvider();
		}
		else
		{
			lobbyRemoteGameProvider = new SteamworksLobbyRemoteGameProvider();
		}
		aggregateRemoteGameProvider = new AggregateRemoteGameProvider();
		advancedFilterRemoteGameProvider = new AdvancedFilterRemoteGameProvider(aggregateRemoteGameProvider);
		sortRemoteGameProvider = new SortRemoteGameProvider(advancedFilterRemoteGameProvider);
		pageRemoteGameProvider = new PageRemoteGameProvider(sortRemoteGameProvider);
		primaryRemoteGameProvider = pageRemoteGameProvider;
		primaryRemoteGameProvider.onNewInfoAvailable += OnNewInfoAvailable;
		((UnityEvent)((Button)previousPageButton).onClick).AddListener(new UnityAction(OnPreviousPageButtonClick));
		((UnityEvent)((Button)nextPageButton).onClick).AddListener(new UnityAction(OnNextPageButtonClick));
		cardPrefabHeight = cardPrefab.GetComponent<LayoutElement>().preferredHeight;
	}

	private void Start()
	{
		filters = new FilterManager(this);
		initialRequestTime = Time.unscaledTime + 0.2f;
	}

	private void Update()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)286) || initialRequestTime <= Time.unscaledTime)
		{
			initialRequestTime = float.PositiveInfinity;
			RequestRefresh();
		}
		UpdateSearchFiltersInternal();
		UpdateSorting();
		Rect rect = cardContainer.rect;
		float height = ((Rect)(ref rect)).height;
		float num = cardPrefabHeight;
		int gamesPerPage = Mathf.FloorToInt(height / num);
		pageRemoteGameProvider.SetGamesPerPage(gamesPerPage);
		((Selectable)previousPageButton).interactable = pageRemoteGameProvider.CanGoToPreviousPage();
		((Selectable)nextPageButton).interactable = pageRemoteGameProvider.CanGoToNextPage();
		if (displayDataDirty)
		{
			SetDisplayData(primaryRemoteGameProvider.GetKnownGames());
		}
		UpdateBusyIcon();
	}

	private void OnDestroy()
	{
		pageRemoteGameProvider.Dispose();
		sortRemoteGameProvider.Dispose();
		advancedFilterRemoteGameProvider.Dispose();
		aggregateRemoteGameProvider.Dispose();
		lobbyRemoteGameProvider.Dispose();
		serverRemoteGameProvider.Dispose();
	}

	private void OnEnable()
	{
		primaryRemoteGameProvider.RequestRefresh();
		RebuildSortTypeDropdown();
	}

	private void OnPreviousPageButtonClick()
	{
		pageRemoteGameProvider.GoToPreviousPage();
	}

	private void OnNextPageButtonClick()
	{
		pageRemoteGameProvider.GoToNextPage();
	}

	public void RequestRefresh()
	{
		primaryRemoteGameProvider.RequestRefresh();
	}

	private void OnNewInfoAvailable()
	{
		displayDataDirty = true;
	}

	private void SetDisplayData(IList<RemoteGameInfo> remoteGameInfos)
	{
		displayDataDirty = false;
		cardAllocator.AllocateElements(remoteGameInfos.Count);
		ReadOnlyCollection<RemoteGameCardController> elements = cardAllocator.elements;
		for (int i = 0; i < elements.Count; i++)
		{
			elements[i].SetDisplayData(remoteGameInfos[i]);
		}
		if (Object.op_Implicit((Object)(object)pageNumberLabel))
		{
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			pageRemoteGameProvider.GetCurrentPageInfo(out var pageIndex, out var maxPages);
			stringBuilder.AppendInt(pageIndex + 1).Append("/").AppendInt(Math.Max(maxPages, 1));
			((TMP_Text)pageNumberLabel).SetText(stringBuilder);
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
	}

	private void UpdateSearchFiltersInternal()
	{
		filters.CopyUIValuesToInternal();
		if (!PlatformSystems.ShouldUseEpicOnlineSystems)
		{
			aggregateRemoteGameProvider.SetProviderAdded(serverRemoteGameProvider, filters.showDedicatedServers.value.boolValue);
			SteamworksServerRemoteGameProvider.SearchFilters searchFilters = serverRemoteGameProvider.GetSearchFilters();
			searchFilters.allowDedicatedServers = filters.showDedicatedServers.value.boolValue;
			searchFilters.allowListenServers = true;
			searchFilters.mustNotBeFull = filters.mustHaveEnoughSlots.value.boolValue;
			searchFilters.mustHavePlayers = filters.mustHavePlayers.value.boolValue;
			searchFilters.requiredTags = filters.requiredTags.value.stringValue;
			searchFilters.forbiddenTags = filters.forbiddenTags.value.stringValue;
			searchFilters.allowInProgressGames = filters.showStartedGames.value.boolValue;
			searchFilters.allowMismatchedMods = !filters.hideIncompatibleGames.value.boolValue;
			serverRemoteGameProvider.SetSearchFilters(searchFilters);
		}
		aggregateRemoteGameProvider.SetProviderAdded(lobbyRemoteGameProvider, filters.showLobbies.value.boolValue);
		BaseAsyncRemoteGameProvider.SearchFilters searchFilters2 = lobbyRemoteGameProvider.GetSearchFilters();
		searchFilters2.allowMismatchedMods = !filters.hideIncompatibleGames.value.boolValue;
		lobbyRemoteGameProvider.SetSearchFilters(searchFilters2);
		int requiredSlots = 0;
		if (!filters.mustHaveEnoughSlots.value.boolValue && PlatformSystems.lobbyManager.calculatedTotalPlayerCount > 0)
		{
			requiredSlots = PlatformSystems.lobbyManager.calculatedTotalPlayerCount;
		}
		AdvancedFilterRemoteGameProvider.SearchFilters searchFilters3 = advancedFilterRemoteGameProvider.GetSearchFilters();
		searchFilters3.allowPassword = filters.showPasswordedGames.value.boolValue;
		searchFilters3.minMaxPlayers = filters.minMaxPlayers.value.intValue;
		searchFilters3.maxMaxPlayers = filters.maxMaxPlayers.value.intValue;
		searchFilters3.maxPing = filters.maxPing.value.intValue;
		searchFilters3.requiredSlots = requiredSlots;
		searchFilters3.allowDifficultyEasy = filters.showDifficultyEasyGames.value.boolValue;
		searchFilters3.allowDifficultyNormal = filters.showDifficultyNormalGames.value.boolValue;
		searchFilters3.allowDifficultyHard = filters.showDifficultyHardGames.value.boolValue;
		searchFilters3.showGamesWithRuleVoting = filters.showGamesWithRuleVoting.value.boolValue;
		searchFilters3.showGamesWithoutRuleVoting = filters.showGamesWithoutRuleVoting.value.boolValue;
		searchFilters3.allowInProgressGames = filters.showStartedGames.value.boolValue;
		advancedFilterRemoteGameProvider.SetSearchFilters(searchFilters3);
	}

	private void RebuildSortTypeDropdown()
	{
		List<string> list = CollectionPool<string, List<string>>.RentCollection();
		((TMP_Dropdown)sortTypeDropdown).ClearOptions();
		for (int i = 0; i < SortRemoteGameProvider.sorters.Length; i++)
		{
			list.Add(Language.GetString(SortRemoteGameProvider.sorters[i].nameToken));
		}
		((TMP_Dropdown)sortTypeDropdown).AddOptions(list);
		SortRemoteGameProvider.Parameters parameters = sortRemoteGameProvider.GetParameters();
		((TMP_Dropdown)sortTypeDropdown).SetValueWithoutNotify(parameters.sorterIndex);
		CollectionPool<string, List<string>>.ReturnCollection(list);
	}

	private void UpdateSorting()
	{
		SortRemoteGameProvider.Parameters parameters = sortRemoteGameProvider.GetParameters();
		parameters.ascending = ((Toggle)sortAscendToggle).isOn;
		parameters.sorterIndex = ((TMP_Dropdown)sortTypeDropdown).value;
		sortRemoteGameProvider.SetParameters(parameters);
	}

	private void UpdateBusyIcon()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)busyIcon))
		{
			Color color = busyIcon.color;
			float num = 1f;
			if (!primaryRemoteGameProvider.IsBusy())
			{
				num = color.a - Time.unscaledDeltaTime;
			}
			if (num != color.a)
			{
				color.a = num;
				busyIcon.color = color;
			}
		}
	}

	[ContextMenu("Copy Filter Tokens")]
	private void CopyFilterTokens()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (FilterInfo allFilter in filters.allFilters)
		{
			stringBuilder.Append('"').Append(allFilter.token).Append('"')
				.Append(": \"\",")
				.AppendLine();
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
	}
}
