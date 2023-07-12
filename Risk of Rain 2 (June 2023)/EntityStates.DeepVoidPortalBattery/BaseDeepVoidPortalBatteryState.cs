using RoR2;
using UnityEngine;

namespace EntityStates.DeepVoidPortalBattery;

public class BaseDeepVoidPortalBatteryState : BaseState
{
	[SerializeField]
	public string onEnterSoundString;

	[SerializeField]
	public string onEnterChildToEnable;

	[SerializeField]
	public string animationStateName;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(onEnterSoundString, base.gameObject);
		GameObject val = FindModelChildGameObject(onEnterChildToEnable);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(true);
		}
		PlayAnimation("Base", animationStateName);
	}

	public override void OnExit()
	{
		GameObject val = FindModelChildGameObject(onEnterChildToEnable);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(false);
		}
		base.OnExit();
	}
}
