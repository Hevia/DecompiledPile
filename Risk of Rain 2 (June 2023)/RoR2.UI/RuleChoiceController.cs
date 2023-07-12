using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

public class RuleChoiceController : MonoBehaviour
{
	private static readonly List<RuleChoiceController> instancesList;

	[HideInInspector]
	public RuleBookViewerStrip strip;

	public HGButton hgButton;

	public Image image;

	public TooltipProvider tooltipProvider;

	public TextMeshProUGUI voteCounter;

	public GameObject chosenDisplayObject;

	public GameObject disabledDisplayObject;

	public GameObject cantVoteDisplayObject;

	public UILayerKey requiredTopLayer;

	public bool displayVoteCounter = true;

	public bool canVote;

	public bool cycleThroughOptions;

	private RuleChoiceDef choiceDef;

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	static RuleChoiceController()
	{
		instancesList = new List<RuleChoiceController>();
		PreGameRuleVoteController.onVotesUpdated += delegate
		{
			foreach (RuleChoiceController instances in instancesList)
			{
				instances.UpdateFromVotes();
			}
		};
	}

	private void Start()
	{
		UpdateFromVotes();
		UpdateChoiceDisplay(choiceDef);
		if (Object.op_Implicit((Object)(object)requiredTopLayer) && Object.op_Implicit((Object)(object)hgButton))
		{
			hgButton.requiredTopLayer = requiredTopLayer;
		}
	}

	public void UpdateFromVotes()
	{
		int num = PreGameRuleVoteController.votesForEachChoice[choiceDef.globalIndex];
		bool isInSinglePlayer = RoR2Application.isInSinglePlayer;
		if (Object.op_Implicit((Object)(object)voteCounter))
		{
			if (displayVoteCounter && num > 0 && !isInSinglePlayer)
			{
				((Behaviour)voteCounter).enabled = true;
				((TMP_Text)voteCounter).text = num.ToString();
			}
			else
			{
				((Behaviour)voteCounter).enabled = false;
			}
		}
		bool flag = false;
		NetworkUser networkUser = FindNetworkUser();
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			PreGameRuleVoteController preGameRuleVoteController = PreGameRuleVoteController.FindForUser(networkUser);
			if (Object.op_Implicit((Object)(object)preGameRuleVoteController))
			{
				flag = preGameRuleVoteController.IsChoiceVoted(choiceDef);
			}
		}
		bool flag2 = choiceDef.globalName.Contains(".Off");
		if (Object.op_Implicit((Object)(object)chosenDisplayObject))
		{
			chosenDisplayObject.SetActive(flag && !flag2);
		}
		if (Object.op_Implicit((Object)(object)disabledDisplayObject))
		{
			disabledDisplayObject.SetActive(flag && flag2);
		}
		if (Object.op_Implicit((Object)(object)cantVoteDisplayObject))
		{
			cantVoteDisplayObject.SetActive(!canVote);
		}
	}

	public void SetChoice([NotNull] RuleChoiceDef newChoiceDef)
	{
		if (newChoiceDef != choiceDef)
		{
			choiceDef = newChoiceDef;
			UpdateChoiceDisplay(choiceDef);
			UpdateFromVotes();
		}
	}

	private void UpdateChoiceDisplay(RuleChoiceDef displayChoiceDef)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		((Object)((Component)this).gameObject).name = "Choice (" + displayChoiceDef.globalName + ")";
		image.sprite = displayChoiceDef.sprite;
		if (Object.op_Implicit((Object)(object)tooltipProvider))
		{
			if (displayChoiceDef.tooltipNameToken == null)
			{
				Debug.LogErrorFormat("Rule choice {0} .tooltipNameToken is null", new object[1] { displayChoiceDef.globalName });
			}
			if (displayChoiceDef.tooltipBodyToken == null)
			{
				Debug.LogErrorFormat("Rule choice {0} .tooltipBodyToken is null", new object[1] { displayChoiceDef.tooltipBodyToken });
			}
			tooltipProvider.overrideTitleText = displayChoiceDef.getTooltipName(displayChoiceDef);
			tooltipProvider.titleColor = displayChoiceDef.tooltipNameColor;
			tooltipProvider.bodyToken = displayChoiceDef.tooltipBodyToken;
			tooltipProvider.bodyColor = displayChoiceDef.tooltipBodyColor;
		}
		if (Object.op_Implicit((Object)(object)hgButton))
		{
			if (hgButton.updateTextOnHover && Object.op_Implicit((Object)(object)hgButton.hoverLanguageTextMeshController))
			{
				string @string = Language.GetString(displayChoiceDef.tooltipNameToken);
				string string2 = Language.GetString(displayChoiceDef.tooltipBodyToken);
				Color tooltipNameColor = displayChoiceDef.tooltipNameColor;
				tooltipNameColor.a = 0.2f;
				string stringFormatted = Language.GetStringFormatted("RULE_DESCRIPTION_FORMAT", @string, string2, ColorUtility.ToHtmlStringRGBA(tooltipNameColor));
				hgButton.hoverToken = stringFormatted;
				hgButton.hoverLanguageTextMeshController.token = stringFormatted;
			}
			hgButton.uiClickSoundOverride = choiceDef.selectionUISound;
		}
	}

	private NetworkUser FindNetworkUser()
	{
		return ((MPEventSystem)(object)EventSystem.current).localUser?.currentNetworkUser;
	}

	public void OnClick()
	{
		if (!canVote)
		{
			return;
		}
		NetworkUser networkUser = FindNetworkUser();
		Debug.Log((object)networkUser);
		if (!Object.op_Implicit((Object)(object)networkUser))
		{
			return;
		}
		PreGameRuleVoteController preGameRuleVoteController = PreGameRuleVoteController.FindForUser(networkUser);
		if (Object.op_Implicit((Object)(object)preGameRuleVoteController))
		{
			RuleChoiceDef ruleChoiceDef;
			RuleChoiceDef ruleChoiceDef2 = (ruleChoiceDef = choiceDef);
			RuleDef ruleDef = ruleChoiceDef2.ruleDef;
			int count = ruleDef.choices.Count;
			int localIndex = ruleChoiceDef2.localIndex;
			bool flag = false;
			Debug.LogFormat("maxRuleCount={0}, currentChoiceIndex={1}", new object[2] { count, localIndex });
			if (cycleThroughOptions)
			{
				localIndex = (preGameRuleVoteController.IsChoiceVoted(choiceDef) ? (localIndex + 1) : 0);
				if (localIndex > count - 1)
				{
					localIndex = ruleDef.defaultChoiceIndex;
					flag = true;
				}
				ruleChoiceDef = ruleDef.choices[localIndex];
			}
			else if (preGameRuleVoteController.IsChoiceVoted(choiceDef))
			{
				flag = true;
			}
			SetChoice(ruleChoiceDef);
			preGameRuleVoteController.SetVote(ruleChoiceDef.ruleDef.globalIndex, flag ? (-1) : ruleChoiceDef.localIndex);
		}
		else
		{
			Debug.Log((object)"voteController=null");
		}
	}
}
