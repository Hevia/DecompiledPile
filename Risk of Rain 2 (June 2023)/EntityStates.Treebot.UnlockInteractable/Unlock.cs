using System;
using RoR2;
using RoR2.Mecanim;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Treebot.UnlockInteractable;

public class Unlock : BaseState
{
	public static event Action<Interactor> onActivated;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active)
		{
			Unlock.onActivated?.Invoke(GetComponent<PurchaseInteraction>().lastActivator);
		}
		GetModelAnimator().SetBool("Revive", true);
		((Behaviour)((Component)GetModelTransform()).GetComponent<RandomBlinkController>()).enabled = true;
	}
}
