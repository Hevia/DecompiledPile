using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(VoteController))]
public class CreditsController : MonoBehaviour
{
	private CreditsPanelController creditsPanelController;

	private VoteController voteController;

	private static GameObject creditsPanelPrefab => LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Credits/CreditsPanel");

	private void Awake()
	{
		voteController = ((Component)this).GetComponent<VoteController>();
	}

	private void OnEnable()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		creditsPanelController = Object.Instantiate<GameObject>(creditsPanelPrefab, ((Component)RoR2Application.instance.mainCanvas).transform).GetComponent<CreditsPanelController>();
		creditsPanelController.voteInfoPanel.voteController = voteController;
		((UnityEvent)((Button)creditsPanelController.skipButton).onClick).AddListener(new UnityAction(SubmitLocalVotesToEnd));
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)creditsPanelController))
		{
			Object.Destroy((Object)(object)((Component)creditsPanelController).gameObject);
		}
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)creditsPanelController))
		{
			SubmitLocalVotesToEnd();
			((Behaviour)this).enabled = false;
		}
	}

	private void SubmitLocalVotesToEnd()
	{
		foreach (NetworkUser readOnlyLocalPlayers in NetworkUser.readOnlyLocalPlayersList)
		{
			readOnlyLocalPlayers.CallCmdSubmitVote(((Component)this).gameObject, 0);
		}
	}
}
