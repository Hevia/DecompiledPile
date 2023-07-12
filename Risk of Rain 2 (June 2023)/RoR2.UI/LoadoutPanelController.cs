using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
[RequireComponent(typeof(RectTransform))]
public class LoadoutPanelController : MonoBehaviour
{
	public struct DisplayData : IEquatable<DisplayData>
	{
		public UserProfile userProfile;

		public BodyIndex bodyIndex;

		public bool Equals(DisplayData other)
		{
			if (userProfile == other.userProfile)
			{
				return bodyIndex == other.bodyIndex;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DisplayData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((userProfile != null) ? userProfile.GetHashCode() : 0) * 397) ^ (int)bodyIndex;
		}
	}

	private class Row : IDisposable
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static UnityAction _003C_003E9__12_0;

			internal void _003CFinishSetup_003Eb__12_0()
			{
			}
		}

		private List<MPButton> buttons = new List<MPButton>();

		private LoadoutPanelController owner;

		private UserProfile userProfile;

		private RectTransform rowPanelTransform;

		private RectTransform buttonContainerTransform;

		private RectTransform choiceHighlightRect;

		private Color primaryColor;

		private Color complementaryColor;

		private Func<Loadout, int> findCurrentChoice;

		private Row(LoadoutPanelController owner, BodyIndex bodyIndex, string titleToken)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			this.owner = owner;
			userProfile = owner.currentDisplayData.userProfile;
			rowPanelTransform = (RectTransform)Object.Instantiate<GameObject>(rowPrefab, (Transform)(object)owner.rowContainer).transform;
			buttonContainerTransform = (RectTransform)((Transform)rowPanelTransform).Find("ButtonContainer");
			choiceHighlightRect = (RectTransform)((Transform)rowPanelTransform).Find("ButtonSelectionHighlight, Checkbox");
			UserProfile.onLoadoutChangedGlobal += OnLoadoutChangedGlobal;
			SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.GetBodyPrefab(bodyIndex));
			CharacterBody bodyPrefabBodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
			if ((Object)(object)bodyPrefabBodyComponent != (Object)null)
			{
				primaryColor = bodyPrefabBodyComponent.bodyColor;
			}
			float num2 = default(float);
			float num3 = default(float);
			float num = default(float);
			Color.RGBToHSV(primaryColor, ref num, ref num2, ref num3);
			num += 0.5f;
			if (num > 1f)
			{
				num -= 1f;
			}
			complementaryColor = Color.HSVToRGB(num, num2, num3);
			RectTransform val = (RectTransform)((Transform)rowPanelTransform).Find("SlotLabel");
			((Component)val).GetComponent<LanguageTextMeshController>().token = titleToken;
			((Graphic)((Component)val).GetComponent<HGTextMeshProUGUI>()).color = primaryColor;
		}

		private void OnLoadoutChangedGlobal(UserProfile userProfile)
		{
			if (userProfile == this.userProfile)
			{
				UpdateHighlightedChoice();
			}
		}

		public static Row FromSkillSlot(LoadoutPanelController owner, BodyIndex bodyIndex, int skillSlotIndex, GenericSkill skillSlot)
		{
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Expected O, but got Unknown
			SkillFamily skillFamily = skillSlot.skillFamily;
			SkillLocator component = ((Component)BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex)).GetComponent<SkillLocator>();
			bool addWIPIcons = false;
			string titleToken;
			switch (component.FindSkillSlot(skillSlot))
			{
			case SkillSlot.None:
				titleToken = "LOADOUT_SKILL_MISC";
				addWIPIcons = false;
				break;
			case SkillSlot.Primary:
				titleToken = "LOADOUT_SKILL_PRIMARY";
				addWIPIcons = false;
				break;
			case SkillSlot.Secondary:
				titleToken = "LOADOUT_SKILL_SECONDARY";
				break;
			case SkillSlot.Utility:
				titleToken = "LOADOUT_SKILL_UTILITY";
				break;
			case SkillSlot.Special:
				titleToken = "LOADOUT_SKILL_SPECIAL";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Row row = new Row(owner, bodyIndex, titleToken);
			for (int i = 0; i < skillFamily.variants.Length; i++)
			{
				ref SkillFamily.Variant reference = ref skillFamily.variants[i];
				uint skillVariantIndexToAssign = (uint)i;
				row.AddButton(owner, reference.skillDef.icon, reference.skillDef.skillNameToken, reference.skillDef.skillDescriptionToken, row.primaryColor, (UnityAction)delegate
				{
					Loadout loadout2 = new Loadout();
					row.userProfile.CopyLoadout(loadout2);
					loadout2.bodyLoadoutManager.SetSkillVariant(bodyIndex, skillSlotIndex, skillVariantIndexToAssign);
					row.userProfile.SetLoadout(loadout2);
				}, reference.unlockableDef?.cachedName ?? "", reference.viewableNode);
			}
			row.findCurrentChoice = (Loadout loadout) => (int)loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, skillSlotIndex);
			row.FinishSetup(addWIPIcons);
			return row;
		}

		public static Row FromSkin(LoadoutPanelController owner, BodyIndex bodyIndex)
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			Row row = new Row(owner, bodyIndex, "LOADOUT_SKIN");
			SkinDef[] bodySkins = BodyCatalog.GetBodySkins(bodyIndex);
			for (int i = 0; i < bodySkins.Length; i++)
			{
				SkinDef skinDef = bodySkins[i];
				uint skinToAssign = (uint)i;
				ViewablesCatalog.Node viewableNode = ViewablesCatalog.FindNode($"/Loadout/Bodies/{BodyCatalog.GetBodyName(bodyIndex)}/Skins/{((Object)skinDef).name}");
				row.AddButton(owner, skinDef.icon, skinDef.nameToken, string.Empty, row.primaryColor, (UnityAction)delegate
				{
					Loadout loadout2 = new Loadout();
					row.userProfile.CopyLoadout(loadout2);
					loadout2.bodyLoadoutManager.SetSkinIndex(bodyIndex, skinToAssign);
					row.userProfile.SetLoadout(loadout2);
				}, skinDef.unlockableDef?.cachedName ?? "", viewableNode);
			}
			row.findCurrentChoice = (Loadout loadout) => (int)loadout.bodyLoadoutManager.GetSkinIndex(bodyIndex);
			row.FinishSetup();
			return row;
		}

		private void FinishSetup(bool addWIPIcons = false)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			if (addWIPIcons)
			{
				Sprite icon = LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texWIPIcon");
				for (int i = buttons.Count; i < minimumEntriesPerRow; i++)
				{
					LoadoutPanelController loadoutPanelController = owner;
					Color tooltipColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.WIP));
					object obj = _003C_003Ec._003C_003E9__12_0;
					if (obj == null)
					{
						UnityAction val = delegate
						{
						};
						obj = (object)val;
						_003C_003Ec._003C_003E9__12_0 = val;
					}
					AddButton(loadoutPanelController, icon, "TOOLTIP_WIP_CONTENT_NAME", "TOOLTIP_WIP_CONTENT_DESCRIPTION", tooltipColor, (UnityAction)obj, "", null, isWIP: true);
				}
			}
			RectTransform val2 = (RectTransform)((Transform)rowPanelTransform).Find("ButtonContainer/Spacer");
			if (Object.op_Implicit((Object)(object)val2))
			{
				((Transform)val2).SetAsLastSibling();
			}
			UpdateHighlightedChoice();
		}

		private void SetButtonColorMultiplier(int i, float f)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			MPButton mPButton = buttons[i];
			ColorBlock colors = ((Selectable)mPButton).colors;
			((ColorBlock)(ref colors)).colorMultiplier = f;
			((Selectable)mPButton).colors = colors;
		}

		private void UpdateHighlightedChoice()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			foreach (MPButton button in buttons)
			{
				ColorBlock colors = ((Selectable)button).colors;
				((ColorBlock)(ref colors)).colorMultiplier = 0.5f;
				((Selectable)button).colors = colors;
			}
			for (int i = 0; i < buttons.Count; i++)
			{
				SetButtonColorMultiplier(i, 0.5f);
			}
			Loadout loadout = new Loadout();
			userProfile?.CopyLoadout(loadout);
			int num = findCurrentChoice(loadout);
			if (buttons.Count > num)
			{
				((Transform)choiceHighlightRect).SetParent((Transform)(RectTransform)((Component)buttons[num]).transform, false);
				SetButtonColorMultiplier(num, 1f);
			}
		}

		private void AddButton(LoadoutPanelController owner, Sprite icon, string titleToken, string bodyToken, Color tooltipColor, UnityAction callback, string unlockableName, ViewablesCatalog.Node viewableNode, bool isWIP = false)
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			HGButton component = Object.Instantiate<GameObject>(loadoutButtonPrefab, (Transform)(object)buttonContainerTransform).GetComponent<HGButton>();
			component.updateTextOnHover = true;
			component.hoverLanguageTextMeshController = owner.hoverTextDescription;
			component.requiredTopLayer = owner.requiredUILayerKey;
			string text = "";
			TooltipProvider component2 = ((Component)component).GetComponent<TooltipProvider>();
			UserProfile obj = userProfile;
			string @string;
			string text2;
			Color val;
			if (obj != null && obj.HasUnlockable(unlockableName))
			{
				((UnityEvent)((Button)component).onClick).AddListener(callback);
				((Selectable)component).interactable = true;
				if (viewableNode != null)
				{
					ViewableTag component3 = ((Component)component).GetComponent<ViewableTag>();
					component3.viewableName = viewableNode.fullName;
					component3.Refresh();
				}
				@string = Language.GetString(titleToken);
				text2 = Language.GetString(bodyToken);
				val = tooltipColor;
			}
			else
			{
				UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(unlockableName);
				icon = lockedIcon;
				((Selectable)component).interactable = true;
				component.disableGamepadClick = true;
				component.disablePointerClick = true;
				@string = Language.GetString("UNIDENTIFIED");
				text2 = unlockableDef.getHowToUnlockString();
				val = Color.gray;
			}
			if (isWIP)
			{
				((Selectable)component).interactable = false;
			}
			component2.titleColor = val;
			component2.overrideTitleText = @string;
			component2.overrideBodyText = text2;
			val.a = 0.2f;
			text = Language.GetStringFormatted("LOGBOOK_HOVER_DESCRIPTION_FORMAT", @string, text2, ColorUtility.ToHtmlStringRGBA(val));
			component.hoverToken = text;
			((Image)((Selectable)component).targetGraphic).sprite = icon;
			buttons.Add(component);
		}

		public void Dispose()
		{
			UserProfile.onLoadoutChangedGlobal -= OnLoadoutChangedGlobal;
			for (int num = buttons.Count - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)buttons[num]).gameObject);
			}
			Object.Destroy((Object)(object)((Component)rowPanelTransform).gameObject);
		}
	}

	public UILayerKey requiredUILayerKey;

	public LanguageTextMeshController hoverTextDescription;

	private DisplayData currentDisplayData = new DisplayData
	{
		userProfile = null,
		bodyIndex = BodyIndex.None
	};

	private UIElementAllocator<RectTransform> buttonAllocator;

	private readonly List<Row> rows = new List<Row>();

	public static int minimumEntriesPerRow = 2;

	private MPEventSystemLocator eventSystemLocator;

	private UIElementAllocator<RectTransform> rowAllocator;

	private static GameObject loadoutButtonPrefab;

	private static GameObject rowPrefab;

	private static Sprite lockedIcon;

	private RectTransform rowContainer => (RectTransform)((Component)this).transform;

	public void SetDisplayData(DisplayData displayData)
	{
		if (!displayData.Equals(currentDisplayData))
		{
			currentDisplayData = displayData;
			Rebuild();
		}
	}

	private void OnEnable()
	{
		UpdateDisplayData();
	}

	private void Update()
	{
		UpdateDisplayData();
	}

	private void UpdateDisplayData()
	{
		UserProfile userProfile = eventSystemLocator.eventSystem?.localUser?.userProfile;
		NetworkUser networkUser = eventSystemLocator.eventSystem?.localUser?.currentNetworkUser;
		BodyIndex bodyIndex = (Object.op_Implicit((Object)(object)networkUser) ? networkUser.bodyIndexPreference : BodyIndex.None);
		SetDisplayData(new DisplayData
		{
			userProfile = userProfile,
			bodyIndex = bodyIndex
		});
	}

	private void DestroyRows()
	{
		for (int num = rows.Count - 1; num >= 0; num--)
		{
			rows[num].Dispose();
		}
		rows.Clear();
	}

	private void Rebuild()
	{
		DestroyRows();
		CharacterBody bodyPrefabBodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(currentDisplayData.bodyIndex);
		if (Object.op_Implicit((Object)(object)bodyPrefabBodyComponent))
		{
			List<GenericSkill> gameObjectComponents = GetComponentsCache<GenericSkill>.GetGameObjectComponents(((Component)bodyPrefabBodyComponent).gameObject);
			int i = 0;
			for (int count = gameObjectComponents.Count; i < count; i++)
			{
				GenericSkill skillSlot = gameObjectComponents[i];
				rows.Add(Row.FromSkillSlot(this, currentDisplayData.bodyIndex, i, skillSlot));
			}
			_ = BodyCatalog.GetBodySkins(currentDisplayData.bodyIndex).Length;
			if (true)
			{
				rows.Add(Row.FromSkin(this, currentDisplayData.bodyIndex));
			}
		}
	}

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		loadoutButtonPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Loadout/LoadoutButton");
		rowPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Loadout/Row");
		lockedIcon = LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texUnlockIcon");
	}

	private void OnDestroy()
	{
		DestroyRows();
	}
}
