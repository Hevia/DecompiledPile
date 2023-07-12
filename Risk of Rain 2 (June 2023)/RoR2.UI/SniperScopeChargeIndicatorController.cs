using EntityStates.Sniper.Scope;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(HudElement))]
public class SniperScopeChargeIndicatorController : MonoBehaviour
{
	private GameObject sourceGameObject;

	private HudElement hudElement;

	public Image image;

	private void Awake()
	{
		hudElement = ((Component)this).GetComponent<HudElement>();
	}

	private void FixedUpdate()
	{
		float fillAmount = 0f;
		if (Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			SkillLocator component = ((Component)hudElement.targetCharacterBody).GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.secondary))
			{
				EntityStateMachine stateMachine = component.secondary.stateMachine;
				if (Object.op_Implicit((Object)(object)stateMachine) && stateMachine.state is ScopeSniper scopeSniper)
				{
					fillAmount = scopeSniper.charge;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)image))
		{
			image.fillAmount = fillAmount;
		}
	}
}
