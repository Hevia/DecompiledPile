using RoR2;
using UnityEngine;

namespace EntityStates.Engi.Mine;

public class BaseMineArmingState : BaseState
{
	[SerializeField]
	public float damageScale;

	[SerializeField]
	public float forceScale;

	[SerializeField]
	public float blastRadiusScale;

	[SerializeField]
	public float triggerRadius;

	[SerializeField]
	public string onEnterSfx;

	[SerializeField]
	public float onEnterSfxPlaybackRate;

	[SerializeField]
	public string pathToChildToEnable;

	private Transform enabledChild;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlayAttackSpeedSound(onEnterSfx, base.gameObject, onEnterSfxPlaybackRate);
		if (!string.IsNullOrEmpty(pathToChildToEnable))
		{
			enabledChild = base.transform.Find(pathToChildToEnable);
			if (Object.op_Implicit((Object)(object)enabledChild))
			{
				((Component)enabledChild).gameObject.SetActive(true);
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)enabledChild))
		{
			((Component)enabledChild).gameObject.SetActive(false);
		}
		base.OnExit();
	}
}
