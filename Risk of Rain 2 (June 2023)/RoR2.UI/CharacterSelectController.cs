using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HG;
using Rewired;
using RoR2.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class CharacterSelectController : MonoBehaviour
{
	[Serializable]
	public struct SkillStrip
	{
		public GameObject stripRoot;

		public Image skillIcon;

		public TextMeshProUGUI skillName;

		public TextMeshProUGUI skillDescription;
	}

	private struct BodyInfo
	{
		public readonly BodyIndex bodyIndex;

		public readonly GameObject bodyPrefab;

		public readonly CharacterBody bodyPrefabBodyComponent;

		public readonly Color bodyColor;

		public readonly SkillLocator skillLocator;

		public readonly GenericSkill[] skillSlots;

		public BodyInfo(BodyIndex bodyIndex)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			this.bodyIndex = bodyIndex;
			bodyPrefab = BodyCatalog.GetBodyPrefab(bodyIndex);
			bodyPrefabBodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
			bodyColor = (Object.op_Implicit((Object)(object)bodyPrefabBodyComponent) ? bodyPrefabBodyComponent.bodyColor : Color.gray);
			skillLocator = (Object.op_Implicit((Object)(object)bodyPrefab) ? bodyPrefab.GetComponent<SkillLocator>() : null);
			skillSlots = BodyCatalog.GetBodyPrefabSkillSlots(bodyIndex);
		}
	}

	private struct StripDisplayData
	{
		public bool enabled;

		public Color primaryColor;

		public Sprite icon;

		public string titleString;

		public string descriptionString;

		public string keywordString;

		public string actionName;
	}

	[Header("Survivor Panel")]
	public TextMeshProUGUI survivorName;

	public Image[] primaryColorImages;

	public TextMeshProUGUI[] primaryColorTexts;

	public GameObject activeSurvivorInfoPanel;

	public GameObject inactiveSurvivorInfoPanel;

	[Header("Overview Panel")]
	public TextMeshProUGUI survivorDescription;

	[Header("Skill Panel")]
	public GameObject skillStripPrefab;

	public GameObject skillStripFillerPrefab;

	public RectTransform skillStripContainer;

	private UIElementAllocator<RectTransform> skillStripAllocator;

	private UIElementAllocator<RectTransform> skillStripFillerAllocator;

	[Tooltip("The header button for the loadout tab. Will be disabled if the user has no unlocked loadout options.")]
	[Header("Loadout Panel")]
	public GameObject loadoutHeaderButton;

	public ViewableTag loadoutViewableTag;

	[Header("Ready and Misc")]
	public MPButton readyButton;

	public MPButton unreadyButton;

	public GameObject[] multiplayerOnlyObjects;

	private MPEventSystemLocator eventSystemLocator;

	private MPEventSystem eventSystem;

	private LocalUser localUser;

	private SurvivorDef currentSurvivorDef;

	private static UnlockableDef[] loadoutAssociatedUnlockableDefs;

	private bool shouldRebuild = true;

	private bool shouldShowSurvivorInfoPanel;

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		skillStripAllocator = new UIElementAllocator<RectTransform>(skillStripContainer, skillStripPrefab);
		skillStripFillerAllocator = new UIElementAllocator<RectTransform>(skillStripContainer, skillStripFillerPrefab);
		bool active = true;
		loadoutHeaderButton.SetActive(active);
	}

	private void OnEnable()
	{
		UserProfile.onLoadoutChangedGlobal += OnLoadoutChangedGlobal;
		eventSystemLocator.onEventSystemDiscovered += OnEventSystemDiscovered;
		eventSystemLocator.onEventSystemLost += OnEventSystemLost;
		if (Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem))
		{
			OnEventSystemDiscovered(eventSystemLocator.eventSystem);
		}
	}

	private void OnDisable()
	{
		eventSystemLocator.onEventSystemLost -= OnEventSystemLost;
		eventSystemLocator.onEventSystemDiscovered -= OnEventSystemDiscovered;
		if (Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem))
		{
			OnEventSystemLost(eventSystemLocator.eventSystem);
		}
		UserProfile.onLoadoutChangedGlobal -= OnLoadoutChangedGlobal;
	}

	private void Update()
	{
		SurvivorDef survivorDef = null;
		NetworkUser networkUser = localUser?.currentNetworkUser;
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			survivorDef = networkUser.GetSurvivorPreference();
		}
		if (currentSurvivorDef != survivorDef)
		{
			currentSurvivorDef = survivorDef;
			shouldRebuild = true;
		}
		if (shouldRebuild)
		{
			shouldRebuild = false;
			RebuildLocal();
		}
		UpdateSurvivorInfoPanel();
		if (!RoR2Application.isInSinglePlayer)
		{
			bool flag = IsClientReady();
			((Component)readyButton).gameObject.SetActive(!flag);
			((Component)unreadyButton).gameObject.SetActive(flag);
		}
	}

	private void OnEventSystemDiscovered(MPEventSystem discoveredEventSystem)
	{
		eventSystem = discoveredEventSystem;
		localUser = (Object.op_Implicit((Object)(object)eventSystem) ? eventSystem.localUser : null);
		shouldRebuild = true;
	}

	private void OnEventSystemLost(MPEventSystem lostEventSystem)
	{
		eventSystem = null;
		localUser = null;
		shouldRebuild = true;
	}

	private static UnlockableDef[] GenerateLoadoutAssociatedUnlockableDefs()
	{
		HashSet<UnlockableDef> encounteredUnlockables = new HashSet<UnlockableDef>();
		foreach (SkillFamily allSkillFamily in SkillCatalog.allSkillFamilies)
		{
			for (int i = 0; i < allSkillFamily.variants.Length; i++)
			{
				TryAddUnlockableByDef(allSkillFamily.variants[i].unlockableDef);
			}
		}
		foreach (CharacterBody allBodyPrefabBodyBodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
		{
			SkinDef[] bodySkins = BodyCatalog.GetBodySkins(allBodyPrefabBodyBodyComponent.bodyIndex);
			for (int j = 0; j < bodySkins.Length; j++)
			{
				TryAddUnlockableByDef(bodySkins[j].unlockableDef);
			}
		}
		return encounteredUnlockables.ToArray();
		void TryAddUnlockableByDef(UnlockableDef unlockableDef)
		{
			if ((Object)(object)unlockableDef != (Object)null)
			{
				encounteredUnlockables.Add(unlockableDef);
			}
		}
	}

	private static bool UserHasAnyLoadoutUnlockables(LocalUser localUser)
	{
		if (loadoutAssociatedUnlockableDefs == null)
		{
			loadoutAssociatedUnlockableDefs = GenerateLoadoutAssociatedUnlockableDefs();
		}
		UserProfile userProfile = localUser.userProfile;
		UnlockableDef[] array = loadoutAssociatedUnlockableDefs;
		foreach (UnlockableDef unlockableDef in array)
		{
			if (userProfile.HasUnlockable(unlockableDef))
			{
				return true;
			}
		}
		return false;
	}

	private void RebuildLocal()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Loadout loadout = Loadout.RequestInstance();
		try
		{
			NetworkUser networkUser = localUser?.currentNetworkUser;
			SurvivorDef survivorDef = (Object.op_Implicit((Object)(object)networkUser) ? networkUser.GetSurvivorPreference() : null);
			localUser?.userProfile.CopyLoadout(loadout);
			BodyInfo bodyInfo = new BodyInfo(SurvivorCatalog.GetBodyIndexFromSurvivorIndex(Object.op_Implicit((Object)(object)survivorDef) ? survivorDef.survivorIndex : SurvivorIndex.None));
			string text = string.Empty;
			string text2 = string.Empty;
			if (Object.op_Implicit((Object)(object)survivorDef))
			{
				text = Language.GetString(survivorDef.displayNameToken);
				text2 = Language.GetString(survivorDef.descriptionToken);
			}
			((TMP_Text)survivorName).SetText(text, true);
			((TMP_Text)survivorDescription).SetText(text2, true);
			Image[] array = primaryColorImages;
			for (int i = 0; i < array.Length; i++)
			{
				((Graphic)array[i]).color = bodyInfo.bodyColor;
			}
			TextMeshProUGUI[] array2 = primaryColorTexts;
			for (int i = 0; i < array2.Length; i++)
			{
				((Graphic)array2[i]).color = bodyInfo.bodyColor;
			}
			List<StripDisplayData> list = new List<StripDisplayData>();
			BuildSkillStripDisplayData(loadout, in bodyInfo, list);
			skillStripFillerAllocator.AllocateElements(0);
			skillStripAllocator.AllocateElements(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				RebuildStrip(skillStripAllocator.elements[j], list[j]);
			}
			skillStripFillerAllocator.AllocateElements(Mathf.Max(0, 5 - list.Count));
			string empty = string.Empty;
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			stringBuilder.Append("/Loadout/Bodies/").Append(BodyCatalog.GetBodyName(bodyInfo.bodyIndex)).Append("/");
			empty = stringBuilder.ToString();
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
			loadoutViewableTag.viewableName = empty;
			bool active = !RoR2Application.isInSinglePlayer;
			GameObject[] array3 = multiplayerOnlyObjects;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].SetActive(active);
			}
		}
		finally
		{
			Loadout.ReturnInstance(loadout);
		}
	}

	private void RebuildStrip(RectTransform skillStrip, StripDisplayData stripDisplayData)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)skillStrip).gameObject;
		Image component = ((Component)((Transform)skillStrip).Find("Inner/Icon")).GetComponent<Image>();
		HGTextMeshProUGUI component2 = ((Component)((Transform)skillStrip).Find("Inner/SkillDescriptionPanel/SkillName")).GetComponent<HGTextMeshProUGUI>();
		HGTextMeshProUGUI component3 = ((Component)((Transform)skillStrip).Find("Inner/SkillDescriptionPanel/SkillDescription")).GetComponent<HGTextMeshProUGUI>();
		HGButton component4 = ((Component)skillStrip).gameObject.GetComponent<HGButton>();
		if (stripDisplayData.enabled)
		{
			gameObject.SetActive(true);
			component.sprite = stripDisplayData.icon;
			((TMP_Text)component2).SetText(stripDisplayData.titleString, true);
			((Graphic)component2).color = stripDisplayData.primaryColor;
			((TMP_Text)component3).SetText(stripDisplayData.descriptionString, true);
			component4.hoverToken = stripDisplayData.keywordString;
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void BuildSkillStripDisplayData(Loadout loadout, in BodyInfo bodyInfo, List<StripDisplayData> dest)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)bodyInfo.bodyPrefab) || !Object.op_Implicit((Object)(object)bodyInfo.bodyPrefabBodyComponent) || !Object.op_Implicit((Object)(object)bodyInfo.skillLocator))
		{
			return;
		}
		BodyIndex bodyIndex = bodyInfo.bodyIndex;
		SkillLocator skillLocator = bodyInfo.skillLocator;
		GenericSkill[] skillSlots = bodyInfo.skillSlots;
		Color bodyColor = bodyInfo.bodyColor;
		if (skillLocator.passiveSkill.enabled)
		{
			dest.Add(new StripDisplayData
			{
				enabled = true,
				primaryColor = bodyColor,
				icon = skillLocator.passiveSkill.icon,
				titleString = Language.GetString(skillLocator.passiveSkill.skillNameToken),
				descriptionString = Language.GetString(skillLocator.passiveSkill.skillDescriptionToken),
				keywordString = (string.IsNullOrEmpty(skillLocator.passiveSkill.keywordToken) ? "" : Language.GetString(skillLocator.passiveSkill.keywordToken)),
				actionName = ""
			});
		}
		for (int i = 0; i < skillSlots.Length; i++)
		{
			GenericSkill genericSkill = skillSlots[i];
			if (genericSkill.hideInCharacterSelect)
			{
				continue;
			}
			uint skillVariant = loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, i);
			SkillDef skillDef = genericSkill.skillFamily.variants[skillVariant].skillDef;
			string actionName = "";
			switch (skillLocator.FindSkillSlot(genericSkill))
			{
			case SkillSlot.Primary:
				actionName = "PrimarySkill";
				break;
			case SkillSlot.Secondary:
				actionName = "SecondarySkill";
				break;
			case SkillSlot.Utility:
				actionName = "UtilitySkill";
				break;
			case SkillSlot.Special:
				actionName = "SpecialSkill";
				break;
			}
			string keywordString = string.Empty;
			if (skillDef.keywordTokens != null)
			{
				StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
				for (int j = 0; j < skillDef.keywordTokens.Length; j++)
				{
					string @string = Language.GetString(skillDef.keywordTokens[j]);
					stringBuilder.Append(@string).Append("\n\n");
				}
				keywordString = stringBuilder.ToString();
				stringBuilder = StringBuilderPool.ReturnStringBuilder(stringBuilder);
			}
			dest.Add(new StripDisplayData
			{
				enabled = true,
				primaryColor = bodyColor,
				icon = skillDef.icon,
				titleString = Language.GetString(skillDef.skillNameToken),
				descriptionString = Language.GetString(skillDef.skillDescriptionToken),
				keywordString = keywordString,
				actionName = actionName
			});
		}
	}

	private void OnLoadoutChangedGlobal(UserProfile userProfile)
	{
		if (userProfile == localUser?.userProfile)
		{
			shouldRebuild = true;
		}
	}

	private void UpdateSurvivorInfoPanel()
	{
		if (Object.op_Implicit((Object)(object)eventSystem) && eventSystem.currentInputSource == MPEventSystem.InputSource.MouseAndKeyboard)
		{
			shouldShowSurvivorInfoPanel = true;
		}
		activeSurvivorInfoPanel.SetActive(shouldShowSurvivorInfoPanel);
		inactiveSurvivorInfoPanel.SetActive(!shouldShowSurvivorInfoPanel);
	}

	public void SetSurvivorInfoPanelActive(bool active)
	{
		shouldShowSurvivorInfoPanel = active;
		UpdateSurvivorInfoPanel();
	}

	private static bool InputPlayerIsAssigned(Player inputPlayer)
	{
		ReadOnlyCollection<NetworkUser> readOnlyInstancesList = NetworkUser.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			if (readOnlyInstancesList[i].inputPlayer == inputPlayer)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsClientReady()
	{
		int num = 0;
		if (!Object.op_Implicit((Object)(object)PreGameController.instance))
		{
			return false;
		}
		VoteController component = ((Component)PreGameController.instance).GetComponent<VoteController>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return false;
		}
		int i = 0;
		for (int voteCount = component.GetVoteCount(); i < voteCount; i++)
		{
			UserVote vote = component.GetVote(i);
			if (Object.op_Implicit((Object)(object)vote.networkUserObject) && vote.receivedVote)
			{
				NetworkUser component2 = vote.networkUserObject.GetComponent<NetworkUser>();
				if (Object.op_Implicit((Object)(object)component2) && ((NetworkBehaviour)component2).isLocalPlayer)
				{
					num++;
				}
			}
		}
		return num == NetworkUser.readOnlyLocalPlayersList.Count;
	}

	public void ClientSetReady()
	{
		foreach (NetworkUser readOnlyLocalPlayers in NetworkUser.readOnlyLocalPlayersList)
		{
			if (Object.op_Implicit((Object)(object)readOnlyLocalPlayers))
			{
				readOnlyLocalPlayers.CallCmdSubmitVote(((Component)PreGameController.instance).gameObject, 0);
			}
			else
			{
				Debug.Log((object)"Null network user in readonly local player list!!");
			}
		}
	}

	public void ClientSetUnready()
	{
		foreach (NetworkUser readOnlyLocalPlayers in NetworkUser.readOnlyLocalPlayersList)
		{
			readOnlyLocalPlayers.CallCmdSubmitVote(((Component)PreGameController.instance).gameObject, -1);
		}
	}
}
