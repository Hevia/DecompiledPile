using System;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI.LogBook;

public class CategoryDef
{
	[NotNull]
	public delegate Entry[] BuildEntriesDelegate([CanBeNull] UserProfile viewerProfile);

	[NotNull]
	public string nameToken = string.Empty;

	private GameObject _iconPrefab;

	public Vector2 iconSize = Vector2.one;

	public bool fullWidth;

	public Action<GameObject, Entry, EntryStatus, UserProfile> initializeElementGraphics = InitializeDefault;

	[CanBeNull]
	public ViewablesCatalog.Node viewableNode;

	public GameObject iconPrefab
	{
		get
		{
			return _iconPrefab;
		}
		[NotNull]
		set
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			_iconPrefab = value;
			iconSize = ((RectTransform)_iconPrefab.transform).sizeDelta;
		}
	}

	[NotNull]
	public BuildEntriesDelegate buildEntries { private get; set; }

	public Entry[] BuildEntries([CanBeNull] UserProfile viewerProfile)
	{
		Entry[] array = buildEntries(viewerProfile);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].category = this;
		}
		return array;
	}

	public static void InitializeDefault(GameObject gameObject, Entry entry, EntryStatus status, UserProfile userProfile)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		Texture val = null;
		Texture val2 = null;
		Color color = Color.white;
		switch (status)
		{
		case EntryStatus.Unimplemented:
			val = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texWIPIcon");
			break;
		case EntryStatus.Locked:
			val = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texUnlockIcon");
			color = Color.gray;
			break;
		case EntryStatus.Unencountered:
			val = entry.iconTexture;
			color = Color.black;
			break;
		case EntryStatus.Available:
			val = entry.iconTexture;
			val2 = entry.bgTexture;
			color = Color.white;
			break;
		case EntryStatus.New:
			val = entry.iconTexture;
			val2 = entry.bgTexture;
			((Color)(ref color))._002Ector(1f, 0.8f, 0.5f, 1f);
			break;
		default:
			throw new ArgumentOutOfRangeException("status", status, null);
		}
		RawImage val3 = null;
		RawImage val4 = null;
		ChildLocator component = gameObject.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			val3 = ((Component)component.FindChild("Icon")).GetComponent<RawImage>();
			val4 = ((Component)component.FindChild("BG")).GetComponent<RawImage>();
		}
		else
		{
			val3 = gameObject.GetComponentInChildren<RawImage>();
		}
		val3.texture = val;
		((Graphic)val3).color = color;
		if (Object.op_Implicit((Object)(object)val4))
		{
			if ((Object)(object)val2 != (Object)null)
			{
				val4.texture = val2;
			}
			else
			{
				((Behaviour)val4).enabled = false;
			}
		}
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			if (status >= EntryStatus.Available)
			{
				((TMP_Text)componentInChildren).text = entry.GetDisplayName(userProfile);
			}
			else
			{
				((TMP_Text)componentInChildren).text = Language.GetString("UNIDENTIFIED");
			}
		}
	}

	public static void InitializeChallenge(GameObject gameObject, Entry entry, EntryStatus status, UserProfile userProfile)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI val = null;
		TextMeshProUGUI val2 = null;
		RawImage val3 = null;
		AchievementDef achievementDef = (AchievementDef)entry.extraData;
		float achievementProgress = AchievementManager.GetUserAchievementManager(LocalUserManager.readOnlyLocalUsersList.FirstOrDefault((LocalUser v) => v.userProfile == userProfile)).GetAchievementProgress(achievementDef);
		HGButton component = gameObject.GetComponent<HGButton>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.disablePointerClick = true;
			component.disableGamepadClick = true;
		}
		ChildLocator component2 = gameObject.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			val = ((Component)component2.FindChild("DescriptionLabel")).GetComponent<TextMeshProUGUI>();
			val2 = ((Component)component2.FindChild("NameLabel")).GetComponent<TextMeshProUGUI>();
			val3 = ((Component)component2.FindChild("RewardImage")).GetComponent<RawImage>();
			((TMP_Text)val2).text = Language.GetString(achievementDef.nameToken);
			((TMP_Text)val).text = Language.GetString(achievementDef.descriptionToken);
		}
		Texture val4 = null;
		Color color = Color.white;
		switch (status)
		{
		case EntryStatus.Unimplemented:
			val4 = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texWIPIcon");
			break;
		case EntryStatus.Locked:
			val4 = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texUnlockIcon");
			color = Color.black;
			((TMP_Text)val2).text = Language.GetString("UNIDENTIFIED");
			((TMP_Text)val).text = Language.GetString("UNIDENTIFIED_DESCRIPTION");
			((Component)component2.FindChild("CantBeAchieved")).gameObject.SetActive(true);
			break;
		case EntryStatus.Unencountered:
			val4 = (Texture)(object)LegacyResourcesAPI.Load<Texture2D>("Textures/MiscIcons/texUnlockIcon");
			color = Color.gray;
			((Component)component2.FindChild("ProgressTowardsUnlocking")).GetComponent<Image>().fillAmount = achievementProgress;
			break;
		case EntryStatus.Available:
			val4 = entry.iconTexture;
			color = Color.white;
			((Component)component2.FindChild("HasBeenUnlocked")).gameObject.SetActive(true);
			break;
		case EntryStatus.New:
			val4 = entry.iconTexture;
			((Color)(ref color))._002Ector(1f, 0.8f, 0.5f, 1f);
			((Component)component2.FindChild("HasBeenUnlocked")).gameObject.SetActive(true);
			break;
		case EntryStatus.None:
			((Component)component2.FindChild("RewardImageContainer")).gameObject.SetActive(true);
			((TMP_Text)val2).text = "";
			((TMP_Text)val).text = "";
			break;
		default:
			throw new ArgumentOutOfRangeException("status", status, null);
		}
		if ((Object)(object)val4 != (Object)null)
		{
			val3.texture = val4;
			((Graphic)val3).color = color;
		}
		else
		{
			((Behaviour)val3).enabled = false;
		}
	}

	public static void InitializeMorgue(GameObject gameObject, Entry entry, EntryStatus status, UserProfile userProfile)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		RunReport runReport = entry.extraData as RunReport;
		GameEndingDef gameEnding = runReport.gameEnding;
		ChildLocator component = gameObject.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			TextMeshProUGUI component2 = ((Component)component.FindChild("DescriptionLabel")).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI component3 = ((Component)component.FindChild("NameLabel")).GetComponent<TextMeshProUGUI>();
			RawImage component4 = ((Component)component.FindChild("IconImage")).GetComponent<RawImage>();
			Image component5 = ((Component)component.FindChild("BackgroundImage")).GetComponent<Image>();
			Texture iconTexture = entry.iconTexture;
			((TMP_Text)component3).text = entry.GetDisplayName(userProfile);
			((TMP_Text)component2).text = Language.GetString(runReport.gameEnding?.endingTextToken ?? string.Empty);
			((Graphic)component2).color = gameEnding?.foregroundColor ?? Color.white;
			((Graphic)component5).color = gameEnding?.backgroundColor ?? Color.black;
			if ((Object)(object)iconTexture != (Object)null)
			{
				component4.texture = iconTexture;
			}
			else
			{
				((Behaviour)component4).enabled = false;
			}
		}
	}

	public static void InitializeStats(GameObject gameObject, Entry entry, EntryStatus status, UserProfile userProfile)
	{
		UserProfile userProfile2 = entry.extraData as UserProfile;
		ChildLocator component = gameObject.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			TextMeshProUGUI component2 = ((Component)component.FindChild("NameLabel")).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI component3 = ((Component)component.FindChild("TimeLabel")).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI component4 = ((Component)component.FindChild("CompletionLabel")).GetComponent<TextMeshProUGUI>();
			RawImage component5 = ((Component)component.FindChild("IconImage")).GetComponent<RawImage>();
			((Component)component.FindChild("BackgroundImage")).GetComponent<Image>();
			Texture iconTexture = entry.iconTexture;
			IntFraction value = new GameCompletionStatsHelper().GetTotalCompletion(userProfile2);
			float num = (float)value;
			string text = $"{num:0%}";
			TimeSpan timeSpan = TimeSpan.FromSeconds(userProfile2.totalLoginSeconds);
			string text2 = $"{(uint)timeSpan.TotalHours}:{(uint)timeSpan.Minutes:D2}";
			((TMP_Text)component2).text = entry.GetDisplayName(userProfile2);
			((TMP_Text)component3).text = text2;
			((TMP_Text)component4).text = text;
			if ((Object)(object)iconTexture != (Object)null)
			{
				component5.texture = iconTexture;
			}
			else
			{
				((Behaviour)component5).enabled = false;
			}
		}
	}
}
