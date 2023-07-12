using RoR2;
using UnityEngine;

namespace EntityStates.MinorConstruct;

public class BaseHideState : BaseState
{
	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string childToEnable;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		Transform val = FindModelChild(childToEnable);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(true);
		}
	}

	public override void OnExit()
	{
		Transform val = FindModelChild(childToEnable);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(false);
		}
		base.OnExit();
	}
}
