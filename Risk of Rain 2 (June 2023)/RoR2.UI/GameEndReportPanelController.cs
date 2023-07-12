using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using RoR2.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemProvider))]
public class GameEndReportPanelController : MonoBehaviour
{
	public struct DisplayData : IEquatable<DisplayData>
	{
		[CanBeNull]
		public RunReport runReport;

		public int playerIndex;

		public bool Equals(DisplayData other)
		{
			if (object.Equals(runReport, other.runReport))
			{
				return playerIndex == other.playerIndex;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is DisplayData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((-1418150836 * -1521134295 + base.GetHashCode()) * -1521134295 + EqualityComparer<RunReport>.Default.GetHashCode(runReport)) * -1521134295 + playerIndex.GetHashCode();
		}
	}

	[Tooltip("The TextMeshProUGUI component to use to display the result of the game: Win or Loss")]
	[Header("Result")]
	public TextMeshProUGUI resultLabel;

	public Image resultIconBackgroundImage;

	public Image resultIconForegroundImage;

	public HGTextMeshProUGUI finalMessageLabel;

	[Header("Run Settings")]
	[Tooltip("The Image component to use to display the selected difficulty of the run.")]
	public Image selectedDifficultyImage;

	[Tooltip("The LanguageTextMeshController component to use to display the selected difficulty of the run.")]
	public LanguageTextMeshController selectedDifficultyLabel;

	[Tooltip("The ArtifactDisplayPanelController component to send the enabled artifacts of the run to.")]
	public ArtifactDisplayPanelController artifactDisplayPanelController;

	[Tooltip("A list of StatDef names to display in the stats section.")]
	[Header("Stats")]
	public string[] statsToDisplay;

	[Tooltip("Prefab to be used for stat display.")]
	public GameObject statStripPrefab;

	[Tooltip("The RectTransform in which to build the stat strips.")]
	public RectTransform statContentArea;

	[Tooltip("The TextMeshProUGUI component used to display the total points.")]
	public TextMeshProUGUI totalPointsLabel;

	[Header("Unlocks")]
	[Tooltip("Prefab to be used for unlock display.")]
	public GameObject unlockStripPrefab;

	[Tooltip("The RectTransform in which to build the unlock strips.")]
	public RectTransform unlockContentArea;

	[Header("Items")]
	[Tooltip("The inventory display controller.")]
	public ItemInventoryDisplay itemInventoryDisplay;

	[Header("Player Info")]
	[Tooltip("The RawImage component to use to display the player character's portrait.")]
	public RawImage playerBodyPortraitImage;

	[Tooltip("The TextMeshProUGUI component to use to display the player character's body name.")]
	public TextMeshProUGUI playerBodyLabel;

	[Tooltip("The TextMeshProUGUI component to use to display the player's username.")]
	public TextMeshProUGUI playerUsernameLabel;

	[Tooltip("The RawImage component to use to display the killer character's portrait.")]
	[Header("Killer Info")]
	public RawImage killerBodyPortraitImage;

	[Tooltip("The TextMeshProUGUI component to use to display the killer character's body name.")]
	public TextMeshProUGUI killerBodyLabel;

	[Tooltip("The GameObject used as the panel for the killer information. This is used to disable the killer panel when the player has won the game.")]
	public GameObject killerPanelObject;

	[Header("Navigation and Misc")]
	public MPButton continueButton;

	public RectTransform chatboxTransform;

	public CarouselNavigationController playerNavigationController;

	public RectTransform selectedPlayerEffectRoot;

	public RectTransform acceptButtonArea;

	private readonly List<GameObject> statStrips = new List<GameObject>();

	private readonly List<GameObject> unlockStrips = new List<GameObject>();

	public DisplayData displayData { get; private set; }

	private void Awake()
	{
		playerNavigationController.onPageChangeSubmitted += OnPlayerNavigationControllerPageChangeSubmitted;
		SetContinueButtonAction(null);
	}

	private void AllocateStatStrips(int count)
	{
		while (statStrips.Count > count)
		{
			int index = statStrips.Count - 1;
			Object.Destroy((Object)(object)statStrips[index].gameObject);
			statStrips.RemoveAt(index);
		}
		while (statStrips.Count < count)
		{
			GameObject val = Object.Instantiate<GameObject>(statStripPrefab, (Transform)(object)statContentArea);
			val.SetActive(true);
			statStrips.Add(val);
		}
	}

	private void AllocateUnlockStrips(int count)
	{
		while (unlockStrips.Count > count)
		{
			int index = unlockStrips.Count - 1;
			Object.Destroy((Object)(object)unlockStrips[index].gameObject);
			unlockStrips.RemoveAt(index);
		}
		while (unlockStrips.Count < count)
		{
			GameObject val = Object.Instantiate<GameObject>(unlockStripPrefab, (Transform)(object)unlockContentArea);
			val.SetActive(true);
			unlockStrips.Add(val);
		}
	}

	public void SetDisplayData(DisplayData newDisplayData)
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		if (displayData.Equals(newDisplayData))
		{
			return;
		}
		displayData = newDisplayData;
		bool flag = Object.op_Implicit((Object)(object)Run.instance) && displayData.runReport != null && Run.instance.GetUniqueId() == displayData.runReport.runGuid;
		if (Object.op_Implicit((Object)(object)resultLabel) && Object.op_Implicit((Object)(object)resultIconBackgroundImage) && Object.op_Implicit((Object)(object)resultIconForegroundImage))
		{
			GameEndingDef gameEndingDef = null;
			if (displayData.runReport != null)
			{
				gameEndingDef = displayData.runReport.gameEnding;
			}
			((Component)resultLabel).gameObject.SetActive(Object.op_Implicit((Object)(object)gameEndingDef));
			((Component)resultIconBackgroundImage).gameObject.SetActive(Object.op_Implicit((Object)(object)gameEndingDef));
			((Component)resultIconForegroundImage).gameObject.SetActive(Object.op_Implicit((Object)(object)gameEndingDef));
			if (Object.op_Implicit((Object)(object)gameEndingDef))
			{
				((TMP_Text)resultLabel).text = Language.GetString(gameEndingDef.endingTextToken);
				((Graphic)resultIconBackgroundImage).color = gameEndingDef.backgroundColor;
				((Graphic)resultIconForegroundImage).color = gameEndingDef.foregroundColor;
				resultIconForegroundImage.sprite = gameEndingDef.icon;
				((Graphic)resultIconBackgroundImage).material = gameEndingDef.material;
			}
		}
		DifficultyIndex difficultyIndex = DifficultyIndex.Invalid;
		if (displayData.runReport != null)
		{
			difficultyIndex = displayData.runReport.ruleBook.FindDifficulty();
		}
		DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(difficultyIndex);
		if (Object.op_Implicit((Object)(object)selectedDifficultyImage))
		{
			selectedDifficultyImage.sprite = difficultyDef?.GetIconSprite();
		}
		if (Object.op_Implicit((Object)(object)selectedDifficultyLabel))
		{
			selectedDifficultyLabel.token = difficultyDef?.nameToken;
		}
		if (Object.op_Implicit((Object)(object)artifactDisplayPanelController))
		{
			RuleBook ruleBook = displayData.runReport.ruleBook;
			List<ArtifactDef> list = new List<ArtifactDef>(ArtifactCatalog.artifactCount);
			for (int i = 0; i < RuleCatalog.choiceCount; i++)
			{
				RuleChoiceDef choiceDef = RuleCatalog.GetChoiceDef(i);
				if (choiceDef.artifactIndex != ArtifactIndex.None && ruleBook.GetRuleChoice(choiceDef.ruleDef) == choiceDef)
				{
					list.Add(ArtifactCatalog.GetArtifactDef(choiceDef.artifactIndex));
				}
			}
			List<ArtifactDef>.Enumerator enabledArtifacts = list.GetEnumerator();
			artifactDisplayPanelController.SetDisplayData(ref enabledArtifacts);
		}
		RunReport.PlayerInfo playerInfo = displayData.runReport?.GetPlayerInfoSafe(displayData.playerIndex);
		SetPlayerInfo(playerInfo);
		int pageCount = displayData.runReport?.playerInfoCount ?? 0;
		((Component)playerNavigationController).gameObject.SetActive((displayData.runReport?.playerInfoCount ?? 0) > 1);
		if (Object.op_Implicit((Object)(object)chatboxTransform))
		{
			((Component)chatboxTransform).gameObject.SetActive(!RoR2Application.isInSinglePlayer && flag);
		}
		playerNavigationController.SetDisplayData(new CarouselNavigationController.DisplayData(pageCount, displayData.playerIndex));
		ReadOnlyCollection<MPButton> elements = playerNavigationController.buttonAllocator.elements;
		for (int j = 0; j < elements.Count; j++)
		{
			MPButton mPButton = elements[j];
			RunReport.PlayerInfo playerInfo2 = displayData.runReport.GetPlayerInfo(j);
			CharacterBody bodyPrefabBodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(playerInfo2.bodyIndex);
			Texture texture = (Object.op_Implicit((Object)(object)bodyPrefabBodyComponent) ? bodyPrefabBodyComponent.portraitIcon : null);
			((Component)mPButton).GetComponentInChildren<RawImage>().texture = texture;
			((Component)mPButton).GetComponent<TooltipProvider>().SetContent(TooltipProvider.GetPlayerNameTooltipContent(playerInfo2.name));
		}
		((Component)selectedPlayerEffectRoot).transform.SetParent(((Component)playerNavigationController.buttonAllocator.elements[displayData.playerIndex]).transform);
		((Component)selectedPlayerEffectRoot).gameObject.SetActive(false);
		((Component)selectedPlayerEffectRoot).gameObject.SetActive(true);
		selectedPlayerEffectRoot.offsetMin = Vector2.zero;
		selectedPlayerEffectRoot.offsetMax = Vector2.zero;
		((Transform)selectedPlayerEffectRoot).localScale = Vector3.one;
	}

	public void SetContinueButtonAction(UnityAction action)
	{
		((UnityEventBase)((Button)continueButton).onClick).RemoveAllListeners();
		if (action != null)
		{
			((UnityEvent)((Button)continueButton).onClick).AddListener(action);
		}
		((Component)acceptButtonArea).gameObject.SetActive(action != null);
	}

	private void OnPlayerNavigationControllerPageChangeSubmitted(int newPage)
	{
		DisplayData displayData = this.displayData;
		displayData.playerIndex = newPage;
		SetDisplayData(displayData);
	}

	private void SetPlayerInfo([CanBeNull] RunReport.PlayerInfo playerInfo)
	{
		ulong num = 0uL;
		if (playerInfo != null && playerInfo.statSheet != null)
		{
			StatSheet statSheet = playerInfo.statSheet;
			AllocateStatStrips(statsToDisplay.Length);
			for (int i = 0; i < statsToDisplay.Length; i++)
			{
				string text = statsToDisplay[i];
				StatDef statDef = StatDef.Find(text);
				if (statDef == null)
				{
					Debug.LogWarningFormat("GameEndReportPanelController.SetStatSheet: Could not find stat def \"{0}\".", new object[1] { text });
				}
				else
				{
					AssignStatToStrip(statSheet, statDef, statStrips[i]);
					num += statSheet.GetStatPointValue(statDef);
				}
			}
			int unlockableCount = statSheet.GetUnlockableCount();
			int num2 = 0;
			for (int j = 0; j < unlockableCount; j++)
			{
				UnlockableDef unlockable = statSheet.GetUnlockable(j);
				if ((Object)(object)unlockable != (Object)null && !unlockable.hidden)
				{
					num2++;
				}
			}
			AllocateUnlockStrips(num2);
			int num3 = 0;
			for (int k = 0; k < unlockableCount; k++)
			{
				UnlockableDef unlockable2 = statSheet.GetUnlockable(k);
				if ((Object)(object)unlockable2 != (Object)null && !unlockable2.hidden)
				{
					AssignUnlockToStrip(unlockable2, unlockStrips[num3]);
					num3++;
				}
			}
			if (Object.op_Implicit((Object)(object)itemInventoryDisplay) && playerInfo.itemAcquisitionOrder != null)
			{
				itemInventoryDisplay.SetItems(playerInfo.itemAcquisitionOrder, playerInfo.itemAcquisitionOrder.Length, playerInfo.itemStacks);
				itemInventoryDisplay.UpdateDisplay();
			}
			string token = playerInfo.finalMessageToken + "_2P";
			if (Language.IsTokenInvalid(token))
			{
				token = playerInfo.finalMessageToken;
			}
			((TMP_Text)finalMessageLabel).SetText(Language.GetStringFormatted(token, playerInfo.name), true);
		}
		else
		{
			AllocateStatStrips(0);
			AllocateUnlockStrips(0);
			if (Object.op_Implicit((Object)(object)itemInventoryDisplay))
			{
				itemInventoryDisplay.ResetItems();
			}
			((TMP_Text)finalMessageLabel).SetText(string.Empty, true);
		}
		string @string = Language.GetString("STAT_POINTS_FORMAT");
		((TMP_Text)totalPointsLabel).text = string.Format(@string, TextSerialization.ToStringNumeric(num));
		GameObject val = null;
		if (playerInfo != null)
		{
			val = BodyCatalog.GetBodyPrefab(playerInfo.bodyIndex);
			((TMP_Text)playerUsernameLabel).text = playerInfo.name;
		}
		string arg = "";
		Texture texture = null;
		if (Object.op_Implicit((Object)(object)val))
		{
			texture = val.GetComponent<CharacterBody>().portraitIcon;
			arg = Language.GetString(val.GetComponent<CharacterBody>().baseNameToken);
		}
		string string2 = Language.GetString("STAT_CLASS_NAME_FORMAT");
		((TMP_Text)playerBodyLabel).text = string.Format(string2, arg);
		playerBodyPortraitImage.texture = texture;
		GameObject val2 = null;
		if (playerInfo != null)
		{
			val2 = BodyCatalog.GetBodyPrefab(playerInfo.killerBodyIndex);
			GameObject obj = killerPanelObject;
			if (obj != null)
			{
				obj.SetActive(playerInfo.isDead);
			}
		}
		string string3 = Language.GetString("UNIDENTIFIED_KILLER_NAME");
		Texture texture2 = LegacyResourcesAPI.Load<Texture>("Textures/BodyIcons/texUnidentifiedKillerIcon");
		if (Object.op_Implicit((Object)(object)val2))
		{
			Texture portraitIcon = val2.GetComponent<CharacterBody>().portraitIcon;
			string baseNameToken = val2.GetComponent<CharacterBody>().baseNameToken;
			if ((Object)(object)portraitIcon != (Object)null)
			{
				texture2 = portraitIcon;
			}
			if (!Language.IsTokenInvalid(baseNameToken))
			{
				string3 = Language.GetString(val2.GetComponent<CharacterBody>().baseNameToken);
			}
		}
		string string4 = Language.GetString("STAT_KILLER_NAME_FORMAT");
		((TMP_Text)killerBodyLabel).text = string.Format(string4, string3);
		killerBodyPortraitImage.texture = texture2;
	}

	private void AssignStatToStrip([CanBeNull] StatSheet srcStatSheet, [NotNull] StatDef statDef, GameObject destStatStrip)
	{
		string arg = "0";
		ulong value = 0uL;
		if (srcStatSheet != null)
		{
			arg = srcStatSheet.GetStatDisplayValue(statDef);
			value = srcStatSheet.GetStatPointValue(statDef);
		}
		string @string = Language.GetString(statDef.displayToken);
		string text = string.Format(Language.GetString("STAT_NAME_VALUE_FORMAT"), @string, arg);
		((TMP_Text)((Component)destStatStrip.transform.Find("StatNameLabel")).GetComponent<TextMeshProUGUI>()).text = text;
		string string2 = Language.GetString("STAT_POINTS_FORMAT");
		((TMP_Text)((Component)destStatStrip.transform.Find("PointValueLabel")).GetComponent<TextMeshProUGUI>()).text = string.Format(string2, TextSerialization.ToStringNumeric(value));
	}

	private void AssignUnlockToStrip(UnlockableDef unlockableDef, GameObject destUnlockableStrip)
	{
		AchievementDef achievementDefFromUnlockable = AchievementManager.GetAchievementDefFromUnlockable(unlockableDef.cachedName);
		Texture val = null;
		string @string = Language.GetString("TOOLTIP_UNLOCK_GENERIC_NAME");
		string string2 = Language.GetString("TOOLTIP_UNLOCK_GENERIC_DESCRIPTION");
		if (unlockableDef.cachedName.Contains("Items."))
		{
			@string = Language.GetString("TOOLTIP_UNLOCK_ITEM_NAME");
			string2 = Language.GetString("TOOLTIP_UNLOCK_ITEM_DESCRIPTION");
		}
		else if (unlockableDef.cachedName.Contains("Logs."))
		{
			@string = Language.GetString("TOOLTIP_UNLOCK_LOG_NAME");
			string2 = Language.GetString("TOOLTIP_UNLOCK_LOG_DESCRIPTION");
		}
		else if (unlockableDef.cachedName.Contains("Characters."))
		{
			@string = Language.GetString("TOOLTIP_UNLOCK_SURVIVOR_NAME");
			string2 = Language.GetString("TOOLTIP_UNLOCK_SURVIVOR_DESCRIPTION");
		}
		else if (unlockableDef.cachedName.Contains("Artifacts."))
		{
			@string = Language.GetString("TOOLTIP_UNLOCK_ARTIFACT_NAME");
			string2 = Language.GetString("TOOLTIP_UNLOCK_ARTIFACT_DESCRIPTION");
		}
		string string3;
		if (achievementDefFromUnlockable != null)
		{
			val = (Texture)(object)achievementDefFromUnlockable.GetAchievedIcon().texture;
			string3 = Language.GetString(achievementDefFromUnlockable.nameToken);
		}
		else
		{
			string3 = Language.GetString(unlockableDef.nameToken);
		}
		if ((Object)(object)val != (Object)null)
		{
			((Component)destUnlockableStrip.transform.Find("IconImage")).GetComponent<RawImage>().texture = val;
		}
		((TMP_Text)((Component)destUnlockableStrip.transform.Find("NameLabel")).GetComponent<TextMeshProUGUI>()).text = string3;
		destUnlockableStrip.GetComponent<TooltipProvider>().overrideTitleText = @string;
		destUnlockableStrip.GetComponent<TooltipProvider>().overrideBodyText = string2;
	}
}
