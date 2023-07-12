using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class RuleCategoryController : MonoBehaviour
{
	[Header("Header")]
	public Image[] headerColorImages;

	public LanguageTextMeshController categoryHeaderLanguageController;

	public GameObject tipPrefab;

	public RectTransform tipContainer;

	public GameObject editCategoryButtonObject;

	[Header("Rules, Strip")]
	public GameObject stripPrefab;

	public RectTransform stripContainer;

	public RectTransform framePanel;

	[Header("Rules, Grid +  Popout Panel")]
	public RectTransform voteResultGridContainer;

	public RectTransform voteResultIconPrefab;

	public RectTransform popoutPanelIconPrefab;

	public GameObject popoutPanelPrefab;

	public RectTransform popoutPanelContainer;

	public bool displayOnlyNonDefaultResults;

	private MPEventSystemLocator eventSystemLocator;

	private readonly List<RuleDef> rulesToDisplay = new List<RuleDef>(RuleCatalog.ruleCount);

	private RuleCatalog.RuleCategoryType ruleCategoryType;

	private GameObject tipObject;

	private HGPopoutPanel popoutPanelInstance;

	private UIElementAllocator<RectTransform> voteStripAllocator;

	private UIElementAllocator<RuleChoiceController> voteResultIconAllocator;

	private UIElementAllocator<RuleChoiceController> popoutButtonIconAllocator;

	private GameObject popoutRandomButtonContainer;

	private MPButton popoutRandomButton;

	private RuleChoiceMask cachedAvailability;

	private RuleCategoryDef currentCategory;

	public RectTransform popoutPanelContentContainer => popoutPanelInstance.popoutPanelContentContainer;

	public LanguageTextMeshController popoutPanelTitleText => popoutPanelInstance.popoutPanelTitleText;

	public LanguageTextMeshController popoutPanelSubtitleText => popoutPanelInstance.popoutPanelSubtitleText;

	public LanguageTextMeshController popoutPanelDescriptionText => popoutPanelInstance.popoutPanelDescriptionText;

	public bool shouldHide
	{
		get
		{
			if ((!isEmpty || Object.op_Implicit((Object)(object)tipObject)) && currentCategory != null)
			{
				return currentCategory.isHidden;
			}
			return true;
		}
	}

	public bool isEmpty
	{
		get
		{
			if (voteStripAllocator.elements.Count == 0)
			{
				return voteResultIconAllocator.elements.Count == 0;
			}
			return false;
		}
	}

	private void Awake()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		cachedAvailability = new RuleChoiceMask();
		if (Object.op_Implicit((Object)(object)popoutPanelPrefab) && Object.op_Implicit((Object)(object)popoutPanelContainer) && Object.op_Implicit((Object)(object)editCategoryButtonObject))
		{
			popoutPanelInstance = Object.Instantiate<GameObject>(popoutPanelPrefab, (Transform)(object)popoutPanelContainer).GetComponent<HGPopoutPanel>();
			ChildLocator component = ((Component)popoutPanelInstance).GetComponent<ChildLocator>();
			object obj;
			if (component == null)
			{
				obj = null;
			}
			else
			{
				Transform obj2 = component.FindChild("RandomButtonContainer");
				obj = ((obj2 != null) ? ((Component)obj2).gameObject : null);
			}
			popoutRandomButtonContainer = (GameObject)obj;
			object obj3;
			if (component == null)
			{
				obj3 = null;
			}
			else
			{
				Transform obj4 = component.FindChild("RandomButton");
				obj3 = ((obj4 != null) ? ((Component)obj4).GetComponent<HGButton>() : null);
			}
			popoutRandomButton = (MPButton)obj3;
			if (Object.op_Implicit((Object)(object)popoutRandomButton))
			{
				((UnityEvent)((Button)popoutRandomButton).onClick).AddListener(new UnityAction(SetRandomVotes));
			}
			((UnityEvent)((Button)editCategoryButtonObject.GetComponent<HGButton>()).onClick).AddListener(new UnityAction(TogglePopoutPanel));
		}
		voteStripAllocator = new UIElementAllocator<RectTransform>(stripContainer, stripPrefab.gameObject);
		voteResultIconAllocator = new UIElementAllocator<RuleChoiceController>(voteResultGridContainer, ((Component)voteResultIconPrefab).gameObject);
		popoutButtonIconAllocator = new UIElementAllocator<RuleChoiceController>(popoutPanelContentContainer, ((Component)popoutPanelIconPrefab).gameObject);
	}

	private void TogglePopoutPanel()
	{
		if (Object.op_Implicit((Object)(object)popoutPanelInstance))
		{
			((Component)popoutPanelInstance).gameObject.SetActive(!((Component)popoutPanelInstance).gameObject.activeSelf);
		}
	}

	public void SetRandomVotes()
	{
		PreGameRuleVoteController preGameRuleVoteController = PreGameRuleVoteController.FindForUser(eventSystemLocator.eventSystem?.localUser?.currentNetworkUser);
		if (!Object.op_Implicit((Object)(object)preGameRuleVoteController))
		{
			return;
		}
		List<RuleChoiceDef> list = new List<RuleChoiceDef>();
		foreach (RuleDef child in currentCategory.children)
		{
			list.Clear();
			foreach (RuleChoiceDef choice in child.choices)
			{
				if (cachedAvailability[choice.globalIndex])
				{
					list.Add(choice);
				}
			}
			int choiceValue = -1;
			if (list.Count > 0 && Random.value > 0.5f)
			{
				choiceValue = list[Random.Range(0, list.Count)].localIndex;
			}
			preGameRuleVoteController.SetVote(child.globalIndex, choiceValue);
		}
	}

	private void SetTip(string tipToken)
	{
		if (tipToken == null)
		{
			Object.Destroy((Object)(object)tipObject);
			tipObject = null;
			return;
		}
		((Component)stripContainer).gameObject.SetActive(false);
		((Component)voteResultGridContainer).gameObject.SetActive(false);
		if (!Object.op_Implicit((Object)(object)tipObject))
		{
			tipObject = Object.Instantiate<GameObject>(tipPrefab, (Transform)(object)tipContainer);
			tipObject.SetActive(true);
		}
		tipObject.GetComponentInChildren<LanguageTextMeshController>().token = tipToken;
	}

	private void AllocateStrips(int desiredCount)
	{
		voteStripAllocator.AllocateElements(desiredCount);
		((Transform)framePanel).SetAsLastSibling();
	}

	private void AllocateResultIcons(int desiredCount)
	{
		voteResultIconAllocator.AllocateElements(desiredCount);
	}

	public void SetData(RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		currentCategory = categoryDef;
		ruleCategoryType = categoryDef.ruleCategoryType;
		cachedAvailability.Copy(availability);
		rulesToDisplay.Clear();
		bool active = false;
		List<RuleDef> children = categoryDef.children;
		for (int i = 0; i < children.Count; i++)
		{
			RuleDef ruleDef = children[i];
			bool flag = false;
			int num = ruleDef.AvailableChoiceCount(availability);
			if (!availability[ruleDef.choices[ruleDef.defaultChoiceIndex].globalIndex] && num != 0)
			{
				flag = true;
			}
			if (num > 1)
			{
				flag = true;
				active = true;
			}
			if (ruleDef.globalName == "Difficulty")
			{
				flag = true;
			}
			if (flag || ruleDef.forceLobbyDisplay)
			{
				rulesToDisplay.Add(children[i]);
			}
		}
		Image[] array = headerColorImages;
		for (int j = 0; j < array.Length; j++)
		{
			((Graphic)array[j]).color = categoryDef.color;
		}
		categoryHeaderLanguageController.token = categoryDef.displayToken;
		switch (ruleCategoryType)
		{
		case RuleCatalog.RuleCategoryType.StripVote:
		{
			((Component)stripContainer).gameObject.SetActive(true);
			((Component)voteResultGridContainer).gameObject.SetActive(false);
			editCategoryButtonObject.SetActive(false);
			AllocateStrips(rulesToDisplay.Count);
			List<RuleChoiceDef> list = new List<RuleChoiceDef>();
			for (int m = 0; m < rulesToDisplay.Count; m++)
			{
				RuleDef ruleDef4 = rulesToDisplay[m];
				list.Clear();
				foreach (RuleChoiceDef choice3 in ruleDef4.choices)
				{
					if (availability[choice3.globalIndex])
					{
						list.Add(choice3);
					}
				}
				((Component)voteStripAllocator.elements[m]).GetComponent<RuleBookViewerStrip>().SetData(list, ruleBook.GetRuleChoiceIndex(ruleDef4));
			}
			break;
		}
		case RuleCatalog.RuleCategoryType.VoteResultGrid:
		{
			((Component)stripContainer).gameObject.SetActive(false);
			((Component)voteResultGridContainer).gameObject.SetActive(true);
			Color color = categoryDef.color;
			color.a = 0.2f;
			editCategoryButtonObject.SetActive(true);
			editCategoryButtonObject.GetComponent<HGButton>().hoverToken = Language.GetStringFormatted("RULE_EDIT_FORMAT", Language.GetString(categoryDef.displayToken), Language.GetString(categoryDef.editToken), ColorUtility.ToHtmlStringRGBA(color));
			int count = rulesToDisplay.Count;
			AllocateResultIcons(count);
			for (int k = 0; k < rulesToDisplay.Count; k++)
			{
				RuleDef ruleDef2 = rulesToDisplay[k];
				RuleChoiceController ruleChoiceController = voteResultIconAllocator.elements[k];
				int ruleChoiceIndex = ruleBook.GetRuleChoiceIndex(ruleDef2);
				RuleChoiceDef choice = ruleDef2.choices[ruleChoiceIndex];
				ruleChoiceController.SetChoice(choice);
			}
			popoutPanelTitleText.token = categoryDef.displayToken;
			popoutPanelSubtitleText.token = categoryDef.subtitleToken;
			popoutButtonIconAllocator.AllocateElements(rulesToDisplay.Count);
			for (int l = 0; l < rulesToDisplay.Count; l++)
			{
				RuleDef ruleDef3 = rulesToDisplay[l];
				bool num2 = ruleDef3.choices.Count == 2;
				bool flag2 = ruleDef3.AvailableChoiceCount(availability) > 1;
				int ruleChoiceIndex2 = ruleBook.GetRuleChoiceIndex(ruleDef3);
				RuleChoiceDef choice2 = ruleDef3.choices[ruleChoiceIndex2];
				RuleChoiceController ruleChoiceController2 = popoutButtonIconAllocator.elements[l];
				ruleChoiceController2.displayVoteCounter = false;
				ruleChoiceController2.SetChoice(choice2);
				ruleChoiceController2.cycleThroughOptions = true;
				ruleChoiceController2.requiredTopLayer = ((Component)popoutPanelInstance).GetComponent<UILayerKey>();
				((Behaviour)ruleChoiceController2.tooltipProvider).enabled = false;
				ruleChoiceController2.hgButton.updateTextOnHover = true;
				ruleChoiceController2.hgButton.hoverLanguageTextMeshController = popoutPanelDescriptionText;
				if (num2 && flag2)
				{
					ruleChoiceController2.canVote = true;
				}
				else
				{
					ruleChoiceController2.canVote = false;
				}
			}
			break;
		}
		}
		SetTip(isEmpty ? categoryDef.emptyTipToken : null);
		if (Object.op_Implicit((Object)(object)popoutRandomButtonContainer))
		{
			popoutRandomButtonContainer.SetActive(active);
		}
	}
}
